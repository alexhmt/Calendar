using System;
using System.Globalization;
using System.Windows.Data;

namespace OutlookCalendar.Converters;

/// <summary>
/// Преобразует время начала события в позицию Canvas.Top.
/// </summary>
public class TimeToCanvasTopConverter : IMultiValueConverter
{
    private const int StartHour = 8;
    private const double HourHeight = 60.0;

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length > 0 && values[0] is DateTime dateTime)
        {
            double hoursFromStart = (dateTime.Hour - StartHour) + dateTime.Minute / 60.0;
            return Math.Max(0, hoursFromStart * HourHeight);
        }
        return 0.0;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
