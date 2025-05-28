# Observable Subscription Fix for ViridiscaUi LMS

## Problem Analysis

### Issue Description
The navigation Sidebar in ViridiscaUi LMS was not displaying menu buttons despite successful user authentication. The root cause was identified as a broken Observable subscription between `UserSessionService` and `MainViewModel`.

### Symptoms
- ✅ User authentication successful (admin@viridisca.local)
- ✅ Navigation service correctly returns 10 filtered routes for Administrator role
- ✅ All ViewModels have proper RouteAttribute configuration
- ❌ MainViewModel.HandleUserChanged() and UpdateMenuItems() NOT called during authentication
- ❌ GroupedMenuItems remains empty, Sidebar renders no buttons

### Root Cause Analysis

The issue stemmed from a **service lifecycle mismatch** in the dependency injection configuration:

1. **UserSessionService**: Registered as **Singleton** ✅
2. **MainViewModel**: Registered as **Transient** ❌ (PROBLEM)
3. **AuthService**: Registered as **Scoped** ❌ (PROBLEM)

#### The Problem Sequence:
1. Application starts → `MainViewModel` (Transient) created → subscribes to `UserSessionService.CurrentUserObservable`
2. User authenticates → `AuthService` (Scoped) calls `_userSessionService.SetCurrentUser(user)`
3. `BehaviorSubject<User?>` correctly emits via `OnNext(user)`
4. **However**: The `MainViewModel` instance that subscribed may not be the same instance bound to the UI

#### Critical Issues:
- **Transient MainViewModel**: Each DI resolution creates a new instance, breaking Observable continuity
- **Scoped AuthService**: Lifetime doesn't align with Singleton UserSessionService
- **Threading Issues**: Observable subscription not guaranteed to execute on UI thread

## Solution Implementation

### 1. Service Lifecycle Alignment

**File**: `ViridiscaUi/DI/DependencyInjectionExtensions.cs`

```csharp
// BEFORE (Problematic)
services.AddTransient<MainViewModel>();
services.AddScoped<IAuthService, AuthService>();

// AFTER (Fixed)
services.AddSingleton<MainViewModel>();  // ✅ Single instance for Observable continuity
services.AddSingleton<IAuthService, AuthService>();  // ✅ Aligned with UserSessionService
```

### 2. UI Thread Marshalling

**File**: `ViridiscaUi/ViewModels/MainViewModel.cs`

```csharp
private void SubscribeToUserChanges()
{
    _userSessionService.CurrentUserObservable
        .ObserveOn(RxApp.MainThreadScheduler)  // ✅ Ensure UI thread execution
        .Subscribe(user => 
        {
            LogInfo("🔥 Observable subscription triggered: User={User}", user?.Email ?? "null");
            HandleUserChanged(user);
        })
        .DisposeWith(_disposables);
        
    LogInfo("🔥 MainViewModel subscribed to CurrentUserObservable");
}
```

### 3. Enhanced Debugging

Added comprehensive logging to track the Observable flow:

**UserSessionService**:
```csharp
public void SetCurrentUser(User? user)
{
    StatusLogger.LogInfo($"🔥 UserSessionService.SetCurrentUser called: User={user?.Email ?? "null"}", "UserSessionService");
    StatusLogger.LogInfo($"🔥 BehaviorSubject has {(_currentUserSubject.HasObservers ? "observers" : "NO observers")}", "UserSessionService");
    
    _currentUserSubject.OnNext(user);
    
    StatusLogger.LogInfo($"🔥 BehaviorSubject.OnNext completed for user: {user?.Email ?? "null"}", "UserSessionService");
}
```

**AuthService**:
```csharp
StatusLogger.LogInfo($"🔥 [AuthService] About to call SetCurrentUser for: {user.Email}", "AuthService");
_userSessionService.SetCurrentUser(user);
StatusLogger.LogInfo($"🔥 [AuthService] SetCurrentUser completed for: {user.Email}", "AuthService");
```

**MainViewModel**:
```csharp
private void HandleUserChanged(User? user)
{
    LogInfo("🔥 HandleUserChanged called: IsLoggedIn={IsLoggedIn}, User={User}", 
        IsLoggedIn, user?.Email ?? "null");
    
    // ... existing logic ...
    
    UpdateMenuItems(user);
}
```

## Expected Behavior After Fix

### Authentication Flow:
1. User enters credentials → AuthService.AuthenticateAsync()
2. Authentication succeeds → AuthService calls UserSessionService.SetCurrentUser()
3. UserSessionService emits user via BehaviorSubject.OnNext()
4. MainViewModel (Singleton) receives notification on UI thread
5. HandleUserChanged() executes → UpdateMenuItems() called
6. GroupedMenuItems populated → Sidebar renders menu buttons

### Debug Log Sequence:
```
🔥 UserSessionService created (Singleton)
🔥 MainViewModel constructor started (should be Singleton)
🔥 MainViewModel subscribed to CurrentUserObservable
🔥 [AuthService] About to call SetCurrentUser for: admin@viridisca.local
🔥 UserSessionService.SetCurrentUser called: User=admin@viridisca.local
🔥 BehaviorSubject has observers
🔥 Observable subscription triggered: User=admin@viridisca.local
🔥 HandleUserChanged called: IsLoggedIn=True, User=admin@viridisca.local
🔥 UpdateMenuItems called: User=admin@viridisca.local, Routes=10
🔥 Menu updated with 3 groups and 10 total routes
```

## Testing Instructions

### 1. Verify Service Registration
Check that services are registered with correct lifetimes:
- `UserSessionService`: Singleton ✅
- `MainViewModel`: Singleton ✅
- `AuthService`: Singleton ✅

### 2. Test Authentication Flow
1. Start application
2. Navigate to authentication page
3. Login with: admin@viridisca.local / password
4. Verify debug logs show Observable subscription triggering
5. Confirm Sidebar displays menu buttons

### 3. Verify Menu Population
After successful authentication, Sidebar should display:
- **Основное**: Home, Profile
- **Образование**: Students, Groups, Teachers, Subjects, Courses, Assignments, Grades
- **Система**: Notifications, Departments

### 4. Debug Log Verification
Monitor console output for the expected debug log sequence above.

## Additional Recommendations

### 1. Consider Scoped Services for EF Core
While AuthService is now Singleton, consider if database operations need Scoped lifetime:
```csharp
// If EF Core context issues arise, consider:
services.AddScoped<IUserService, UserService>();
services.AddScoped<IRoleService, RoleService>();
// But keep AuthService as Singleton for session management
```

### 2. Observable Error Handling
Add error handling to the Observable subscription:
```csharp
_userSessionService.CurrentUserObservable
    .ObserveOn(RxApp.MainThreadScheduler)
    .Subscribe(
        onNext: user => HandleUserChanged(user),
        onError: ex => LogError(ex, "Error in user session observable")
    )
    .DisposeWith(_disposables);
```

### 3. Memory Leak Prevention
Ensure proper disposal of subscriptions:
```csharp
public override void Dispose()
{
    _disposables?.Dispose();
    StatusBar?.Dispose();
    base.Dispose();
}
```

## Architecture Benefits

This fix aligns with ReactiveUI best practices:
- **Single Source of Truth**: UserSessionService as Singleton maintains consistent state
- **Reactive Data Flow**: Observable pattern ensures UI updates automatically
- **Proper Lifecycle Management**: Service lifetimes match their responsibilities
- **Thread Safety**: UI updates marshalled to main thread

## Conclusion

The Observable subscription issue was resolved by aligning service lifetimes and ensuring proper thread marshalling. The MainViewModel now correctly receives user session changes and updates the navigation menu accordingly. 