namespace ClamCalendar.Tests;

public sealed class CalendarStyleTests
{
    [Fact]
    public void DefaultsMatchTheDocumentedValues()
    {
        // Act
        var style = new CalendarStyle();

        // Assert
        Assert.Equal("sans-serif", style.FontFamily);
        Assert.Equal(20, style.HeaderFontSize);
        Assert.Equal(48, style.HeaderHeight);
        Assert.Equal(32, style.WeekdayHeaderHeight);
        Assert.Equal(28, style.DateRowHeight);
        Assert.Equal(18, style.SlotRowHeight);
        Assert.Equal(16, style.EventRowHeight);
        Assert.Equal(24, style.DateNumberSize);
        Assert.Equal(44, style.NavigationButtonWidth);
        Assert.Equal(2, style.DateNumberMarkWidth);
        Assert.Equal(0.5, style.GridLineWidth);
        Assert.Equal(CalendarMarkStyle.Fill, style.TodayMarkStyle);
        Assert.Equal(CalendarMarkStyle.Fill, style.SelectedMarkStyle);
        Assert.False(style.TodayMarkOnSelected);
        Assert.Equal(CalendarDateNumberShape.RoundedSquare, style.DateNumberShape);
        Assert.True(style.DateNumberBold);
        Assert.Equal(CalendarGridLineMode.All, style.GridLineMode);
        Assert.Equal(CalendarEventDisplayMode.Bars, style.EventDisplayMode);
        Assert.Equal(6, style.EventDotSize);
        Assert.Equal(3, style.EventDotSpacing);
        Assert.Equal(4, style.MaxVisibleDots);
        Assert.Equal(24, style.WeekNumberWidth);
        Assert.Equal(9, style.WeekNumberFontSize);
        Assert.Equal(Colors.White, style.Background);
        Assert.Equal(Colors.Black, style.StampColor);
        Assert.Equal(Color.FromArgb("#E53935"), style.HolidayTextColor);
        Assert.Equal(Color.FromArgb("#FFF1F1"), style.HolidayBackground);
    }

    [Fact]
    public void StylesAreRecordsThatCanBeCopied()
    {
        // Arrange
        var style = new CalendarStyle();

        // Act
        var dark = style with { Background = Colors.Black, HeaderTextColor = Colors.White };

        // Assert
        Assert.Equal(Colors.White, style.Background);
        Assert.Equal(Colors.Black, dark.Background);
        Assert.Equal(style.HeaderHeight, dark.HeaderHeight);
        Assert.NotEqual(style, dark);
    }

    [Fact]
    public void InvalidSizesAreRejectedWhenTheRendererIsCreated()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(static () => new CalendarRenderer(new CalendarStyle { HeaderFontSize = 0 }));
        Assert.Throws<ArgumentException>(static () => new CalendarRenderer(new CalendarStyle { DateRowHeight = Double.NaN }));
        Assert.Throws<ArgumentException>(static () => new CalendarRenderer(new CalendarStyle { EventPadding = -1 }));
        Assert.Throws<ArgumentException>(static () => new CalendarRenderer(new CalendarStyle { DateNumberMarkWidth = 0 }));
        Assert.Throws<ArgumentException>(static () => new CalendarRenderer(new CalendarStyle { GridLineWidth = -1 }));
        Assert.Throws<ArgumentException>(static () => new CalendarRenderer(new CalendarStyle { EventDotSize = 0 }));
        Assert.Throws<ArgumentException>(static () => new CalendarRenderer(new CalendarStyle { EventDotSpacing = -1 }));
        Assert.Throws<ArgumentException>(static () => new CalendarRenderer(new CalendarStyle { MaxVisibleDots = -1 }));
        Assert.Throws<ArgumentException>(static () => new CalendarRenderer(new CalendarStyle { WeekNumberWidth = 0 }));
        Assert.Throws<ArgumentException>(static () => new CalendarRenderer(new CalendarStyle { WeekNumberFontSize = -1 }));
    }

    [Fact]
    public void InvalidStylesAreRejectedWhenAssigned()
    {
        // Arrange
        using var view = new ClamCalendarView();
        var style = view.CalendarStyle;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => view.CalendarStyle = new CalendarStyle { HeaderFontSize = 0 });
        Assert.Throws<ArgumentNullException>(() => view.CalendarStyle = null!);
        Assert.Same(style, view.CalendarStyle);
    }

    [Fact]
    public void ApplyStyleRejectsAStyleChangedToInvalidValues()
    {
        // Arrange
        using var view = new ClamCalendarView();

        // Act
        view.CalendarStyle.DateRowHeight = Double.NaN;

        // Assert
        Assert.Throws<ArgumentException>(view.ApplyStyle);
    }

    [Fact]
    public void ApplyStyleRedrawsUntilTheViewIsDisposed()
    {
        // Arrange
        var view = new ClamCalendarView();

        // Act
        view.CalendarStyle.HeaderHeight = 56;
        view.ApplyStyle();

        // Assert
        Assert.Equal(56, view.CalendarStyle.HeaderHeight);

        // Act
        view.Dispose();

        // Assert
        Assert.Throws<ObjectDisposedException>(view.ApplyStyle);
    }
}
