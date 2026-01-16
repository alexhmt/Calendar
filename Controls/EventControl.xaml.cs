using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using OutlookCalendar.Models;

namespace OutlookCalendar.Controls;

/// <summary>
/// Контрол для отображения события календаря с поддержкой Drag & Drop.
/// </summary>
public partial class EventControl : UserControl
{
    private Point _dragStartPoint;
    private bool _isDragging;
    private const double DragThreshold = 5.0;

    /// <summary>
    /// Событие, возникающее при начале перетаскивания.
    /// </summary>
    public event EventHandler<CalendarEvent>? DragStarted;

    /// <summary>
    /// Событие, возникающее при выборе события.
    /// </summary>
    public event EventHandler<CalendarEvent>? EventSelected;

    /// <summary>
    /// Событие, возникающее при двойном клике (редактирование).
    /// </summary>
    public event EventHandler<CalendarEvent>? EventDoubleClicked;

    public EventControl()
    {
        InitializeComponent();
        MouseDoubleClick += EventControl_MouseDoubleClick;
    }

    /// <summary>
    /// Связанное событие календаря.
    /// </summary>
    public CalendarEvent? CalendarEvent => DataContext as CalendarEvent;

    private void EventBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _dragStartPoint = e.GetPosition(this);
        _isDragging = false;
        
        // Захватываем мышь для отслеживания движения
        EventBorder.CaptureMouse();
        
        // Уведомляем о выборе события
        if (CalendarEvent != null)
        {
            EventSelected?.Invoke(this, CalendarEvent);
        }
        
        e.Handled = true;
    }

    private void EventBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        EventBorder.ReleaseMouseCapture();
        _isDragging = false;
    }

    private void EventBorder_MouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed) return;
        
        var currentPosition = e.GetPosition(this);
        var diff = currentPosition - _dragStartPoint;

        // Проверяем, превысило ли движение порог для начала перетаскивания
        if (!_isDragging && (Math.Abs(diff.X) > DragThreshold || Math.Abs(diff.Y) > DragThreshold))
        {
            _isDragging = true;
            
            if (CalendarEvent != null)
            {
                DragStarted?.Invoke(this, CalendarEvent);
                
                // Начинаем операцию Drag & Drop
                var data = new DataObject(typeof(CalendarEvent), CalendarEvent);
                DragDrop.DoDragDrop(this, data, DragDropEffects.Move);
            }
            
            EventBorder.ReleaseMouseCapture();
        }
    }

    private void EventControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (CalendarEvent != null)
        {
            EventDoubleClicked?.Invoke(this, CalendarEvent);
        }
        e.Handled = true;
    }

    /// <summary>
    /// Подсвечивает контрол при наведении во время перетаскивания.
    /// </summary>
    public void HighlightForDrop(bool highlight)
    {
        EventBorder.BorderThickness = highlight ? new Thickness(2) : new Thickness(1);
        EventBorder.BorderBrush = highlight 
            ? new SolidColorBrush(Colors.DodgerBlue) 
            : (SolidColorBrush)FindResource("CategoryToBackgroundConverter");
    }
}
