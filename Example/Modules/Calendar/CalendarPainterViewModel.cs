namespace Example.Modules.Calendar;

using SkiaExtension;

public sealed partial class CalendarPainterViewModel : AppViewModelBase
{
    private const float BadgeRadius = 9;

    private const float BadgeFontSize = 11;

    private const float BarHeight = 3;

    private static readonly SKColor BadgeColor = new(0xE8, 0x71, 0x0A);

    private static readonly SKColor BarColor = new(0x1A, 0x73, 0xE8);

    private static readonly string[] CountTexts = ["0", "1", "2", "3", "4", "5", "6", "7", "8", "9"];

    private readonly FontResolver fonts = new("sans-serif");

    private readonly SKPaint paint = new() { IsAntialias = true };

    [ObservableProperty]
    public partial DateOnly DisplayDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [ObservableProperty]
    public partial IReadOnlyList<CalendarEvent> Events { get; set; } = [];

    [ObservableProperty]
    public partial IReadOnlyList<DateOnly> Holidays { get; set; } = [];

    [ObservableProperty]
    public partial IReadOnlyDictionary<DateOnly, CalendarDayAppearance>? DayAppearances { get; set; }

    [ObservableProperty]
    public partial bool IsSettingOpen { get; set; }

    [ObservableProperty]
    public partial bool AnimationEnabled { get; set; } = true;

    [ObservableProperty]
    public partial string Statistics { get; set; } = String.Empty;

    public IObserveCommand DisplayDateChangedCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public CalendarPainterViewModel()
    {
        DisplayDateChangedCommand = MakeDelegateCommand<CalendarDisplayDateChangedEventArgs>(CommandMode.Simple, Load);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            paint.Dispose();
            fonts.Dispose();
        }
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    protected override Task OnNotifyBackAsync()
    {
        if (IsSettingOpen)
        {
            IsSettingOpen = false;
            return Task.CompletedTask;
        }

        return Navigator.ForwardAsync(ViewId.CalendarMenu);
    }

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction2()
    {
        IsSettingOpen = !IsSettingOpen;
        return Task.CompletedTask;
    }

    protected override Task OnNotifyFunction4()
    {
        DisplayDate = DateOnly.FromDateTime(DateTime.Today);
        return Task.CompletedTask;
    }

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    public void UpdateStatistics(CalendarFrameEventArgs e) =>
        Statistics = $"{e.ElapsedMilliseconds:F2}ms / {e.DayCount} days / {e.TextMeasurements} measures{(e.Animating ? " / sliding" : String.Empty)}";

    public void PaintDay(SKCanvas canvas, CalendarDayPaintContext context)
    {
        if (context.Day.IsToday)
        {
            paint.Color = BarColor;
            canvas.DrawRect(context.DateNumberBounds.Left, context.DateNumberBounds.Bottom - BarHeight, context.DateNumberBounds.Width, BarHeight, paint);
        }

        var count = context.Day.Events.Count;
        if (count == 0)
        {
            return;
        }

        var x = context.Bounds.Right - BadgeRadius - 2;
        var y = context.Bounds.Top + BadgeRadius + 2;
        paint.Color = BadgeColor;
        canvas.DrawCircle(x, y, BadgeRadius, paint);

        paint.Color = SKColors.White;
        canvas.DrawText(fonts.CreateText(count < CountTexts.Length ? CountTexts[count] : "9+", BadgeFontSize, true), x, fonts.GetMetrics(BadgeFontSize, true).GetCenterBaseline(y), SKTextAlign.Center, paint);
    }

    private void Load(CalendarDisplayDateChangedEventArgs e)
    {
        Events = SampleSchedule.CreateEvents(e.FirstDate, e.LastDate);
        Holidays = SampleHolidays.GetHolidays(e.FirstDate, e.LastDate);
        DayAppearances = SampleSchedule.CreateDayAppearances(e.FirstDate, e.LastDate);
    }
}
