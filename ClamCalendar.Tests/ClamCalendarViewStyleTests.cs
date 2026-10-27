namespace ClamCalendar.Tests;

public sealed class ClamCalendarViewStyleTests
{
    [Fact]
    public void SnapshotMatchesTheDefaultsWhenNothingIsSet()
    {
        // Arrange
        using var view = new ClamCalendarView();

        // Act & Assert
        Assert.Equal(new CalendarStyle(), view.EffectiveStyle);
    }

    [Fact]
    public void DefaultsMatchTheDocumentedValues()
    {
        // Arrange
        using var view = new ClamCalendarView();

        // Assert
        Assert.Equal("sans-serif", view.FontFamily);
        Assert.Equal(20, view.HeaderFontSize);
        Assert.Equal(48, view.HeaderHeight);
        Assert.Equal(32, view.WeekdayHeaderHeight);
        Assert.Equal(28, view.DateRowHeight);
        Assert.Equal(18, view.SlotRowHeight);
        Assert.Equal(16, view.EventRowHeight);
        Assert.Equal(24, view.DateNumberSize);
        Assert.Equal(44, view.NavigationButtonWidth);
        Assert.Equal(2, view.DateNumberMarkWidth);
        Assert.Equal(0.5, view.GridLineWidth);
        Assert.Equal(CalendarMarkStyle.Fill, view.TodayMarkStyle);
        Assert.Equal(CalendarMarkStyle.Fill, view.SelectedMarkStyle);
        Assert.False(view.TodayMarkOnSelected);
        Assert.Equal(CalendarDateNumberShape.RoundedSquare, view.DateNumberShape);
        Assert.True(view.DateNumberBold);
        Assert.Equal(CalendarGridLineMode.All, view.GridLineMode);
        Assert.Equal(CalendarEventDisplayMode.Bars, view.EventDisplayMode);
        Assert.Equal(6, view.EventDotSize);
        Assert.Equal(3, view.EventDotSpacing);
        Assert.Equal(4, view.MaxVisibleDots);
        Assert.Equal(24, view.WeekNumberWidth);
        Assert.Equal(9, view.WeekNumberFontSize);
        Assert.Equal(Colors.Black, view.StampColor);
        Assert.Equal(Color.FromArgb("#E53935"), view.HolidayTextColor);
        Assert.Equal(Color.FromArgb("#FFF1F1"), view.HolidayBackground);
    }

    [Fact]
    public void BackgroundColorFillsTheCalendar()
    {
        // Arrange
        using var view = new ClamCalendarView();

        // Act & Assert
        Assert.Equal(Colors.White, view.EffectiveStyle.Background);

        // Act
        view.BackgroundColor = Color.FromArgb("#121212");

        // Assert
        Assert.Equal(Color.FromArgb("#121212"), view.EffectiveStyle.Background);
    }

    [Fact]
    public void SnapshotFollowsEveryChange()
    {
        // Arrange
        using var view = new ClamCalendarView();

        // Act
        view.HeaderHeight = 56;
        view.TodayBackground = Colors.Red;
        view.GridLineMode = CalendarGridLineMode.Horizontal;
        view.MaxVisibleDots = 0;

        // Assert
        Assert.Equal(56d, view.EffectiveStyle.HeaderHeight);
        Assert.Equal(Colors.Red, view.EffectiveStyle.TodayBackground);
        Assert.Equal(CalendarGridLineMode.Horizontal, view.EffectiveStyle.GridLineMode);
        Assert.Equal(0, view.EffectiveStyle.MaxVisibleDots);
    }

    [Fact]
    public void InvalidSizesAreRejected()
    {
        // Arrange
        using var view = new ClamCalendarView();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => view.HeaderFontSize = 0);
        Assert.Throws<ArgumentOutOfRangeException>(() => view.DateRowHeight = Double.NaN);
        Assert.Throws<ArgumentOutOfRangeException>(() => view.EventPadding = -1);
        Assert.Throws<ArgumentOutOfRangeException>(() => view.DateNumberMarkWidth = 0);
        Assert.Throws<ArgumentOutOfRangeException>(() => view.GridLineWidth = -1);
        Assert.Throws<ArgumentOutOfRangeException>(() => view.EventDotSize = 0);
        Assert.Throws<ArgumentOutOfRangeException>(() => view.EventDotSpacing = -1);
        Assert.Throws<ArgumentOutOfRangeException>(() => view.MaxVisibleDots = -1);
        Assert.Throws<ArgumentOutOfRangeException>(() => view.WeekNumberWidth = 0);
        Assert.Throws<ArgumentOutOfRangeException>(() => view.WeekNumberFontSize = -1);
    }

    [Fact]
    public void ZeroIsAcceptedWhereItHasAMeaning()
    {
        // Arrange
        using var view = new ClamCalendarView();

        // Act
        view.DateNumberMargin = 0;
        view.EventCornerRadius = 0;
        view.StampMarginEdge = 0;

        // Assert
        Assert.Equal(0d, view.EffectiveStyle.DateNumberMargin);
        Assert.Equal(0d, view.EffectiveStyle.EventCornerRadius);
        Assert.Equal(0d, view.EffectiveStyle.StampMarginEdge);
    }

    [Fact]
    public void ThemeIsAppliedThroughAStyle()
    {
        // Arrange
        var style = new Style(typeof(ClamCalendarView));
        style.Setters.Add(new Setter { Property = ClamCalendarView.HeaderBackgroundProperty, Value = Colors.Red });
        style.Setters.Add(new Setter { Property = ClamCalendarView.DateNumberShapeProperty, Value = CalendarDateNumberShape.Circle });
        using var view = new ClamCalendarView();

        // Act
        view.Style = style;

        // Assert
        Assert.Equal(Colors.Red, view.EffectiveStyle.HeaderBackground);
        Assert.Equal(CalendarDateNumberShape.Circle, view.EffectiveStyle.DateNumberShape);
    }

    [Fact]
    public void BackgroundColorIsAppliedThroughAStyle()
    {
        // Arrange
        var style = new Style(typeof(ClamCalendarView));
        style.Setters.Add(new Setter { Property = VisualElement.BackgroundColorProperty, Value = Color.FromArgb("#121212") });
        style.Setters.Add(new Setter { Property = ClamCalendarView.HeaderBackgroundProperty, Value = Color.FromArgb("#1F1F1F") });
        using var view = new ClamCalendarView();

        // Act
        view.Style = style;

        // Assert
        Assert.Equal(Color.FromArgb("#1F1F1F"), view.EffectiveStyle.HeaderBackground);
        Assert.Equal(Color.FromArgb("#121212"), view.EffectiveStyle.Background);
    }

    [Fact]
    public void ColorsFollowADynamicResource()
    {
        // Arrange
        using var view = new ClamCalendarView();
        view.Resources = new ResourceDictionary { { "TodayBackground", Colors.Red } };

        // Act
        view.SetDynamicResource(ClamCalendarView.TodayBackgroundProperty, "TodayBackground");

        // Assert
        Assert.Equal(Colors.Red, view.EffectiveStyle.TodayBackground);

        // Act
        view.Resources["TodayBackground"] = Colors.Blue;

        // Assert
        Assert.Equal(Colors.Blue, view.EffectiveStyle.TodayBackground);
    }
}
