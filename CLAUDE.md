# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build Commands

```bash
dotnet restore      # Восстановление пакетов
dotnet build        # Сборка проекта
dotnet run          # Запуск приложения
```

## Architecture

WPF-приложение календаря в стиле Microsoft Outlook 2003-2007. Использует .NET 8.0 и CommunityToolkit.MVVM.

### MVVM Pattern

- **Models** (`Models/`) — `CalendarEvent`, `CalendarTask` с enum'ами `EventCategory`, `TaskPriority`, `TaskStatus`
- **ViewModels** (`ViewModels/WeekViewModel.cs`) — единственный ViewModel для всех режимов; содержит вложенные классы `DayViewModel` и `MonthDayViewModel`
- **Views** (`Views/`) — `WeekView`, `MonthView`, `TasksSidePanel`, `EventEditDialog`
- **Services** (`Services/CalendarService.cs`) — in-memory хранилище с JSON-сериализацией, thread-safe (lock)
- **Helpers** (`Helpers/EventLayoutHelper.cs`) — алгоритм расположения перекрывающихся событий

### View Modes

Enum `CalendarViewMode` в `WeekViewModel.cs`:
- `Day` — один день
- `WorkWeek` — рабочая неделя (Пн-Пт)
- `FullWeek` — полная неделя (Пн-Вс)
- `Month` — месячный вид

Переключение через `SetViewModeCommand`. Навигация: `PreviousWeekCommand`, `NextWeekCommand`, `GoToTodayCommand`.

### Event Layout Algorithm

`EventLayoutHelper.CalculateLayout()` использует жадный алгоритм:
1. Группирует перекрывающиеся события (`FindOverlapGroups`)
2. Назначает колонки через `AssignColumns` — первая свободная колонка
3. Возвращает `EventLayoutInfo` с `ColumnIndex`, `TotalColumns`, `WidthFraction`, `LeftFraction`

### Data Flow

1. События хранятся в `WeekViewModel._allEvents`
2. `UpdateDays()` распределяет события по `DayViewModel.Events` и `DayViewModel.AllDayEvents`
3. `DayViewModel.RecalculateLayout()` вызывает `EventLayoutHelper` для позиционирования
4. Конвертеры (`TimeToCanvasTopConverter`, `EventDurationToHeightConverter`) преобразуют данные в координаты Canvas

### UI Constants

В `CalendarConverters.cs` и `WeekViewModel`:
- `StartHour = 8` — начало отображения
- `EndHour = 23` — конец отображения
- `HourHeight = 60.0` — высота часа в пикселях
- `MinEventHeight = 25.0` — минимальная высота события

Формула позиции: `Top = (hour - StartHour + minute/60) * HourHeight`

### Event Categories

`EventCategory` enum с цветами (`CalendarEvent.GetCategoryColor()`):
- Important → Tomato, Business → CornflowerBlue, Personal → LightGreen
- Holiday → Orange, Meeting → LightYellow, Travel → LightBlue
- Birthday → Plum, Reminder → Gray, None → Lavender

### Keyboard Shortcuts

Определены в `MainWindow.xaml` через `Window.InputBindings`:

| Клавиши | Команда | Действие |
|---------|---------|----------|
| Ctrl+N | CreateEventDialogCommand | Создать событие |
| Alt+← | PreviousWeekCommand | Предыдущий период |
| Alt+→ | NextWeekCommand | Следующий период |
| Ctrl+T | GoToTodayCommand | Перейти к сегодня |
| Delete | DeleteEventCommand | Удалить выбранное событие |
| Enter | EditEventCommand | Редактировать выбранное событие |

### Drag & Drop

Реализовано в `WeekView.xaml.cs`. Использует overlay Canvas (`DragOverlayCanvas`) для отображения ghost-элемента.

Поддерживает:
- Перемещение по времени (вертикально) с округлением до 15 минут
- Перемещение между днями (горизонтально)
- Изменение длительности перетаскиванием верхнего/нижнего края

Алгоритм перемещения:
1. `PreviewMouseDown` — запоминает начальные X/Y позиции, время и индекс дня
2. `MouseMove` — создаёт и перемещает полупрозрачный ghost
3. `PreviewMouseUp` — вычисляет дельту по X (дни) и Y (минуты), обновляет событие
4. `GetDayIndexAtPosition()` — определяет день по X-координате относительно `DaysItemsControl`

Изменение размера (resize):
- `ResizeTopHandle_*` — изменение StartTime (верхний край)
- `ResizeBottomHandle_*` — изменение EndTime (нижний край)
- Минимальная длительность: 15 минут
- Ограничения: 8:00–23:59

### Selection & Context Menu

Выделение события:
- Клик левой кнопкой выделяет событие (`SelectEventCommand`)
- Визуальное выделение — синяя рамка (#0078D7)
- `EqualityToVisibilityConverter` сравнивает событие с `SelectedEvent`

Контекстное меню событий:
- Копировать (`CopyEventCommand`) — сохраняет событие в буфер
- Вырезать (`CutEventCommand`) — сохраняет с флагом удаления
- Вставить (`PasteEventCommand`) — создаёт копию или перемещает
- Удалить (`DeleteEventCommand`)

Контекстное меню пустых клеток:
- Вставить — вставляет на время клика
- Создать событие — открывает диалог

`BindingProxy` (`Helpers/BindingProxy.cs`) используется для привязки команд в ContextMenu, который находится вне визуального дерева.

### Data Persistence

Автосохранение в `%AppData%/OutlookCalendar/calendar_data.json`:
- `LoadData()` — загрузка при старте (или demo-данные)
- `SaveData()` — при изменении событий
- DTO-классы для JSON-сериализации (file-scoped)

## Localization

Интерфейс на русском языке. Названия месяцев и дней — методы `GetRussianMonth()` и `GetRussianMonthNominative()` в `WeekViewModel`.
