namespace ClamCalendar.Tests.Rendering;

using SkiaSharp;

public sealed class CalendarDayPainterTests
{
    private const int Width = 350;

    private const int Height = 600;

    private static readonly DateOnly Today = new(2026, 9, 3);

    [Fact]
    public void PainterIsCalledForEveryVisibleDay()
    {
        // Arrange
        var calls = new List<CalendarDayPaintContext>();
        var style = new CalendarStyle();
        var month = new CalendarMonthBuilder().Build(2026, 9, Today);
        var layout = new CalendarLayout(month, style, Width, Height);

        // Act
        using var bitmap = Render(month, style, new CalendarRenderState { DayPainter = (_, context) => calls.Add(context) });

        // Assert
        Assert.Equal(layout.WeekCount * CalendarLayout.DaysPerWeek, calls.Count);
        Assert.Equal(month.Weeks[0].Days[0].Date, calls[0].Day.Date);
        Assert.Equal(ToSkRect(layout.GetCellBounds(0, 0)), calls[0].Bounds);
        Assert.Equal(ToSkRect(layout.GetDateNumberBounds(0, 0)), calls[0].DateNumberBounds);
        Assert.Equal(ToSkRect(layout.GetCellBounds(layout.WeekCount - 1, CalendarLayout.DaysPerWeek - 1)), calls[^1].Bounds);
    }

    [Fact]
    public void DaysOutsideTheMonthAreSkippedWhenTheyAreHidden()
    {
        // Arrange
        var calls = new List<CalendarDayPaintContext>();
        var style = new CalendarStyle();
        var month = new CalendarMonthBuilder().Build(2026, 9, Today);

        // Act
        using var bitmap = Render(month, style, new CalendarRenderState { OutsideMonthDaysVisible = false, DayPainter = (_, context) => calls.Add(context) });

        // Assert
        Assert.Equal(30, calls.Count);
        Assert.All(calls, static x => Assert.True(x.Day.IsCurrentMonth));
    }

    [Fact]
    public void PainterIsClippedToTheCellAndCannotChangeTheCanvas()
    {
        // Arrange
        var calls = new List<(SKRect Bounds, SKRectI Clip, SKMatrix Matrix)>();
        var style = new CalendarStyle();
        var month = new CalendarMonthBuilder().Build(2026, 9, Today);
        var state = new CalendarRenderState
        {
            DayPainter = (canvas, context) =>
            {
                calls.Add((context.Bounds, canvas.DeviceClipBounds, canvas.TotalMatrix));
                canvas.Save();
                canvas.Translate(10, 10);
                canvas.ClipRect(new SKRect(0, 0, 1, 1));
            }
        };
        using var renderer = new CalendarRenderer(style);
        using var surface = SKSurface.Create(new SKImageInfo(Width, Height));
        var canvas = surface.Canvas;
        var saveCount = canvas.SaveCount;

        // Act
        renderer.Render(canvas, new CalendarLayout(month, style, Width, Height), state);

        // Assert
        var matrix = calls[0].Matrix;
        Assert.Equal(saveCount, canvas.SaveCount);
        Assert.All(calls, x => Assert.Equal(matrix, x.Matrix));
        Assert.All(calls, static x => Assert.True(x.Clip.Contains((int)x.Bounds.MidX, (int)x.Bounds.MidY)));
        Assert.All(calls, static x => Assert.True((x.Clip.Width <= (x.Bounds.Width + 1)) && (x.Clip.Height <= (x.Bounds.Height + 1))));
    }

    [Fact]
    public void PainterIsDrawnOverTheDateNumberAndTheEvents()
    {
        // Arrange
        var date = new DateOnly(2026, 9, 10);
        var calls = new List<CalendarDayPaintContext>();
        var style = new CalendarStyle();
        var month = new CalendarMonthBuilder().Build(2026, 9, Today, [new CalendarEvent { Title = "a", StartDate = date, EndDate = date, BackgroundColor = Colors.Blue }]);

        // Act
        using var bitmap = Render(month, style, new CalendarRenderState
        {
            DayPainter = (canvas, context) =>
            {
                calls.Add(context);
                canvas.Clear(SKColors.Red);
            }
        });

        // Assert
        var painted = calls.First(x => x.Day.Date == date);
        Assert.Equal(SKColors.Red, bitmap.GetPixel((int)painted.DateNumberBounds.MidX, (int)painted.DateNumberBounds.MidY));
        Assert.False(ContainsPixel(bitmap, painted.Bounds, SKColors.Blue));
    }

