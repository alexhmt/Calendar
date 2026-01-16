using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using OutlookCalendar.Models;

namespace OutlookCalendar.Services;

/// <summary>
/// Интерфейс сервиса календаря.
/// </summary>
public interface ICalendarService
{
    IEnumerable<CalendarEvent> GetEvents(DateTime startDate, DateTime endDate);
    IEnumerable<CalendarTask> GetTasks(DateTime? dueDate = null);
    void AddEvent(CalendarEvent calendarEvent);
    void UpdateEvent(CalendarEvent calendarEvent);
    void DeleteEvent(Guid eventId);
    void AddTask(CalendarTask task);
    void UpdateTask(CalendarTask task);
    void DeleteTask(Guid taskId);
    void SaveToFile(string filePath);
    void LoadFromFile(string filePath);
}

/// <summary>
/// DTO для сериализации данных календаря.
/// </summary>
public class CalendarData
{
    [JsonPropertyName("events")]
    public List<CalendarEventDto> Events { get; set; } = [];

    [JsonPropertyName("tasks")]
    public List<CalendarTaskDto> Tasks { get; set; } = [];
}

/// <summary>
/// DTO для события (для сериализации).
/// </summary>
public class CalendarEventDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("location")]
    public string Location { get; set; } = string.Empty;

    [JsonPropertyName("startTime")]
    public DateTime StartTime { get; set; }

    [JsonPropertyName("endTime")]
    public DateTime EndTime { get; set; }

    [JsonPropertyName("isAllDay")]
    public bool IsAllDay { get; set; }

    [JsonPropertyName("category")]
    public EventCategory Category { get; set; }

    [JsonPropertyName("isHighPriority")]
    public bool IsHighPriority { get; set; }

    [JsonPropertyName("notes")]
    public string Notes { get; set; } = string.Empty;

    public static CalendarEventDto FromModel(CalendarEvent e) => new()
    {
        Id = e.Id,
        Title = e.Title,
        Description = e.Description,
        Location = e.Location,
        StartTime = e.StartTime,
        EndTime = e.EndTime,
        IsAllDay = e.IsAllDay,
        Category = e.Category,
        IsHighPriority = e.IsHighPriority,
        Notes = e.Notes
    };

    public CalendarEvent ToModel() => new()
    {
        Id = Id,
        Title = Title,
        Description = Description,
        Location = Location,
        StartTime = StartTime,
        EndTime = EndTime,
        IsAllDay = IsAllDay,
        Category = Category,
        IsHighPriority = IsHighPriority,
        Notes = Notes
    };
}

/// <summary>
/// DTO для задачи (для сериализации).
/// </summary>
public class CalendarTaskDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("dueDate")]
    public DateTime? DueDate { get; set; }

    [JsonPropertyName("priority")]
    public TaskPriority Priority { get; set; }

    [JsonPropertyName("status")]
    public TaskStatus Status { get; set; }

    [JsonPropertyName("percentComplete")]
    public int PercentComplete { get; set; }

    public static CalendarTaskDto FromModel(CalendarTask t) => new()
    {
        Id = t.Id,
        Title = t.Title,
        Description = t.Description,
        DueDate = t.DueDate,
        Priority = t.Priority,
        Status = t.Status,
        PercentComplete = t.PercentComplete
    };

    public CalendarTask ToModel() => new()
    {
        Id = Id,
        Title = Title,
        Description = Description,
        DueDate = DueDate,
        Priority = Priority,
        Status = Status,
        PercentComplete = PercentComplete
    };
}

/// <summary>
/// Реализация сервиса календаря с in-memory хранилищем.
/// </summary>
public class CalendarService : ICalendarService
{
    private readonly List<CalendarEvent> _events = [];
    private readonly List<CalendarTask> _tasks = [];
    private readonly object _lock = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>
    /// Получает события в указанном диапазоне дат.
    /// </summary>
    public IEnumerable<CalendarEvent> GetEvents(DateTime startDate, DateTime endDate)
    {
        lock (_lock)
        {
            return _events
                .Where(e => e.OverlapsWith(startDate, endDate))
                .OrderBy(e => e.StartTime)
                .ToList();
        }
    }

    /// <summary>
    /// Получает задачи, опционально фильтруя по дате.
    /// </summary>
    public IEnumerable<CalendarTask> GetTasks(DateTime? dueDate = null)
    {
        lock (_lock)
        {
            var query = _tasks.AsEnumerable();

            if (dueDate.HasValue)
            {
                query = query.Where(t => t.DueDate?.Date == dueDate.Value.Date);
            }

            return query
                .OrderBy(t => t.DueDate)
                .ThenByDescending(t => t.Priority)
                .ToList();
        }
    }

    /// <summary>
    /// Добавляет новое событие.
    /// </summary>
    public void AddEvent(CalendarEvent calendarEvent)
    {
        ArgumentNullException.ThrowIfNull(calendarEvent);

        lock (_lock)
        {
            _events.Add(calendarEvent);
        }
    }

    /// <summary>
    /// Обновляет существующее событие.
    /// </summary>
    public void UpdateEvent(CalendarEvent calendarEvent)
    {
        ArgumentNullException.ThrowIfNull(calendarEvent);

        lock (_lock)
        {
            var index = _events.FindIndex(e => e.Id == calendarEvent.Id);
            if (index >= 0)
            {
                _events[index] = calendarEvent;
            }
        }
    }

    /// <summary>
    /// Удаляет событие по ID.
    /// </summary>
    public void DeleteEvent(Guid eventId)
    {
        lock (_lock)
        {
            _events.RemoveAll(e => e.Id == eventId);
        }
    }

    /// <summary>
    /// Добавляет новую задачу.
    /// </summary>
    public void AddTask(CalendarTask task)
    {
        ArgumentNullException.ThrowIfNull(task);

        lock (_lock)
        {
            _tasks.Add(task);
        }
    }

    /// <summary>
    /// Обновляет существующую задачу.
    /// </summary>
    public void UpdateTask(CalendarTask task)
    {
        ArgumentNullException.ThrowIfNull(task);

        lock (_lock)
        {
            var index = _tasks.FindIndex(t => t.Id == task.Id);
            if (index >= 0)
            {
                _tasks[index] = task;
            }
        }
    }

    /// <summary>
    /// Удаляет задачу по ID.
    /// </summary>
    public void DeleteTask(Guid taskId)
    {
        lock (_lock)
        {
            _tasks.RemoveAll(t => t.Id == taskId);
        }
    }

    /// <summary>
    /// Сохраняет данные в JSON-файл.
    /// </summary>
    public void SaveToFile(string filePath)
    {
        lock (_lock)
        {
            var data = new CalendarData
            {
                Events = _events.Select(CalendarEventDto.FromModel).ToList(),
                Tasks = _tasks.Select(CalendarTaskDto.FromModel).ToList()
            };

            var json = JsonSerializer.Serialize(data, JsonOptions);
            File.WriteAllText(filePath, json);
        }
    }

    /// <summary>
    /// Загружает данные из JSON-файла.
    /// </summary>
    public void LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath)) return;

        var json = File.ReadAllText(filePath);
        var data = JsonSerializer.Deserialize<CalendarData>(json, JsonOptions);

        if (data == null) return;

        lock (_lock)
        {
            _events.Clear();
            _events.AddRange(data.Events.Select(dto => dto.ToModel()));

            _tasks.Clear();
            _tasks.AddRange(data.Tasks.Select(dto => dto.ToModel()));
        }
    }
}
