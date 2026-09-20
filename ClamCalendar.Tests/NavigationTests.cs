namespace ClamCalendar.Tests;

public sealed class NavigationTests
{
    [Fact]
    public void MonthOffsetsCrossYearsAndStopAtTheCalendarLimits()
    {
        // Act & Assert
        Assert.Equal(new DateOnly(2027, 1, 1), CalendarNavigation.OffsetMonth(new DateOnly(2026, 11, 15), 2));
        Assert.Equal(new DateOnly(2025, 12, 1), CalendarNavigation.OffsetMonth(new DateOnly(2026, 1, 31), -1));
        Assert.Equal(new DateOnly(2026, 9, 1), CalendarNavigation.OffsetMonth(new DateOnly(2026, 9, 20), 0));
        Assert.Null(CalendarNavigation.OffsetMonth(DateOnly.MaxValue, 1));
        Assert.Null(CalendarNavigation.OffsetMonth(DateOnly.MinValue, -1));
        Assert.Equal(new DateOnly(9999, 12, 1), CalendarNavigation.OffsetMonth(new DateOnly(9999, 11, 1), 1));
    }

    [Fact]
    public void ClampKeepsTheMonthInsideTheLimits()
    {
        // Arrange
        var min = new DateOnly(2026, 8, 15);
        var max = new DateOnly(2026, 10, 5);

        // Act & Assert
        Assert.Equal(new DateOnly(2026, 8, 1), CalendarNavigation.ClampMonth(new DateOnly(2026, 6, 1), min, max));
        Assert.Equal(new DateOnly(2026, 10, 1), CalendarNavigation.ClampMonth(new DateOnly(2027, 1, 1), min, max));
        Assert.Equal(new DateOnly(2026, 9, 1), CalendarNavigation.ClampMonth(new DateOnly(2026, 9, 20), min, max));
        Assert.True(CalendarNavigation.IsMonthWithinLimits(new DateOnly(2026, 8, 1), min, max));
        Assert.False(CalendarNavigation.IsMonthWithinLimits(new DateOnly(2026, 7, 31), min, max));
        Assert.True(CalendarNavigation.IsMonthWithinLimits(new DateOnly(2026, 7, 31), null, null));
    }

    [Fact]
    public void NavigateMovesTheDisplayMonthAndAnnouncesTheRange()
    {
        // Arrange
        using var view = new ClamCalendarView();
        view.DisplayDate = new DateOnly(2026, 9, 15);
        var ranges = new List<CalendarDisplayDateChangedEventArgs>();
        view.DisplayDateChanged += (_, e) => ranges.Add(e);

        // Act
        view.Navigate(1);

        // Assert
        Assert.Equal(new DateOnly(2026, 10, 1), view.DisplayDate);
        Assert.Equal(10, view.Month.Month);
        var range = Assert.Single(ranges);
        Assert.Equal(new DateOnly(2026, 10, 1), range.DisplayDate);
        Assert.Equal(new DateOnly(2026, 9, 28), range.FirstDate);
        Assert.Equal(new DateOnly(2026, 11, 8), range.LastDate);

        // Act
        view.DisplayDate = new DateOnly(2026, 10, 20);
        view.Navigate(-2);

        // Assert
        Assert.Equal(new DateOnly(2026, 8, 1), view.DisplayDate);
        Assert.Equal(2, ranges.Count);
    }

    [Fact]
    public void NavigationStopsAtTheMonthsOfMinAndMax()
    {
        // Arrange
        using var view = new ClamCalendarView();
        view.DisplayDate = new DateOnly(2026, 9, 1);
        view.MinDate = new DateOnly(2026, 8, 15);
        view.MaxDate = new DateOnly(2026, 10, 5);

        // Act & Assert
        Assert.True(view.CanNavigate(-1));
        view.Navigate(-2);
        Assert.Equal(new DateOnly(2026, 8, 1), view.DisplayDate);
        Assert.False(view.CanNavigate(-1));
        view.Navigate(-1);
        Assert.Equal(new DateOnly(2026, 8, 1), view.DisplayDate);
        view.Navigate(5);
        Assert.Equal(new DateOnly(2026, 10, 1), view.DisplayDate);
        Assert.False(view.CanNavigate(1));
        Assert.True(view.CanNavigate(-1));
    }

    [Fact]
    public void GoToTodayUsesTheTodayPropertyAndTheLimits()
    {
        // Arrange
        using var view = new ClamCalendarView();
        view.Today = new DateOnly(2026, 3, 3);
        view.DisplayDate = new DateOnly(2026, 9, 1);

        // Act
        view.GoToToday();

        // Assert
        Assert.Equal(new DateOnly(2026, 3, 1), view.DisplayDate);
        Assert.True(view.Month.FindDay(new DateOnly(2026, 3, 3))!.IsToday);

        // Arrange
        view.MinDate = new DateOnly(2026, 5, 1);

        // Act
        view.GoToToday();

        // Assert
        Assert.Equal(new DateOnly(2026, 5, 1), view.DisplayDate);
    }

    [Fact]
    public void DisplayDateInsideTheSameMonthAndDataChangesDoNotAnnounceAgain()
    {
        // Arrange
        using var view = new ClamCalendarView();
        view.DisplayDate = new DateOnly(2026, 9, 1);
        var count = 0;
        view.DisplayDateChanged += (_, _) => count++;

        // Act
        view.DisplayDate = new DateOnly(2026, 9, 2);
        view.Events = [new CalendarEvent { Title = "a", StartDate = new DateOnly(2026, 9, 2), EndDate = new DateOnly(2026, 9, 2) }];

        // Assert
        Assert.Equal(0, count);
        Assert.Single(view.Month.FindDay(new DateOnly(2026, 9, 2))!.Events);

        // Act
        view.FirstDayOfWeek = DayOfWeek.Sunday;

        // Assert
        Assert.Equal(1, count);
        Assert.Equal(new DateOnly(2026, 8, 30), view.Month.FirstDate);
    }

    [Fact]
    public void CommandReceivesTheSameArgumentsAsTheEvent()
    {
        // Arrange
        using var view = new ClamCalendarView();
        view.DisplayDate = new DateOnly(2026, 9, 1);
        CalendarDisplayDateChangedEventArgs? received = null;
        view.DisplayDateChangedCommand = new Command<CalendarDisplayDateChangedEventArgs>(e => received = e);

        // Act
        view.Navigate(1);

        // Assert
        Assert.NotNull(received);
        Assert.Equal(new DateOnly(2026, 10, 1), received.DisplayDate);
    }
}
