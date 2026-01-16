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

## Лицензия

MIT License

## Автор

Создано с помощью Claude (Anthropic)
