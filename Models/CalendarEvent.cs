using System;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace OutlookCalendar.Models;

/// <summary>
/// Категория события календаря с предустановленными цветами в стиле Outlook.
/// </summary>
public enum EventCategory
{
    None,
    Important,      // Красный
    Business,       // Синий
    Personal,       // Зелёный
    Holiday,        // Оранжевый
    Meeting,        // Жёлтый
    Travel,         // Голубой
    Birthday,       // Фиолетовый
    Reminder        // Серый
}

/// <summary>
/// Модель события календаря.
/// Использует ObservableObject из CommunityToolkit.Mvvm для уведомлений об изменениях.
/// </summary>
public partial class CalendarEvent : ObservableObject
{
    [ObservableProperty]
    private Guid _id = Guid.NewGuid();

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _location = string.Empty;

    [ObservableProperty]
    private DateTime _startTime;

    [ObservableProperty]
    private DateTime _endTime;

    [ObservableProperty]
    private bool _isAllDay;

    [ObservableProperty]
    private EventCategory _category = EventCategory.None;

    [ObservableProperty]
    private bool _isHighPriority;

    [ObservableProperty]
    private string _notes = string.Empty;

    [ObservableProperty]
    private bool _hasReminder;

    [ObservableProperty]
    private TimeSpan _reminderTime = TimeSpan.FromMinutes(15);

    /// <summary>
    /// Продолжительность события.
    /// </summary>
    public TimeSpan Duration => EndTime - StartTime;

    /// <summary>
    /// Проверяет, является ли событие многодневным.
    /// </summary>
    public bool IsMultiDay => StartTime.Date != EndTime.Date;

    /// <summary>
    /// Проверяет, попадает ли событие в указанную дату.
    /// </summary>
    public bool OccursOnDate(DateTime date)
    {
        var dateOnly = date.Date;
        return StartTime.Date <= dateOnly && EndTime.Date >= dateOnly;
    }

    /// <summary>
    /// Проверяет, пересекается ли событие с указанным временным диапазоном.
    /// </summary>
    public bool OverlapsWith(DateTime start, DateTime end)
    {
        return StartTime < end && EndTime > start;
    }

    /// <summary>
    /// Возвращает цвет фона для категории события.
    /// </summary>
    public static Color GetCategoryColor(EventCategory category) => category switch
    {
        EventCategory.Important => Color.FromRgb(255, 99, 71),    // Томатный красный
        EventCategory.Business => Color.FromRgb(100, 149, 237),   // Васильковый синий
        EventCategory.Personal => Color.FromRgb(144, 238, 144),   // Светло-зелёный
        EventCategory.Holiday => Color.FromRgb(255, 165, 0),      // Оранжевый
        EventCategory.Meeting => Color.FromRgb(255, 255, 150),    // Светло-жёлтый
        EventCategory.Travel => Color.FromRgb(173, 216, 230),     // Светло-голубой
        EventCategory.Birthday => Color.FromRgb(221, 160, 221),   // Сливовый
        EventCategory.Reminder => Color.FromRgb(192, 192, 192),   // Серый
        _ => Color.FromRgb(230, 230, 250)                         // Лавандовый по умолчанию
    };

    /// <summary>
    /// Возвращает цвет текста для категории (тёмный для светлых фонов).
    /// </summary>
    public static Color GetCategoryTextColor(EventCategory category) => category switch
    {
        EventCategory.Important => Colors.White,
        EventCategory.Business => Colors.White,
        _ => Color.FromRgb(33, 33, 33)
    };

    /// <summary>
    /// Создаёт копию события.
    /// </summary>
    public CalendarEvent Clone() => new()
    {
        Id = Guid.NewGuid(),
        Title = Title,
        Description = Description,
        Location = Location,
        StartTime = StartTime,
        EndTime = EndTime,
        IsAllDay = IsAllDay,
        Category = Category,
        IsHighPriority = IsHighPriority,
        Notes = Notes,
        HasReminder = HasReminder,
        ReminderTime = ReminderTime
    };
}
