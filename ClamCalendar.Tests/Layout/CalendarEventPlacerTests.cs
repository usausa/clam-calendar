namespace ClamCalendar.Tests.Layout;

public sealed class CalendarEventPlacerTests
{
    [Fact]
    public void EventsOnDifferentDaysShareTheFirstSlot()
    {
        // Arrange
        var candidates = new[] { Candidate("a", 0, 0), Candidate("b", 1, 3), Candidate("c", 4, 6) };

        // Act
        var placement = CalendarEventPlacer.Place(candidates, 0);

        // Assert
        Assert.Equal(1, placement.SlotCount);
        Assert.All(placement.Placements, static x => Assert.Equal(0, x.Slot));
        Assert.All(placement.HiddenCounts, static x => Assert.Equal(0, x));
    }

    [Fact]
    public void OverlappingEventsTakeTheNextFreeSlot()
    {
        // Arrange
        var candidates = new[] { Candidate("a", 0, 2), Candidate("b", 2, 4), Candidate("c", 3, 3), Candidate("d", 5, 6) };

        // Act
        var placement = CalendarEventPlacer.Place(candidates, 0);

        // Assert
        Assert.Equal(2, placement.SlotCount);
        Assert.Equal(0, Find(placement, "a").Slot);
        Assert.Equal(1, Find(placement, "b").Slot);
        Assert.Equal(0, Find(placement, "c").Slot);
        Assert.Equal(0, Find(placement, "d").Slot);
    }

    [Fact]
    public void LongerEventsArePlacedFirstAtTheSameStart()
    {
        // Arrange
        var candidates = new[] { Candidate("short", 1, 1), Candidate("long", 1, 5), Candidate("later", 0, 0) };

        // Act
        var placement = CalendarEventPlacer.Place(candidates, 0);

        // Assert
        Assert.Equal("later", placement.Placements[0].Event.Key);
        Assert.Equal("long", placement.Placements[1].Event.Key);
        Assert.Equal(0, placement.Placements[1].Slot);
        Assert.Equal("short", placement.Placements[2].Event.Key);
        Assert.Equal(1, placement.Placements[2].Slot);
        Assert.Equal(5, placement.Placements[1].ColumnSpan);
    }

    [Fact]
    public void MaxVisibleSlotsHidesTheOverflowAndCountsItPerColumn()
    {
        // Arrange
        var candidates = new[] { Candidate("a", 0, 6), Candidate("b", 0, 3), Candidate("c", 0, 1), Candidate("d", 5, 6) };

        // Act
        var placement = CalendarEventPlacer.Place(candidates, 2);

        // Assert
        Assert.Equal(2, placement.SlotCount);
        Assert.Equal(3, placement.Placements.Count);
        Assert.Equal(1, Find(placement, "d").Slot);
        Assert.DoesNotContain(placement.Placements, static x => x.Event.Key == "c");
        Assert.Equal([1, 1, 0, 0, 0, 0, 0], placement.HiddenCounts);
        Assert.Equal(3, placement.Events[0].Count);
        Assert.Equal(2, placement.Events[6].Count);
        Assert.Single(placement.Events[4]);
    }

    [Fact]
    public void ContinuationFlagsAreCopiedToThePlacement()
    {
        // Arrange
        var candidates = new[] { new CalendarEventCandidate(Event("a"), 0, 6, true, true) };

        // Act
        var placement = CalendarEventPlacer.Place(candidates, 0);

        // Assert
        var single = Assert.Single(placement.Placements);
        Assert.True(single.ContinuesFromPreviousWeek);
        Assert.True(single.ContinuesToNextWeek);
        Assert.Equal(6, single.EndColumn);
    }

    [Fact]
    public void EmptyInputReturnsTheSharedEmptyPlacement()
    {
        // Act
        var placement = CalendarEventPlacer.Place([], 3);

        // Assert
        Assert.Same(CalendarWeekPlacement.Empty, placement);
        Assert.Empty(placement.Placements);
        Assert.Equal(0, placement.SlotCount);
        Assert.Equal(7, placement.Events.Count);
        Assert.All(placement.Events, static x => Assert.Empty(x));
    }

    [Fact]
    public void InvalidColumnsAreRejected()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(static () => CalendarEventPlacer.Place([Candidate("a", 3, 7)], 0));
        Assert.Throws<ArgumentException>(static () => CalendarEventPlacer.Place([Candidate("a", 4, 3)], 0));
        Assert.Throws<ArgumentOutOfRangeException>(static () => CalendarEventPlacer.Place([Candidate("a", 0, 0)], -1));
    }

    private static CalendarEventPlacement Find(CalendarWeekPlacement placement, string key) =>
        placement.Placements.Single(x => x.Event.Key == key);

    private static CalendarEventCandidate Candidate(string key, int start, int end) =>
        new(Event(key), start, end, false, false);

    private static CalendarEvent Event(string key) =>
        new() { Key = key, Title = key, StartDate = new DateOnly(2026, 9, 1), EndDate = new DateOnly(2026, 9, 1) };
}
