namespace ClamCalendar.Tests;

public sealed class SelectionTests
{
    private static readonly DateOnly Day1 = new(2026, 9, 1);
    private static readonly DateOnly Day5 = new(2026, 9, 5);
    private static readonly DateOnly Day9 = new(2026, 9, 9);

    [Fact]
    public void SingleTapTogglesUnlessDeselectIsDisabled()
    {
        // Act & Assert
        Assert.Equal(Day1, CalendarSelectionLogic.TapSingle(null, Day1, true));
        Assert.Null(CalendarSelectionLogic.TapSingle(Day1, Day1, true));
        Assert.Equal(Day1, CalendarSelectionLogic.TapSingle(Day1, Day1, false));
        Assert.Equal(Day5, CalendarSelectionLogic.TapSingle(Day1, Day5, false));
    }

    [Fact]
    public void MultipleTapTogglesAndStopsAtTheLimit()
    {
        // Arrange
        var dates = new List<DateOnly>();

        // Act & Assert
        Assert.True(CalendarSelectionLogic.TapMultiple(dates, Day1, 2));
        Assert.True(CalendarSelectionLogic.TapMultiple(dates, Day5, 2));
        Assert.False(CalendarSelectionLogic.TapMultiple(dates, Day9, 2));
        Assert.Equal([Day1, Day5], dates);
        Assert.True(CalendarSelectionLogic.TapMultiple(dates, Day1, 2));
        Assert.Equal([Day5], dates);
        Assert.True(CalendarSelectionLogic.TapMultiple(dates, Day9, 0));
        Assert.True(CalendarSelectionLogic.TapMultiple(dates, Day1, 0));
        Assert.Equal(3, dates.Count);
    }

    [Fact]
    public void RangeOrdersTheEndsAndRestartsAfterCompletion()
    {
        // Act & Assert
        Assert.Equal((Day5, null), CalendarSelectionLogic.TapRange(null, null, Day5, 0, static _ => false));
        Assert.Equal((Day1, Day5), CalendarSelectionLogic.TapRange(Day5, null, Day1, 0, static _ => false));
        Assert.Equal((Day5, Day9), CalendarSelectionLogic.TapRange(Day5, null, Day9, 0, static _ => false));
        Assert.Equal((Day5, Day5), CalendarSelectionLogic.TapRange(Day5, null, Day5, 0, static _ => false));
        Assert.Equal((Day9, null), CalendarSelectionLogic.TapRange(Day1, Day5, Day9, 0, static _ => false));
    }

    [Fact]
    public void RangeRestartsWhenItCrossesADisabledDayOrExceedsTheLimit()
    {
        // Act & Assert
        Assert.Equal((Day9, null), CalendarSelectionLogic.TapRange(Day1, null, Day9, 0, static date => date == new DateOnly(2026, 9, 4)));
        Assert.Equal((Day1, Day5), CalendarSelectionLogic.TapRange(Day1, null, Day5, 5, static _ => false));
        Assert.Equal((Day9, null), CalendarSelectionLogic.TapRange(Day1, null, Day9, 5, static _ => false));
    }

    [Fact]
    public void DisabledCombinesLimitsAndExplicitDates()
    {
        // Arrange
        var disabled = new HashSet<DateOnly> { Day5 };

        // Act & Assert
        Assert.False(CalendarSelectionLogic.IsDisabled(Day1, null, null, null));
        Assert.True(CalendarSelectionLogic.IsDisabled(Day1, new DateOnly(2026, 9, 2), null, null));
        Assert.True(CalendarSelectionLogic.IsDisabled(Day9, null, new DateOnly(2026, 9, 8), null));
        Assert.True(CalendarSelectionLogic.IsDisabled(Day5, null, null, disabled));
        Assert.False(CalendarSelectionLogic.IsDisabled(Day9, Day1, Day9, disabled));
    }

