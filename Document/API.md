# ClamCalendar API reference

## 📖 ClamCalendarView

### 🧩 Properties

| Name | Type | Default | Bindable | Description |
|---|---|---|:-:|---|
| `DisplayDate` | `DateOnly` | today | ✓ TwoWay | Year and month to show; the day is ignored |
| `Events` | `IEnumerable<CalendarEvent>?` | `null` | ✓ | Events of the visible range; an `INotifyCollectionChanged` source rebuilds the month on change |
| `Stamps` | `IEnumerable<CalendarStamp>?` | `null` | ✓ | Emoji stamps |
| `Holidays` | `IEnumerable<DateOnly>?` | `null` | ✓ | Dates drawn as `CalendarDayKind.Holiday` |
| `DayLabels` | `IReadOnlyDictionary<DateOnly, string>?` | `null` | ✓ | Small text next to the date number, for example the holiday name |
| `DayAppearances` | `IReadOnlyDictionary<DateOnly, CalendarDayAppearance>?` | `null` | ✓ | Colors of single days that override the style values |
| `Month` | `CalendarMonth` | | | Month model built from `DisplayDate` and the data properties |
| `Today` | `DateOnly?` | `null` | ✓ | Overrides the system date for the today mark and `GoToToday`; while it is `null` the today mark follows the date change of the device |
| `EffectiveToday` | `DateOnly` | | | `Today` or the system date |
| `DayAreaBounds` | `Rect` | | | Area of the day cells in DIP, without the header rows and the week number column; `Rect.Zero` before the view has a size |
| `SlideOffset`, `SlideOpacity` | `double` | | | Horizontal offset in DIP and opacity of the body while the month slides; `0` and `1` when it does not slide, and both raise `PropertyChanged` on every frame of the animation |
| `FirstDayOfWeek` | `DayOfWeek` | `Monday` | ✓ | First column of the week |
| `Culture` | `CultureInfo?` | `null` | ✓ | Culture of the header and the weekday names; `null` uses `CultureInfo.CurrentCulture` |
| `MinDate`, `MaxDate` | `DateOnly?` | `null` | ✓ | Dates outside are disabled and the month navigation stops at their months |
| `DisabledDates` | `IEnumerable<DateOnly>?` | `null` | ✓ | Dates that cannot be selected or tapped |
| `FixedWeekRows` | `bool` | `true` | ✓ | Always six rows, or the four to six rows the month needs |
| `OutsideMonthDaysVisible` | `bool` | `true` | ✓ | Draw the days of the neighboring months; when `false` they stay blank and their events and stamps are dropped |
| `MaxVisibleSlots` | `int` | `0` | ✓ | Event rows per week; `0` is unlimited, hidden events are counted per day and drawn as `+N` |
| `SwipeEnabled` | `bool` | `true` | ✓ | Horizontal swipe moves the month |
| `NavigationAnimationEnabled` | `bool` | `true` | ✓ | Slide animation of the body when the month changes |
| `HeaderVisible` | `bool` | `true` | ✓ | Row with the title and the navigation buttons; set `false` to use a header of the page |
| `NavigationButtonsVisible` | `bool` | `true` | ✓ | Previous and next buttons |
| `TodayButtonVisible` | `bool` | `false` | ✓ | Today button next to the next button. The title stays centered in the header whether or not it is shown, and the button is drawn over the title |
| `HeaderFormat` | `string?` | `null` | ✓ | Format of the first day of the month for the title: composite when it contains `{` (`{0:yyyy/MM} plan`), otherwise a format string (`yyyy/MM`); `null` uses `YearMonthPattern` of the culture |
| `HeaderFormatter` | `Func<CalendarMonth, string>?` | `null` | ✓ | Title text from code; used before `HeaderFormat` |
| `DayPainter` | `Action<SKCanvas, CalendarDayPaintContext>?` | `null` | ✓ | Draws over each day of the month (see below) |
| `PrevButtonText`, `NextButtonText`, `TodayButtonText` | `string` | `◀`, `▶`, `Today` | ✓ | Button texts |
| `WeekdayHeaderVisible` | `bool` | `true` | ✓ | Weekday row |
| `WeekdayNameFormat` | `CalendarWeekdayNameFormat` | `Initial` | ✓ | `Initial` (M / 月), `Abbreviated` (Mon) or `Full` (Monday) |
| `MonthIndicatorVisible` | `bool` | `false` | ✓ | Month number watermark behind the cells |
| `WeekNumberVisible` | `bool` | `false` | ✓ | Narrow column on the left with the ISO 8601 week of the Thursday of the row, so the number does not shift with `FirstDayOfWeek`; a tap of the column is a tap of the first day of the row |
| `SelectionMode` | `CalendarSelectionMode` | `None` | ✓ | `None`, `SingleDate`, `MultipleDates`, `DateRange` or `Week` |
| `SelectedDate` | `DateOnly?` | `null` | ✓ TwoWay | Selection of `SingleDate` |
| `SelectedDates` | `ObservableCollection<DateOnly>` | empty | ✓ | Selection of `MultipleDates` and `Week`; bind a collection of the view model to share it |
| `SelectedStartDate`, `SelectedEndDate` | `DateOnly?` | `null` | ✓ TwoWay | Selection of `DateRange`; the end is `null` until the second tap and the ends are ordered. `Week` sets them to the first and the last selected day of the week |
| `AllowDeselect` | `bool` | `true` | ✓ | In `SingleDate`, tapping the selected date again clears it |
| `MaxSelectableDays` | `int` | `0` | ✓ | Limit of `MultipleDates` and `DateRange`; `0` is unlimited and `Week` is never limited |
| `DisplayDateChangedCommand` | `ICommand?` | `null` | ✓ | Runs after `DisplayDateChanged` with `CalendarDisplayDateChangedEventArgs`; a command assigned while the view is attached runs at once with the current range |
| `DayTappedCommand` | `ICommand?` | `null` | ✓ | Runs after `DayTapped` with `CalendarDayEventArgs` |
| `DayLongPressedCommand` | `ICommand?` | `null` | ✓ | Runs after `DayLongPressed` with `CalendarDayEventArgs` |
| `EventTappedCommand` | `ICommand?` | `null` | ✓ | Runs after `EventTapped` with `CalendarEventEventArgs` |
| `OverflowTappedCommand` | `ICommand?` | `null` | ✓ | Runs after `OverflowTapped` with `CalendarOverflowEventArgs` |

