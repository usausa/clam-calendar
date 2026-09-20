namespace ClamCalendar;

using ClamCalendar.Layout;

// Builds the month model drawn by ClamCalendarView; also usable by the application to prepare month data itself
public sealed class CalendarMonthBuilder
{
    private const int DaysPerWeek = CalendarEventPlacer.DaysPerWeek;
    private const int FixedWeekCount = 6;

    public DayOfWeek FirstDayOfWeek { get; init; } = DayOfWeek.Monday;

    // Always six rows, or only the rows the month needs (four to six)
    public bool FixedWeekRows { get; init; } = true;

    // Draw the days of the neighboring months; when false they stay blank and their events and stamps are dropped
    public bool OutsideMonthDaysVisible { get; init; } = true;

    // Event rows per week; zero means unlimited, the rest is counted as hidden on each day
    public int MaxVisibleSlots
    {
        get;
        init
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            field = value;
        }
    }

    public CalendarDateRange GetDisplayRange(int year, int month)
    {
        var first = new DateOnly(year, month, 1);
        var start = AlignToWeekStart(first);
        return new CalendarDateRange(start, start.AddDays((GetWeekCount(start, first) * DaysPerWeek) - 1));
    }

    public CalendarMonth Build(int year, int month, DateOnly today, IEnumerable<CalendarEvent>? events = null, IEnumerable<CalendarStamp>? stamps = null, IEnumerable<DateOnly>? holidays = null, IReadOnlyDictionary<DateOnly, string>? labels = null)
    {
        var first = new DateOnly(year, month, 1);
        var last = first.AddDays(DateTime.DaysInMonth(year, month) - 1);
        var range = GetDisplayRange(year, month);
        var weekCount = range.Days / DaysPerWeek;
        var visible = OutsideMonthDaysVisible ? range : new CalendarDateRange(first, last);
        var holidaySet = holidays is null ? null : new HashSet<DateOnly>(holidays);
        var stampLookup = CreateStampLookup(stamps, visible);
        var placements = PlaceEvents(events, range, visible, weekCount);

        var weeks = new CalendarWeek[weekCount];
        for (var w = 0; w < weekCount; w++)
        {
            var weekStart = range.Start.AddDays(w * DaysPerWeek);
            var placement = placements[w];
            var days = new CalendarDay[DaysPerWeek];
            for (var d = 0; d < DaysPerWeek; d++)
            {
                var date = weekStart.AddDays(d);
                var inside = visible.Contains(date);
                days[d] = new CalendarDay
                {
                    Date = date,
                    IsCurrentMonth = (date >= first) && (date <= last),
                    IsToday = date == today,
                    Kind = DetermineKind(date, holidaySet),
                    Stamps = (stampLookup is not null) && stampLookup.TryGetValue(date, out var dayStamps) ? dayStamps : [],
                    Label = inside && (labels is not null) && labels.TryGetValue(date, out var label) ? label : null,
                    Events = placement.Events[d],
                    HiddenEventCount = placement.HiddenCounts[d]
                };
            }

            weeks[w] = new CalendarWeek
            {
                Days = days,
                Placements = placement.Placements,
                SlotCount = placement.SlotCount
            };
        }

        return new CalendarMonth
        {
            Year = year,
            Month = month,
            Today = today,
            Weeks = weeks,
            FirstDate = range.Start,
            LastDate = range.End
        };
    }

    private DateOnly AlignToWeekStart(DateOnly date)
    {
        var diff = ((int)date.DayOfWeek - (int)FirstDayOfWeek + DaysPerWeek) % DaysPerWeek;
        return date.AddDays(-diff);
    }

    private int GetWeekCount(DateOnly start, DateOnly first)
    {
        if (FixedWeekRows)
        {
            return FixedWeekCount;
        }

        var days = (first.DayNumber - start.DayNumber) + DateTime.DaysInMonth(first.Year, first.Month);
        return (days + DaysPerWeek - 1) / DaysPerWeek;
    }

    private static Dictionary<DateOnly, List<CalendarStamp>>? CreateStampLookup(IEnumerable<CalendarStamp>? stamps, CalendarDateRange visible)
    {
        if (stamps is null)
        {
            return null;
        }

        var lookup = new Dictionary<DateOnly, List<CalendarStamp>>();
        foreach (var stamp in stamps)
        {
            if (!visible.Contains(stamp.Date))
            {
                continue;
            }

            if (!lookup.TryGetValue(stamp.Date, out var list))
            {
                list = [];
                lookup[stamp.Date] = list;
            }

            list.Add(stamp);
        }

        return lookup;
    }

    private CalendarWeekPlacement[] PlaceEvents(IEnumerable<CalendarEvent>? events, CalendarDateRange range, CalendarDateRange visible, int weekCount)
    {
        var placements = new CalendarWeekPlacement[weekCount];
        if (events is null)
        {
            Array.Fill(placements, CalendarWeekPlacement.Empty);
            return placements;
        }

        var candidates = new List<CalendarEventCandidate>[weekCount];
        for (var w = 0; w < weekCount; w++)
        {
            candidates[w] = [];
        }

        foreach (var ev in events)
        {
            var eventStart = ev.StartDate;
            var eventEnd = ev.EndDate < ev.StartDate ? ev.StartDate : ev.EndDate;
            var clippedStart = eventStart < visible.Start ? visible.Start : eventStart;
            var clippedEnd = eventEnd > visible.End ? visible.End : eventEnd;
            if (clippedStart > clippedEnd)
            {
                continue;
            }

            var firstWeek = (clippedStart.DayNumber - range.Start.DayNumber) / DaysPerWeek;
            var lastWeek = (clippedEnd.DayNumber - range.Start.DayNumber) / DaysPerWeek;
            for (var w = firstWeek; w <= lastWeek; w++)
            {
                var weekStart = range.Start.AddDays(w * DaysPerWeek);
                var weekEnd = weekStart.AddDays(DaysPerWeek - 1);
                var start = clippedStart < weekStart ? weekStart : clippedStart;
                var end = clippedEnd > weekEnd ? weekEnd : clippedEnd;
                candidates[w].Add(new CalendarEventCandidate(ev, start.DayNumber - weekStart.DayNumber, end.DayNumber - weekStart.DayNumber, eventStart < weekStart, eventEnd > weekEnd));
            }
        }

        for (var w = 0; w < weekCount; w++)
        {
            placements[w] = CalendarEventPlacer.Place(candidates[w], MaxVisibleSlots);
        }

        return placements;
    }

    private static CalendarDayKind DetermineKind(DateOnly date, HashSet<DateOnly>? holidays)
    {
        if ((holidays is not null) && holidays.Contains(date))
        {
            return CalendarDayKind.Holiday;
        }

        return date.DayOfWeek switch
        {
            DayOfWeek.Sunday => CalendarDayKind.Sunday,
            DayOfWeek.Saturday => CalendarDayKind.Saturday,
            _ => CalendarDayKind.Weekday
        };
    }
}