    [Fact]
    public void RangeMembershipIncludesBothEnds()
    {
        // Act & Assert
        Assert.True(CalendarSelectionLogic.IsInRange(Day5, Day9, Day1));
        Assert.True(CalendarSelectionLogic.IsInRange(Day1, Day1, Day9));
        Assert.True(CalendarSelectionLogic.IsInRange(Day9, Day1, Day9));
        Assert.False(CalendarSelectionLogic.IsInRange(new DateOnly(2026, 9, 10), Day1, Day9));
        Assert.False(CalendarSelectionLogic.IsInRange(Day5, Day1, null));
        Assert.True(CalendarSelectionLogic.IsInRange(Day1, Day1, null));
        Assert.False(CalendarSelectionLogic.IsInRange(Day1, null, null));
        Assert.Equal((Day1, Day9), CalendarSelectionLogic.Normalize(Day9, Day1));
    }

    [Fact]
    public void ViewAppliesTapsToTheBoundProperties()
    {
        // Arrange
        using var view = new ClamCalendarView();
        view.SelectionMode = CalendarSelectionMode.SingleDate;
        var changes = 0;
        view.SelectionChanged += (_, _) => changes++;

        // Act & Assert
        Assert.True(view.SelectDay(Day5));
        Assert.Equal(Day5, view.SelectedDate);
        Assert.True(view.IsDateSelected(Day5));
        Assert.Equal(1, changes);
        Assert.True(view.SelectDay(Day5));
        Assert.Null(view.SelectedDate);
        Assert.Equal(2, changes);

        // Arrange
        view.AllowDeselect = false;
        view.SelectDay(Day5);

        // Act & Assert
        Assert.False(view.SelectDay(Day5));
        Assert.Equal(Day5, view.SelectedDate);
    }

    [Fact]
    public void ViewMultipleAndRangeSelectionsFollowTheLimits()
    {
        // Arrange
        using var view = new ClamCalendarView();
        view.SelectionMode = CalendarSelectionMode.MultipleDates;
        view.MaxSelectableDays = 2;
        var changes = 0;
        view.SelectionChanged += (_, _) => changes++;

        // Act & Assert
        Assert.True(view.SelectDay(Day1));
        Assert.True(view.SelectDay(Day5));
        Assert.False(view.SelectDay(Day9));
        Assert.Equal([Day1, Day5], view.SelectedDates);
        Assert.True(view.IsDateSelected(Day5));
        Assert.False(view.IsDateSelected(Day9));
        Assert.Equal(2, changes);

        // Arrange
        view.SelectionMode = CalendarSelectionMode.DateRange;
        view.MaxSelectableDays = 0;
        view.DisabledDates = [new DateOnly(2026, 9, 7)];

        // Act & Assert
        Assert.True(view.SelectDay(Day9));
        Assert.True(view.SelectDay(Day5));
        Assert.Equal(Day5, view.SelectedStartDate);
        Assert.Null(view.SelectedEndDate);
        Assert.True(view.SelectDay(Day1));
        Assert.Equal(Day1, view.SelectedStartDate);
        Assert.Equal(Day5, view.SelectedEndDate);
        Assert.True(view.IsDateInSelectedRange(new DateOnly(2026, 9, 3)));
        Assert.True(view.IsDateInSelectedRange(Day1));
        Assert.False(view.IsDateInSelectedRange(Day9));
        Assert.Equal(5, changes);
        Assert.False(view.SelectDay(new DateOnly(2026, 9, 7)));

        // Act
        view.ClearSelection();

        // Assert
        Assert.Null(view.SelectedStartDate);
        Assert.Null(view.SelectedEndDate);
        Assert.Empty(view.SelectedDates);
        Assert.Equal(6, changes);
    }

    [Fact]
    public void ViewRejectsDisabledAndOutOfRangeDates()
    {
        // Arrange
        using var view = new ClamCalendarView();
        view.SelectionMode = CalendarSelectionMode.SingleDate;
        view.MinDate = Day5;
        view.MaxDate = Day9;

        // Act & Assert
        Assert.True(view.IsDateDisabled(Day1));
        Assert.False(view.IsDateDisabled(Day5));
        Assert.True(view.IsDateDisabled(new DateOnly(2026, 9, 10)));
        Assert.False(view.SelectDay(Day1));
        Assert.Null(view.SelectedDate);

        // Arrange
        view.MaxSelectableDays = 3;

        // Act
        view.MaxSelectableDays = -1;

        // Assert
        Assert.Equal(3, view.MaxSelectableDays);
    }
}
