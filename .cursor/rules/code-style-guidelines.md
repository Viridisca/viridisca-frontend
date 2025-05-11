# Правила стиля кода ViridiscaUi

## C# Правила

### 1. Namespace и организация файлов

```csharp
// Правильно
namespace ViridiscaUi.Domain.Models;

public class Student
{
    // Implementation
}

// Неправильно
namespace ViridiscaUi.Domain.Models
{
    public class Student
    {
        // Implementation
    }
}
```

### 2. Структура файлов
- Один публичный тип на файл
- Имя файла должно точно соответствовать имени типа
- Используйте PascalCase для имен файлов
- Группируйте связанные типы в соответствующих namespace

### 3. Именование
- Используйте PascalCase для публичных членов
- Используйте camelCase для приватных членов
- Используйте префикс `_` для приватных полей
- Используйте суффиксы для типов: `ViewModel`, `View`, `Service`, `Repository`

## Avalonia UI Правила

### 1. Адаптивная верстка

#### 1.1 Запрещенные практики
```xml
<!-- НЕПРАВИЛЬНО - Фиксированные размеры -->
<StackPanel Width="200" Height="100">
    <TextBlock Text="Content" />
</StackPanel>

<!-- НЕПРАВИЛЬНО - Проценты -->
<Grid ColumnDefinitions="50%, 50%">
    <TextBlock Grid.Column="0" Text="Left" />
    <TextBlock Grid.Column="1" Text="Right" />
</Grid>
```

#### 1.2 Правильные подходы
```xml
<!-- ПРАВИЛЬНО - Использование Grid с * -->
<Grid RowDefinitions="Auto,*,Auto" ColumnDefinitions="*,Auto">
    <TextBlock Grid.Row="0" Grid.Column="0" Text="Header" />
    <ScrollViewer Grid.Row="1" Grid.Column="0" Grid.ColumnSpan="2">
        <StackPanel>
            <!-- Content -->
        </StackPanel>
    </ScrollViewer>
    <Button Grid.Row="2" Grid.Column="1" Content="Action" />
</Grid>

<!-- ПРАВИЛЬНО - Использование DockPanel -->
<DockPanel>
    <Menu DockPanel.Dock="Top">
        <!-- Menu items -->
    </Menu>
    <ToolBar DockPanel.Dock="Top">
        <!-- Toolbar items -->
    </ToolBar>
    <StatusBar DockPanel.Dock="Bottom">
        <!-- Status items -->
    </StatusBar>
    <Grid>
        <!-- Main content -->
    </Grid>
</DockPanel>
```

### 2. Обработка текста

#### 2.1 Правильный подход к тексту
```xml
<!-- ПРАВИЛЬНО - Адаптивный текст с переносом -->
<TextBlock TextWrapping="Wrap" 
           TextTrimming="None"
           MaxWidth="{Binding RelativeSource={RelativeSource AncestorType=Control}, Path=ActualWidth}">
    <TextBlock.Text>
        <MultiBinding StringFormat="{}{0} - {1}">
            <Binding Path="Title" />
            <Binding Path="Description" />
        </MultiBinding>
    </TextBlock.Text>
</TextBlock>

<!-- ПРАВИЛЬНО - Адаптивный текст в списке -->
<ItemsControl Items="{Binding Items}">
    <ItemsControl.ItemsPanel>
        <ItemsPanelTemplate>
            <UniformGrid Columns="3" />
        </ItemsPanelTemplate>
    </ItemsControl.ItemsPanel>
    <ItemsControl.ItemTemplate>
        <DataTemplate>
            <Border Margin="4" Padding="8">
                <StackPanel>
                    <TextBlock Text="{Binding Title}" 
                             TextWrapping="Wrap" 
                             TextTrimming="None" />
                    <TextBlock Text="{Binding Description}" 
                             TextWrapping="Wrap" 
                             TextTrimming="None" />
                </StackPanel>
            </Border>
        </DataTemplate>
    </ItemsControl.ItemTemplate>
</ItemsControl>
```

### 3. Паттерны UI

#### 3.1 Композиция компонентов
```xml
<!-- ПРАВИЛЬНО - Композиция через пользовательские контролы -->
<StackPanel>
    <local:HeaderView />
    <local:ContentArea>
        <local:DataGrid />
    </local:ContentArea>
    <local:StatusBar />
</StackPanel>
```

#### 3.2 Использование стилей
```xml
<!-- ПРАВИЛЬНО - Определение стилей -->
<Styles>
    <Style Selector="TextBlock.header">
        <Setter Property="FontSize" Value="16" />
        <Setter Property="FontWeight" Value="SemiBold" />
        <Setter Property="Margin" Value="0,0,0,8" />
    </Style>
</Styles>
```

### 4. Масштабирование и адаптивность

#### 4.1 Изображения
```xml
<!-- ПРАВИЛЬНО - Адаптивные изображения -->
<Image Stretch="Uniform" 
       StretchDirection="DownOnly"
       MaxWidth="{Binding RelativeSource={RelativeSource AncestorType=Control}, Path=ActualWidth}">
    <Image.Source>
        <Binding Path="ImageSource" />
    </Image.Source>
</Image>
```

#### 4.2 Контейнеры
```xml
<!-- ПРАВИЛЬНО - Адаптивные контейнеры -->
<ScrollViewer HorizontalScrollBarVisibility="Disabled" 
              VerticalScrollBarVisibility="Auto">
    <StackPanel>
        <TextBlock TextWrapping="Wrap" />
        <ItemsControl Items="{Binding Items}">
            <ItemsControl.ItemsPanel>
                <ItemsPanelTemplate>
                    <WrapPanel />
                </ItemsPanelTemplate>
            </ItemsControl.ItemsPanel>
        </ItemsControl>
    </StackPanel>
</ScrollViewer>
```

### 5. Рекомендации по производительности

1. Используйте виртуализацию для больших списков
2. Применяйте ленивую загрузку для тяжелых компонентов
3. Используйте кэширование для часто используемых данных
4. Оптимизируйте привязки данных, избегая сложных преобразований

### 6. Отладка UI

1. Используйте `Debug.WriteLine` для отслеживания изменений в привязках
2. Применяйте временные границы для визуализации layout
3. Используйте инструменты разработчика Avalonia для инспекции UI 