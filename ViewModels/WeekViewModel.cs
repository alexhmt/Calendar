using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OutlookCalendar.Helpers;
using OutlookCalendar.Models;
using OutlookCalendar.Views;

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

    [ObservableProperty]
    private ObservableCollection<EventLayoutInfo> _eventLayouts = [];

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

    /// <summary>
    /// Пересчитывает layout для всех событий дня.
    /// </summary>
    public void RecalculateLayout()
    {
        EventLayouts.Clear();
        var layouts = EventLayoutHelper.CalculateLayout(Events);
        foreach (var layout in layouts)
        {
            EventLayouts.Add(layout);
        }
    }
}

/// <summary>
/// ViewModel для одного дня в месячном представлении.
/// </summary>
public partial class MonthDayViewModel : ObservableObject
{
    [ObservableProperty]
    private DateTime _date;

    [ObservableProperty]
    private bool _isCurrentMonth;

    [ObservableProperty]
    private bool _isToday;

    [ObservableProperty]
    private bool _isWeekend;

    [ObservableProperty]
    private bool _isSelected;

    [ObservableProperty]
    private ObservableCollection<CalendarEvent> _events = [];

    /// <summary>
    /// День месяца.
    /// </summary>
    public int DayNumber => Date.Day;

    /// <summary>
    /// Количество событий сверх отображаемого лимита.
    /// </summary>
    public int MoreEventsCount => Math.Max(0, Events.Count - 3);

    /// <summary>
    /// Есть ли дополнительные события.
    /// </summary>
    public bool HasMoreEvents => MoreEventsCount > 0;

