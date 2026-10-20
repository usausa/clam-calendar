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
        Assert.Equal("September 2026", CalendarRenderer.FormatTitle(month, CultureInfo.GetCultureInfo("en-US"), CalendarTextFormat.Default));
        Assert.Equal("2026年9月", CalendarRenderer.FormatTitle(month, CultureInfo.GetCultureInfo("ja-JP"), CalendarTextFormat.Default));
        Assert.Equal("2026/09", CalendarRenderer.FormatTitle(month, CultureInfo.GetCultureInfo("ja-JP"), CalendarTextFormat.Parse("yyyy/MM")));
        Assert.Equal("2026年9月の予定", CalendarRenderer.FormatTitle(month, CultureInfo.GetCultureInfo("ja-JP"), CalendarTextFormat.Parse("{0:yyyy年M月}の予定")));
        Assert.Equal("Plan for September", CalendarRenderer.FormatTitle(month, CultureInfo.GetCultureInfo("en-US"), CalendarTextFormat.Parse("Plan for {0:MMMM}")));
    }

    [Fact]
    public void HeaderFormatIsParsedOnceUntilItChanges()
    {
        // Arrange
        using var view = new ClamCalendarView();
        view.HeaderFormat = "yyyy/MM";

        // Act
        var first = view.CreateRenderState().HeaderFormat;
        var second = view.CreateRenderState().HeaderFormat;
        view.HeaderFormat = "{0:yyyy/MM} plan";
        var changed = view.CreateRenderState().HeaderFormat;

        // Assert
        Assert.Same(first, second);
        Assert.NotSame(first, changed);
        Assert.Equal("2026/09 plan", changed.Format(new DateTime(2026, 9, 1), CultureInfo.InvariantCulture));
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
    public void WeekNumberFollowsTheThursdayOfTheRow()
    {
        // Arrange
        var monday = new CalendarMonthBuilder().Build(2026, 9, Today);
        var sunday = new CalendarMonthBuilder { FirstDayOfWeek = DayOfWeek.Sunday }.Build(2026, 9, Today);

        // Act & Assert
        Assert.Equal(36, CalendarRenderer.GetWeekNumber(monday.Weeks[0]));
        Assert.Equal(36, CalendarRenderer.GetWeekNumber(sunday.Weeks[0]));
        Assert.Equal(37, CalendarRenderer.GetWeekNumber(monday.Weeks[1]));
        Assert.Equal(41, CalendarRenderer.GetWeekNumber(monday.Weeks[5]));
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

    [Fact]
    public void StampsUseTheStyleColorUntilOneIsGiven()
    {
        // Arrange
        var style = new CalendarStyle { StampColor = Colors.Blue };
        var stamps = new[]
        {
            new CalendarStamp { Date = new DateOnly(2026, 9, 7), Glyph = "■" },
            new CalendarStamp { Date = new DateOnly(2026, 9, 8), Glyph = "■", Color = Colors.Red }
        };
        var month = new CalendarMonthBuilder().Build(2026, 9, Today, stamps: stamps);
        var layout = new CalendarLayout(month, style, Width, Height);
        using var renderer = new CalendarRenderer(style);

        // Act
        using var surface = SKSurface.Create(new SKImageInfo(Width, Height));
        renderer.Render(surface.Canvas, layout, new CalendarRenderState());
        using var image = surface.Snapshot();
        using var bitmap = SKBitmap.FromImage(image);

        // Assert
        Assert.True(ContainsPixel(bitmap, layout.GetCellBounds(1, 0), ToSkColor(Colors.Blue)));
        Assert.True(ContainsPixel(bitmap, layout.GetCellBounds(1, 1), ToSkColor(Colors.Red)));
    }

    [Fact]
    public void DayAppearanceOverridesTheColorsOfTheCell()
    {
        // Arrange
        var appearance = new CalendarDayAppearance
        {
            Background = Colors.Lime,
            DateNumberColor = Colors.Magenta,
            DateNumberBackground = Colors.Navy,
            LabelColor = Colors.Orange,
            BorderColor = Colors.Teal,
            BorderWidth = 3
        };
        var date = new DateOnly(2026, 9, 10);
        var style = new CalendarStyle { DayLabelFontSize = 16 };
        var labels = new Dictionary<DateOnly, string> { [date] = "■" };
        var appearances = new Dictionary<DateOnly, CalendarDayAppearance> { [date] = appearance };
        var month = new CalendarMonthBuilder().Build(2026, 9, Today, labels: labels, appearances: appearances);
        var layout = new CalendarLayout(month, style, Width, Height);
        using var renderer = new CalendarRenderer(style);

        // Act
        using var surface = SKSurface.Create(new SKImageInfo(Width, Height));
        renderer.Render(surface.Canvas, layout, new CalendarRenderState { OutsideMonthDaysVisible = true });
        using var image = surface.Snapshot();
        using var bitmap = SKBitmap.FromImage(image);

        // Assert
        var cell = layout.GetCellBounds(1, 3);
        Assert.Equal(ToSkColor(Colors.Lime), Pixel(bitmap, cell.X + 10, cell.Bottom - 10));
        Assert.True(ContainsPixel(bitmap, cell, ToSkColor(Colors.Navy)));
        Assert.True(ContainsPixel(bitmap, cell, ToSkColor(Colors.Magenta)));
        Assert.True(ContainsPixel(bitmap, cell, ToSkColor(Colors.Orange)));
        Assert.True(ContainsPixel(bitmap, cell, ToSkColor(Colors.Teal)));
        Assert.Equal(ToSkColor(style.Background), CellPixel(bitmap, layout, 1, 2));
    }

    [Fact]
    public void GridLineModeChoosesTheLinesOfTheBodyAndKeepsTheHeaderSeparator()
    {
        // Arrange
        const int height = 620;
        var defaults = new CalendarStyle();
        var line = ToSkColor(defaults.GridLineColor);
        var background = ToSkColor(defaults.Background);
        var month = new CalendarMonthBuilder().Build(2026, 9, Today);
        var modes = new[]
        {
            (Mode: CalendarGridLineMode.All, Horizontal: true, Vertical: true),
            (Mode: CalendarGridLineMode.Horizontal, Horizontal: true, Vertical: false),
            (Mode: CalendarGridLineMode.Vertical, Horizontal: false, Vertical: true),
            (Mode: CalendarGridLineMode.None, Horizontal: false, Vertical: false)
        };

        // Act & Assert
        foreach (var (mode, horizontal, vertical) in modes)
        {
            using var bitmap = Render(month, new CalendarStyle { GridLineMode = mode, GridLineWidth = 2 }, new CalendarRenderState(), Width, height);
            Assert.Equal(horizontal ? line : background, Pixel(bitmap, 25, 170));
            Assert.Equal(vertical ? line : background, Pixel(bitmap, 49, 220));
            Assert.Equal(line, Pixel(bitmap, 25, 79));
        }
    }

    [Fact]
    public void DateNumberMarkFollowsTheMarkStyleAndTheShape()
    {
        // Arrange
        const int height = 620;
        var defaults = new CalendarStyle();
        var todayColor = ToSkColor(defaults.TodayBackground);
        var selectedColor = ToSkColor(defaults.SelectedDayBackground);
        var background = ToSkColor(defaults.Background);
        var month = new CalendarMonthBuilder().Build(2026, 9, Today);
        var selectedState = new CalendarRenderState { IsSelected = static date => date == Today };
        var bubble = new CalendarLayout(month, new CalendarStyle { DateNumberSize = 40 }, Width, height).GetDateNumberBounds(0, 3);

        // Act & Assert
        using (var bitmap = Render(month, new CalendarStyle { DateNumberSize = 40 }, new CalendarRenderState(), Width, height))
        {
            Assert.Equal(todayColor, Pixel(bitmap, bubble.X + 4, bubble.CenterY));
            Assert.Equal(todayColor, Pixel(bitmap, bubble.X + 4, bubble.Y + 4));
        }

        using (var bitmap = Render(month, new CalendarStyle { DateNumberSize = 40, DateNumberShape = CalendarDateNumberShape.Circle }, new CalendarRenderState(), Width, height))
        {
            Assert.Equal(todayColor, Pixel(bitmap, bubble.X + 4, bubble.CenterY));
            Assert.Equal(background, Pixel(bitmap, bubble.X + 4, bubble.Y + 4));
        }

        using (var bitmap = Render(month, new CalendarStyle { DateNumberSize = 40, TodayMarkStyle = CalendarMarkStyle.Outline }, new CalendarRenderState(), Width, height))
        {
            Assert.Equal(background, Pixel(bitmap, bubble.X + 4, bubble.CenterY));
            Assert.True(ContainsPixel(bitmap, bubble, todayColor));
        }

        using (var bitmap = Render(month, new CalendarStyle { DateNumberSize = 40, TodayMarkStyle = CalendarMarkStyle.Underline }, new CalendarRenderState(), Width, height))
        {
            Assert.Equal(background, Pixel(bitmap, bubble.X + 4, bubble.CenterY));
            Assert.Equal(todayColor, Pixel(bitmap, bubble.CenterX, bubble.Bottom - 1));
        }

        using (var bitmap = Render(month, new CalendarStyle { DateNumberSize = 40, TodayMarkStyle = CalendarMarkStyle.None }, new CalendarRenderState(), Width, height))
        {
            Assert.False(ContainsPixel(bitmap, bubble, todayColor));
        }

        using (var bitmap = Render(month, new CalendarStyle { DateNumberSize = 40, TodayMarkStyle = CalendarMarkStyle.Outline }, selectedState, Width, height))
        {
            Assert.Equal(selectedColor, Pixel(bitmap, bubble.X + 4, bubble.CenterY));
            Assert.False(ContainsPixel(bitmap, bubble, todayColor));
        }

        using (var bitmap = Render(month, new CalendarStyle { DateNumberSize = 40, TodayMarkStyle = CalendarMarkStyle.Outline, TodayMarkOnSelected = true }, selectedState, Width, height))
        {
            Assert.Equal(selectedColor, Pixel(bitmap, bubble.X + 4, bubble.CenterY));
            Assert.True(ContainsPixel(bitmap, bubble, todayColor));
        }
    }

    [Fact]
    public void EventDotsReplaceTheBarsAndStopAtTheLimit()
    {
        // Arrange
        const int height = 620;
        var date = new DateOnly(2026, 9, 10);
        var events = new[]
        {
            new CalendarEvent { Title = "a", StartDate = date, EndDate = date, BackgroundColor = Colors.Red },
            new CalendarEvent { Title = "b", StartDate = date, EndDate = date, Style = CalendarEventStyle.Text, TextColor = Colors.Lime },
            new CalendarEvent { Title = "c", StartDate = date, EndDate = date, BackgroundColor = Colors.Magenta }
        };
        var style = new CalendarStyle { EventDisplayMode = CalendarEventDisplayMode.Dots, EventDotSize = 10, EventDotSpacing = 4, MaxVisibleDots = 2 };
        var month = new CalendarMonthBuilder().Build(2026, 9, Today, events);
        var cell = new CalendarLayout(month, style, Width, height).GetCellBounds(1, 3);

        // Act
        using var bitmap = Render(month, style, new CalendarRenderState(), Width, height);

        // Assert
        Assert.True(ContainsPixel(bitmap, cell, ToSkColor(Colors.Red)));
        Assert.True(ContainsPixel(bitmap, cell, ToSkColor(Colors.Lime)));
        Assert.False(ContainsPixel(bitmap, cell, ToSkColor(Colors.Magenta)));
    }

    private static SKBitmap Render(CalendarMonth month, CalendarStyle style, CalendarRenderState state, int width, int height)
    {
        var layout = new CalendarLayout(month, style, width, height);
        using var renderer = new CalendarRenderer(style);
        using var surface = SKSurface.Create(new SKImageInfo(width, height));
        renderer.Render(surface.Canvas, layout, state);
        using var image = surface.Snapshot();
        return SKBitmap.FromImage(image);
    }

    private static bool ContainsPixel(SKBitmap bitmap, CalendarRect rect, SKColor color)
    {
        for (var y = (int)rect.Y; y < (int)rect.Bottom; y++)
        {
            for (var x = (int)rect.X; x < (int)rect.Right; x++)
            {
                if (bitmap.GetPixel(x, y) == color)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static SKColor CellPixel(SKBitmap bitmap, CalendarLayout layout, int week, int column)
    {
        var cell = layout.GetCellBounds(week, column);
        return Pixel(bitmap, cell.Right - 3, cell.Bottom - 3);
    }

    private static SKColor Pixel(SKBitmap bitmap, double x, double y) => bitmap.GetPixel((int)x, (int)y);

    private static SKColor ToSkColor(Color color) => new((byte)(color.Red * 255), (byte)(color.Green * 255), (byte)(color.Blue * 255), (byte)(color.Alpha * 255));
}
