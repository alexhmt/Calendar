# Outlook Calendar Component

Профессиональный WPF-компонент календаря в стиле Microsoft Outlook 2003-2007.

![Calendar Preview](docs/preview.png)

## Возможности

- ✅ Недельное представление (рабочая/полная неделя)
- ✅ Дневное и месячное представление
- ✅ Многодневные события и события "Весь день"
- ✅ Цветовые категории событий
- ✅ Drag & Drop перетаскивание событий между днями
- ✅ Изменение длительности события перетаскиванием краёв
- ✅ Выделение событий с визуальной рамкой
- ✅ Контекстное меню (Копировать, Вырезать, Вставить, Удалить)
- ✅ Панель задач
- ✅ Навигация по периодам
- ✅ Диалог редактирования событий
- ✅ Автосохранение/загрузка в JSON
- ✅ MVVM-архитектура
- ✅ Горячие клавиши

## Требования

- .NET 8.0 SDK
- Windows 10/11
- Visual Studio 2022 или JetBrains Rider

## Быстрый старт

### Сборка и запуск

```bash
# Клонируйте репозиторий
cd OutlookCalendar

# Восстановите пакеты и соберите
dotnet restore
dotnet build

# Запустите приложение
dotnet run
```

### Использование в своём проекте

1. Добавьте ссылку на проект или скопируйте исходники
2. Добавьте namespace в XAML:

```xml
xmlns:calendar="clr-namespace:OutlookCalendar.Views;assembly=OutlookCalendar"
```

3. Используйте компонент:

```xml
<calendar:WeekView DataContext="{Binding CalendarViewModel}"/>
```

## Структура проекта

```
OutlookCalendar/
├── Models/
│   ├── CalendarEvent.cs      # Модель события
│   └── CalendarTask.cs       # Модель задачи
├── ViewModels/
│   └── WeekViewModel.cs      # ViewModel для всех режимов
├── Views/
│   ├── WeekView.xaml         # Недельное/дневное представление
│   ├── MonthView.xaml        # Месячное представление
│   ├── TasksSidePanel.xaml   # Боковая панель задач
│   └── EventEditDialog.xaml  # Диалог редактирования
├── Converters/
│   └── CalendarConverters.cs # XAML-конвертеры
├── Helpers/
│   ├── EventLayoutHelper.cs  # Расчёт позиций событий
│   └── BindingProxy.cs       # Прокси для ContextMenu
├── Behaviors/
│   └── DoubleClickBehavior.cs # Поведение двойного клика
├── Services/
│   └── CalendarService.cs    # Сервис данных
├── App.xaml
└── MainWindow.xaml
```

## Горячие клавиши

| Клавиши | Действие |
|---------|----------|
| `Ctrl+N` | Создать событие |
| `Alt+←` | Предыдущая неделя |
| `Alt+→` | Следующая неделя |
| `Ctrl+T` | Перейти к сегодня |
| `Delete` | Удалить выбранное событие |
| `Enter` | Редактировать выбранное событие |

## API

### CalendarEvent

```csharp
public class CalendarEvent
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Location { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsAllDay { get; set; }
    public EventCategory Category { get; set; }
    public bool IsHighPriority { get; set; }
}
```

### EventCategory

```csharp
public enum EventCategory
{
    None,       // Лавандовый
    Important,  // Красный
    Business,   // Синий
    Personal,   // Зелёный
    Holiday,    // Оранжевый
    Meeting,    // Жёлтый
    Travel,     // Голубой
    Birthday,   // Фиолетовый
    Reminder    // Серый
}
```

### WeekViewModel

```csharp
// Основные свойства
DateTime CurrentWeekStart { get; }
CalendarViewMode ViewMode { get; }              // Day, WorkWeek, FullWeek, Month
ObservableCollection<DayViewModel> Days { get; }
ObservableCollection<CalendarEvent> MultiDayEvents { get; }
ObservableCollection<CalendarTask> Tasks { get; }
CalendarEvent? SelectedEvent { get; }

// Команды навигации
ICommand PreviousWeekCommand { get; }
ICommand NextWeekCommand { get; }
ICommand GoToTodayCommand { get; }
ICommand SetViewModeCommand { get; }

// Команды событий
ICommand CreateEventDialogCommand { get; }
ICommand EditEventCommand { get; }
ICommand DeleteEventCommand { get; }
ICommand SelectEventCommand { get; }

// Команды буфера обмена
ICommand CopyEventCommand { get; }
ICommand CutEventCommand { get; }
ICommand PasteEventCommand { get; }
```

## Кастомизация

