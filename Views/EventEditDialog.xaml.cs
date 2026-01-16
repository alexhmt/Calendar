using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using OutlookCalendar.Models;

namespace OutlookCalendar.Views;

/// <summary>
/// ViewModel для диалога редактирования события.
/// </summary>
public class EventEditViewModel : INotifyPropertyChanged
{
    private string _title = string.Empty;
    private DateTime _startDate = DateTime.Today;
    private string _startTimeText = "09:00";
    private DateTime _endDate = DateTime.Today;
    private string _endTimeText = "10:00";
    private bool _isAllDay;
    private string _location = string.Empty;
    private EventCategory _category = EventCategory.Personal;
    private bool _isHighPriority;
    private string _description = string.Empty;

    public string Title
    {
        get => _title;
        set => SetField(ref _title, value);
    }

    public DateTime StartDate
    {
        get => _startDate;
        set => SetField(ref _startDate, value);
    }

    public string StartTimeText
    {
        get => _startTimeText;
        set => SetField(ref _startTimeText, value);
    }

    public DateTime EndDate
    {
        get => _endDate;
        set => SetField(ref _endDate, value);
    }

    public string EndTimeText
    {
        get => _endTimeText;
        set => SetField(ref _endTimeText, value);
    }

    public bool IsAllDay
    {
        get => _isAllDay;
        set => SetField(ref _isAllDay, value);
    }

    public string Location
    {
        get => _location;
        set => SetField(ref _location, value);
    }

    public EventCategory Category
    {
        get => _category;
        set => SetField(ref _category, value);
    }

    public bool IsHighPriority
    {
        get => _isHighPriority;
        set => SetField(ref _isHighPriority, value);
    }

    public string Description
    {
        get => _description;
        set => SetField(ref _description, value);
    }

    /// <summary>
    /// Загружает данные из существующего события.
    /// </summary>
    public void LoadFromEvent(CalendarEvent calendarEvent)
    {
        Title = calendarEvent.Title;
        StartDate = calendarEvent.StartTime.Date;
        StartTimeText = calendarEvent.StartTime.ToString("HH:mm");
        EndDate = calendarEvent.EndTime.Date;
        EndTimeText = calendarEvent.EndTime.ToString("HH:mm");
        IsAllDay = calendarEvent.IsAllDay;
        Location = calendarEvent.Location;
        Category = calendarEvent.Category;
        IsHighPriority = calendarEvent.IsHighPriority;
        Description = calendarEvent.Description;
    }

    /// <summary>
    /// Сохраняет данные в событие.
    /// </summary>
    public void SaveToEvent(CalendarEvent calendarEvent)
    {
        calendarEvent.Title = Title;
        calendarEvent.StartTime = ParseDateTime(StartDate, StartTimeText);
        calendarEvent.EndTime = ParseDateTime(EndDate, EndTimeText);
        calendarEvent.IsAllDay = IsAllDay;
        calendarEvent.Location = Location;
        calendarEvent.Category = Category;
        calendarEvent.IsHighPriority = IsHighPriority;
        calendarEvent.Description = Description;
    }

    /// <summary>
    /// Создаёт новое событие из данных формы.
    /// </summary>
    public CalendarEvent CreateEvent() => new()
    {
        Title = Title,
        StartTime = ParseDateTime(StartDate, StartTimeText),
        EndTime = ParseDateTime(EndDate, EndTimeText),
        IsAllDay = IsAllDay,
        Location = Location,
        Category = Category,
        IsHighPriority = IsHighPriority,
        Description = Description
    };

    /// <summary>
    /// Валидирует данные формы.
    /// </summary>
    public bool Validate(out string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(Title))
        {
            errorMessage = "Введите название события.";
            return false;
        }

        var startDateTime = ParseDateTime(StartDate, StartTimeText);
        var endDateTime = ParseDateTime(EndDate, EndTimeText);

        if (endDateTime <= startDateTime)
        {
            errorMessage = "Время окончания должно быть позже времени начала.";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }

    private static DateTime ParseDateTime(DateTime date, string timeText)
    {
        if (TimeSpan.TryParse(timeText, out var time))
        {
            return date.Date + time;
        }
        return date.Date;
    }

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    #endregion
}

/// <summary>
/// Диалоговое окно редактирования события.
/// </summary>
public partial class EventEditDialog : Window
{
    private readonly EventEditViewModel _viewModel = new();
    private CalendarEvent? _existingEvent;

    public EventEditDialog()
    {
        InitializeComponent();
        DataContext = _viewModel;
        TitleTextBox.Focus();
    }

    /// <summary>
    /// Результат - созданное или изменённое событие.
    /// </summary>
    public CalendarEvent? ResultEvent { get; private set; }

    /// <summary>
    /// Открывает диалог для создания нового события.
    /// </summary>
    public static CalendarEvent? CreateNew(Window owner, DateTime? startTime = null)
    {
        var dialog = new EventEditDialog
        {
            Owner = owner,
            Title = "Новое событие"
        };

        if (startTime.HasValue)
        {
            dialog._viewModel.StartDate = startTime.Value.Date;
            dialog._viewModel.StartTimeText = startTime.Value.ToString("HH:mm");
            dialog._viewModel.EndDate = startTime.Value.Date;
            dialog._viewModel.EndTimeText = startTime.Value.AddHours(1).ToString("HH:mm");
        }

        return dialog.ShowDialog() == true ? dialog.ResultEvent : null;
    }

    /// <summary>
    /// Открывает диалог для редактирования существующего события.
    /// </summary>
    public static bool Edit(Window owner, CalendarEvent calendarEvent)
    {
        var dialog = new EventEditDialog
        {
            Owner = owner,
            Title = "Редактирование события",
            _existingEvent = calendarEvent
        };

        dialog._viewModel.LoadFromEvent(calendarEvent);

        if (dialog.ShowDialog() == true)
        {
            dialog._viewModel.SaveToEvent(calendarEvent);
            return true;
        }

        return false;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (!_viewModel.Validate(out var errorMessage))
        {
            MessageBox.Show(errorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        ResultEvent = _existingEvent ?? _viewModel.CreateEvent();

        if (_existingEvent != null)
        {
            _viewModel.SaveToEvent(_existingEvent);
        }

        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