### 🛠 Methods

| Name | Returns | Description |
|---|---|---|
| `Navigate(int months)` | `void` | Moves by months and stops at the months of `MinDate` and `MaxDate` |
| `CanNavigate(int months)` | `bool` | Whether the month that far away is inside the limits |
| `GoToToday()` | `void` | Shows the month of `EffectiveToday`, or the nearest month inside the limits |
| `IsDateDisabled(DateOnly date)` | `bool` | Outside `MinDate` / `MaxDate` or listed in `DisabledDates` |
| `IsDateSelected(DateOnly date)` | `bool` | Selected in the current mode |
| `GetDayBounds(DateOnly date)` | `Rect?` | Cell of the date in DIP, or `null` when the date is outside the drawn range or the view has no size yet |
| `GetDateNumberBounds(DateOnly date)` | `Rect?` | Date number of the date in DIP, same `null` cases |
| `ClearSelection()` | `void` | Clears the selection of every mode |
| `CancelInteraction()` | `void` | Stops the current gesture |
| `InvalidateSurface()` | `void` | Redraws |
| `Dispose()` | `void` | Releases the timers, subscriptions and native resources |

### 📣 Events

| Name | EventArgs | Description |
|---|---|---|
| `DisplayDateChanged` | `CalendarDisplayDateChangedEventArgs` | The month or the visible range changed; raised once more when the view is attached so the initial range can be loaded |
| `DayTapped` | `CalendarDayEventArgs` | Day tapped, after the selection was updated; `Handled` suppresses the command |
| `DayLongPressed` | `CalendarDayEventArgs` | Day, event or `+N` long pressed |
| `EventTapped` | `CalendarEventEventArgs` | Event bar tapped, with the day under the tap. With neither the event nor the command set, the tap becomes a day tap and selects the day |
| `OverflowTapped` | `CalendarOverflowEventArgs` | `+N` tapped, with every event of the day. With neither the event nor the command set, the tap becomes a day tap and selects the day |
| `SelectionChanged` | `EventArgs` | Selection changed by a tap or a property |
| `FrameRendered` | `CalendarFrameEventArgs` | Raised after every painted frame with `ElapsedMilliseconds`, `DayCount` (the drawn day cells), `TextMeasurements` (measurements added by this frame) and `Animating` (a month slide is running); not raised while the view has no size |

