namespace ClamCalendar.Tests.Layout;

public sealed class CalendarLayoutTests
{
    private const double Tolerance = 0.001;

    private static readonly DateOnly Today = new(2026, 9, 20);

    [Fact]
    public void HeaderAndWeekdayRowsStackFromTheTop()
    {
        // Arrange
        var month = Build();
        var style = new CalendarStyle();

        // Act
        var full = new CalendarLayout(month, style, 350, 600);
        var noHeader = new CalendarLayout(month, style, 350, 600, headerVisible: false);
        var bodyOnly = new CalendarLayout(month, style, 350, 600, headerVisible: false, weekdayHeaderVisible: false);

        // Assert
        Assert.Equal(new CalendarRect(0, 0, 350, 48), full.HeaderBounds);
        Assert.Equal(new CalendarRect(0, 48, 350, 32), full.WeekdayHeaderBounds);
        Assert.Equal(new CalendarRect(0, 80, 350, 520), full.BodyBounds);
        Assert.Equal(6, full.WeekCount);
        Assert.Equal(50, full.ColumnWidth, Tolerance);
        Assert.Equal(520d / 6, full.WeekHeight, Tolerance);
        Assert.True(noHeader.HeaderBounds.IsEmpty);
        Assert.Equal(new CalendarRect(0, 0, 350, 32), noHeader.WeekdayHeaderBounds);
        Assert.Equal(new CalendarRect(0, 32, 350, 568), noHeader.BodyBounds);
        Assert.Equal(new CalendarRect(0, 0, 350, 600), bodyOnly.BodyBounds);
        Assert.True(bodyOnly.GetWeekdayHeaderCellBounds(0).IsEmpty);
    }

    [Fact]
    public void NavigationAndTodayButtonsSplitTheHeader()
    {
        // Arrange
        var month = Build();
        var style = new CalendarStyle();

        // Act
        var withToday = new CalendarLayout(month, style, 350, 600, todayButtonVisible: true);
        var withoutToday = new CalendarLayout(month, style, 350, 600);
        var noButtons = new CalendarLayout(month, style, 350, 600, navigationButtonsVisible: false);

        // Assert
        Assert.Equal(new CalendarRect(0, 0, 44, 48), withToday.PrevButtonBounds);
        Assert.Equal(new CalendarRect(306, 0, 44, 48), withToday.NextButtonBounds);
        Assert.Equal(new CalendarRect(242, 0, 64, 48), withToday.TodayButtonBounds);
        Assert.Equal(new CalendarRect(44, 0, 198, 48), withToday.TitleBounds);
        Assert.Equal(new CalendarRect(44, 0, 262, 48), withoutToday.TitleBounds);
        Assert.True(withoutToday.TodayButtonBounds.IsEmpty);
        Assert.Equal(new CalendarRect(0, 0, 350, 48), noButtons.TitleBounds);
        Assert.False(noButtons.NavigationButtonsVisible);
        Assert.Equal(CalendarHitKind.PrevButton, withToday.HitTest(10, 10).Kind);
        Assert.Equal(CalendarHitKind.NextButton, withToday.HitTest(340, 10).Kind);
        Assert.Equal(CalendarHitKind.TodayButton, withToday.HitTest(250, 10).Kind);
        Assert.Equal(CalendarHitKind.Header, withToday.HitTest(150, 10).Kind);
        Assert.Equal(CalendarHitKind.Header, noButtons.HitTest(10, 10).Kind);
    }

    [Fact]
    public void CellBoundsAndHitTestAgree()
    {
        // Arrange
        var layout = new CalendarLayout(Build(), new CalendarStyle(), 350, 600);

        // Act
        var cell = layout.GetCellBounds(1, 2);
        var hit = layout.HitTest(cell.X + 1, cell.Y + 1);
        var weekday = layout.HitTest(120, 60);

        // Assert
        Assert.Equal(100, cell.X, Tolerance);
        Assert.Equal(80 + (520d / 6), cell.Y, Tolerance);
        Assert.Equal(50, cell.Width, Tolerance);
        Assert.Equal(new CalendarHit(CalendarHitKind.Day, 1, 2), hit);
        Assert.Equal(new CalendarHit(CalendarHitKind.WeekdayHeader, -1, 2), weekday);
        Assert.Equal(new CalendarHit(CalendarHitKind.Day, 5, 6), layout.HitTest(349.9, 599.9));
        Assert.Equal(new DateOnly(2026, 9, 9), layout.Month.Weeks[1].Days[2].Date);
    }

    [Fact]
    public void DateRowAndNumberBoundsUseTheStyleSizes()
    {
        // Arrange
        var layout = new CalendarLayout(Build(), new CalendarStyle(), 350, 600);
        var cell = layout.GetCellBounds(0, 0);

        // Act
        var row = layout.GetDateRowBounds(0, 0);
        var number = layout.GetDateNumberBounds(0, 0);

        // Assert
        Assert.Equal(cell with { Height = 28 }, row);
        Assert.Equal(new CalendarRect(cell.X + 4, cell.Y + 4, 24, 24), number);
        Assert.True(layout.GetDateNumberBounds(6, 0).IsEmpty);
    }

