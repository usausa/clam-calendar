namespace Example.Models;

// Availability and rates of a hotel used by the Booking screen
public static class SampleBooking
{
    public const int DayUseRate = 5000;

    private const int WeekdayRate = 12000;

    private const int WeekendRate = 15000;

    // Sold-out days spread over the bookable period in a fixed pattern
    public static IReadOnlyList<DateOnly> GetSoldOutDates(DateOnly start, DateOnly end)
    {
        var dates = new List<DateOnly>();
        for (var date = start; date <= end; date = date.AddDays(1))
        {
            if (IsSoldOut(date))
            {
                dates.Add(date);
            }
        }

        return dates;
    }

    public static bool IsSoldOut(DateOnly date) =>
        (date.DayNumber % 9 == 4) || (date.DayNumber % 13 == 6);

    // Rate of the night that starts on the date
    public static int GetRate(DateOnly night) =>
        night.DayOfWeek is DayOfWeek.Friday or DayOfWeek.Saturday ? WeekendRate : WeekdayRate;

    // Total of the nights from the check-in to the night before the check-out
    public static int GetStayTotal(DateOnly checkIn, DateOnly checkOut)
    {
        var total = 0;
        for (var night = checkIn; night < checkOut; night = night.AddDays(1))
        {
            total += GetRate(night);
        }

        return total;
    }
}
