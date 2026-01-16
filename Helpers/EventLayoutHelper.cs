using System;
using System.Collections.Generic;
using System.Linq;
using OutlookCalendar.Models;

namespace OutlookCalendar.Helpers;

/// <summary>
/// Информация о позиционировании события в сетке.
/// </summary>
public class EventLayoutInfo
{
    /// <summary>
    /// Событие календаря.
    /// </summary>
    public CalendarEvent Event { get; init; } = null!;

    /// <summary>
    /// Индекс колонки (при наличии перекрытий).
    /// </summary>
    public int ColumnIndex { get; set; }

    /// <summary>
    /// Общее количество колонок в группе перекрытий.
    /// </summary>
    public int TotalColumns { get; set; } = 1;

    /// <summary>
    /// Ширина события относительно доступной ширины (0.0 - 1.0).
    /// </summary>
    public double WidthFraction => 1.0 / TotalColumns;

    /// <summary>
    /// Смещение слева относительно доступной ширины (0.0 - 1.0).
    /// </summary>
    public double LeftFraction => (double)ColumnIndex / TotalColumns;
}

/// <summary>
/// Вычисляет позиции событий с учётом перекрытий.
/// </summary>
public static class EventLayoutHelper
{
    /// <summary>
    /// Рассчитывает позиции для списка событий одного дня.
    /// </summary>
    public static List<EventLayoutInfo> CalculateLayout(IEnumerable<CalendarEvent> events)
    {
        var sortedEvents = events
            .OrderBy(e => e.StartTime)
            .ThenBy(e => e.EndTime)
            .ToList();

        if (sortedEvents.Count == 0)
            return [];

        var result = new List<EventLayoutInfo>();
        var overlapGroups = FindOverlapGroups(sortedEvents);

        foreach (var group in overlapGroups)
        {
            var columns = AssignColumns(group);

            foreach (var (evt, columnIndex) in columns)
            {
                result.Add(new EventLayoutInfo
                {
                    Event = evt,
                    ColumnIndex = columnIndex,
                    TotalColumns = columns.Max(c => c.columnIndex) + 1
                });
            }
        }

        return result;
    }

    /// <summary>
    /// Группирует перекрывающиеся события.
    /// </summary>
    private static List<List<CalendarEvent>> FindOverlapGroups(List<CalendarEvent> events)
    {
        var groups = new List<List<CalendarEvent>>();
        var used = new HashSet<Guid>();

        foreach (var evt in events)
        {
            if (used.Contains(evt.Id)) continue;

            var group = new List<CalendarEvent> { evt };
            used.Add(evt.Id);

            // Находим все события, перекрывающиеся с текущей группой
            bool foundNew;
            do
            {
                foundNew = false;
                foreach (var other in events)
                {
                    if (used.Contains(other.Id)) continue;

                    // Проверяем перекрытие с любым событием в группе
                    if (group.Any(g => EventsOverlap(g, other)))
                    {
                        group.Add(other);
                        used.Add(other.Id);
                        foundNew = true;
                    }
                }
            } while (foundNew);

            groups.Add(group.OrderBy(e => e.StartTime).ToList());
        }

        return groups;
    }

    /// <summary>
    /// Проверяет, перекрываются ли два события.
    /// </summary>
    private static bool EventsOverlap(CalendarEvent a, CalendarEvent b)
    {
        return a.StartTime < b.EndTime && b.StartTime < a.EndTime;
    }

    /// <summary>
    /// Назначает колонки для группы перекрывающихся событий.
    /// Использует жадный алгоритм - назначает первую свободную колонку.
    /// </summary>
    private static List<(CalendarEvent evt, int columnIndex)> AssignColumns(List<CalendarEvent> group)
    {
        var result = new List<(CalendarEvent, int)>();
        var columnEndTimes = new List<DateTime>(); // Время окончания последнего события в каждой колонке

        foreach (var evt in group.OrderBy(e => e.StartTime))
        {
            // Ищем первую колонку, где событие не перекрывается
            int assignedColumn = -1;
            for (int col = 0; col < columnEndTimes.Count; col++)
            {
                if (evt.StartTime >= columnEndTimes[col])
                {
                    assignedColumn = col;
                    columnEndTimes[col] = evt.EndTime;
                    break;
                }
            }

            // Если не нашли свободную колонку - создаём новую
            if (assignedColumn == -1)
            {
                assignedColumn = columnEndTimes.Count;
                columnEndTimes.Add(evt.EndTime);
            }

            result.Add((evt, assignedColumn));
        }

        return result;
    }

    /// <summary>
    /// Вычисляет позицию Y (отступ сверху) для события.
    /// </summary>
    public static double CalculateTopPosition(CalendarEvent evt, int startHour, double hourHeight)
    {
        double hoursFromStart = (evt.StartTime.Hour - startHour) + evt.StartTime.Minute / 60.0;
        return Math.Max(0, hoursFromStart * hourHeight);
    }

    /// <summary>
    /// Вычисляет высоту для события.
    /// </summary>
    public static double CalculateHeight(CalendarEvent evt, double hourHeight, double minHeight = 20)
    {
        double height = evt.Duration.TotalHours * hourHeight;
        return Math.Max(minHeight, height);
    }
}
