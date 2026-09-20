namespace ClamCalendar.Layout;

// Event clipped to one week row; columns are days from the first day of the week
internal readonly record struct CalendarEventCandidate(CalendarEvent Event, int StartColumn, int EndColumn, bool ContinuesFromPreviousWeek, bool ContinuesToNextWeek)
{
    public int ColumnSpan => (EndColumn - StartColumn) + 1;
}

internal sealed class CalendarWeekPlacement
{
    public static CalendarWeekPlacement Empty { get; } = new([], 0, new int[CalendarEventPlacer.DaysPerWeek], CreateEmptyEvents());

    public IReadOnlyList<CalendarEventPlacement> Placements { get; }

    public int SlotCount { get; }

    public IReadOnlyList<int> HiddenCounts { get; }

    public IReadOnlyList<IReadOnlyList<CalendarEvent>> Events { get; }

    public CalendarWeekPlacement(IReadOnlyList<CalendarEventPlacement> placements, int slotCount, IReadOnlyList<int> hiddenCounts, IReadOnlyList<IReadOnlyList<CalendarEvent>> events)
    {
        Placements = placements;
        SlotCount = slotCount;
        HiddenCounts = hiddenCounts;
        Events = events;
    }

    private static IReadOnlyList<CalendarEvent>[] CreateEmptyEvents()
    {
        var events = new IReadOnlyList<CalendarEvent>[CalendarEventPlacer.DaysPerWeek];
        Array.Fill(events, Array.Empty<CalendarEvent>());
        return events;
    }
}

// Greedy slot assignment: events are placed by start column, longer ones first, into the lowest slot that is free on every column they cover
internal static class CalendarEventPlacer
{
    public const int DaysPerWeek = 7;

    public static CalendarWeekPlacement Place(IReadOnlyList<CalendarEventCandidate> candidates, int maxVisibleSlots)
    {
        ArgumentNullException.ThrowIfNull(candidates);
        ArgumentOutOfRangeException.ThrowIfNegative(maxVisibleSlots);
        if (candidates.Count == 0)
        {
            return CalendarWeekPlacement.Empty;
        }

        var ordered = candidates.OrderBy(static x => x.StartColumn).ThenByDescending(static x => x.ColumnSpan).ToArray();
        var occupancy = new List<int>();
        var placements = new List<CalendarEventPlacement>(ordered.Length);
        var hidden = new int[DaysPerWeek];
        var events = new List<CalendarEvent>[DaysPerWeek];
        for (var column = 0; column < DaysPerWeek; column++)
        {
            events[column] = [];
        }

        foreach (var candidate in ordered)
        {
            if ((candidate.StartColumn < 0) || (candidate.EndColumn >= DaysPerWeek) || (candidate.EndColumn < candidate.StartColumn))
            {
                throw new ArgumentException("Candidate columns must be within the week.", nameof(candidates));
            }

            for (var column = candidate.StartColumn; column <= candidate.EndColumn; column++)
            {
                events[column].Add(candidate.Event);
            }

            var mask = ((1 << candidate.ColumnSpan) - 1) << candidate.StartColumn;
            var slot = FindAvailableSlot(occupancy, mask);
            if ((maxVisibleSlots > 0) && (slot >= maxVisibleSlots))
            {
                for (var column = candidate.StartColumn; column <= candidate.EndColumn; column++)
                {
                    hidden[column]++;
                }

                continue;
            }

            if (slot == occupancy.Count)
            {
                occupancy.Add(0);
            }

            occupancy[slot] |= mask;
            placements.Add(new CalendarEventPlacement
            {
                Event = candidate.Event,
                StartColumn = candidate.StartColumn,
                ColumnSpan = candidate.ColumnSpan,
                Slot = slot,
                ContinuesFromPreviousWeek = candidate.ContinuesFromPreviousWeek,
                ContinuesToNextWeek = candidate.ContinuesToNextWeek
            });
        }

        return new CalendarWeekPlacement(placements, occupancy.Count, hidden, events);
    }

    private static int FindAvailableSlot(List<int> occupancy, int mask)
    {
        for (var slot = 0; slot < occupancy.Count; slot++)
        {
            if ((occupancy[slot] & mask) == 0)
            {
                return slot;
            }
        }

        return occupancy.Count;
    }
}
