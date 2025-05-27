using System;
using System.Collections.Generic;

namespace ViridiscaUi.Domain.Models.Financial;

/// <summary>
/// Базовая сущность с аудитом для финансовых моделей
/// </summary>
public abstract class FinancialAuditableEntity
{
    public Guid Uid { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
}

/// <summary>
/// Финансовая сводка
/// </summary>
public class FinancialSummary
{
    public decimal TotalRevenue { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetProfit { get; set; }
    public decimal Budget { get; set; }
    public decimal BudgetUtilization { get; set; }
    public decimal PendingPayments { get; set; }
    public decimal OverduePayments { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
}

/// <summary>
/// Финансовый обзор
/// </summary>
public class FinancialOverview
{
    public Guid Uid { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal Balance { get; set; }
    public DateTime ReportDate { get; set; }
    public string Currency { get; set; } = "RUB";
    public List<FinancialCategory> Categories { get; set; } = new();
}

/// <summary>
/// Бюджетный отчет
/// </summary>
public class BudgetReport
{
    public Guid Uid { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal PlannedIncome { get; set; }
    public decimal ActualIncome { get; set; }
    public decimal PlannedExpenses { get; set; }
    public decimal ActualExpenses { get; set; }
    public decimal Variance { get; set; }
    public BudgetStatus Status { get; set; }
    public List<BudgetItem> Items { get; set; } = new();
}

/// <summary>
/// Бюджет
/// </summary>
public class Budget : FinancialAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal AllocatedAmount { get; set; }
    public decimal SpentAmount { get; set; }
    public decimal RemainingAmount => Amount - SpentAmount;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public BudgetStatus Status { get; set; }
    public BudgetCategory Category { get; set; }
    public Guid? DepartmentId { get; set; }
    public List<BudgetItem> Items { get; set; } = new();
}

/// <summary>
/// Финансовая транзакция
/// </summary>
public class FinancialTransaction : FinancialAuditableEntity
{
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public DateTime TransactionDate { get; set; }
    public string Reference { get; set; } = string.Empty;
    public Guid? BudgetId { get; set; }
    public Budget? Budget { get; set; }
    public Guid? PaymentId { get; set; }
    public Payment? Payment { get; set; }
    public Guid? InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }
    public TransactionStatus Status { get; set; }
    public string? Notes { get; set; }
    public string Currency { get; set; } = "RUB";
}

/// <summary>
/// Платеж
/// </summary>
public class Payment : FinancialAuditableEntity
{
    public string PaymentNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public DateTime? DueDate { get; set; }
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid? StudentId { get; set; }
    public Guid? InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }
    public string? TransactionReference { get; set; }
    public string Currency { get; set; } = "RUB";
}

/// <summary>
/// Счет-фактура
/// </summary>
public class Invoice : FinancialAuditableEntity
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount => Amount - PaidAmount;
    public InvoiceStatus Status { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid? StudentId { get; set; }
    public Guid? CourseId { get; set; }
    public List<InvoiceItem> Items { get; set; } = new();
    public List<Payment> Payments { get; set; } = new();
    public string Currency { get; set; } = "RUB";
}

/// <summary>
/// Отчет о доходах и расходах
/// </summary>
public class IncomeExpenseReport
{
    public Guid Uid { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetIncome => TotalIncome - TotalExpenses;
    public List<IncomeCategory> IncomeCategories { get; set; } = new();
    public List<ExpenseCategory> ExpenseCategories { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

#region Supporting Classes

/// <summary>
/// Элемент бюджета
/// </summary>
public class BudgetItem
{
    public Guid Uid { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal PlannedAmount { get; set; }
    public decimal ActualAmount { get; set; }
    public decimal Variance => ActualAmount - PlannedAmount;
    public BudgetItemCategory Category { get; set; }
}

/// <summary>
/// Элемент счета-фактуры
/// </summary>
public class InvoiceItem
{
    public Guid Uid { get; set; } = Guid.NewGuid();
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => Quantity * UnitPrice;
    public string? Notes { get; set; }
}

/// <summary>
/// Финансовая категория
/// </summary>
public class FinancialCategory
{
    public Guid Uid { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public FinancialCategoryType Type { get; set; }
}

/// <summary>
/// Категория доходов
/// </summary>
public class IncomeCategory
{
    public Guid Uid { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Категория расходов
/// </summary>
public class ExpenseCategory
{
    public Guid Uid { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
}

#endregion

#region Enums

/// <summary>
/// Тип транзакции
/// </summary>
public enum TransactionType
{
    Income,
    Expense,
    Transfer,
    Refund,
    Fee,
    Tuition,
    Scholarship,
    Grant
}

/// <summary>
/// Статус транзакции
/// </summary>
public enum TransactionStatus
{
    Pending,
    Completed,
    Failed,
    Cancelled,
    Refunded
}

/// <summary>
/// Метод платежа
/// </summary>
public enum PaymentMethod
{
    Cash,
    BankTransfer,
    CreditCard,
    DebitCard,
    OnlinePayment,
    Check,
    Other
}

/// <summary>
/// Статус платежа
/// </summary>
public enum PaymentStatus
{
    Pending,
    Completed,
    Failed,
    Cancelled,
    Refunded,
    PartiallyPaid
}

/// <summary>
/// Статус счета-фактуры
/// </summary>
public enum InvoiceStatus
{
    Draft,
    Sent,
    Paid,
    PartiallyPaid,
    Overdue,
    Cancelled,
    Refunded
}

/// <summary>
/// Статус бюджета
/// </summary>
public enum BudgetStatus
{
    Draft,
    Active,
    Completed,
    Cancelled,
    OnHold
}

/// <summary>
/// Категория бюджета
/// </summary>
public enum BudgetCategory
{
    Education,
    Infrastructure,
    Personnel,
    Marketing,
    Technology,
    Operations,
    Research,
    Other
}

/// <summary>
/// Категория элемента бюджета
/// </summary>
public enum BudgetItemCategory
{
    Salaries,
    Equipment,
    Utilities,
    Supplies,
    Services,
    Travel,
    Training,
    Other
}

/// <summary>
/// Тип финансовой категории
/// </summary>
public enum FinancialCategoryType
{
    Income,
    Expense,
    Asset,
    Liability
}

#endregion 