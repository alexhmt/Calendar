using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using OutlookCalendar.Helpers;
using OutlookCalendar.Models;
using OutlookCalendar.ViewModels;

namespace OutlookCalendar.Views;

/// <summary>
/// Недельное представление календаря.
/// </summary>
public partial class WeekView : UserControl
{
    private bool _isDragging;
    private Point _dragStartPoint;
    private double _dragStartY;
    private double _dragStartX;
    private int _originalDayIndex;
    private EventLayoutInfo? _draggingLayoutInfo;
    private CalendarEvent? _draggingAllDayEvent;
    private Rectangle? _dragGhost;
    private FrameworkElement? _dragSourceElement;
    private DateTime _originalStartTime;

    private const double HourHeight = 60.0;
    private const int StartHour = 8;
    private const double TimeColumnWidth = 50.0;

    public WeekView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Начало перетаскивания события.
    /// </summary>
    public void EventBorder_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed &&
            sender is FrameworkElement element &&
            element.DataContext is EventLayoutInfo layoutInfo)
        {
            _dragStartPoint = e.GetPosition(this);
            var canvasPos = e.GetPosition(DragOverlayCanvas);
            _dragStartY = canvasPos.Y;
            _dragStartX = canvasPos.X;
            _draggingLayoutInfo = layoutInfo;
            _originalStartTime = layoutInfo.Event.StartTime;
            _originalDayIndex = GetDayIndexAtPosition(_dragStartX);
            element.CaptureMouse();
        }
    }

    /// <summary>
    /// Обработка движения мыши при перетаскивании.
    /// </summary>
    public void EventBorder_MouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed || _draggingLayoutInfo == null)
            return;

        var currentPos = e.GetPosition(this);
        var diff = currentPos - _dragStartPoint;

        // Начинаем drag только после перемещения на 5 пикселей
        if (!_isDragging && (Math.Abs(diff.X) > 5 || Math.Abs(diff.Y) > 5))
        {
            _isDragging = true;
            _dragSourceElement = sender as FrameworkElement;
            CreateDragGhost();
        }

        if (_isDragging && _dragGhost != null)
        {
            // Обновляем позицию призрака относительно overlay canvas
            var canvasPos = e.GetPosition(DragOverlayCanvas);
            Canvas.SetTop(_dragGhost, Math.Max(0, canvasPos.Y - _dragGhost.Height / 2));
            Canvas.SetLeft(_dragGhost, canvasPos.X - _dragGhost.Width / 2);
        }
    }

    /// <summary>
    /// Завершение перетаскивания.
    /// </summary>
    public void EventBorder_PreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            element.ReleaseMouseCapture();
        }

        if (_isDragging && _draggingLayoutInfo != null && DataContext is WeekViewModel vm)
        {
            var dropPos = e.GetPosition(DragOverlayCanvas);

            // Вычисляем дельту перемещения по Y (время)
            var deltaY = dropPos.Y - _dragStartY;
            var deltaHours = deltaY / HourHeight;
            var deltaMinutes = (int)(deltaHours * 60);
            deltaMinutes = (deltaMinutes / 15) * 15;

            // Вычисляем новый индекс дня
            var newDayIndex = GetDayIndexAtPosition(dropPos.X);
            var deltaDays = newDayIndex - _originalDayIndex;

            // Вычисляем новое время
            var evt = _draggingLayoutInfo.Event;
            var duration = evt.Duration;
            var newTime = _originalStartTime.AddDays(deltaDays).AddMinutes(deltaMinutes);

            // Ограничиваем диапазон времени (8:00 - 23:00)
            var minTime = newTime.Date.AddHours(StartHour);
            var maxTime = newTime.Date.AddHours(23).AddMinutes(59);

            if (newTime < minTime)
                newTime = minTime;

            // Убедимся, что событие не станет многодневным (EndTime в тот же день)
            var newEndTime = newTime + duration;
            if (newEndTime.Date > newTime.Date)
            {
                // Сдвигаем начало назад, чтобы конец был в 23:59
                newTime = newTime.Date.AddHours(23).AddMinutes(59) - duration;
                if (newTime < minTime)
                    newTime = minTime;
                newEndTime = newTime + duration;
            }

            // Перемещаем событие
            evt.StartTime = newTime;
            evt.EndTime = newEndTime;
            vm.RefreshView();
        }

        CleanupDrag();
    }

    /// <summary>
    /// Отмена перетаскивания при потере захвата мыши.
    /// </summary>
    public void EventBorder_LostMouseCapture(object sender, MouseEventArgs e)
    {
        // Очистка происходит в PreviewMouseUp
    }

    /// <summary>
    /// Определяет индекс дня по X-позиции на DragOverlayCanvas.
    /// </summary>
    private int GetDayIndexAtPosition(double xPosition)
    {
        if (DataContext is not WeekViewModel vm || vm.Days.Count == 0)
            return 0;

        // Получаем позицию DaysItemsControl относительно DragOverlayCanvas
        var daysControlPos = DaysItemsControl.TranslatePoint(new Point(0, 0), DragOverlayCanvas);
        var daysWidth = DaysItemsControl.ActualWidth;
        var dayCount = vm.Days.Count;

        if (dayCount == 0 || daysWidth <= 0)
            return 0;

        // Вычисляем относительную позицию внутри области дней
        var relativeX = xPosition - daysControlPos.X;
        var dayWidth = daysWidth / dayCount;

        // Определяем индекс дня
        var dayIndex = (int)(relativeX / dayWidth);

        // Ограничиваем диапазон
        return Math.Max(0, Math.Min(dayCount - 1, dayIndex));
    }

    /// <summary>
    /// Вычисляет время из позиции Y на Canvas.
    /// </summary>
    private DateTime CalculateTimeFromPosition(double yPosition, DateTime date)
    {
        double hoursFromStart = yPosition / HourHeight;
        int hour = StartHour + (int)hoursFromStart;
        int minute = (int)((hoursFromStart % 1) * 60);

        // Округляем до 15 минут
        minute = (minute / 15) * 15;

        // Ограничиваем диапазон
        hour = Math.Max(StartHour, Math.Min(23, hour));
        if (hour == 23 && minute > 0) minute = 0;

        return date.Date.AddHours(hour).AddMinutes(minute);
    }

    /// <summary>
    /// Создаёт визуальный призрак для перетаскивания.
    /// </summary>
    private void CreateDragGhost()
    {
        if (_dragSourceElement == null || _draggingLayoutInfo == null) return;

        // Создаём полупрозрачный прямоугольник
        _dragGhost = new Rectangle
        {
            Width = _dragSourceElement.ActualWidth,
            Height = _dragSourceElement.ActualHeight,
            Fill = new SolidColorBrush(Color.FromArgb(128, 100, 149, 237)), // Полупрозрачный синий
            Stroke = new SolidColorBrush(Colors.CornflowerBlue),
            StrokeThickness = 2,
            StrokeDashArray = new DoubleCollection { 4, 2 },
            RadiusX = 3,
            RadiusY = 3,
            IsHitTestVisible = false
        };

        // Позиционируем относительно overlay canvas
        var sourcePos = _dragSourceElement.TranslatePoint(new Point(0, 0), DragOverlayCanvas);
        Canvas.SetTop(_dragGhost, sourcePos.Y);
        Canvas.SetLeft(_dragGhost, sourcePos.X);

        DragOverlayCanvas.Children.Add(_dragGhost);
    }

    /// <summary>
    /// Очищает состояние перетаскивания.
    /// </summary>
    private void CleanupDrag()
    {
        if (_dragGhost != null)
        {
            DragOverlayCanvas.Children.Remove(_dragGhost);
        }

        _isDragging = false;
        _draggingLayoutInfo = null;
        _draggingAllDayEvent = null;
        _dragGhost = null;
        _dragSourceElement = null;
    }

    #region AllDay Events Drag & Drop

    /// <summary>
    /// Начало перетаскивания AllDay события.
    /// </summary>
    public void AllDayEvent_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed &&
            sender is FrameworkElement element &&
            element.DataContext is CalendarEvent evt)
        {
            _dragStartPoint = e.GetPosition(this);
            _dragStartX = e.GetPosition(DragOverlayCanvas).X;
            _draggingAllDayEvent = evt;
            _originalStartTime = evt.StartTime;
            _originalDayIndex = GetDayIndexAtPosition(_dragStartX);
            _dragSourceElement = element;
            element.CaptureMouse();
        }
    }

    /// <summary>
    /// Обработка движения мыши при перетаскивании AllDay события.
    /// </summary>
    public void AllDayEvent_MouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed || _draggingAllDayEvent == null)
            return;

        var currentPos = e.GetPosition(this);
        var diff = currentPos - _dragStartPoint;

        // Начинаем drag только после перемещения на 5 пикселей
        if (!_isDragging && Math.Abs(diff.X) > 5)
        {
            _isDragging = true;
            CreateAllDayDragGhost();
        }

        if (_isDragging && _dragGhost != null)
        {
            var canvasPos = e.GetPosition(DragOverlayCanvas);
            Canvas.SetLeft(_dragGhost, canvasPos.X - _dragGhost.Width / 2);
        }
    }

    /// <summary>
    /// Завершение перетаскивания AllDay события.
    /// </summary>
    public void AllDayEvent_PreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            element.ReleaseMouseCapture();
        }

        if (_isDragging && _draggingAllDayEvent != null && DataContext is WeekViewModel vm)
        {
            var dropX = e.GetPosition(DragOverlayCanvas).X;
            var newDayIndex = GetDayIndexAtPosition(dropX);
            var deltaDays = newDayIndex - _originalDayIndex;

            if (deltaDays != 0)
            {
                var evt = _draggingAllDayEvent;
                var duration = evt.Duration;
                evt.StartTime = _originalStartTime.AddDays(deltaDays);
                evt.EndTime = evt.StartTime + duration;
                vm.RefreshView();
            }
        }

        CleanupDrag();
    }

    /// <summary>
    /// Отмена перетаскивания AllDay события.
    /// </summary>
    public void AllDayEvent_LostMouseCapture(object sender, MouseEventArgs e)
    {
        // Очистка происходит в PreviewMouseUp
    }

    /// <summary>
    /// Создаёт визуальный призрак для перетаскивания AllDay события.
    /// </summary>
    private void CreateAllDayDragGhost()
    {
        if (_dragSourceElement == null || _draggingAllDayEvent == null) return;

        _dragGhost = new Rectangle
        {
            Width = _dragSourceElement.ActualWidth,
            Height = _dragSourceElement.ActualHeight,
            Fill = new SolidColorBrush(Color.FromArgb(128, 100, 149, 237)),
            Stroke = new SolidColorBrush(Colors.CornflowerBlue),
            StrokeThickness = 2,
            StrokeDashArray = new DoubleCollection { 4, 2 },
            RadiusX = 3,
            RadiusY = 3,
            IsHitTestVisible = false
        };

        var sourcePos = _dragSourceElement.TranslatePoint(new Point(0, 0), DragOverlayCanvas);
        Canvas.SetTop(_dragGhost, sourcePos.Y);
        Canvas.SetLeft(_dragGhost, sourcePos.X);

        DragOverlayCanvas.Children.Add(_dragGhost);
    }

    #endregion

    /// <summary>
    /// Обработка двойного клика на пустом месте для создания события.
    /// </summary>
    private void DayGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2 && sender is FrameworkElement element)
        {
            // Получаем дату дня из DataContext
            if (element.DataContext is DayViewModel dayVm)
            {
                var position = e.GetPosition(element);
                var newTime = CalculateTimeFromPosition(position.Y, dayVm.Date);

                // Вызываем команду создания события
                if (DataContext is WeekViewModel vm)
                {
                    vm.CreateEventDialogCommand.Execute(newTime);
                }

                e.Handled = true;
            }
        }
    }
}
