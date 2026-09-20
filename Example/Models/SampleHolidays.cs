namespace Example.Models;

// Japanese public holidays with their names, computed for any year
public static class SampleHolidays
{
    public static IReadOnlyList<DateOnly> GetHolidays(DateOnly start, DateOnly end) =>
        GetLabels(start, end).Keys.Order().ToArray();

    public static IReadOnlyDictionary<DateOnly, string> GetLabels(DateOnly start, DateOnly end)
    {
        var labels = new Dictionary<DateOnly, string>();
        for (var year = start.Year; year <= end.Year; year++)
        {
            foreach (var (date, name) in GetYearHolidays(year))
            {
                if ((date >= start) && (date <= end))
                {
                    labels[date] = name;
                }
            }
        }

        return labels;
    }

    private static Dictionary<DateOnly, string> GetYearHolidays(int year)
    {
        var holidays = new Dictionary<DateOnly, string>
        {
            [new DateOnly(year, 1, 1)] = "元日",
            [NthWeekday(year, 1, DayOfWeek.Monday, 2)] = "成人の日",
            [new DateOnly(year, 2, 11)] = "建国記念の日",
            [new DateOnly(year, 3, SpringEquinox(year))] = "春分の日",
            [new DateOnly(year, 4, 29)] = "昭和の日",
            [new DateOnly(year, 5, 3)] = "憲法記念日",
            [new DateOnly(year, 5, 4)] = "みどりの日",
            [new DateOnly(year, 5, 5)] = "こどもの日",
            [NthWeekday(year, 7, DayOfWeek.Monday, 3)] = "海の日",
            [NthWeekday(year, 9, DayOfWeek.Monday, 3)] = "敬老の日",
            [new DateOnly(year, 9, AutumnEquinox(year))] = "秋分の日",
            [NthWeekday(year, 10, DayOfWeek.Monday, 2)] = "スポーツの日",
            [new DateOnly(year, 11, 3)] = "文化の日",
            [new DateOnly(year, 11, 23)] = "勤労感謝の日"
        };
        if (year >= 2020)
        {
            holidays[new DateOnly(year, 2, 23)] = "天皇誕生日";
        }

        if (year >= 2016)
        {
            holidays[new DateOnly(year, 8, 11)] = "山の日";
        }

        // Substitute holidays for the ones on a Sunday
        foreach (var date in holidays.Keys.Where(static x => x.DayOfWeek == DayOfWeek.Sunday).ToArray())
        {
            var substitute = date.AddDays(1);
            while (holidays.ContainsKey(substitute))
            {
                substitute = substitute.AddDays(1);
            }

            holidays[substitute] = "振替休日";
        }

        // A weekday between two holidays becomes a holiday
        foreach (var date in holidays.Keys.ToArray())
        {
            var candidate = date.AddDays(1);
            if (!holidays.ContainsKey(candidate) && (candidate.DayOfWeek != DayOfWeek.Sunday) && (candidate.DayOfWeek != DayOfWeek.Saturday) && holidays.ContainsKey(candidate.AddDays(1)))
            {
                holidays[candidate] = "国民の休日";
            }
        }

        return holidays;
    }

    private static DateOnly NthWeekday(int year, int month, DayOfWeek dayOfWeek, int n)
    {
        var first = new DateOnly(year, month, 1);
        var offset = ((int)dayOfWeek - (int)first.DayOfWeek + 7) % 7;
        return first.AddDays(offset + ((n - 1) * 7));
    }

    private static int SpringEquinox(int year)
    {
        var x = year - 1980;
        return (int)(20.69115 + (0.242194 * x) - Math.Floor(x / 4.0));
    }

    private static int AutumnEquinox(int year)
    {
        var x = year - 1980;
        return (int)(23.09 + (0.242194 * x) - Math.Floor(x / 4.0));
    }
}