Disabled days and blank days of the neighboring months raise no tap events.  
With `EventDisplayMode` of `Dots` there are no bars and no `+N`, so every tap of the body is a day tap and `EventTapped` and `OverflowTapped` are never raised; read the events of the day from `CalendarDayEventArgs.Day`.  
The slide animation and the long press timer need a handler, so they are skipped when the view is not attached.  
The month is rebuilt at the date change of the device while `Today` is not set and the view is attached, so the today mark moves without a reload; the visible range does not change, so `DisplayDateChanged` is not raised.  
The first `DisplayDateChanged` is raised while the page is being attached, which can be inside a navigation; a command that refuses to run during a busy state should allow it there.  

## 🖌 DayPainter

`ClamCalendarView` draws on a single canvas and takes no children, so content of its own is drawn by `DayPainter`: images, gradients, progress rings, badges and anything a style value or a `CalendarDayAppearance` cannot draw.  
The painter is called for every drawn day in every frame, after the backgrounds, the grid lines, the date numbers, the stamps and the events of the whole month, so it draws over all of them.  
The canvas is in DIP and clipped to the cell, and its state is saved before the call and restored after it.  
The days outside the month are painted as well unless `OutsideMonthDaysVisible` is `False`, and the painter runs while the calendar is drawn, so it must not change the calendar.  

| Name | Type | Description |
|---|---|---|
| `Bounds` | `SKRect` | Whole day cell in DIP |
| `DateNumberBounds` | `SKRect` | Box of the date number of the day |
| `Day` | `CalendarDay` | Day being painted, with its date, kind, label, appearance, stamps and events |
| `IsSelected` | `bool` | Whether the day is selected; a disabled day is never reported as selected |
| `IsDisabled` | `bool` | Whether the day is disabled |
| `TextColor` | `Color` | Color the date number was drawn with, after the disabled state, the selection, today and `CalendarDayAppearance` were applied |

