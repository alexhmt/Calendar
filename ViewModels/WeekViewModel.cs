using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OutlookCalendar.Models;

namespace OutlookCalendar.ViewModels;

/// <summary>
/// Режим отображения календаря.
/// </summary>
public enum CalendarViewMode
{
    Day,
    WorkWeek,
    FullWeek,
    Month
}

/// <summary>
/// ViewModel для одного дня в недельном представлении.
/// </summary>
public partial class DayViewModel : ObservableObject
{
    [ObservableProperty]
    private DateTime _date;

    [ObservableProperty]
    private bool _isToday;

    [ObservableProperty]
    private bool _isSelected;

    [ObservableProperty]
    private bool _isWeekend;

    [ObservableProperty]
    private ObservableCollection<CalendarEvent> _events = [];

    [ObservableProperty]
    private ObservableCollection<CalendarEvent> _allDayEvents = [];

    /// <summary>
    /// День месяца.
    /// </summary>
    public int DayNumber => Date.Day;

    /// <summary>
    /// День недели.
    /// </summary>
    public DayOfWeek DayOfWeek => Date.DayOfWeek;

    /// <summary>
    /// Русское название дня недели (краткое).
    /// </summary>
    public string DayOfWeekShort => DayOfWeek switch
    {
        DayOfWeek.Monday => "Пн",
        DayOfWeek.Tuesday => "Вт",
        DayOfWeek.Wednesday => "Ср",
        DayOfWeek.Thursday => "Чт",
        DayOfWeek.Friday => "Пт",
        DayOfWeek.Saturday => "Сб",
        DayOfWeek.Sunday => "Вс",
        _ => string.Empty
    };

    public DayViewModel(DateTime date)
    {
        Date = date;
        IsToday = date.Date == DateTime.Today;
        IsWeekend = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
    }
}

/// <summary>
/// Основной ViewModel для недельного представления календаря.
/// </summary>
public partial class WeekViewModel : ObservableObject
{
    private readonly List<CalendarEvent> _allEvents = [];

    [ObservableProperty]
    private DateTime _currentWeekStart;

    [ObservableProperty]
    private CalendarViewMode _viewMode = CalendarViewMode.WorkWeek;

    [ObservableProperty]
    private ObservableCollection<DayViewModel> _days = [];

    [ObservableProperty]
    private ObservableCollection<CalendarEvent> _multiDayEvents = [];

    [ObservableProperty]
    private ObservableCollection<CalendarTask> _tasks = [];

    [ObservableProperty]
    private CalendarEvent? _selectedEvent;

    [ObservableProperty]
    private DayViewModel? _selectedDay;

    [ObservableProperty]
    private int _startHour = 8;

    [ObservableProperty]
    private int _endHour = 23;

    [ObservableProperty]
    private double _hourHeight = 60.0;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private bool _showWorkWeekOnly = true;

    /// <summary>
    /// Заголовок текущего периода (например, "14 – 18 марта 2005").
    /// </summary>
    public string PeriodTitle
    {
        get
        {
            var endDate = ShowWorkWeekOnly
                ? CurrentWeekStart.AddDays(4)
                : CurrentWeekStart.AddDays(6);

            if (CurrentWeekStart.Month == endDate.Month)
            {
                return $"{CurrentWeekStart.Day} – {endDate.Day} {GetRussianMonth(CurrentWeekStart.Month)} {CurrentWeekStart.Year}";
            }
            return $"{CurrentWeekStart.Day} {GetRussianMonth(CurrentWeekStart.Month)} – {endDate.Day} {GetRussianMonth(endDate.Month)} {CurrentWeekStart.Year}";
        }
    }

    /// <summary>
    /// Часы для отображения в боковой шкале.
    /// </summary>
    public IEnumerable<int> Hours => Enumerable.Range(StartHour, EndHour - StartHour + 1);

    public WeekViewModel()
    {
        CurrentWeekStart = GetWeekStart(DateTime.Today);
        UpdateDays();
        LoadSampleData();
    }

    /// <summary>
    /// Переход к предыдущей неделе.
    /// </summary>
    [RelayCommand]
    private void PreviousWeek()
    {
        CurrentWeekStart = CurrentWeekStart.AddDays(-7);
        UpdateDays();
        OnPropertyChanged(nameof(PeriodTitle));
    }

    /// <summary>
    /// Переход к следующей неделе.
    /// </summary>
    [RelayCommand]
    private void NextWeek()
    {
        CurrentWeekStart = CurrentWeekStart.AddDays(7);
        UpdateDays();
        OnPropertyChanged(nameof(PeriodTitle));
    }

