namespace ClamCalendar.Tests.Rendering;

using SkiaSharp;

public sealed class CalendarRendererTests
{
    private const int Width = 350;
    private const int Height = 600;

    private static readonly DateOnly Today = new(2026, 9, 3);

    [Fact]
    public void TitleFollowsTheCultureOrTheFormat()
    {
        // Arrange
        var month = new CalendarMonthBuilder().Build(2026, 9, Today);

        // Act & Assert
        Assert.Equal("September 2026", CalendarRenderer.FormatTitle(month, CultureInfo.GetCultureInfo("en-US"), null));
        Assert.Equal("2026年9月", CalendarRenderer.FormatTitle(month, CultureInfo.GetCultureInfo("ja-JP"), null));
        Assert.Equal("2026/09", CalendarRenderer.FormatTitle(month, CultureInfo.GetCultureInfo("ja-JP"), "yyyy/MM"));
    }

    [Fact]
    public void WeekdayNamesFollowTheFormat()
    {
        // Arrange
        var english = CultureInfo.GetCultureInfo("en-US");
        var japanese = CultureInfo.GetCultureInfo("ja-JP");

        // Act & Assert
        Assert.Equal("M", CalendarRenderer.FormatWeekdayName(DayOfWeek.Monday, english, CalendarWeekdayNameFormat.Initial));
        Assert.Equal("Mon", CalendarRenderer.FormatWeekdayName(DayOfWeek.Monday, english, CalendarWeekdayNameFormat.Abbreviated));
        Assert.Equal("Monday", CalendarRenderer.FormatWeekdayName(DayOfWeek.Monday, english, CalendarWeekdayNameFormat.Full));
        Assert.Equal("月", CalendarRenderer.FormatWeekdayName(DayOfWeek.Monday, japanese, CalendarWeekdayNameFormat.Initial));
        Assert.Equal("月曜日", CalendarRenderer.FormatWeekdayName(DayOfWeek.Monday, japanese, CalendarWeekdayNameFormat.Full));
    }

    [Fact]
    public void EventTextCombinesTimeGlyphAndTitle()
    {
        // Arrange
        var plain = new CalendarEvent { Title = "Title", StartDate = Today, EndDate = Today };
        var timed = new CalendarEvent { Title = "Title", StartDate = Today, EndDate = Today, StartTime = new TimeSpan(9, 5, 0), LeadingGlyph = "🎂" };
        var glyph = new CalendarEvent { Title = "Title", StartDate = Today, EndDate = Today, LeadingGlyph = "★" };

        // Act & Assert
        Assert.Equal("Title", CalendarRenderer.FormatEventText(plain));
        Assert.Equal("9:05 🎂 Title", CalendarRenderer.FormatEventText(timed));
        Assert.Equal("★ Title", CalendarRenderer.FormatEventText(glyph));
    }

