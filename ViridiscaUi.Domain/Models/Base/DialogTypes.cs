using System;

namespace ViridiscaUi.Domain.Models.Base;

/// <summary>
/// Результаты диалогов
/// </summary>
public enum DialogResult
{
    None,
    OK,
    Cancel,
    Yes,
    No,
    Save,
    Abort,
    Retry,
    Ignore
}

/// <summary>
/// Кнопки диалогов
/// </summary>
public enum DialogButtons
{
    OK,
    OKCancel,
    YesNo,
    YesNoCancel,
    SaveCancel,
    AbortRetryIgnore
} 