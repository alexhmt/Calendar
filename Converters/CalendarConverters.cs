using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace OutlookCalendar.Converters;

/// <summary>
/// Инвертирует булево значение.
/// </summary>
public class InverseBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool b)
            return !b;
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool b)
            return !b;
        return value;
    }
}

/// <summary>
/// Преобразует строку в Visibility (пустая строка = Collapsed).
/// </summary>
public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string str && !string.IsNullOrWhiteSpace(str))
        {
            return Visibility.Visible;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Константы для расчёта позиций в календаре.
/// </summary>
public static class CalendarConstants
{
    public const int StartHour = 8;
    public const int EndHour = 23;
    public const double HourHeight = 60.0;
    public const double MinEventHeight = 25.0;
}

/// <summary>
/// Преобразует время начала события в позицию Canvas.Top.
/// </summary>
public class TimeToCanvasTopConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length > 0 && values[0] is DateTime dateTime)
        {
            double hoursFromStart = (dateTime.Hour - CalendarConstants.StartHour) + dateTime.Minute / 60.0;
            return Math.Max(0, hoursFromStart * CalendarConstants.HourHeight);
        }
        return 0.0;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Преобразует длительность события (StartTime, EndTime) в высоту в пикселях.
/// </summary>
public class EventDurationToHeightConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length >= 2 && values[0] is DateTime startTime && values[1] is DateTime endTime)
        {
            double durationHours = (endTime - startTime).TotalHours;
            return Math.Max(CalendarConstants.MinEventHeight, durationHours * CalendarConstants.HourHeight);
        }
        return CalendarConstants.MinEventHeight;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Преобразует WidthFraction и ширину контейнера в абсолютную ширину.
/// </summary>
public class WidthFractionToWidthConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length >= 2 &&
            values[0] is double widthFraction &&
            values[1] is double containerWidth &&
            containerWidth > 0)
        {
            double width = (containerWidth - 8) * widthFraction; // -8 для отступов
            return Math.Max(50, width - 4); // -4 для margin между событиями
        }
        return 120.0;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Преобразует LeftFraction и ширину контейнера в Canvas.Left.
/// </summary>
public class LeftFractionToLeftConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length >= 2 &&
            values[0] is double leftFraction &&
            values[1] is double containerWidth &&
            containerWidth > 0)
        {
            return leftFraction * (containerWidth - 8) + 2;
        }
        return 2.0;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Сравнивает два объекта и возвращает Visibility.
/// Visible если равны, Collapsed если нет.
/// </summary>
public class EqualityToVisibilityConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length >= 2 && values[0] != null && values[1] != null)
        {
            return ReferenceEquals(values[0], values[1]) ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
