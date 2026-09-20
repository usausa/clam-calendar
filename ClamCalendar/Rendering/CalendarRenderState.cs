namespace ClamCalendar.Rendering;

using System.Globalization;

// Everything the renderer needs besides the layout: texts, culture, selection callbacks and the month slide animation
internal sealed class CalendarRenderState
{
    public CultureInfo Culture { get; init; } = CultureInfo.CurrentCulture;

    public string? HeaderFormat { get; init; }

    public CalendarWeekdayNameFormat WeekdayNameFormat { get; init; }

    public string PrevButtonText { get; init; } = "◀";

    public string NextButtonText { get; init; } = "▶";

    public string TodayButtonText { get; init; } = "Today";

    public bool CanNavigateBackward { get; init; } = true;

    public bool CanNavigateForward { get; init; } = true;

    public bool MonthIndicatorVisible { get; init; }

    public bool OutsideMonthDaysVisible { get; init; } = true;

    public Func<DateOnly, bool>? IsSelected { get; init; }

    // Dates of a selected range, both ends included
    public Func<DateOnly, bool>? IsInRange { get; init; }

    public Func<DateOnly, bool>? IsDisabled { get; init; }

    // Horizontal offset of the body in DIP and its opacity while the month slides in
    public float SlideOffset { get; init; }

    public float SlideOpacity { get; init; } = 1;
}
