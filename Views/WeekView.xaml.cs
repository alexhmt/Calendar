using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using OutlookCalendar.Models;
using OutlookCalendar.ViewModels;

namespace OutlookCalendar.Views;

/// <summary>
/// Недельное представление календаря.
/// </summary>
public partial class WeekView : UserControl
{
    public WeekView()
    {
        InitializeComponent();
        
        // Подписываемся на события drag & drop
        AllowDrop = true;
        Drop += WeekView_Drop;
        DragOver += WeekView_DragOver;
    }

    /// <summary>
    /// ViewModel календаря.
    /// </summary>
    public WeekViewModel? ViewModel => DataContext as WeekViewModel;

    /// <summary>
    /// Обработка завершения перетаскивания события.
    /// </summary>
    private void WeekView_Drop(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(typeof(CalendarEvent))) return;
        
        var calendarEvent = e.Data.GetData(typeof(CalendarEvent)) as CalendarEvent;
        if (calendarEvent == null || ViewModel == null) return;

        // Определяем позицию сброса и пересчитываем время
        var dropPosition = e.GetPosition(this);
        var newDateTime = CalculateDateTimeFromPosition(dropPosition);

        if (newDateTime.HasValue)
        {
            var duration = calendarEvent.Duration;
            calendarEvent.StartTime = newDateTime.Value;
            calendarEvent.EndTime = newDateTime.Value + duration;
        }

        e.Handled = true;
    }

    /// <summary>
    /// Визуальная обратная связь при перетаскивании.
    /// </summary>
    private void WeekView_DragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(typeof(CalendarEvent)))
        {
            e.Effects = DragDropEffects.Move;
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
        e.Handled = true;
    }

    /// <summary>
    /// Вычисляет дату и время на основе позиции мыши в сетке.
    /// </summary>
    private DateTime? CalculateDateTimeFromPosition(Point position)
    {
        if (ViewModel == null) return null;

        // Эта логика должна учитывать:
        // 1. Отступ временной шкалы слева (50px)
        // 2. Высоту часового слота (HourHeight)
        // 3. Количество отображаемых дней

        const double timeColumnWidth = 50;
        double calendarWidth = ActualWidth - timeColumnWidth - 25; // минус боковая панель
        int daysCount = ViewModel.ShowWorkWeekOnly ? 5 : 7;
        double dayWidth = calendarWidth / daysCount;

        // Определяем день
        double xInCalendar = position.X - timeColumnWidth;
        int dayIndex = (int)(xInCalendar / dayWidth);
        dayIndex = Math.Clamp(dayIndex, 0, daysCount - 1);

        // Определяем время (с учётом прокрутки и заголовков)
        double yInCalendar = position.Y - 40 - 26; // минус заголовки
        double hoursFromStart = yInCalendar / ViewModel.HourHeight;
        int hour = ViewModel.StartHour + (int)hoursFromStart;
        int minute = (int)((hoursFromStart - (int)hoursFromStart) * 60);
        
        // Округляем до 15 минут
        minute = (minute / 15) * 15;

        hour = Math.Clamp(hour, ViewModel.StartHour, ViewModel.EndHour);
        minute = Math.Clamp(minute, 0, 59);

        var targetDate = ViewModel.CurrentWeekStart.AddDays(dayIndex);
        return new DateTime(targetDate.Year, targetDate.Month, targetDate.Day, hour, minute, 0);
    }

    /// <summary>
    /// Обработка двойного клика для создания нового события.
    /// </summary>
    protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
    {
        base.OnMouseDoubleClick(e);

        if (ViewModel == null) return;

        var position = e.GetPosition(this);
        var dateTime = CalculateDateTimeFromPosition(position);

        if (dateTime.HasValue)
        {
            ViewModel.CreateEventCommand.Execute(dateTime.Value);
        }
    }

    /// <summary>
    /// Обработка клавиш (Delete для удаления выбранного события).
    /// </summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Key == Key.Delete && ViewModel?.SelectedEvent != null)
        {
            var result = MessageBox.Show(
                $"Удалить событие \"{ViewModel.SelectedEvent.Title}\"?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                ViewModel.DeleteEventCommand.Execute(ViewModel.SelectedEvent);
            }
        }
    }
}