    /// <summary>
    /// Переход к текущей неделе (сегодня).
    /// </summary>
    [RelayCommand]
    private void GoToToday()
    {
        CurrentWeekStart = GetWeekStart(DateTime.Today);
        UpdateDays();
        OnPropertyChanged(nameof(PeriodTitle));
    }

    /// <summary>
    /// Переключение режима отображения.
    /// </summary>
    [RelayCommand]
    private void SetViewMode(CalendarViewMode mode)
    {
        ViewMode = mode;
        UpdateDays();
        OnPropertyChanged(nameof(PeriodTitle));
    }

    /// <summary>
    /// Переключение между рабочей и полной неделей.
    /// </summary>
    [RelayCommand]
    private void ToggleWeekMode()
    {
        ShowWorkWeekOnly = !ShowWorkWeekOnly;
        UpdateDays();
        OnPropertyChanged(nameof(PeriodTitle));
    }

    /// <summary>
    /// Создание нового события.
    /// </summary>
    [RelayCommand]
    private void CreateEvent(DateTime? startTime)
    {
        var newEvent = new CalendarEvent
        {
            Title = "Новое событие",
            StartTime = startTime ?? DateTime.Now,
            EndTime = (startTime ?? DateTime.Now).AddHours(1),
            Category = EventCategory.Personal
        };

        AddEvent(newEvent);
    }

    /// <summary>
    /// Удаление события.
    /// </summary>
    [RelayCommand]
    private void DeleteEvent(CalendarEvent? calendarEvent)
    {
        if (calendarEvent is null) return;

        _allEvents.Remove(calendarEvent);
        UpdateDays();
    }

    /// <summary>
    /// Выбор события.
    /// </summary>
    [RelayCommand]
    private void SelectEvent(CalendarEvent? calendarEvent)
    {
        SelectedEvent = calendarEvent;
    }

    /// <summary>
    /// Добавляет событие в календарь.
    /// </summary>
    public void AddEvent(CalendarEvent calendarEvent)
    {
        _allEvents.Add(calendarEvent);
        UpdateDays();
    }

    /// <summary>
    /// Добавляет задачу в список.
    /// </summary>
    public void AddTask(CalendarTask task)
    {
        Tasks.Add(task);
    }

    /// <summary>
    /// Обновляет отображаемые дни.
    /// </summary>
    private void UpdateDays()
    {
        Days.Clear();
        MultiDayEvents.Clear();

        int daysToShow = ShowWorkWeekOnly ? 5 : 7;

        for (int i = 0; i < daysToShow; i++)
        {
            var date = CurrentWeekStart.AddDays(i);
            var dayVm = new DayViewModel(date);

            // Фильтруем события для этого дня
            var dayEvents = _allEvents
                .Where(e => e.OccursOnDate(date) && !e.IsAllDay && !e.IsMultiDay)
                .OrderBy(e => e.StartTime)
                .ToList();

            foreach (var evt in dayEvents)
            {
                dayVm.Events.Add(evt);
            }

            // All-day события для этого дня
            var allDayEvents = _allEvents
                .Where(e => e.OccursOnDate(date) && e.IsAllDay)
                .ToList();

            foreach (var evt in allDayEvents)
            {
                dayVm.AllDayEvents.Add(evt);
            }

            Days.Add(dayVm);
        }

        // Многодневные события (отображаются в верхней полосе)
        var multiDay = _allEvents
            .Where(e => e.IsMultiDay && !e.IsAllDay)
            .Where(e => e.OverlapsWith(CurrentWeekStart, CurrentWeekStart.AddDays(daysToShow)))
            .Distinct()
            .ToList();

        foreach (var evt in multiDay)
        {
            MultiDayEvents.Add(evt);
        }
    }