### Изменение цветов категорий

Отредактируйте метод `GetCategoryColor` в `CalendarEvent.cs`:

```csharp
public static Color GetCategoryColor(EventCategory category) => category switch
{
    EventCategory.Important => Color.FromRgb(255, 0, 0),    // Ваш цвет
    // ...
};
```

### Изменение временного диапазона

В `WeekViewModel.cs`:

```csharp
[ObservableProperty]
private int _startHour = 8;   // Начальный час (по умолчанию 08:00)

[ObservableProperty]
private int _endHour = 23;    // Конечный час (по умолчанию 23:00)

[ObservableProperty]
private double _hourHeight = 60.0;  // Высота часового слота в пикселях
```

### Добавление собственных стилей

Создайте ResourceDictionary и объедините с `CalendarStyles.xaml`:

```xml
<ResourceDictionary.MergedDictionaries>
    <ResourceDictionary Source="/Styles/CalendarStyles.xaml"/>
    <ResourceDictionary Source="/Styles/CustomStyles.xaml"/>
</ResourceDictionary.MergedDictionaries>
```

## Использование в VB.NET

### 1. Добавление ссылки на проект

Добавьте ссылку на скомпилированную DLL `OutlookCalendar.dll` или на проект C#.

### 2. Создание MainWindow.xaml

```xml
<Window x:Class="MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:calendar="clr-namespace:OutlookCalendar.Views;assembly=OutlookCalendar"
        xmlns:vm="clr-namespace:OutlookCalendar.ViewModels;assembly=OutlookCalendar"
        Title="Календарь VB.NET" Height="700" Width="1000">

    <Window.DataContext>
        <vm:WeekViewModel/>
    </Window.DataContext>

    <Grid>
        <calendar:WeekView/>
    </Grid>
</Window>
```

### 3. Работа с событиями в VB.NET

```vb
Imports OutlookCalendar.Models
Imports OutlookCalendar.ViewModels

Class MainWindow
    Private ReadOnly _viewModel As WeekViewModel

    Public Sub New()
        InitializeComponent()
        _viewModel = CType(DataContext, WeekViewModel)
    End Sub

    ' Добавление события программно
    Private Sub AddEvent_Click(sender As Object, e As RoutedEventArgs)
        Dim newEvent As New CalendarEvent() With {
            .Id = Guid.NewGuid(),
            .Title = "Встреча",
            .Description = "Описание встречи",
            .StartTime = DateTime.Today.AddHours(10),
            .EndTime = DateTime.Today.AddHours(11),
            .Category = EventCategory.Meeting
        }

        _viewModel.AddEvent(newEvent)
    End Sub

    ' Удаление выбранного события
    Private Sub DeleteSelected_Click(sender As Object, e As RoutedEventArgs)
        If _viewModel.SelectedEvent IsNot Nothing Then
            _viewModel.DeleteEventCommand.Execute(_viewModel.SelectedEvent)
        End If
    End Sub

    ' Переход к определённой дате
    Private Sub GoToDate(targetDate As DateTime)
        _viewModel.GoToTodayCommand.Execute(Nothing)
        ' Или используйте навигацию:
        ' _viewModel.NextWeekCommand.Execute(Nothing)
        ' _viewModel.PreviousWeekCommand.Execute(Nothing)
    End Sub
End Class
```

### 4. Подписка на изменения

```vb
Imports System.ComponentModel

Class MainWindow
    Private WithEvents _viewModel As WeekViewModel

    Public Sub New()
        InitializeComponent()
        _viewModel = CType(DataContext, WeekViewModel)
        AddHandler _viewModel.PropertyChanged, AddressOf ViewModel_PropertyChanged
    End Sub

    Private Sub ViewModel_PropertyChanged(sender As Object, e As PropertyChangedEventArgs)
        Select Case e.PropertyName
            Case NameOf(WeekViewModel.SelectedEvent)
                ' Реакция на выбор события
                If _viewModel.SelectedEvent IsNot Nothing Then
                    Console.WriteLine($"Выбрано: {_viewModel.SelectedEvent.Title}")
                End If
            Case NameOf(WeekViewModel.ViewMode)
                ' Реакция на смену режима просмотра
                Console.WriteLine($"Режим: {_viewModel.ViewMode}")
        End Select
    End Sub
End Class
```

### 5. Файл проекта (.vbproj)

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
    <RootNamespace>CalendarVBApp</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\OutlookCalendar\OutlookCalendar.csproj" />
  </ItemGroup>
</Project>
```

## Лицензия

MIT License

## Автор

Создано с помощью Claude (Anthropic)
