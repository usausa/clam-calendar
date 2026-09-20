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
| `Month` | `CalendarMonth` | | | Month model built from `DisplayDate` and the data properties |
| `Today` | `DateOnly?` | `null` | ✓ | Overrides the system date for the today mark and `GoToToday` |
| `EffectiveToday` | `DateOnly` | | | `Today` or the system date |
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
| `TodayButtonVisible` | `bool` | `false` | ✓ | Today button next to the next button |
| `HeaderFormat` | `string?` | `null` | ✓ | Title format; `null` uses `YearMonthPattern` of the culture |
| `PrevButtonText`, `NextButtonText`, `TodayButtonText` | `string` | `◀`, `▶`, `Today` | ✓ | Button texts |
| `WeekdayHeaderVisible` | `bool` | `true` | ✓ | Weekday row |
| `WeekdayNameFormat` | `CalendarWeekdayNameFormat` | `Initial` | ✓ | `Initial` (M / 月), `Abbreviated` (Mon) or `Full` (Monday) |
| `MonthIndicatorVisible` | `bool` | `false` | ✓ | Month number watermark behind the cells |
| `CalendarStyle` | `CalendarStyle` | `new()` | ✓ | Fonts, sizes and colors |
| `SelectionMode` | `CalendarSelectionMode` | `None` | ✓ | `None`, `SingleDate`, `MultipleDates` or `DateRange` |
| `SelectedDate` | `DateOnly?` | `null` | ✓ TwoWay | Selection of `SingleDate` |
| `SelectedDates` | `ObservableCollection<DateOnly>` | empty | ✓ | Selection of `MultipleDates`; bind a collection of the view model to share it |
| `SelectedStartDate`, `SelectedEndDate` | `DateOnly?` | `null` | ✓ TwoWay | Selection of `DateRange`; the end is `null` until the second tap and the ends are ordered |
| `AllowDeselect` | `bool` | `true` | ✓ | In `SingleDate`, tapping the selected date again clears it |
| `MaxSelectableDays` | `int` | `0` | ✓ | Limit of `MultipleDates` and `DateRange`; `0` is unlimited |
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
| `ClearSelection()` | `void` | Clears the selection of every mode |
| `CancelInteraction()` | `void` | Stops the current gesture |
| `Invalidate()` | `void` | Recreates the renderer and redraws after the content of `CalendarStyle` was changed in place |
| `InvalidateSurface()` | `void` | Redraws |
| `Dispose()` | `void` | Releases the timers, subscriptions and native resources |

### 📣 Events

| Name | EventArgs | Description |
|---|---|---|
| `DisplayDateChanged` | `CalendarDisplayDateChangedEventArgs` | The month or the visible range changed; raised once more when the view is attached so the initial range can be loaded |
| `DayTapped` | `CalendarDayEventArgs` | Day tapped, after the selection was updated; `Handled` suppresses the command |
| `DayLongPressed` | `CalendarDayEventArgs` | Day, event or `+N` long pressed |
| `EventTapped` | `CalendarEventEventArgs` | Event bar tapped, with the day under the tap |
| `OverflowTapped` | `CalendarOverflowEventArgs` | `+N` tapped, with every event of the day |
| `SelectionChanged` | `EventArgs` | Selection changed by a tap or a property |

Disabled days and blank days of the neighboring months raise no tap events.  
The slide animation and the long press timer need a handler, so they are skipped when the view is not attached.  
The first `DisplayDateChanged` is raised while the page is being attached, which can be inside a navigation; a command that refuses to run during a busy state should allow it there.  

## 🎨 CalendarStyle

A style can be a XAML resource.  
Treat it as immutable once assigned and replace it with a `with` expression to change it, or call `Invalidate()` after changing it in place.  
Sizes are in DIP.  

