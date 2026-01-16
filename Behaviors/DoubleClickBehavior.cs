using System.Windows;
using System.Windows.Input;

namespace OutlookCalendar.Behaviors;

/// <summary>
/// Attached behavior для обработки двойного клика с вызовом команды.
/// </summary>
public static class DoubleClickBehavior
{
    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.RegisterAttached(
            "Command",
            typeof(ICommand),
            typeof(DoubleClickBehavior),
            new PropertyMetadata(null, OnCommandChanged));

    public static readonly DependencyProperty CommandParameterProperty =
        DependencyProperty.RegisterAttached(
            "CommandParameter",
            typeof(object),
            typeof(DoubleClickBehavior),
            new PropertyMetadata(null));

    public static ICommand? GetCommand(DependencyObject obj)
        => (ICommand?)obj.GetValue(CommandProperty);

    public static void SetCommand(DependencyObject obj, ICommand? value)
        => obj.SetValue(CommandProperty, value);

    public static object? GetCommandParameter(DependencyObject obj)
        => obj.GetValue(CommandParameterProperty);

    public static void SetCommandParameter(DependencyObject obj, object? value)
        => obj.SetValue(CommandParameterProperty, value);

    private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UIElement element)
        {
            element.MouseLeftButtonDown -= OnMouseLeftButtonDown;
            if (e.NewValue != null)
            {
                element.MouseLeftButtonDown += OnMouseLeftButtonDown;
            }
        }
    }

    private static void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2 && sender is DependencyObject d)
        {
            var command = GetCommand(d);
            var parameter = GetCommandParameter(d);
            if (command?.CanExecute(parameter) == true)
            {
                command.Execute(parameter);
                e.Handled = true;
            }
        }
    }
}
