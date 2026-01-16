using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using OutlookCalendar.Models;

namespace OutlookCalendar.Converters;

/// <summary>
/// Преобразует время начала события в отступ сверху (позицию Y) в сетке календаря.
/// </summary>
public class TimeToTopConverter : IValueConverter
{
    /// <summary>
    /// Высота одного часового слота в пикселях.
    /// </summary>
    public double HourHeight { get; set; } = 60.0;

    /// <summary>
    /// Начальный час отображения календаря (например, 8 для 08:00).
    /// </summary>
    public int StartHour { get; set; } = 8;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTime dateTime)
        {
            double totalHours = dateTime.Hour + dateTime.Minute / 60.0 - StartHour;
            return Math.Max(0, totalHours * HourHeight);
        }
        return 0.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Преобразует продолжительность события в высоту элемента.
/// </summary>
public class DurationToHeightConverter : IValueConverter
{
    public double HourHeight { get; set; } = 60.0;
    public double MinHeight { get; set; } = 20.0;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TimeSpan duration)
        {
            double height = duration.TotalHours * HourHeight;
            return Math.Max(MinHeight, height);
        }
        return MinHeight;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Преобразует категорию события в цвет фона.
/// </summary>
public class CategoryToBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is EventCategory category)
        {
            var color = CalendarEvent.GetCategoryColor(category);
            return new SolidColorBrush(color);
        }
        return new SolidColorBrush(Colors.LavenderBlush);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Преобразует категорию события в цвет текста.
/// </summary>
public class CategoryToForegroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is EventCategory category)
        {
            var color = CalendarEvent.GetCategoryTextColor(category);
            return new SolidColorBrush(color);
        }
        return new SolidColorBrush(Colors.Black);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Преобразует булево значение в Visibility.
/// </summary>
public class BoolToVisibilityConverter : IValueConverter
{
    public bool Invert { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool boolValue = value is bool b && b;
        if (Invert) boolValue = !boolValue;
        return boolValue ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            bool result = visibility == Visibility.Visible;
            return Invert ? !result : result;
        }
        return false;
    }
}

/// <summary>
/// Преобразует приоритет задачи в цвет индикатора.
/// </summary>
public class PriorityToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TaskPriority priority)
        {
            return priority switch
            {
                TaskPriority.High => new SolidColorBrush(Colors.Red),
                TaskPriority.Normal => new SolidColorBrush(Colors.Orange),
                TaskPriority.Low => new SolidColorBrush(Colors.Green),
                _ => new SolidColorBrush(Colors.Gray)
            };
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Преобразует DateTime в строку времени (HH:mm).
/// </summary>
public class DateTimeToTimeStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTime dateTime)
        {
            return dateTime.ToString("HH:mm", culture);
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Многозначный конвертер для вычисления позиции события с учётом перекрытий.
/// </summary>
public class EventPositionMultiConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length >= 3 
            && values[0] is int columnIndex 
            && values[1] is int totalColumns 
            && values[2] is double columnWidth)
        {
            return columnIndex * (columnWidth / totalColumns);
        }
        return 0.0;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Преобразует день недели в русскую аббревиатуру.
/// </summary>
public class DayOfWeekToRussianConverter : IValueConverter
{
    private static readonly string[] RussianDayNames = 
    { 
        "Вс", "Пн", "Вт", "Ср", "Чт", "Пт", "Сб" 
    };

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DayOfWeek dayOfWeek)
        {
            return RussianDayNames[(int)dayOfWeek];
        }
        if (value is DateTime date)
        {
            return RussianDayNames[(int)date.DayOfWeek];
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Преобразует статус задачи в зачёркнутый текст.
/// </summary>
public class TaskStatusToStrikethroughConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TaskStatus status && status == TaskStatus.Completed)
        {
            return TextDecorations.Strikethrough;
        }
        return null!;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
