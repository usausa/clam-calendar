namespace ClamCalendar.Tests;

public sealed class CalendarMonthBuilderTests
{
    private static readonly DateOnly Today = new(2026, 9, 20);

    [Fact]
    public void FirstDayOfWeekAlignsTheFirstRow()
    {
        // Arrange
        var monday = new CalendarMonthBuilder();
        var sunday = new CalendarMonthBuilder { FirstDayOfWeek = DayOfWeek.Sunday };

        // Act
        var mondayMonth = monday.Build(2026, 9, Today);
        var sundayMonth = sunday.Build(2026, 9, Today);

        // Assert
        Assert.Equal(new DateOnly(2026, 8, 31), mondayMonth.FirstDate);
        Assert.Equal(new DateOnly(2026, 10, 11), mondayMonth.LastDate);
        Assert.Equal(DayOfWeek.Monday, mondayMonth.Weeks[0].Days[0].Date.DayOfWeek);
        Assert.Equal(new DateOnly(2026, 8, 30), sundayMonth.FirstDate);
        Assert.Equal(new DateOnly(2026, 10, 10), sundayMonth.LastDate);
        Assert.Equal(DayOfWeek.Sunday, sundayMonth.Weeks[0].Days[0].Date.DayOfWeek);
    }

    [Fact]
    public void FixedWeekRowsAlwaysProducesSixWeeks()
    {
        // Arrange
        var builder = new CalendarMonthBuilder();

        // Act
        var four = builder.Build(2027, 2, Today);
        var five = builder.Build(2026, 9, Today);
        var six = builder.Build(2026, 8, Today);

        // Assert
        Assert.Equal(6, four.Weeks.Count);
        Assert.Equal(6, five.Weeks.Count);
        Assert.Equal(6, six.Weeks.Count);
        Assert.All(four.Weeks, static week => Assert.Equal(7, week.Days.Count));
    }

    [Fact]
    public void ActualWeekRowsFollowTheMonth()
    {
        // Arrange
        var builder = new CalendarMonthBuilder { FixedWeekRows = false };

        // Act
        var four = builder.Build(2027, 2, Today);
        var five = builder.Build(2026, 9, Today);
        var six = builder.Build(2026, 8, Today);

        // Assert
        Assert.Equal(4, four.Weeks.Count);
        Assert.Equal(new DateOnly(2027, 2, 1), four.FirstDate);
        Assert.Equal(new DateOnly(2027, 2, 28), four.LastDate);
        Assert.Equal(5, five.Weeks.Count);
        Assert.Equal(new DateOnly(2026, 10, 4), five.LastDate);
        Assert.Equal(6, six.Weeks.Count);
        Assert.Equal(builder.GetDisplayRange(2026, 9), new CalendarDateRange(five.FirstDate, five.LastDate));
    }

    [Fact]
    public void DisplayRangeMatchesTheBuiltMonth()
    {
        // Arrange
        var builder = new CalendarMonthBuilder { FirstDayOfWeek = DayOfWeek.Sunday };

        // Act
        var range = builder.GetDisplayRange(2026, 9);
        var month = builder.Build(2026, 9, Today);

        // Assert
        Assert.Equal(range.Start, month.FirstDate);
        Assert.Equal(range.End, month.LastDate);
        Assert.Equal(42, range.Days);
        Assert.True(range.Contains(Today));
        Assert.False(range.Contains(new DateOnly(2026, 10, 11)));
    }

    [Fact]
    public void OutsideMonthDaysAreFlaggedAndCanBeHidden()
    {
        // Arrange
        var stamps = new[] { Stamp(new DateOnly(2026, 8, 31)), Stamp(new DateOnly(2026, 9, 1)) };
        var events = new[] { Event("a", new DateOnly(2026, 8, 30), new DateOnly(2026, 9, 2)) };
        var labels = new Dictionary<DateOnly, string> { [new DateOnly(2026, 8, 31)] = "outside", [new DateOnly(2026, 9, 1)] = "inside" };
        var visible = new CalendarMonthBuilder();
        var hidden = new CalendarMonthBuilder { OutsideMonthDaysVisible = false };

        // Act
        var visibleMonth = visible.Build(2026, 9, Today, events, stamps, null, labels);
        var hiddenMonth = hidden.Build(2026, 9, Today, events, stamps, null, labels);

        // Assert
        var outside = visibleMonth.Weeks[0].Days[0];
        Assert.False(outside.IsCurrentMonth);
        Assert.Single(outside.Stamps);
        Assert.Equal("outside", outside.Label);
        Assert.Single(outside.Events);
        Assert.Equal(0, visibleMonth.Weeks[0].Placements[0].StartColumn);
        Assert.True(visibleMonth.Weeks[0].Placements[0].ContinuesFromPreviousWeek);

        var blank = hiddenMonth.Weeks[0].Days[0];
        Assert.False(blank.IsCurrentMonth);
        Assert.Empty(blank.Stamps);
        Assert.Null(blank.Label);
        Assert.Empty(blank.Events);
        Assert.Single(hiddenMonth.Weeks[0].Days[1].Stamps);
        Assert.Equal("inside", hiddenMonth.Weeks[0].Days[1].Label);
        var placement = Assert.Single(hiddenMonth.Weeks[0].Placements);
        Assert.Equal(1, placement.StartColumn);
        Assert.Equal(2, placement.ColumnSpan);
        Assert.True(placement.ContinuesFromPreviousWeek);
        Assert.False(placement.ContinuesToNextWeek);
    }