| Name | Type | Default | Description |
|---|---|---|---|
| `FontFamily` | `string` | `sans-serif` | Primary font; missing characters are resolved through `CalendarFonts` |
| `HeaderFontSize`, `WeekdayHeaderFontSize`, `DateNumberFontSize`, `EventFontSize`, `DayLabelFontSize`, `MonthIndicatorFontSize` | `float` | `20`, `12`, `14`, `11`, `9`, `120` | Font sizes |
| `HeaderHeight`, `WeekdayHeaderHeight` | `float` | `48`, `32` | Heights of the header rows |
| `DateRowHeight` | `float` | `28` | Upper part of the cell with the date number and the label |
| `SlotRowHeight`, `EventRowHeight` | `float` | `18`, `16` | Pitch of the event rows and height of the bars |
| `DateNumberSize`, `DateNumberMargin` | `float` | `24`, `4` | Size and offset of the date number bubble |
| `StampMarginEdge` | `float` | `2` | Distance of the stamps from the cell edge |
| `EventCornerRadius`, `EventPadding` | `float` | `4`, `4` | Corner radius of the bars and the text padding |
| `NavigationButtonWidth`, `TodayButtonWidth` | `float` | `44`, `64` | Widths of the header buttons |
| `Background`, `GridLineColor` | `Color` | `White`, `#E0E0E0` | Body background and cell lines |
| `WeekdayTextColor`, `SaturdayTextColor`, `SundayTextColor`, `HolidayTextColor` | `Color` | `#1F1F1F`, `#2196F3`, `#E53935`, `#E53935` | Date number colors by day kind |
| `OutsideMonthTextColor`, `OutsideMonthBackground` | `Color` | `#BDBDBD`, `#F2F2F2` | Days of the neighboring months |
| `WeekendBackground`, `HolidayBackground` | `Color` | `#FFF1F1`, `#FFF1F1` | Cell backgrounds of weekends and holidays |
| `TodayBackground`, `TodayTextColor` | `Color` | `Black`, `White` | Today bubble |
| `SelectedDayBackground`, `SelectedDayTextColor` | `Color` | `#1A73E8`, `White` | Selected date bubble |
| `RangeBackground` | `Color` | `#BDD7F5` | Date row of the days of a selected range, both ends included (the selected-day bubble is drawn over it) |
| `DisabledDayTextColor` | `Color` | `#C0C0C0` | Date number of disabled days |
| `DayLabelTextColor`, `OverflowTextColor` | `Color` | `#757575`, `#757575` | Labels and `+N` |
| `HeaderBackground`, `HeaderTextColor`, `NavigationButtonColor` | `Color` | `White`, `Black`, `#333333` | Header row |
| `WeekdayHeaderBackground`, `WeekdayHeaderTextColor`, `SaturdayHeaderTextColor`, `SundayHeaderTextColor` | `Color` | `White`, `#333333`, `#2196F3`, `#E53935` | Weekday row |
| `MonthIndicatorColor` | `Color` | `#10000000` | Month number watermark |

## 🔤 CalendarFonts

Global font fallback settings, read when a view creates its renderer.  
Set them at startup, before the first view is shown.  
Emoji presentation sequences (a character followed by U+FE0F) are matched against the emoji font first.  
Variation selectors and zero width joiners only steer the font choice and are not drawn.  

| Name | Type | Default | Description |
|---|---|---|---|
| `Languages` | `IReadOnlyList<string>` | `["ja"]` | BCP-47 tags for the per character system font lookup; the tag selects the glyph variant of shared CJK ideographs |
| `Fallbacks` | `IReadOnlyList<SKTypeface>` | `[]` | Typefaces tried before the system lookup, for example a bundled font; they stay owned by the caller |

```csharp
CalendarFonts.Languages = ["ja", "en"];
CalendarFonts.Fallbacks = [SKTypeface.FromStream(await FileSystem.OpenAppPackageFileAsync("NotoSansJP-Regular.ttf"))];
```

## 🏗 CalendarMonthBuilder

Builds the `CalendarMonth` drawn by the view; the view uses it internally and an application can build month data itself.  

| Member | Description |
|---|---|
| `FirstDayOfWeek`, `FixedWeekRows`, `OutsideMonthDaysVisible`, `MaxVisibleSlots` | Same meaning as the view properties; init-only |
| `GetDisplayRange(int year, int month)` | `CalendarDateRange` of the rows, including the days of the neighboring months |
| `Build(int year, int month, DateOnly today, events, stamps, holidays, labels)` | Builds the month; the data parameters are optional |

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
| `FontSize` | `float` | `28` | Size in DIP |
| `Opacity` | `float` | `1` | Alpha of the glyph |
| `Tag` | `object?` | `null` | Application data |

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
