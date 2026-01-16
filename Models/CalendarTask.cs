using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace OutlookCalendar.Models;

/// <summary>
/// Приоритет задачи.
/// </summary>
public enum TaskPriority
{
    Low,
    Normal,
    High
}

/// <summary>
/// Статус выполнения задачи.
/// </summary>
public enum TaskStatus
{
    NotStarted,
    InProgress,
    Completed,
    Deferred,
    Cancelled
}

/// <summary>
/// Модель задачи календаря.
/// </summary>
public partial class CalendarTask : ObservableObject
{
    [ObservableProperty]
    private Guid _id = Guid.NewGuid();

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private DateTime? _dueDate;

    [ObservableProperty]
    private DateTime? _startDate;

    [ObservableProperty]
    private DateTime? _completedDate;

    [ObservableProperty]
    private TaskPriority _priority = TaskPriority.Normal;

    [ObservableProperty]
    private TaskStatus _status = TaskStatus.NotStarted;

    [ObservableProperty]
    private int _percentComplete;

    [ObservableProperty]
    private bool _hasReminder;

    [ObservableProperty]
    private DateTime? _reminderDateTime;

    /// <summary>
    /// Проверяет, просрочена ли задача.
    /// </summary>
    public bool IsOverdue => DueDate.HasValue 
        && DueDate.Value.Date < DateTime.Today 
        && Status != TaskStatus.Completed 
        && Status != TaskStatus.Cancelled;

    /// <summary>
    /// Проверяет, выполнена ли задача.
    /// </summary>
    public bool IsCompleted => Status == TaskStatus.Completed;

    /// <summary>
    /// Отмечает задачу как выполненную.
    /// </summary>
    public void MarkComplete()
    {
        Status = TaskStatus.Completed;
        PercentComplete = 100;
        CompletedDate = DateTime.Now;
    }

    /// <summary>
    /// Сбрасывает статус задачи.
    /// </summary>
    public void Reset()
    {
        Status = TaskStatus.NotStarted;
        PercentComplete = 0;
        CompletedDate = null;
    }
}
