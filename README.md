# Outlook Calendar Component

Профессиональный WPF-компонент календаря в стиле Microsoft Outlook 2003-2007.

![Calendar Preview](docs/preview.png)

## Возможности

- ✅ Недельное представление (рабочая/полная неделя)
- ✅ Многодневные события
- ✅ Цветовые категории событий
- ✅ Drag & Drop перетаскивание событий
- ✅ Панель задач
- ✅ Навигация по неделям
- ✅ Диалог редактирования событий
- ✅ Сохранение/загрузка в JSON
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
│   └── WeekViewModel.cs      # ViewModel недельного вида
├── Views/
│   ├── WeekView.xaml         # Недельное представление
│   └── EventEditDialog.xaml  # Диалог редактирования
├── Controls/
│   └── EventControl.xaml     # Контрол события
├── Converters/
│   └── CalendarConverters.cs # XAML-конвертеры
├── Helpers/
│   └── EventLayoutHelper.cs  # Расчёт позиций событий
├── Services/
│   └── CalendarService.cs    # Сервис данных
├── Styles/
│   └── CalendarStyles.xaml   # Стили Outlook
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
ObservableCollection<DayViewModel> Days { get; }
ObservableCollection<CalendarEvent> MultiDayEvents { get; }
ObservableCollection<CalendarTask> Tasks { get; }
CalendarEvent? SelectedEvent { get; }

// Команды
ICommand PreviousWeekCommand { get; }
ICommand NextWeekCommand { get; }
ICommand GoToTodayCommand { get; }
ICommand CreateEventCommand { get; }
ICommand DeleteEventCommand { get; }
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
