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
    private EventLayoutInfo? _draggingLayoutInfo;
    private Rectangle? _dragGhost;
    private Canvas? _dragCanvas;
    private DateTime _originalStartTime;

    private const double HourHeight = 60.0;
    private const int StartHour = 8;

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
            _draggingLayoutInfo = layoutInfo;
            _originalStartTime = layoutInfo.Event.StartTime;
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
            CreateDragGhost(sender as FrameworkElement);
        }

        if (_isDragging && _dragGhost != null && _dragCanvas != null)
        {
            // Обновляем позицию призрака
            var canvasPos = e.GetPosition(_dragCanvas);
            Canvas.SetTop(_dragGhost, Math.Max(0, canvasPos.Y - _dragGhost.Height / 2));
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

        if (_isDragging && _draggingLayoutInfo != null && _dragCanvas != null)
        {
            var dropPos = e.GetPosition(_dragCanvas);
            var newTime = CalculateTimeFromPosition(dropPos.Y, _originalStartTime.Date);

            // Вызываем команду перемещения
            if (DataContext is WeekViewModel vm)
            {
                vm.MoveEventCommand.Execute((_draggingLayoutInfo.Event, newTime));
            }
        }

        CleanupDrag();
    }

    /// <summary>
    /// Отмена перетаскивания при потере захвата мыши.
    /// </summary>
    public void EventBorder_LostMouseCapture(object sender, MouseEventArgs e)
    {
        CleanupDrag();
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
    private void CreateDragGhost(FrameworkElement? source)
    {
        if (source == null || _draggingLayoutInfo == null) return;

        // Находим родительский Canvas
        _dragCanvas = FindParentCanvas(source);
        if (_dragCanvas == null) return;

        // Создаём полупрозрачный прямоугольник
        _dragGhost = new Rectangle
        {
            Width = source.ActualWidth,
            Height = source.ActualHeight,
            Fill = new SolidColorBrush(Color.FromArgb(128, 100, 149, 237)), // Полупрозрачный синий
            Stroke = new SolidColorBrush(Colors.CornflowerBlue),
            StrokeThickness = 2,
            StrokeDashArray = new DoubleCollection { 4, 2 },
            RadiusX = 3,
            RadiusY = 3,
            IsHitTestVisible = false
        };

        Canvas.SetTop(_dragGhost, Canvas.GetTop(source));
        Canvas.SetLeft(_dragGhost, Canvas.GetLeft(source));
        Canvas.SetZIndex(_dragGhost, 1000);

        _dragCanvas.Children.Add(_dragGhost);
    }

    /// <summary>
    /// Находит родительский Canvas.
    /// </summary>
    private static Canvas? FindParentCanvas(DependencyObject? element)
    {
        while (element != null)
        {
            if (element is Canvas canvas)
                return canvas;
            element = VisualTreeHelper.GetParent(element);
        }
        return null;
    }

    /// <summary>
    /// Очищает состояние перетаскивания.
    /// </summary>
    private void CleanupDrag()
    {
        if (_dragGhost != null && _dragCanvas != null)
        {
            _dragCanvas.Children.Remove(_dragGhost);
        }

        _isDragging = false;
        _draggingLayoutInfo = null;
        _dragGhost = null;
        _dragCanvas = null;
    }

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