    [Fact]
    public void EventBoundsFollowThePlacementAndAreHit()
    {
        // Arrange
        var events = new[] { Event("a", new DateOnly(2026, 9, 8), new DateOnly(2026, 9, 10)) };
        var layout = new CalendarLayout(Build(events), new CalendarStyle(), 350, 600);
        var placement = Assert.Single(layout.Month.Weeks[1].Placements);
        var week = layout.GetWeekBounds(1);

        // Act
        var bounds = layout.GetEventBounds(1, placement);
        var hit = layout.HitTest(bounds.X + 10, bounds.Y + 5);
        var below = layout.HitTest(bounds.X + 10, bounds.Bottom + 5);

        // Assert
        Assert.Equal(50, bounds.X, Tolerance);
        Assert.Equal(week.Y + 29, bounds.Y, Tolerance);
        Assert.Equal(150, bounds.Width, Tolerance);
        Assert.Equal(16, bounds.Height, Tolerance);
        Assert.Equal(CalendarHitKind.Event, hit.Kind);
        Assert.Same(placement, hit.Placement);
        Assert.Equal(1, hit.Week);
        Assert.Equal(1, hit.Column);
        Assert.Equal(new CalendarHit(CalendarHitKind.Day, 1, 1), below);
    }

    [Fact]
    public void OverflowRowSitsBelowTheVisibleSlots()
    {
        // Arrange
        var day = new DateOnly(2026, 9, 10);
        var events = new[] { Event("a", day), Event("b", day), Event("c", day) };
        var layout = new CalendarLayout(Build(events, 1), new CalendarStyle(), 350, 600);
        var week = layout.GetWeekBounds(1);

        // Act
        var overflow = layout.GetOverflowBounds(1, 3);
        var hit = layout.HitTest(overflow.X + 5, overflow.Y + 5);

        // Assert
        Assert.Equal(150, overflow.X, Tolerance);
        Assert.Equal(week.Y + 28 + 18 + 1, overflow.Y, Tolerance);
        Assert.Equal(50, overflow.Width, Tolerance);
        Assert.Equal(new CalendarHit(CalendarHitKind.Overflow, 1, 3), hit);
        Assert.True(layout.GetOverflowBounds(1, 4).IsEmpty);
        Assert.Equal(CalendarHitKind.Event, layout.HitTest(overflow.X + 5, overflow.Y - 10).Kind);
    }

    [Fact]
    public void SlotRowsThatDoNotFitAreClippedToTheWeek()
    {
        // Arrange
        var layout = new CalendarLayout(Build(), new CalendarStyle(), 350, 320);
        var week = layout.GetWeekBounds(0);

        // Act
        var first = layout.GetSlotBounds(0, 0, 0, 7);
        var partial = layout.GetSlotBounds(0, 0, 0, 7) with { Y = week.Bottom - 5 };
        var hidden = layout.GetSlotBounds(0, 5, 0, 7);

        // Assert
        Assert.Equal(40, week.Height, Tolerance);
        Assert.Equal(11, first.Height, Tolerance);
        Assert.True(partial.Bottom > week.Bottom);
        Assert.True(hidden.IsEmpty);
        Assert.True(layout.GetSlotBounds(0, 0, 6, 2).IsEmpty);
    }

    [Fact]
    public void OutsideAndInvalidInputsAreRejected()
    {
        // Arrange
        var layout = new CalendarLayout(Build(), new CalendarStyle(), 350, 600);

        // Act & Assert
        Assert.Equal(CalendarHit.None, layout.HitTest(-1, 10));
        Assert.Equal(CalendarHit.None, layout.HitTest(10, 600));
        Assert.Equal(CalendarHit.None, layout.HitTest(Double.NaN, 10));
        Assert.True(layout.GetCellBounds(6, 0).IsEmpty);
        Assert.True(layout.GetCellBounds(0, 7).IsEmpty);
        Assert.Throws<ArgumentOutOfRangeException>(static () => new CalendarLayout(Build(), new CalendarStyle(), -1, 600));
        Assert.Throws<ArgumentOutOfRangeException>(static () => new CalendarLayout(Build(), new CalendarStyle(), 350, Double.NaN));
    }

    private static CalendarMonth Build(IEnumerable<CalendarEvent>? events = null, int maxVisibleSlots = 0) =>
        new CalendarMonthBuilder { MaxVisibleSlots = maxVisibleSlots }.Build(2026, 9, Today, events);

    private static CalendarEvent Event(string key, DateOnly start, DateOnly? end = null) =>
        new() { Key = key, Title = key, StartDate = start, EndDate = end ?? start };
}