    [Fact]
    public void HolidaysAndWeekendsSetTheKind()
    {
        // Arrange
        var builder = new CalendarMonthBuilder();
        var holidays = new[] { new DateOnly(2026, 9, 21), new DateOnly(2026, 9, 23) };

        // Act
        var month = builder.Build(2026, 9, Today, holidays: holidays);

        // Assert
        Assert.Equal(CalendarDayKind.Holiday, month.FindDay(new DateOnly(2026, 9, 21))!.Kind);
        Assert.Equal(CalendarDayKind.Holiday, month.FindDay(new DateOnly(2026, 9, 23))!.Kind);
        Assert.Equal(CalendarDayKind.Weekday, month.FindDay(new DateOnly(2026, 9, 22))!.Kind);
        Assert.Equal(CalendarDayKind.Saturday, month.FindDay(new DateOnly(2026, 9, 19))!.Kind);
        Assert.Equal(CalendarDayKind.Sunday, month.FindDay(new DateOnly(2026, 9, 20))!.Kind);
    }

    [Fact]
    public void TodayStampsAndLabelsAreAttachedToTheirDay()
    {
        // Arrange
        var builder = new CalendarMonthBuilder();
        var stamps = new[] { Stamp(Today), Stamp(Today), Stamp(new DateOnly(2026, 9, 3)) };
        var labels = new Dictionary<DateOnly, string> { [new DateOnly(2026, 9, 21)] = "Respect for the Aged Day" };

        // Act
        var month = builder.Build(2026, 9, Today, null, stamps, null, labels);

        // Assert
        var today = month.FindDay(Today)!;
        Assert.True(today.IsToday);
        Assert.Equal(2, today.Stamps.Count);
        Assert.Single(month.FindDay(new DateOnly(2026, 9, 3))!.Stamps);
        Assert.Equal("Respect for the Aged Day", month.FindDay(new DateOnly(2026, 9, 21))!.Label);
        Assert.Null(month.FindDay(new DateOnly(2026, 9, 22))!.Label);
        Assert.Equal(1, month.Weeks.Sum(static week => week.Days.Count(static day => day.IsToday)));
        Assert.Null(month.FindDay(new DateOnly(2026, 10, 12)));
    }

    [Fact]
    public void EventsAcrossWeeksAreClippedWithContinuationFlags()
    {
        // Arrange
        var builder = new CalendarMonthBuilder();
        var events = new[] { Event("a", new DateOnly(2026, 9, 5), new DateOnly(2026, 9, 8)), Event("b", new DateOnly(2026, 9, 10), new DateOnly(2026, 9, 9)) };

        // Act
        var month = builder.Build(2026, 9, Today, events);

        // Assert
        var first = Assert.Single(month.Weeks[0].Placements);
        Assert.Equal(5, first.StartColumn);
        Assert.Equal(2, first.ColumnSpan);
        Assert.Equal(6, first.EndColumn);
        Assert.False(first.ContinuesFromPreviousWeek);
        Assert.True(first.ContinuesToNextWeek);
        var second = month.Weeks[1].Placements.Single(static x => x.Event.Key == "a");
        Assert.Equal(0, second.StartColumn);
        Assert.Equal(2, second.ColumnSpan);
        Assert.True(second.ContinuesFromPreviousWeek);
        Assert.False(second.ContinuesToNextWeek);
        var reversed = month.Weeks[1].Placements.Single(static x => x.Event.Key == "b");
        Assert.Equal(3, reversed.StartColumn);
        Assert.Equal(1, reversed.ColumnSpan);
        Assert.Equal(3, month.Weeks[1].Days.Sum(static day => day.Events.Count));
    }

    [Fact]
    public void MaxVisibleSlotsCountsHiddenEventsPerDay()
    {
        // Arrange
        var builder = new CalendarMonthBuilder { MaxVisibleSlots = 2 };
        var day = new DateOnly(2026, 9, 10);
        var events = new[] { Event("a", day), Event("b", day), Event("c", day), Event("d", new DateOnly(2026, 9, 11)) };

        // Act
        var month = builder.Build(2026, 9, Today, events);

        // Assert
        var week = month.Weeks[1];
        Assert.Equal(2, week.SlotCount);
        Assert.Equal(3, week.Placements.Count);
        var target = week.Days[3];
        Assert.Equal(1, target.HiddenEventCount);
        Assert.Equal(3, target.Events.Count);
        Assert.Equal(0, week.Days[4].HiddenEventCount);
        Assert.Single(week.Days[4].Events);
    }

    [Fact]
    public void NegativeSlotLimitIsRejected()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(static () => new CalendarMonthBuilder { MaxVisibleSlots = -1 });
    }

    private static CalendarEvent Event(string key, DateOnly start, DateOnly? end = null) =>
        new() { Key = key, Title = key, StartDate = start, EndDate = end ?? start };

    private static CalendarStamp Stamp(DateOnly date) =>
        new() { Date = date, Glyph = "🐈" };
}