    public MonthDayViewModel(DateTime date, bool isCurrentMonth)
    {
        Date = date;
        IsCurrentMonth = isCurrentMonth;
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

    /// <summary>
    /// Задачи на сегодня (фильтр по DueDate).
    /// </summary>
    public IEnumerable<CalendarTask> TodayTasks =>
        Tasks.Where(t => t.DueDate?.Date == DateTime.Today && !t.IsCompleted)
             .OrderBy(t => t.Priority);

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

    [ObservableProperty]
    private ObservableCollection<MonthDayViewModel> _monthDays = [];

    [ObservableProperty]
    private DateTime _currentMonth;

    /// <summary>
    /// Названия дней недели для заголовка месяца.
    /// </summary>
    public IEnumerable<string> WeekDayHeaders => ["Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Вс"];

    /// <summary>
    /// Заголовок текущего периода (например, "14 – 18 марта 2005" или "Март 2005").
    /// </summary>
    public string PeriodTitle
    {
        get
        {
            if (ViewMode == CalendarViewMode.Month)
            {
                return $"{GetRussianMonthNominative(CurrentMonth.Month)} {CurrentMonth.Year}";
            }

            if (ViewMode == CalendarViewMode.Day)
            {
                var date = SelectedDay?.Date ?? DateTime.Today;
                return $"{date.Day} {GetRussianMonth(date.Month)} {date.Year}";
            }

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
        CurrentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        UpdateDays();
        LoadSampleData();
    }

    /// <summary>
    /// Переход к предыдущему периоду (неделя/месяц/день).
    /// </summary>
    [RelayCommand]
    private void PreviousWeek()
    {
        switch (ViewMode)
        {
            case CalendarViewMode.Month:
                CurrentMonth = CurrentMonth.AddMonths(-1);
                UpdateMonthView();
                break;
            case CalendarViewMode.Day:
                if (SelectedDay != null)
                    SelectedDay = new DayViewModel(SelectedDay.Date.AddDays(-1));
                UpdateDays();
                break;
            default:
                CurrentWeekStart = CurrentWeekStart.AddDays(-7);
                UpdateDays();
                break;
        }
        OnPropertyChanged(nameof(PeriodTitle));
    }

    /// <summary>
    /// Переход к следующему периоду (неделя/месяц/день).
    /// </summary>
    [RelayCommand]
    private void NextWeek()
    {
        switch (ViewMode)
        {
            case CalendarViewMode.Month:
                CurrentMonth = CurrentMonth.AddMonths(1);
                UpdateMonthView();
                break;
            case CalendarViewMode.Day:
                if (SelectedDay != null)
                    SelectedDay = new DayViewModel(SelectedDay.Date.AddDays(1));
                UpdateDays();
                break;
            default:
                CurrentWeekStart = CurrentWeekStart.AddDays(7);
                UpdateDays();
                break;
        }
        OnPropertyChanged(nameof(PeriodTitle));
    }

    /// <summary>
    /// Переход к текущей дате (сегодня).
    /// </summary>
    [RelayCommand]
    private void GoToToday()
    {
        CurrentWeekStart = GetWeekStart(DateTime.Today);
        CurrentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        SelectedDay = new DayViewModel(DateTime.Today);

        if (ViewMode == CalendarViewMode.Month)
            UpdateMonthView();
        else
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

        if (mode == CalendarViewMode.Month)
            UpdateMonthView();
        else
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
    /// Редактирование события через диалог.
    /// </summary>
    [RelayCommand]
    private void EditEvent(CalendarEvent? calendarEvent)
    {
        if (calendarEvent is null) return;

        var mainWindow = Application.Current.MainWindow;
        if (mainWindow is null) return;

        if (EventEditDialog.Edit(mainWindow, calendarEvent))
        {
            UpdateDays();
        }
    }

    /// <summary>
    /// Создание нового события через диалог.
    /// </summary>
    [RelayCommand]
    private void CreateEventDialog(DateTime? startTime)
    {
        var mainWindow = Application.Current.MainWindow;
        if (mainWindow is null) return;

        var newEvent = EventEditDialog.CreateNew(mainWindow, startTime);
        if (newEvent != null)
        {
            AddEvent(newEvent);
        }
    }

    /// <summary>
    /// Перемещение события на новое время (для drag & drop).
    /// </summary>
    [RelayCommand]
    private void MoveEvent((CalendarEvent Event, DateTime NewStart)? args)
    {
        if (args is null) return;
        var (evt, newStart) = args.Value;

        var duration = evt.Duration;
        evt.StartTime = newStart;
        evt.EndTime = newStart + duration;

        UpdateDays();
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

        // Определяем количество дней в зависимости от режима
        int daysToShow = ViewMode switch
        {
            CalendarViewMode.Day => 1,
            CalendarViewMode.Month => 0, // Месяц обрабатывается отдельно
            _ => ShowWorkWeekOnly ? 5 : 7
        };

        // Определяем начальную дату
        var startDate = ViewMode == CalendarViewMode.Day
            ? SelectedDay?.Date ?? DateTime.Today
            : CurrentWeekStart;

        for (int i = 0; i < daysToShow; i++)
        {
            var date = startDate.AddDays(i);
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

            // Пересчитываем layout для перекрывающихся событий
            dayVm.RecalculateLayout();

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
        if (daysToShow > 0)
        {
            var multiDay = _allEvents
                .Where(e => e.IsMultiDay && !e.IsAllDay)
                .Where(e => e.OverlapsWith(startDate, startDate.AddDays(daysToShow)))
                .Distinct()
                .ToList();

            foreach (var evt in multiDay)
            {
                MultiDayEvents.Add(evt);
            }
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
    /// Возвращает русское название месяца в именительном падеже.
    /// </summary>
    private static string GetRussianMonthNominative(int month) => month switch
    {
        1 => "Январь",
        2 => "Февраль",
        3 => "Март",
        4 => "Апрель",
        5 => "Май",
        6 => "Июнь",
        7 => "Июль",
        8 => "Август",
        9 => "Сентябрь",
        10 => "Октябрь",
        11 => "Ноябрь",
        12 => "Декабрь",
        _ => string.Empty
    };

    /// <summary>
    /// Обновляет месячное представление.
    /// </summary>
    private void UpdateMonthView()
    {
        MonthDays.Clear();

        var firstDayOfMonth = CurrentMonth;
        var daysInMonth = DateTime.DaysInMonth(firstDayOfMonth.Year, firstDayOfMonth.Month);

        // Определяем день недели для первого дня месяца (понедельник = 0)
        int startOffset = ((int)firstDayOfMonth.DayOfWeek - 1 + 7) % 7;
        var startDate = firstDayOfMonth.AddDays(-startOffset);

        // 42 дня (6 недель) для отображения
        for (int i = 0; i < 42; i++)
        {
            var date = startDate.AddDays(i);
            var isCurrentMonth = date.Month == firstDayOfMonth.Month;
            var dayVm = new MonthDayViewModel(date, isCurrentMonth);

            // Получаем события для этого дня (максимум первые 4 для отображения)
            var dayEvents = _allEvents
                .Where(e => e.OccursOnDate(date))
                .OrderBy(e => e.IsAllDay ? 0 : 1)
                .ThenBy(e => e.StartTime)
                .ToList();

            foreach (var evt in dayEvents)
            {
                dayVm.Events.Add(evt);
            }

            MonthDays.Add(dayVm);
        }
    }

    /// <summary>
    /// Выбор дня из месячного вида и переход в недельный режим.
    /// </summary>
    [RelayCommand]
    private void SelectMonthDay(MonthDayViewModel? dayVm)
    {
        if (dayVm is null) return;

        // Устанавливаем выбранную дату и переключаемся в режим недели
        CurrentWeekStart = GetWeekStart(dayVm.Date);
        SelectedDay = new DayViewModel(dayVm.Date);
        ViewMode = CalendarViewMode.WorkWeek;
        UpdateDays();
        OnPropertyChanged(nameof(PeriodTitle));
    }

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
