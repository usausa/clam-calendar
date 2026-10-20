namespace Example.Models;

// Deterministic sample events, stamps and day colors for any date range
public static class SampleSchedule
{
    private static readonly Color Burnable = Color.FromArgb("#8B1538");
    private static readonly Color Recyclable = Color.FromArgb("#00838F");
    private static readonly Color HotPink = Color.FromArgb("#D81B60");
    private static readonly Color VividMagenta = Color.FromArgb("#C2185B");
    private static readonly Color Cyan = Color.FromArgb("#00ACC1");
    private static readonly Color Green = Color.FromArgb("#43A047");
    private static readonly Color Leave = Color.FromArgb("#E65100");
    private static readonly Color PinkText = Color.FromArgb("#E91E63");
    private static readonly Color Yellow = Color.FromArgb("#FBC02D");
    private static readonly Color Purple = Color.FromArgb("#8E24AA");
    private static readonly Color Blue = Color.FromArgb("#1E88E5");

    private static readonly CalendarDayAppearance FullLeaveAppearance = new() { Background = Color.FromArgb("#FFE0B2") };

    private static readonly CalendarDayAppearance HalfLeaveAppearance = new() { Background = Color.FromArgb("#FFF0DA") };

    // Garbage collection and a weekly lesson; the titles are short so they are not cut off in a cell
    private static readonly (DayOfWeek Dow, string Title, Color Background, Color? Text, TimeSpan? Time, string? Glyph)[] WeeklyTemplates =
    [
        (DayOfWeek.Monday, "可燃", Burnable, null, null, null),
        (DayOfWeek.Thursday, "可燃", Burnable, null, null, null),
        (DayOfWeek.Friday, "資源", Recyclable, null, null, null),
        (DayOfWeek.Wednesday, "英語", Colors.Transparent, PinkText, new TimeSpan(19, 0, 0), null)
    ];

    private static readonly (int Day, string Title, Color Background)[] MonthlyTemplates =
    [
        (5, "ジム", Cyan),
        (19, "ジム", Cyan)
    ];

    // Paid leave; the day also gets its own background through DayAppearances
    private static readonly (int Day, string Title, bool Half)[] LeaveTemplates =
    [
        (8, "年休", false),
        (22, "半休", true)
    ];

    private static readonly (int Day, string Title, int Span, CalendarEventStyle Style, Color Background, Color? Text)[] OccasionalTemplates =
    [
        (2, "帰省", 2, CalendarEventStyle.Filled, VividMagenta, null),
        (6, "研修", 2, CalendarEventStyle.Filled, Green, null),
        (9, "旅行", 1, CalendarEventStyle.Filled, Purple, null),
        (12, "出張", 2, CalendarEventStyle.Text, Blue, Blue),
        (14, "来客", 3, CalendarEventStyle.Filled, HotPink, null),
        (21, "買物", 1, CalendarEventStyle.Filled, Yellow, Colors.Black),
        (26, "海外出張", 4, CalendarEventStyle.Filled, Blue, null)
    ];

    private static readonly (int Day, string Glyph, CalendarStampPosition Position, double FontSize, Color? Color, double Opacity)[] StampTemplates =
    [
        (3, "🚩", CalendarStampPosition.TopRight, 22, null, 1),
        (8, "🐦", CalendarStampPosition.TopRight, 22, null, 1),
        (11, "●", CalendarStampPosition.BottomLeft, 12, Colors.OrangeRed, 1),
        (13, "✈️", CalendarStampPosition.Center, 26, null, 1),
        (16, "🐶", CalendarStampPosition.Center, 32, null, 0.9),
        (20, "👛", CalendarStampPosition.TopCenter, 22, null, 1),
        (22, "🐼", CalendarStampPosition.TopLeft, 22, null, 1),
        (24, "▲", CalendarStampPosition.BottomRight, 12, Colors.SeaGreen, 1),
        (26, "🎏", CalendarStampPosition.TopCenter, 22, null, 1),
        (29, "🐈", CalendarStampPosition.TopRight, 24, null, 1)
    ];

