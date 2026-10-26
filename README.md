# ClamCalendar - Calendar view for .NET MAUI Android

[![NuGet](https://img.shields.io/nuget/v/ClamCalendar.svg)](https://www.nuget.org/packages/ClamCalendar/)

## 🐚 What is this?

A .NET MAUI month calendar for Android drawn with SkiaSharp on a single canvas.  
Month building, event placement, selection and touch handling run in C#, so the view stays light and every visual detail is a style value.  

| 📅 Basic | 🎨 Dark style |
|:-:|:-:|
| <img src="Document/basic.png" width="240" /> | <img src="Document/style.png" width="240" /> |

## 🚀 Quick Start

Call `UseSkiaSharp()` when building the application.  

```csharp
var builder = MauiApp.CreateBuilder()
    .UseMauiApp<App>()
    .UseSkiaSharp();
```

Place `ClamCalendarView` in a `*` row of a `Grid` and give it the events of the visible range.  

```xml
<clam:ClamCalendarView DisplayDate="{Binding DisplayDate, Mode=TwoWay}"
                       DisplayDateChangedCommand="{Binding LoadCommand}"
                       Events="{Binding Events}"
                       Holidays="{Binding Holidays}"
                       SelectionMode="SingleDate"
                       SelectedDate="{Binding SelectedDate, Mode=TwoWay}"
                       DayTappedCommand="{Binding DayTappedCommand}" />
```

```csharp
var calendar = new ClamCalendarView { SelectionMode = CalendarSelectionMode.DateRange, TodayButtonVisible = true };
calendar.Events =
[
    new CalendarEvent { Title = "Trip", StartDate = new DateOnly(2026, 9, 14), EndDate = new DateOnly(2026, 9, 16), BackgroundColor = Colors.HotPink },
    new CalendarEvent { Title = "Lesson", StartDate = new DateOnly(2026, 9, 21), EndDate = new DateOnly(2026, 9, 21), Style = CalendarEventStyle.Text, TextColor = Colors.Green, StartTime = new TimeSpan(19, 0, 0) }
];
calendar.Stamps = [new CalendarStamp { Date = new DateOnly(2026, 9, 13), Glyph = "✈️" }];
calendar.DayTapped += (_, e) => Debug.WriteLine(e.Date);
```

Text is drawn with the `sans-serif` system font; characters it lacks are resolved per character through `FallbackFonts` of [SkiaExtension](https://github.com/usausa/skia-extension).  
Set a bundled font as a fallback at startup when the device fonts do not cover the language.  
The language hints follow the UI language of the device unless `Languages` is set.  

```csharp
FallbackFonts.Languages = ["ja"];
FallbackFonts.Typefaces = [SKTypeface.FromStream(await FileSystem.OpenAppPackageFileAsync("NotoSansJP-Regular.ttf"))];
```

Swipe horizontally to move between months, tap a day to select it and long press a day to get `DayLongPressed`.  

## 🔗 Binding and MVVM

Every setting a view model needs is a bindable property or a command.  
The view reports the visible date range through `DisplayDateChanged` whenever the month changes, so the events of that range are loaded there.  
`Example/Modules/Calendar` contains complete screens built this way.  

```csharp
public sealed class CalendarViewModel : ObservableObject
{
    private readonly IScheduleRepository repository;

    public DateOnly DisplayDate
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = DateOnly.FromDateTime(DateTime.Today);

    public IReadOnlyList<CalendarEvent> Events
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    } = [];

    public IReadOnlyList<DateOnly> Holidays
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    } = [];

    public DateOnly? SelectedDate
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public ICommand LoadCommand { get; }

    public ICommand DayTappedCommand { get; }

    public CalendarViewModel(IScheduleRepository repository)
    {
        this.repository = repository;
        // FirstDate and LastDate cover the days of the neighboring months drawn in the first and last row
        LoadCommand = new Command<CalendarDisplayDateChangedEventArgs>(e =>
        {
            Events = repository.GetEvents(e.FirstDate, e.LastDate);
            Holidays = repository.GetHolidays(e.FirstDate, e.LastDate);
        });
        DayTappedCommand = new Command<CalendarDayEventArgs>(e => Debug.WriteLine($"{e.Date} {e.Day.Events.Count} events"));
    }
}
```

| Task | Binding |
|---|---|
| Month | `DisplayDate` (TwoWay), `Navigate` / `GoToToday`, `MinDate` / `MaxDate`, `DisplayDateChangedCommand` |
| Data | `Events`, `Stamps`, `Holidays`, `DayLabels`, `DayAppearances`; `INotifyCollectionChanged` sources redraw on change |
| Layout | `FirstDayOfWeek`, `FixedWeekRows`, `OutsideMonthDaysVisible`, `MaxVisibleSlots`, `WeekNumberVisible`, `Culture`, `WeekdayNameFormat` |
| Header | `HeaderVisible`, `NavigationButtonsVisible`, `TodayButtonVisible`, `HeaderFormat` (or `HeaderFormatter` for a title the format cannot build) and the button texts, or `HeaderVisible="False"` with a header of the page |
| Selection | `SelectionMode`, `SelectedDate` / `SelectedStartDate` / `SelectedEndDate` (TwoWay), `SelectedDates`, `AllowDeselect`, `MaxSelectableDays`, `DisabledDates` |
| Input | `DayTappedCommand`, `DayLongPressedCommand`, `EventTappedCommand`, `OverflowTappedCommand` with the same arguments as the events |
| Colors | `CalendarStyle` as a resource or a view model property |
| Day painter | `DayPainter` to draw over each day of the month with SkiaSharp, or `DayAreaBounds`, `GetDayBounds` and `GetDateNumberBounds` to place MAUI views over the calendar by hand |

Holiday calculation, event storage and navigation belong to the application.  

## ✅ Supported features

| Category | Detail |
|---|---|
| **Month** | Monday or any other first day of the week, six fixed rows or the rows the month needs, neighboring days shown or blank, month indicator watermark, ISO 8601 week numbers |
| **Events** | Single and multi day events placed across weeks, filled bars or plain text, leading glyph and start time, row limit with `+N` overflow, or one colored dot per event |
| **Days** | Weekend and holiday colors, today mark, small labels such as holiday names, emoji and colored stamps in seven positions, per day colors and frames |
| **Selection** | None / single date / multiple dates / date range / whole week, deselect and count limits, disabled dates and date limits |
| **Navigation** | Header buttons, today button, horizontal swipe with a slide animation, limits by `MinDate` / `MaxDate`, visible range notification |
| **Styling** | Fonts, sizes and colors in `CalendarStyle`, the shape of the today and selection marks and which grid lines are drawn, usable as a XAML resource |
| **Fonts** | Per character fallback to the `FallbackFonts` typefaces of SkiaExtension and system fonts, emoji sequences included |
| **Input** | Tap, long press, swipe, commands for MVVM, parent scroll handed over on vertical movement |
| **Diagnostics** | `FrameRendered` reports the draw time, the drawn day cells and the text measurements of every frame |
| **Performance** | Judged by the p95 of the draw time staying within 16 ms; fps is not used and no extra margin is added for slower devices |

## 📖 API

See the [API reference](Document/API.md).  

## 🌊 Clam family

- [ClamCalendar](https://github.com/usausa/clam-calendar) - Calendar view
- [ClamChart](https://github.com/usausa/clam-chart) - Charts
- [ClamGauge](https://github.com/usausa/clam-gauge) - Circular gauge
- [ClamGrid](https://github.com/usausa/clam-grid) - Grid view