    [Fact]
    public void PainterReceivesTheStateAndTheDateNumberColor()
    {
        // Arrange
        var selected = new DateOnly(2026, 9, 7);
        var disabled = new DateOnly(2026, 9, 8);
        var both = new DateOnly(2026, 9, 9);
        var calls = new List<CalendarDayPaintContext>();
        var style = new CalendarStyle();
        var month = new CalendarMonthBuilder().Build(2026, 9, Today);
        var state = new CalendarRenderState
        {
            IsSelected = x => (x == selected) || (x == both),
            IsDisabled = x => (x == disabled) || (x == both),
            DayPainter = (_, context) => calls.Add(context)
        };

        // Act
        using var bitmap = Render(month, style, state);

        // Assert
        Assert.Equal((true, false, style.SelectedDayTextColor), Resolve(calls, selected));
        Assert.Equal((false, true, style.DisabledDayTextColor), Resolve(calls, disabled));
        Assert.Equal((false, true, style.DisabledDayTextColor), Resolve(calls, both));
        Assert.Equal((false, false, style.TodayTextColor), Resolve(calls, Today));
        Assert.Equal((false, false, style.WeekdayTextColor), Resolve(calls, new DateOnly(2026, 9, 10)));
    }

    [Fact]
    public void DayPainterFlowsIntoTheRenderState()
    {
        // Arrange
        using var view = new ClamCalendarView();
        Action<SKCanvas, CalendarDayPaintContext> painter = static (_, _) => { };

        // Act
        view.DayPainter = painter;

        // Assert
        Assert.Same(painter, view.CreateRenderState().DayPainter);
    }

    [Fact]
    public void PainterAddsNoAllocationPerFrame()
    {
        // Arrange
        var style = new CalendarStyle();
        var month = new CalendarMonthBuilder().Build(2026, 9, Today);
        var plain = new CalendarRenderState();
        var painted = new CalendarRenderState { DayPainter = static (_, _) => { } };
        using var renderer = new CalendarRenderer(style);
        using var surface = SKSurface.Create(new SKImageInfo(Width, Height));
        var layout = new CalendarLayout(month, style, Width, Height);
        renderer.Render(surface.Canvas, layout, plain);
        renderer.Render(surface.Canvas, layout, painted);

        // Act
        var started = GC.GetAllocatedBytesForCurrentThread();
        renderer.Render(surface.Canvas, layout, plain);
        var baseline = GC.GetAllocatedBytesForCurrentThread() - started;
        renderer.Render(surface.Canvas, layout, painted);
        var allocated = GC.GetAllocatedBytesForCurrentThread() - started - baseline;

        // Assert
        Assert.Equal(baseline, allocated);
    }

    private static (bool Selected, bool Disabled, Color TextColor) Resolve(List<CalendarDayPaintContext> calls, DateOnly date)
    {
        var context = calls.First(x => x.Day.Date == date);
        return (context.IsSelected, context.IsDisabled, context.TextColor);
    }

    private static SKBitmap Render(CalendarMonth month, CalendarStyle style, CalendarRenderState state)
    {
        using var renderer = new CalendarRenderer(style);
        using var surface = SKSurface.Create(new SKImageInfo(Width, Height));
        renderer.Render(surface.Canvas, new CalendarLayout(month, style, Width, Height), state);
        using var image = surface.Snapshot();
        return SKBitmap.FromImage(image);
    }

    private static bool ContainsPixel(SKBitmap bitmap, SKRect rect, SKColor color)
    {
        for (var y = (int)rect.Top; y < (int)rect.Bottom; y++)
        {
            for (var x = (int)rect.Left; x < (int)rect.Right; x++)
            {
                if (bitmap.GetPixel(x, y) == color)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static SKRect ToSkRect(CalendarRect rect) => new((float)rect.X, (float)rect.Y, (float)rect.Right, (float)rect.Bottom);
}
