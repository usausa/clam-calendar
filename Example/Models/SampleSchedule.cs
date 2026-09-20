namespace Example.Models;

// Deterministic sample events and stamps for any date range
public static class SampleSchedule
{
    private static readonly Color DarkRed = Color.FromArgb("#8B1538");
    private static readonly Color HotPink = Color.FromArgb("#D81B60");
    private static readonly Color VividMagenta = Color.FromArgb("#C2185B");
    private static readonly Color Cyan = Color.FromArgb("#00ACC1");
    private static readonly Color Green = Color.FromArgb("#43A047");
    private static readonly Color GreenText = Color.FromArgb("#2E7D32");
    private static readonly Color PinkText = Color.FromArgb("#E91E63");
    private static readonly Color Yellow = Color.FromArgb("#FBC02D");
    private static readonly Color Orange = Color.FromArgb("#FB8C00");
    private static readonly Color Blue = Color.FromArgb("#1E88E5");
    private static readonly Color CyanText = Color.FromArgb("#00ACC1");
    private static readonly Color YellowText = Color.FromArgb("#F9A825");

    private static readonly (DayOfWeek Dow, string Title, Color Color, TimeSpan? Time, string? Glyph)[] WeeklyTemplates =
    [
        (DayOfWeek.Monday, "週間報告", GreenText, new TimeSpan(9, 30, 0), null),
        (DayOfWeek.Monday, "英会話", PinkText, new TimeSpan(19, 0, 0), null),
        (DayOfWeek.Wednesday, "サークル", YellowText, null, "🎸"),
        (DayOfWeek.Saturday, "水泳教室", CyanText, new TimeSpan(10, 0, 0), "🏊")
    ];

    private static readonly (int Day, string Title, Color Background)[] MonthlyTemplates =
    [
        (3, "燃えるゴミ", DarkRed),
        (10, "燃えるゴミ", DarkRed),
        (17, "燃えるゴミ", DarkRed),
        (24, "燃えるゴミ", DarkRed),
        (5, "○ジム", Cyan),
        (19, "○ジム", Cyan)
    ];

    private static readonly (int Day, string Title, int Span, CalendarEventStyle Style, Color Background, Color? Text)[] OccasionalTemplates =
    [
        (2, "ぶどう狩り", 1, CalendarEventStyle.Filled, VividMagenta, null),
        (6, "会社研修", 2, CalendarEventStyle.Filled, Green, null),
        (9, "温泉旅行", 1, CalendarEventStyle.Filled, Orange, null),
        (12, "大阪出張", 2, CalendarEventStyle.Text, Blue, Blue),
        (14, "友達泊まり", 3, CalendarEventStyle.Filled, HotPink, null),
        (21, "買い物", 1, CalendarEventStyle.Filled, Yellow, Colors.Black),
        (26, "海外出張", 4, CalendarEventStyle.Filled, Blue, null)
    ];

    private static readonly (int Day, string Glyph, CalendarStampPosition Position, float FontSize, float Opacity)[] StampTemplates =
    [
        (3, "🚩", CalendarStampPosition.TopRight, 22, 1),
        (8, "🐦", CalendarStampPosition.TopRight, 22, 1),
        (13, "✈️", CalendarStampPosition.Center, 26, 1),
        (16, "🐶", CalendarStampPosition.Center, 32, 0.9f),
        (20, "👛", CalendarStampPosition.TopCenter, 22, 1),
        (22, "🐼", CalendarStampPosition.TopLeft, 22, 1),
        (26, "🎏", CalendarStampPosition.TopCenter, 22, 1),
        (29, "🐈", CalendarStampPosition.TopRight, 24, 1)
    ];

    public static IReadOnlyList<CalendarEvent> CreateEvents(DateOnly start, DateOnly end)
    {
        var events = new List<CalendarEvent>();
        var index = 0;
        foreach (var (year, month) in EnumerateMonths(start, end))
        {
            var daysInMonth = DateTime.DaysInMonth(year, month);
            foreach (var (dow, title, color, time, glyph) in WeeklyTemplates)
            {
                for (var day = 1; day <= daysInMonth; day++)
                {
                    var date = new DateOnly(year, month, day);
                    if ((date.DayOfWeek == dow) && (date >= start) && (date <= end))
                    {
                        events.Add(new CalendarEvent { Key = $"w{index++:D4}", Title = title, StartDate = date, EndDate = date, Style = CalendarEventStyle.Text, TextColor = color, StartTime = time, LeadingGlyph = glyph });
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
                var (day, glyph, position, fontSize, opacity) = StampTemplates[(month + (i * 2)) % StampTemplates.Length];
                var date = new DateOnly(year, month, Math.Min(day, daysInMonth));
                if ((date >= start) && (date <= end))
                {
                    stamps.Add(new CalendarStamp { Key = $"s{index++:D4}", Date = date, Glyph = glyph, Position = position, FontSize = fontSize, Opacity = opacity });
                }
            }
        }

        return stamps;
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