    public static IReadOnlyList<CalendarEvent> CreateEvents(DateOnly start, DateOnly end)
    {
        var events = new List<CalendarEvent>();
        var index = 0;
        foreach (var (year, month) in EnumerateMonths(start, end))
        {
            var daysInMonth = DateTime.DaysInMonth(year, month);
            foreach (var (dow, title, background, text, time, glyph) in WeeklyTemplates)
            {
                for (var day = 1; day <= daysInMonth; day++)
                {
                    var date = new DateOnly(year, month, day);
                    if ((date.DayOfWeek == dow) && (date >= start) && (date <= end))
                    {
                        events.Add(text is null
                            ? new CalendarEvent { Key = $"w{index++:D4}", Title = title, StartDate = date, EndDate = date, BackgroundColor = background, StartTime = time, LeadingGlyph = glyph }
                            : new CalendarEvent { Key = $"w{index++:D4}", Title = title, StartDate = date, EndDate = date, Style = CalendarEventStyle.Text, TextColor = text, StartTime = time, LeadingGlyph = glyph });
                    }
                }
            }

            foreach (var (day, title, background) in MonthlyTemplates)
            {
                var date = new DateOnly(year, month, Math.Min(day, daysInMonth));
                if ((date >= start) && (date <= end))
                {
                    events.Add(new CalendarEvent { Key = $"m{index++:D4}", Title = title, StartDate = date, EndDate = date, BackgroundColor = background });
                }
            }

            foreach (var (date, title, _) in EnumerateLeaves(year, month))
            {
                if ((date >= start) && (date <= end))
                {
                    events.Add(new CalendarEvent { Key = $"l{index++:D4}", Title = title, StartDate = date, EndDate = date, BackgroundColor = Leave });
                }
            }

            var pickCount = 3 + (month % 3);
            for (var i = 0; i < pickCount; i++)
            {
                var (day, title, span, style, background, text) = OccasionalTemplates[(month + (i * 3)) % OccasionalTemplates.Length];
                var eventStart = new DateOnly(year, month, Math.Min(day, daysInMonth));
                var eventEnd = eventStart.AddDays(span - 1);
                if (eventEnd.Month != month)
                {
                    eventEnd = new DateOnly(year, month, daysInMonth);
                }

                if ((eventStart <= end) && (eventEnd >= start))
                {
                    events.Add(new CalendarEvent { Key = $"o{index++:D4}", Title = title, StartDate = eventStart, EndDate = eventEnd, Style = style, BackgroundColor = background, TextColor = text ?? Colors.White });
                }
            }
        }

        return events.OrderBy(static x => x.StartDate).ToArray();
    }

    public static IReadOnlyDictionary<DateOnly, CalendarDayAppearance> CreateDayAppearances(DateOnly start, DateOnly end)
    {
        var appearances = new Dictionary<DateOnly, CalendarDayAppearance>();
        foreach (var (year, month) in EnumerateMonths(start, end))
        {
            foreach (var (date, _, half) in EnumerateLeaves(year, month))
            {
                if ((date >= start) && (date <= end))
                {
                    appearances[date] = half ? HalfLeaveAppearance : FullLeaveAppearance;
                }
            }
        }

        return appearances;
    }

    public static IReadOnlyList<CalendarStamp> CreateStamps(DateOnly start, DateOnly end)
    {
        var stamps = new List<CalendarStamp>();
        var index = 0;
        foreach (var (year, month) in EnumerateMonths(start, end))
        {
            var daysInMonth = DateTime.DaysInMonth(year, month);
            var pickCount = 4 + (month % 4);
            for (var i = 0; i < pickCount; i++)
            {
                var (day, glyph, position, fontSize, color, opacity) = StampTemplates[(month + (i * 2)) % StampTemplates.Length];
                var date = new DateOnly(year, month, Math.Min(day, daysInMonth));
                if ((date >= start) && (date <= end))
                {
                    stamps.Add(new CalendarStamp { Key = $"s{index++:D4}", Date = date, Glyph = glyph, Position = position, FontSize = fontSize, Color = color, Opacity = opacity });
                }
            }
        }

        return stamps;
    }

    // Paid leave is taken on a working day, so a weekend or a holiday moves it to the next one
    private static IEnumerable<(DateOnly Date, string Title, bool Half)> EnumerateLeaves(int year, int month)
    {
        var daysInMonth = DateTime.DaysInMonth(year, month);
        var holidays = new HashSet<DateOnly>(SampleHolidays.GetHolidays(new DateOnly(year, month, 1), new DateOnly(year, month, daysInMonth)));
        foreach (var (day, title, half) in LeaveTemplates)
        {
            var date = new DateOnly(year, month, Math.Min(day, daysInMonth));
            while ((date.Day < daysInMonth) && ((date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday) || holidays.Contains(date)))
            {
                date = date.AddDays(1);
            }

            yield return (date, title, half);
        }
    }

    private static IEnumerable<(int Year, int Month)> EnumerateMonths(DateOnly start, DateOnly end)
    {
        var current = new DateOnly(start.Year, start.Month, 1);
        var last = new DateOnly(end.Year, end.Month, 1);
        while (current <= last)
        {
            yield return (current.Year, current.Month);
            current = current.AddMonths(1);
        }
    }
}