    [Fact]
    public void RenderDrawsEveryFeatureWithoutError()
    {
        // Arrange
        var events = new[]
        {
            new CalendarEvent { Title = "Filled 🍎", StartDate = new DateOnly(2026, 9, 5), EndDate = new DateOnly(2026, 9, 8), BackgroundColor = Colors.Blue },
            new CalendarEvent { Title = "Text", StartDate = new DateOnly(2026, 9, 10), EndDate = new DateOnly(2026, 9, 10), Style = CalendarEventStyle.Text, TextColor = Colors.Green, StartTime = TimeSpan.FromHours(10) },
            new CalendarEvent { Title = "Hidden 1", StartDate = new DateOnly(2026, 9, 10), EndDate = new DateOnly(2026, 9, 10) },
            new CalendarEvent { Title = "Hidden 2", StartDate = new DateOnly(2026, 9, 10), EndDate = new DateOnly(2026, 9, 10) }
        };
        var stamps = new[] { new CalendarStamp { Date = new DateOnly(2026, 9, 12), Glyph = "🐈", Position = CalendarStampPosition.TopRight }, new CalendarStamp { Date = new DateOnly(2026, 9, 13), Glyph = "✈️", Opacity = 0.5f } };
        var labels = new Dictionary<DateOnly, string> { [new DateOnly(2026, 9, 21)] = "敬老の日" };
        var month = new CalendarMonthBuilder { MaxVisibleSlots = 1, OutsideMonthDaysVisible = false }.Build(2026, 9, Today, events, stamps, [new DateOnly(2026, 9, 21)], labels);
        using var renderer = new CalendarRenderer(new CalendarStyle());
        var state = new CalendarRenderState
        {
            Culture = CultureInfo.GetCultureInfo("ja-JP"),
            MonthIndicatorVisible = true,
            OutsideMonthDaysVisible = false,
            WeekdayNameFormat = CalendarWeekdayNameFormat.Abbreviated,
            IsSelected = static date => date == new DateOnly(2026, 9, 5),
            IsInRange = static date => (date >= new DateOnly(2026, 9, 5)) && (date <= new DateOnly(2026, 9, 9)),
            IsDisabled = static date => date < new DateOnly(2026, 9, 2),
            SlideOffset = 12,
            SlideOpacity = 0.7f
        };

        // Act
        using var surface = SKSurface.Create(new SKImageInfo(Width, Height));
        renderer.Render(surface.Canvas, new CalendarLayout(month, new CalendarStyle(), Width, Height, todayButtonVisible: true), state);
        renderer.Render(surface.Canvas, new CalendarLayout(month, new CalendarStyle(), Width, Height, headerVisible: false, weekdayHeaderVisible: false), new CalendarRenderState());
        renderer.Render(surface.Canvas, new CalendarLayout(month, new CalendarStyle(), 0, 0), new CalendarRenderState());

        // Assert
        Assert.True(renderer.Measurements > 0);
    }

    [Fact]
    public void CellBackgroundsFollowTheDayKindAndTheSelection()
    {
        // Arrange
        var style = new CalendarStyle();
        var month = new CalendarMonthBuilder().Build(2026, 9, Today, holidays: [new DateOnly(2026, 9, 21)]);
        var layout = new CalendarLayout(month, style, Width, Height);
        using var renderer = new CalendarRenderer(style);
        var state = new CalendarRenderState { IsSelected = static date => date == new DateOnly(2026, 9, 5), IsInRange = static date => date == new DateOnly(2026, 9, 16) };

        // Act
        using var surface = SKSurface.Create(new SKImageInfo(Width, Height));
        renderer.Render(surface.Canvas, layout, state);
        using var image = surface.Snapshot();
        using var bitmap = SKBitmap.FromImage(image);

        // Assert
        Assert.Equal(ToSkColor(style.HeaderBackground), Pixel(bitmap, layout.TitleBounds.X + 2, layout.TitleBounds.Y + 2));
        Assert.Equal(ToSkColor(style.WeekendBackground), CellPixel(bitmap, layout, 1, 5));
        Assert.Equal(ToSkColor(style.WeekendBackground), CellPixel(bitmap, layout, 1, 6));
        Assert.Equal(ToSkColor(style.HolidayBackground), CellPixel(bitmap, layout, 3, 0));
        Assert.Equal(ToSkColor(style.OutsideMonthBackground), CellPixel(bitmap, layout, 0, 0));
        Assert.Equal(ToSkColor(style.Background), CellPixel(bitmap, layout, 1, 1));
        Assert.Equal(ToSkColor(style.RangeBackground), Pixel(bitmap, layout.GetDateRowBounds(2, 2).Right - 2, layout.GetDateRowBounds(2, 2).Y + 2));
        var selected = layout.GetDateNumberBounds(0, 5);
        Assert.Equal(ToSkColor(style.SelectedDayBackground), Pixel(bitmap, selected.X + 3, selected.CenterY));
        var today = layout.GetDateNumberBounds(0, 3);
        Assert.Equal(ToSkColor(style.TodayBackground), Pixel(bitmap, today.X + 3, today.CenterY));
    }

    private static SKColor CellPixel(SKBitmap bitmap, CalendarLayout layout, int week, int column)
    {
        var cell = layout.GetCellBounds(week, column);
        return Pixel(bitmap, cell.Right - 3, cell.Bottom - 3);
    }

    private static SKColor Pixel(SKBitmap bitmap, double x, double y) => bitmap.GetPixel((int)x, (int)y);

    private static SKColor ToSkColor(Color color) => new((byte)(color.Red * 255), (byte)(color.Green * 255), (byte)(color.Blue * 255), (byte)(color.Alpha * 255));
}
