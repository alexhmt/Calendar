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

- **Models** (`Models/`) — доменные объекты: `CalendarEvent`, `CalendarTask` с enum'ами для категорий, приоритетов, статусов
- **ViewModels** (`ViewModels/`) — `WeekViewModel` содержит основную логику, внутренний класс `DayViewModel` для данных дня
- **Views** (`Views/`) — `WeekView` (главное представление), `EventEditDialog` (модальный диалог редактирования)
- **Services** (`Services/`) — `CalendarService` для хранения данных в памяти с JSON-сериализацией
- **Helpers** (`Helpers/`) — `EventLayoutHelper` для расчёта позиций событий с учётом перекрытий

### Key Components

- **EventControl** (`Controls/`) — переиспользуемый контрол для отображения события
- **CalendarConverters** (`Converters/`) — `TimeToCanvasTopConverter` преобразует DateTime в позицию на Canvas

### MVVM Implementation

Используются атрибуты CommunityToolkit.MVVM:
- `[ObservableProperty]` — для автогенерации свойств с INotifyPropertyChanged
- `[RelayCommand]` — для команд (навигация, CRUD-операции)

### UI Constants

В `WeekViewModel`:
- `StartHour = 8` — начало рабочего дня
- `EndHour = 23` — конец отображаемого времени
- `HourHeight = 60` — высота часа в пикселях

### Event Categories Color Mapping

Important → Tomato, Business → CornflowerBlue, Personal → LightGreen, Holiday → Orange, Meeting → LightYellow, Travel → LightBlue, Birthday → Plum, Reminder → Gray

## Localization

Интерфейс на русском языке. Названия месяцев и дней недели — кириллицей.