    /// <summary>
    /// Возвращает дату понедельника для указанной недели.
    /// </summary>
    private static DateTime GetWeekStart(DateTime date)
    {
        int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-diff).Date;
    }

    /// <summary>
    /// Возвращает русское название месяца в родительном падеже.
    /// </summary>
    private static string GetRussianMonth(int month) => month switch
    {
        1 => "января",
        2 => "февраля",
        3 => "марта",
        4 => "апреля",
        5 => "мая",
        6 => "июня",
        7 => "июля",
        8 => "августа",
        9 => "сентября",
        10 => "октября",
        11 => "ноября",
        12 => "декабря",
        _ => string.Empty
    };

    /// <summary>
    /// Загружает тестовые данные (как на скриншоте).
    /// </summary>
    private void LoadSampleData()
    {
        // Используем дату из скриншота: 14-18 марта 2005
        var baseDate = new DateTime(2005, 3, 14);
        CurrentWeekStart = baseDate;

        // Масленица в Петергофе (многодневное событие)
        AddEvent(new CalendarEvent
        {
            Title = "Масленица в Петергофе",
            StartTime = baseDate.AddHours(9),
            EndTime = baseDate.AddDays(4).AddHours(18),
            Category = EventCategory.Holiday,
            IsAllDay = false
        });

        // Понедельник - Распоряжения по департаменту
        AddEvent(new CalendarEvent
        {
            Title = "Распоряжения по департаменту",
            StartTime = baseDate.AddHours(10),
            EndTime = baseDate.AddHours(12),
            Category = EventCategory.Personal,
            Location = "Кабинет"
        });

        // Понедельник - Примерка костюма
        AddEvent(new CalendarEvent
        {
            Title = "Примерка и подгонка костюма к пятничному балу",
            StartTime = baseDate.AddHours(19),
            EndTime = baseDate.AddHours(21).AddMinutes(30),
            Category = EventCategory.Personal
        });

        // Вторник - Просмотреть депеши из Лондона
        AddEvent(new CalendarEvent
        {
            Title = "Просмотреть депеши из Лондона",
            StartTime = baseDate.AddDays(1).AddHours(13),
            EndTime = baseDate.AddDays(1).AddHours(15),
            Category = EventCategory.Meeting
        });

        // Вторник - Заседание Кабинета министров
        AddEvent(new CalendarEvent
        {
            Title = "Заседание Кабинета министров",
            StartTime = baseDate.AddDays(1).AddHours(16),
            EndTime = baseDate.AddDays(1).AddHours(18),
            Category = EventCategory.Business
        });

        // Среда - Литературная среда у Гончаровых
        AddEvent(new CalendarEvent
        {
            Title = "Литературная среда у Гончаровых",
            StartTime = baseDate.AddDays(2).AddHours(19),
            EndTime = baseDate.AddDays(2).AddHours(22),
            Category = EventCategory.Travel,
            Location = "Дом Гончаровых"
        });

        // Четверг - Обед у английского посла
        AddEvent(new CalendarEvent
        {
            Title = "Обед у английского посла",
            StartTime = baseDate.AddDays(3).AddHours(16),
            EndTime = baseDate.AddDays(3).AddHours(18),
            Category = EventCategory.Meeting,
            Location = "Английское посольство"
        });

        // Пятница - Составить отчёт для Департамента
        AddEvent(new CalendarEvent
        {
            Title = "Составить отчёт для Департамента",
            StartTime = baseDate.AddDays(4).AddHours(10),
            EndTime = baseDate.AddDays(4).AddHours(12),
            Category = EventCategory.Personal
        });

        // Пятница - БАЛ У ДОЛГОРУКИХ
        AddEvent(new CalendarEvent
        {
            Title = "БАЛ У ДОЛГОРУКИХ",
            Description = "NB! Ожидается визит Государя Императора",
            StartTime = baseDate.AddDays(4).AddHours(19),
            EndTime = baseDate.AddDays(4).AddHours(23),
            Category = EventCategory.Important,
            IsHighPriority = true,
            Location = "Дом Долгоруких"
        });

        // Задачи
        Tasks.Add(new CalendarTask
        {
            Title = "Поставить задачи Департаменту",
            DueDate = baseDate,
            Priority = TaskPriority.High
        });

        Tasks.Add(new CalendarTask
        {
            Title = "Уточнить, будет ли граф на балу",
            DueDate = baseDate.AddDays(3),
            Priority = TaskPriority.Normal
        });

        Tasks.Add(new CalendarTask
        {
            Title = "Взять документ о награждении",
            DueDate = baseDate.AddDays(1),
            Priority = TaskPriority.Normal
        });

        Tasks.Add(new CalendarTask
        {
            Title = "Подобрать подтверждающие документы",
            DueDate = baseDate.AddDays(4),
            Priority = TaskPriority.High
        });

        Tasks.Add(new CalendarTask
        {
            Title = "Проконтролировать отправку депеш",
            DueDate = baseDate.AddDays(2),
            Priority = TaskPriority.High
        });

        UpdateDays();
    }
}