A paint and a `FontResolver` of [SkiaExtension](https://github.com/usausa/skia-extension) are kept by the caller instead of being created in the painter, so a frame allocates nothing; `ChartDrawing` of [ClamChart](https://github.com/usausa/clam-chart) draws a sparkline into the cell the same way.  

```csharp
private readonly SKPaint paint = new() { IsAntialias = true };

Calendar.DayPainter = (canvas, context) =>
{
    if (context.Day.Events.Count > 0)
    {
        paint.Color = SKColors.Red;
        canvas.DrawCircle(context.Bounds.Right - 11, context.Bounds.Top + 11, 9, paint);
    }
};
```

`DayAreaBounds`, `GetDayBounds` and `GetDateNumberBounds` give the same geometry in DIP for MAUI views placed over the calendar by hand, which is what a day needs when the content on it is tapped.  
`DisplayDateChanged` is raised before the application loaded the data of the new range, so place them on the change of `Month` instead.  

## 🎨 Appearance

The look is set with ordinary bindable properties on the view; there is no style object.  
The calendar builds a snapshot of them when one changes, so a frame reads one object and allocates nothing.  
Each size is validated when it is assigned: the font sizes, the heights, the widths and the mark width must be positive, the margins, the paddings and the dot spacing zero or more, and `MaxVisibleDots` zero or more; otherwise `ArgumentOutOfRangeException` is thrown.  
The background of the calendar is `BackgroundColor` of the view; `null` fills it with white.  
Sizes are in DIP.  

| Name | Type | Default | Description |
|---|---|---|---|
| `FontFamily` | `string` | `sans-serif` | Primary font; missing characters are resolved through `FallbackFonts` of SkiaExtension |
| `HeaderFontSize`, `WeekdayHeaderFontSize`, `DateNumberFontSize`, `EventFontSize`, `DayLabelFontSize`, `WeekNumberFontSize`, `MonthIndicatorFontSize` | `double` | `20`, `12`, `14`, `11`, `9`, `9`, `120` | Font sizes |
| `HeaderHeight`, `WeekdayHeaderHeight` | `double` | `48`, `32` | Heights of the header rows |
| `DateRowHeight` | `double` | `28` | Upper part of the cell with the date number and the label |
| `SlotRowHeight`, `EventRowHeight` | `double` | `18`, `16` | Pitch of the event rows and height of the bars |
| `DateNumberSize`, `DateNumberMargin` | `double` | `24`, `4` | Size and offset of the date number mark |
| `DateNumberMarkWidth` | `double` | `2` | Line width of the `Outline` and `Underline` marks |
| `StampMarginEdge` | `double` | `2` | Distance of the stamps from the cell edge |
| `EventCornerRadius`, `EventPadding` | `double` | `4`, `4` | Corner radius of the bars and the text padding |
| `EventDotSize`, `EventDotSpacing` | `double` | `6`, `3` | Diameter and gap of the dots of `Dots` |
| `NavigationButtonWidth`, `TodayButtonWidth` | `double` | `44`, `64` | Widths of the header buttons |
| `GridLineWidth` | `double` | `0.5` | Width of the grid lines |
| `WeekNumberWidth` | `double` | `24` | Width of the week number column; the day columns share what is left |
| `BackgroundColor` | `Color?` | `null` | Background of the whole calendar; `null` fills it with white. This is the ordinary MAUI property of the view |
| `GridLineColor` | `Color` | `#E0E0E0` | Cell lines |
| `WeekdayTextColor`, `SaturdayTextColor`, `SundayTextColor`, `HolidayTextColor` | `Color` | `#1F1F1F`, `#2196F3`, `#E53935`, `#E53935` | Date number colors by day kind |
| `OutsideMonthTextColor`, `OutsideMonthBackground` | `Color` | `#BDBDBD`, `#F2F2F2` | Days of the neighboring months |
| `WeekendBackground`, `HolidayBackground` | `Color` | `#FFF1F1`, `#FFF1F1` | Cell backgrounds of weekends and holidays |
| `TodayBackground`, `TodayTextColor` | `Color` | `Black`, `White` | Today bubble |
| `SelectedDayBackground`, `SelectedDayTextColor` | `Color` | `#1A73E8`, `White` | Selected date bubble |
| `RangeBackground` | `Color` | `#BDD7F5` | Date row of the days of a selected range, both ends included (the selected-day bubble is drawn over it) |
| `DisabledDayTextColor` | `Color` | `#C0C0C0` | Date number of disabled days |
| `DayLabelTextColor`, `OverflowTextColor` | `Color` | `#757575`, `#757575` | Labels and `+N` |
| `StampColor` | `Color` | `Black` | Stamps that do not set `Color` |
| `WeekNumberTextColor` | `Color` | `#757575` | Week numbers |
| `HeaderBackground`, `HeaderTextColor`, `NavigationButtonColor` | `Color` | `White`, `Black`, `#333333` | Header row |
| `WeekdayHeaderBackground`, `WeekdayHeaderTextColor`, `SaturdayHeaderTextColor`, `SundayHeaderTextColor` | `Color` | `White`, `#333333`, `#2196F3`, `#E53935` | Weekday row |
| `MonthIndicatorColor` | `Color` | `#10000000` | Month number watermark |
| `TodayMarkStyle`, `SelectedMarkStyle` | `CalendarMarkStyle` | `Fill` | Mark behind the date number: `Fill`, `Outline`, `Underline` or `None`, drawn with `TodayBackground` and `SelectedDayBackground` |
| `TodayMarkOnSelected` | `bool` | `false` | Draw the today mark over the selection when today is selected; combine it with `Outline` or `Underline` so the selection stays visible |
| `DateNumberShape` | `CalendarDateNumberShape` | `RoundedSquare` | Shape of the mark: `RoundedSquare`, `Circle` or `Square` |
| `DateNumberBold` | `bool` | `true` | Date number in bold |
| `GridLineMode` | `CalendarGridLineMode` | `All` | Lines of the body: `All`, `Horizontal`, `Vertical` or `None`; the separators under the header and the weekday row are always drawn |
| `EventDisplayMode` | `CalendarEventDisplayMode` | `Bars` | `Bars` draws the bars and the `+N` of the slot rows, `Dots` draws one dot per event of the day in the first slot row |
| `MaxVisibleDots` | `int` | `4` | Dots per day of `Dots`; `0` is unlimited and the events over the limit are not drawn |

### ClamCalendarThemes

The library ships a `ResourceDictionary` whose keyed style carries both themes with `{AppThemeBinding}`, so merging it once is enough.  

| Key | Target | Description |
|---|---|---|
| `ClamCalendarTheme` | `ClamCalendarView` | The twenty-eight colors of both themes; the sizes, the marks and the line mode are left to the application |

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <clam:ClamCalendarThemes />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>

<Style TargetType="clam:ClamCalendarView" BasedOn="{StaticResource ClamCalendarTheme}" />
```

⚠️ MAUI picks the most specific implicit style and does not merge two of them, so base an application style on the key instead of writing a second implicit style for the same type.

## 🏗 CalendarMonthBuilder

Builds the `CalendarMonth` drawn by the view; the view uses it internally and an application can build month data itself.  

| Member | Description |
|---|---|
| `FirstDayOfWeek`, `FixedWeekRows`, `OutsideMonthDaysVisible`, `MaxVisibleSlots` | Same meaning as the view properties; init-only |
| `GetDisplayRange(int year, int month)` | `CalendarDateRange` of the rows, including the days of the neighboring months |
| `Build(int year, int month, DateOnly today, events, stamps, holidays, labels, appearances)` | Builds the month; the data parameters are optional |

```csharp
var builder = new CalendarMonthBuilder { FirstDayOfWeek = DayOfWeek.Sunday, MaxVisibleSlots = 2 };
var range = builder.GetDisplayRange(2026, 9);
var month = builder.Build(2026, 9, DateOnly.FromDateTime(DateTime.Today), events, stamps, holidays);
```

Events are placed week by week: sorted by start column, longer ones first, into the lowest row that is free on every column they cover.  
With `MaxVisibleSlots`, events that do not fit are dropped from the placements and counted in `CalendarDay.HiddenEventCount`.  

## 🧩 Models

### CalendarMonth

| Name | Type | Description |
|---|---|---|
| `Year`, `Month`, `Today` | `int`, `int`, `DateOnly` | Month and the date marked as today |
| `Weeks` | `IReadOnlyList<CalendarWeek>` | Rows |
| `FirstDate`, `LastDate` | `DateOnly` | First and last date drawn |
| `FindDay(DateOnly date)` | `CalendarDay?` | Day of the date, or `null` outside the rows |

### CalendarWeek

| Name | Type | Description |
|---|---|---|
| `Days` | `IReadOnlyList<CalendarDay>` | Seven days starting with `FirstDayOfWeek` |
| `Placements` | `IReadOnlyList<CalendarEventPlacement>` | Visible event placements of the week |
| `SlotCount` | `int` | Number of event rows used |

### CalendarDay

| Name | Type | Description |
|---|---|---|
| `Date` | `DateOnly` | Date |
| `IsCurrentMonth`, `IsToday` | `bool` | Belongs to the displayed month, is today |
| `Kind` | `CalendarDayKind` | `Weekday`, `Saturday`, `Sunday` or `Holiday` |
| `Stamps` | `IReadOnlyList<CalendarStamp>` | Stamps of the day |
| `Label` | `string?` | Text from `DayLabels` |
| `Appearance` | `CalendarDayAppearance?` | Entry of `DayAppearances` |
| `Events` | `IReadOnlyList<CalendarEvent>` | Every event that covers the day, hidden ones included |
| `HiddenEventCount` | `int` | Events not drawn because of `MaxVisibleSlots` |

### CalendarEvent

| Name | Type | Default | Description |
|---|---|---|---|
| `Key` | `string?` | `null` | Identifier for the application |
| `Title` | `string` | required | Text of the bar |
| `StartDate`, `EndDate` | `DateOnly` | required | Inclusive range; an end before the start is treated as a single day |
| `Style` | `CalendarEventStyle` | `Filled` | `Filled` draws a bar with `BackgroundColor`, `Text` draws the text only |
| `BackgroundColor`, `TextColor` | `Color` | `LightGray`, `White` | Colors |
| `LeadingGlyph` | `string?` | `null` | Drawn before the title, for example an emoji |
| `StartTime` | `TimeSpan?` | `null` | Drawn before the title as `H:mm` |
| `Tag` | `object?` | `null` | Application data |
| `DurationDays`, `IsMultiDay` | `int`, `bool` | | Length of the event |

### CalendarStamp

| Name | Type | Default | Description |
|---|---|---|---|
| `Key` | `string?` | `null` | Identifier for the application |
| `Date` | `DateOnly` | required | Day |
| `Glyph` | `string` | required | Emoji or text |
| `Position` | `CalendarStampPosition` | `Center` | `Center`, `TopLeft`, `TopCenter`, `TopRight`, `BottomLeft`, `BottomCenter` or `BottomRight` |
| `FontSize` | `double` | `28` | Size in DIP |
| `Color` | `Color?` | `null` | Color of the glyph; `null` uses `StampColor` of the style and a color emoji keeps its own colors |
| `Opacity` | `double` | `1` | Alpha of the glyph |
| `Tag` | `object?` | `null` | Application data |

### CalendarDayAppearance

Colors of a single day; a `null` member keeps the style value.  
A record, so a variant is made with a `with` expression.  

| Name | Type | Default | Description |
|---|---|---|---|
| `Background` | `Color?` | `null` | Cell background, used instead of the weekend, holiday and neighboring month colors |
| `DateNumberColor` | `Color?` | `null` | Date number; disabled, selected and today still win |
| `DateNumberBackground` | `Color?` | `null` | Bubble behind the date number; disabled, selected and today still win |
| `LabelColor` | `Color?` | `null` | Text from `DayLabels` |
| `BorderColor`, `BorderWidth` | `Color?`, `double?` | `null` | Frame drawn inside the cell; both are needed and a width of zero or less draws nothing |

The selected range is drawn over `Background`, and the frame is drawn over the grid lines.  
Days of the neighboring months that are not drawn because of `OutsideMonthDaysVisible` keep no appearance.  

### CalendarEventPlacement

| Name | Type | Description |
|---|---|---|
| `Event` | `CalendarEvent` | Event |
| `StartColumn`, `ColumnSpan`, `EndColumn` | `int` | Columns of the week covered by the bar |
| `Slot` | `int` | Event row below the date numbers |
| `ContinuesFromPreviousWeek`, `ContinuesToNextWeek` | `bool` | The bar is cut at the edge of the week |

### CalendarDateRange

`readonly record struct CalendarDateRange(DateOnly Start, DateOnly End)` with `Days` and `Contains(date)`.  

### Event arguments

| Type | Members | Description |
|---|---|---|
| `CalendarDayEventArgs` | `Day`, `Date`, `Handled` | Day tapped or long pressed |
| `CalendarEventEventArgs` | `Event`, `Day`, `Handled` | Event bar tapped; `Day` is the column under the tap |
| `CalendarOverflowEventArgs` | `Day`, `HiddenCount`, `Events`, `Handled` | `+N` tapped; `Events` are all events of the day |
| `CalendarDisplayDateChangedEventArgs` | `DisplayDate`, `FirstDate`, `LastDate` | Visible range |

## 🔤 Fonts

Per character font fallback comes from [SkiaExtension](https://github.com/usausa/skia-extension).  
Configure it with `FallbackFonts` in the `SkiaExtension` namespace at startup, before the first view is shown.  
`Languages` defaults to the UI language of the device.  

```csharp
FallbackFonts.Languages = ["ja", "en"];
FallbackFonts.Typefaces = [SKTypeface.FromStream(await FileSystem.OpenAppPackageFileAsync("NotoSansJP-Regular.ttf"))];
```
