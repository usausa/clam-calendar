namespace ClamCalendar;

public partial class ClamCalendarView
{
    public static readonly BindableProperty FontFamilyProperty = BindableProperty.Create(nameof(FontFamily), typeof(string), typeof(ClamCalendarView), "sans-serif", propertyChanged: OnStyleFontChanged);
    public static readonly BindableProperty HeaderFontSizeProperty = BindableProperty.Create(nameof(HeaderFontSize), typeof(double), typeof(ClamCalendarView), 20d, validateValue: static (_, value) => ValidatePositive(value, nameof(HeaderFontSize)), propertyChanged: OnStyleRenderChanged);
    public static readonly BindableProperty WeekdayHeaderFontSizeProperty = BindableProperty.Create(nameof(WeekdayHeaderFontSize), typeof(double), typeof(ClamCalendarView), 12d, validateValue: static (_, value) => ValidatePositive(value, nameof(WeekdayHeaderFontSize)), propertyChanged: OnStyleRenderChanged);
    public static readonly BindableProperty DateNumberFontSizeProperty = BindableProperty.Create(nameof(DateNumberFontSize), typeof(double), typeof(ClamCalendarView), 14d, validateValue: static (_, value) => ValidatePositive(value, nameof(DateNumberFontSize)), propertyChanged: OnStyleRenderChanged);
    public static readonly BindableProperty EventFontSizeProperty = BindableProperty.Create(nameof(EventFontSize), typeof(double), typeof(ClamCalendarView), 11d, validateValue: static (_, value) => ValidatePositive(value, nameof(EventFontSize)), propertyChanged: OnStyleRenderChanged);
    public static readonly BindableProperty DayLabelFontSizeProperty = BindableProperty.Create(nameof(DayLabelFontSize), typeof(double), typeof(ClamCalendarView), 9d, validateValue: static (_, value) => ValidatePositive(value, nameof(DayLabelFontSize)), propertyChanged: OnStyleRenderChanged);
    public static readonly BindableProperty WeekNumberFontSizeProperty = BindableProperty.Create(nameof(WeekNumberFontSize), typeof(double), typeof(ClamCalendarView), 9d, validateValue: static (_, value) => ValidatePositive(value, nameof(WeekNumberFontSize)), propertyChanged: OnStyleRenderChanged);
    public static readonly BindableProperty MonthIndicatorFontSizeProperty = BindableProperty.Create(nameof(MonthIndicatorFontSize), typeof(double), typeof(ClamCalendarView), 120d, validateValue: static (_, value) => ValidatePositive(value, nameof(MonthIndicatorFontSize)), propertyChanged: OnStyleRenderChanged);
    public static readonly BindableProperty HeaderHeightProperty = BindableProperty.Create(nameof(HeaderHeight), typeof(double), typeof(ClamCalendarView), 48d, validateValue: static (_, value) => ValidatePositive(value, nameof(HeaderHeight)), propertyChanged: OnStyleChanged);
    public static readonly BindableProperty WeekdayHeaderHeightProperty = BindableProperty.Create(nameof(WeekdayHeaderHeight), typeof(double), typeof(ClamCalendarView), 32d, validateValue: static (_, value) => ValidatePositive(value, nameof(WeekdayHeaderHeight)), propertyChanged: OnStyleChanged);
    public static readonly BindableProperty DateRowHeightProperty = BindableProperty.Create(nameof(DateRowHeight), typeof(double), typeof(ClamCalendarView), 28d, validateValue: static (_, value) => ValidatePositive(value, nameof(DateRowHeight)), propertyChanged: OnStyleChanged);
    public static readonly BindableProperty SlotRowHeightProperty = BindableProperty.Create(nameof(SlotRowHeight), typeof(double), typeof(ClamCalendarView), 18d, validateValue: static (_, value) => ValidatePositive(value, nameof(SlotRowHeight)), propertyChanged: OnStyleChanged);
    public static readonly BindableProperty EventRowHeightProperty = BindableProperty.Create(nameof(EventRowHeight), typeof(double), typeof(ClamCalendarView), 16d, validateValue: static (_, value) => ValidatePositive(value, nameof(EventRowHeight)), propertyChanged: OnStyleChanged);
    public static readonly BindableProperty DateNumberSizeProperty = BindableProperty.Create(nameof(DateNumberSize), typeof(double), typeof(ClamCalendarView), 24d, validateValue: static (_, value) => ValidatePositive(value, nameof(DateNumberSize)), propertyChanged: OnStyleChanged);
    public static readonly BindableProperty DateNumberMarginProperty = BindableProperty.Create(nameof(DateNumberMargin), typeof(double), typeof(ClamCalendarView), 4d, validateValue: static (_, value) => ValidateSize(value, nameof(DateNumberMargin)), propertyChanged: OnStyleChanged);
    public static readonly BindableProperty DateNumberMarkWidthProperty = BindableProperty.Create(nameof(DateNumberMarkWidth), typeof(double), typeof(ClamCalendarView), 2d, validateValue: static (_, value) => ValidatePositive(value, nameof(DateNumberMarkWidth)), propertyChanged: OnStyleRenderChanged);
    public static readonly BindableProperty StampMarginEdgeProperty = BindableProperty.Create(nameof(StampMarginEdge), typeof(double), typeof(ClamCalendarView), 2d, validateValue: static (_, value) => ValidateSize(value, nameof(StampMarginEdge)), propertyChanged: OnStyleRenderChanged);
    public static readonly BindableProperty EventCornerRadiusProperty = BindableProperty.Create(nameof(EventCornerRadius), typeof(double), typeof(ClamCalendarView), 4d, validateValue: static (_, value) => ValidateSize(value, nameof(EventCornerRadius)), propertyChanged: OnStyleRenderChanged);
    public static readonly BindableProperty EventPaddingProperty = BindableProperty.Create(nameof(EventPadding), typeof(double), typeof(ClamCalendarView), 4d, validateValue: static (_, value) => ValidateSize(value, nameof(EventPadding)), propertyChanged: OnStyleRenderChanged);
    public static readonly BindableProperty EventDotSizeProperty = BindableProperty.Create(nameof(EventDotSize), typeof(double), typeof(ClamCalendarView), 6d, validateValue: static (_, value) => ValidatePositive(value, nameof(EventDotSize)), propertyChanged: OnStyleRenderChanged);
    public static readonly BindableProperty EventDotSpacingProperty = BindableProperty.Create(nameof(EventDotSpacing), typeof(double), typeof(ClamCalendarView), 3d, validateValue: static (_, value) => ValidateSize(value, nameof(EventDotSpacing)), propertyChanged: OnStyleRenderChanged);
    public static readonly BindableProperty NavigationButtonWidthProperty = BindableProperty.Create(nameof(NavigationButtonWidth), typeof(double), typeof(ClamCalendarView), 44d, validateValue: static (_, value) => ValidatePositive(value, nameof(NavigationButtonWidth)), propertyChanged: OnStyleChanged);
    public static readonly BindableProperty TodayButtonWidthProperty = BindableProperty.Create(nameof(TodayButtonWidth), typeof(double), typeof(ClamCalendarView), 64d, validateValue: static (_, value) => ValidatePositive(value, nameof(TodayButtonWidth)), propertyChanged: OnStyleChanged);
    public static readonly BindableProperty GridLineWidthProperty = BindableProperty.Create(nameof(GridLineWidth), typeof(double), typeof(ClamCalendarView), 0.5d, validateValue: static (_, value) => ValidatePositive(value, nameof(GridLineWidth)), propertyChanged: OnStyleRenderChanged);
    public static readonly BindableProperty WeekNumberWidthProperty = BindableProperty.Create(nameof(WeekNumberWidth), typeof(double), typeof(ClamCalendarView), 24d, validateValue: static (_, value) => ValidatePositive(value, nameof(WeekNumberWidth)), propertyChanged: OnStyleChanged);
    public static readonly BindableProperty GridLineColorProperty = BindableProperty.Create(nameof(GridLineColor), typeof(Color), typeof(ClamCalendarView), Color.FromArgb("#E0E0E0"), propertyChanged: OnStyleRenderChanged);
    public static readonly BindableProperty WeekdayTextColorProperty = BindableProperty.Create(nameof(WeekdayTextColor), typeof(Color), typeof(ClamCalendarView), Color.FromArgb("#1F1F1F"), propertyChanged: OnStyleRenderChanged);
    public static readonly BindableProperty SaturdayTextColorProperty = BindableProperty.Create(nameof(SaturdayTextColor), typeof(Color), typeof(ClamCalendarView), Color.FromArgb("#2196F3"), propertyChanged: OnStyleRenderChanged);
    public static readonly BindableProperty SundayTextColorProperty = BindableProperty.Create(nameof(SundayTextColor), typeof(Color), typeof(ClamCalendarView), Color.FromArgb("#E53935"), propertyChanged: OnStyleRenderChanged);
    public static readonly BindableProperty HolidayTextColorProperty = BindableProperty.Create(nameof(HolidayTextColor), typeof(Color), typeof(ClamCalendarView), Color.FromArgb("#E53935"), propertyChanged: OnStyleRenderChanged);
    public static readonly BindableProperty OutsideMonthTextColorProperty = BindableProperty.Create(nameof(OutsideMonthTextColor), typeof(Color), typeof(ClamCalendarView), Color.FromArgb("#BDBDBD"), propertyChanged: OnStyleRenderChanged);
    public static readonly BindableProperty OutsideMonthBackgroundProperty = BindableProperty.Create(nameof(OutsideMonthBackground), typeof(Color), typeof(ClamCalendarView), Color.FromArgb("#F2F2F2"), propertyChanged: OnStyleRenderChanged);
    public static readonly BindableProperty WeekendBackgroundProperty = BindableProperty.Create(nameof(WeekendBackground), typeof(Color), typeof(ClamCalendarView), Color.FromArgb("#FFF1F1"), propertyChanged: OnStyleRenderChanged);
    public static readonly BindableProperty HolidayBackgroundProperty = BindableProperty.Create(nameof(HolidayBackground), typeof(Color), typeof(ClamCalendarView), Color.FromArgb("#FFF1F1"), propertyChanged: OnStyleRenderChanged);
    public static readonly BindableProperty TodayBackgroundProperty = BindableProperty.Create(nameof(TodayBackground), typeof(Color), typeof(ClamCalendarView), Colors.Black, propertyChanged: OnStyleRenderChanged);
    public static readonly BindableProperty TodayTextColorProperty = BindableProperty.Create(nameof(TodayTextColor), typeof(Color), typeof(ClamCalendarView), Colors.White, propertyChanged: OnStyleRenderChanged);
    public static readonly BindableProperty SelectedDayBackgroundProperty = BindableProperty.Create(nameof(SelectedDayBackground), typeof(Color), typeof(ClamCalendarView), Color.FromArgb("#1A73E8"), propertyChanged: OnStyleRenderChanged);
    public static readonly BindableProperty SelectedDayTextColorProperty = BindableProperty.Create(nameof(SelectedDayTextColor), typeof(Color), typeof(ClamCalendarView), Colors.White, propertyChanged: OnStyleRenderChanged);
    public static readonly BindableProperty RangeBackgroundProperty = BindableProperty.Create(nameof(RangeBackground), typeof(Color), typeof(ClamCalendarView), Color.FromArgb("#BDD7F5"), propertyChanged: OnStyleRenderChanged);
    public static readonly BindableProperty DisabledDayTextColorProperty = BindableProperty.Create(nameof(DisabledDayTextColor), typeof(Color), typeof(ClamCalendarView), Color.FromArgb("#C0C0C0"), propertyChanged: OnStyleRenderChanged);
    public static readonly BindableProperty DayLabelTextColorProperty = BindableProperty.Create(nameof(DayLabelTextColor), typeof(Color), typeof(ClamCalendarView), Color.FromArgb("#757575"), propertyChanged: OnStyleRenderChanged);
    public static readonly BindableProperty OverflowTextColorProperty = BindableProperty.Create(nameof(OverflowTextColor), typeof(Color), typeof(ClamCalendarView), Color.FromArgb("#757575"), propertyChanged: OnStyleRenderChanged);
    public static readonly BindableProperty StampColorProperty = BindableProperty.Create(nameof(StampColor), typeof(Color), typeof(ClamCalendarView), Colors.Black, propertyChanged: OnStyleRenderChanged);
    public static readonly BindableProperty WeekNumberTextColorProperty = BindableProperty.Create(nameof(WeekNumberTextColor), typeof(Color), typeof(ClamCalendarView), Color.FromArgb("#757575"), propertyChanged: OnStyleRenderChanged);
    public static readonly BindableProperty HeaderBackgroundProperty = BindableProperty.Create(nameof(HeaderBackground), typeof(Color), typeof(ClamCalendarView), Colors.White, propertyChanged: OnStyleRenderChanged);
    public static readonly BindableProperty HeaderTextColorProperty = BindableProperty.Create(nameof(HeaderTextColor), typeof(Color), typeof(ClamCalendarView), Colors.Black, propertyChanged: OnStyleRenderChanged);
    public static readonly BindableProperty NavigationButtonColorProperty = BindableProperty.Create(nameof(NavigationButtonColor), typeof(Color), typeof(ClamCalendarView), Color.FromArgb("#333333"), propertyChanged: OnStyleRenderChanged);
    public static readonly BindableProperty WeekdayHeaderBackgroundProperty = BindableProperty.Create(nameof(WeekdayHeaderBackground), typeof(Color), typeof(ClamCalendarView), Colors.White, propertyChanged: OnStyleRenderChanged);
    public static readonly BindableProperty WeekdayHeaderTextColorProperty = BindableProperty.Create(nameof(WeekdayHeaderTextColor), typeof(Color), typeof(ClamCalendarView), Color.FromArgb("#333333"), propertyChanged: OnStyleRenderChanged);
    public static readonly BindableProperty SaturdayHeaderTextColorProperty = BindableProperty.Create(nameof(SaturdayHeaderTextColor), typeof(Color), typeof(ClamCalendarView), Color.FromArgb("#2196F3"), propertyChanged: OnStyleRenderChanged);
    public static readonly BindableProperty SundayHeaderTextColorProperty = BindableProperty.Create(nameof(SundayHeaderTextColor), typeof(Color), typeof(ClamCalendarView), Color.FromArgb("#E53935"), propertyChanged: OnStyleRenderChanged);
    public static readonly BindableProperty MonthIndicatorColorProperty = BindableProperty.Create(nameof(MonthIndicatorColor), typeof(Color), typeof(ClamCalendarView), Color.FromArgb("#10000000"), propertyChanged: OnStyleRenderChanged);
    public static readonly BindableProperty TodayMarkStyleProperty = BindableProperty.Create(nameof(TodayMarkStyle), typeof(CalendarMarkStyle), typeof(ClamCalendarView), CalendarMarkStyle.Fill, propertyChanged: OnStyleRenderChanged);
    public static readonly BindableProperty SelectedMarkStyleProperty = BindableProperty.Create(nameof(SelectedMarkStyle), typeof(CalendarMarkStyle), typeof(ClamCalendarView), CalendarMarkStyle.Fill, propertyChanged: OnStyleRenderChanged);
    public static readonly BindableProperty TodayMarkOnSelectedProperty = BindableProperty.Create(nameof(TodayMarkOnSelected), typeof(bool), typeof(ClamCalendarView), false, propertyChanged: OnStyleRenderChanged);
    public static readonly BindableProperty DateNumberShapeProperty = BindableProperty.Create(nameof(DateNumberShape), typeof(CalendarDateNumberShape), typeof(ClamCalendarView), CalendarDateNumberShape.RoundedSquare, propertyChanged: OnStyleRenderChanged);
    public static readonly BindableProperty DateNumberBoldProperty = BindableProperty.Create(nameof(DateNumberBold), typeof(bool), typeof(ClamCalendarView), true, propertyChanged: OnStyleRenderChanged);
    public static readonly BindableProperty GridLineModeProperty = BindableProperty.Create(nameof(GridLineMode), typeof(CalendarGridLineMode), typeof(ClamCalendarView), CalendarGridLineMode.All, propertyChanged: OnStyleRenderChanged);
    public static readonly BindableProperty EventDisplayModeProperty = BindableProperty.Create(nameof(EventDisplayMode), typeof(CalendarEventDisplayMode), typeof(ClamCalendarView), CalendarEventDisplayMode.Bars, propertyChanged: OnStyleChanged);
    public static readonly BindableProperty MaxVisibleDotsProperty = BindableProperty.Create(nameof(MaxVisibleDots), typeof(int), typeof(ClamCalendarView), 4, validateValue: static (_, value) => ValidateCount(value, nameof(MaxVisibleDots)), propertyChanged: OnStyleRenderChanged);

    public string FontFamily
    {
        get => (string)GetValue(FontFamilyProperty);
        set => SetValue(FontFamilyProperty, value);
    }

    public double HeaderFontSize
    {
        get => (double)GetValue(HeaderFontSizeProperty);
        set => SetValue(HeaderFontSizeProperty, value);
    }

    public double WeekdayHeaderFontSize
    {
        get => (double)GetValue(WeekdayHeaderFontSizeProperty);
        set => SetValue(WeekdayHeaderFontSizeProperty, value);
    }

    public double DateNumberFontSize
    {
        get => (double)GetValue(DateNumberFontSizeProperty);
        set => SetValue(DateNumberFontSizeProperty, value);
    }

    public double EventFontSize
    {
        get => (double)GetValue(EventFontSizeProperty);
        set => SetValue(EventFontSizeProperty, value);
    }

    public double DayLabelFontSize
    {
        get => (double)GetValue(DayLabelFontSizeProperty);
        set => SetValue(DayLabelFontSizeProperty, value);
    }

    public double WeekNumberFontSize
    {
        get => (double)GetValue(WeekNumberFontSizeProperty);
        set => SetValue(WeekNumberFontSizeProperty, value);
    }

    public double MonthIndicatorFontSize
    {
        get => (double)GetValue(MonthIndicatorFontSizeProperty);
        set => SetValue(MonthIndicatorFontSizeProperty, value);
    }

    public double HeaderHeight
    {
        get => (double)GetValue(HeaderHeightProperty);
        set => SetValue(HeaderHeightProperty, value);
    }

    public double WeekdayHeaderHeight
    {
        get => (double)GetValue(WeekdayHeaderHeightProperty);
        set => SetValue(WeekdayHeaderHeightProperty, value);
    }

    public double DateRowHeight
    {
        get => (double)GetValue(DateRowHeightProperty);
        set => SetValue(DateRowHeightProperty, value);
    }

    public double SlotRowHeight
    {
        get => (double)GetValue(SlotRowHeightProperty);
        set => SetValue(SlotRowHeightProperty, value);
    }

    public double EventRowHeight
    {
        get => (double)GetValue(EventRowHeightProperty);
        set => SetValue(EventRowHeightProperty, value);
    }

    public double DateNumberSize
    {
        get => (double)GetValue(DateNumberSizeProperty);
        set => SetValue(DateNumberSizeProperty, value);
    }

    public double DateNumberMargin
    {
        get => (double)GetValue(DateNumberMarginProperty);
        set => SetValue(DateNumberMarginProperty, value);
    }

    public double DateNumberMarkWidth
    {
        get => (double)GetValue(DateNumberMarkWidthProperty);
        set => SetValue(DateNumberMarkWidthProperty, value);
    }

    public double StampMarginEdge
    {
        get => (double)GetValue(StampMarginEdgeProperty);
        set => SetValue(StampMarginEdgeProperty, value);
    }

    public double EventCornerRadius
    {
        get => (double)GetValue(EventCornerRadiusProperty);
        set => SetValue(EventCornerRadiusProperty, value);
    }

    public double EventPadding
    {
        get => (double)GetValue(EventPaddingProperty);
        set => SetValue(EventPaddingProperty, value);
    }

    public double EventDotSize
    {
        get => (double)GetValue(EventDotSizeProperty);
        set => SetValue(EventDotSizeProperty, value);
    }

    public double EventDotSpacing
    {
        get => (double)GetValue(EventDotSpacingProperty);
        set => SetValue(EventDotSpacingProperty, value);
    }

    public double NavigationButtonWidth
    {
        get => (double)GetValue(NavigationButtonWidthProperty);
        set => SetValue(NavigationButtonWidthProperty, value);
    }

    public double TodayButtonWidth
    {
        get => (double)GetValue(TodayButtonWidthProperty);
        set => SetValue(TodayButtonWidthProperty, value);
    }

    public double GridLineWidth
    {
        get => (double)GetValue(GridLineWidthProperty);
        set => SetValue(GridLineWidthProperty, value);
    }

    public double WeekNumberWidth
    {
        get => (double)GetValue(WeekNumberWidthProperty);
        set => SetValue(WeekNumberWidthProperty, value);
    }

    public Color GridLineColor
    {
        get => (Color)GetValue(GridLineColorProperty);
        set => SetValue(GridLineColorProperty, value);
    }

    public Color WeekdayTextColor
    {
        get => (Color)GetValue(WeekdayTextColorProperty);
        set => SetValue(WeekdayTextColorProperty, value);
    }

    public Color SaturdayTextColor
    {
        get => (Color)GetValue(SaturdayTextColorProperty);
        set => SetValue(SaturdayTextColorProperty, value);
    }

    public Color SundayTextColor
    {
        get => (Color)GetValue(SundayTextColorProperty);
        set => SetValue(SundayTextColorProperty, value);
    }

    public Color HolidayTextColor
    {
        get => (Color)GetValue(HolidayTextColorProperty);
        set => SetValue(HolidayTextColorProperty, value);
    }

    public Color OutsideMonthTextColor
    {
        get => (Color)GetValue(OutsideMonthTextColorProperty);
        set => SetValue(OutsideMonthTextColorProperty, value);
    }

    public Color OutsideMonthBackground
    {
        get => (Color)GetValue(OutsideMonthBackgroundProperty);
        set => SetValue(OutsideMonthBackgroundProperty, value);
    }

    public Color WeekendBackground
    {
        get => (Color)GetValue(WeekendBackgroundProperty);
        set => SetValue(WeekendBackgroundProperty, value);
    }

    public Color HolidayBackground
    {
        get => (Color)GetValue(HolidayBackgroundProperty);
        set => SetValue(HolidayBackgroundProperty, value);
    }

    public Color TodayBackground
    {
        get => (Color)GetValue(TodayBackgroundProperty);
        set => SetValue(TodayBackgroundProperty, value);
    }

    public Color TodayTextColor
    {
        get => (Color)GetValue(TodayTextColorProperty);
        set => SetValue(TodayTextColorProperty, value);
    }

    public Color SelectedDayBackground
    {
        get => (Color)GetValue(SelectedDayBackgroundProperty);
        set => SetValue(SelectedDayBackgroundProperty, value);
    }

    public Color SelectedDayTextColor
    {
        get => (Color)GetValue(SelectedDayTextColorProperty);
        set => SetValue(SelectedDayTextColorProperty, value);
    }

    public Color RangeBackground
    {
        get => (Color)GetValue(RangeBackgroundProperty);
        set => SetValue(RangeBackgroundProperty, value);
    }

    public Color DisabledDayTextColor
    {
        get => (Color)GetValue(DisabledDayTextColorProperty);
        set => SetValue(DisabledDayTextColorProperty, value);
    }

    public Color DayLabelTextColor
    {
        get => (Color)GetValue(DayLabelTextColorProperty);
        set => SetValue(DayLabelTextColorProperty, value);
    }

    public Color OverflowTextColor
    {
        get => (Color)GetValue(OverflowTextColorProperty);
        set => SetValue(OverflowTextColorProperty, value);
    }

    public Color StampColor
    {
        get => (Color)GetValue(StampColorProperty);
        set => SetValue(StampColorProperty, value);
    }

    public Color WeekNumberTextColor
    {
        get => (Color)GetValue(WeekNumberTextColorProperty);
        set => SetValue(WeekNumberTextColorProperty, value);
    }

    public Color HeaderBackground
    {
        get => (Color)GetValue(HeaderBackgroundProperty);
        set => SetValue(HeaderBackgroundProperty, value);
    }

    public Color HeaderTextColor
    {
        get => (Color)GetValue(HeaderTextColorProperty);
        set => SetValue(HeaderTextColorProperty, value);
    }

    public Color NavigationButtonColor
    {
        get => (Color)GetValue(NavigationButtonColorProperty);
        set => SetValue(NavigationButtonColorProperty, value);
    }

    public Color WeekdayHeaderBackground
    {
        get => (Color)GetValue(WeekdayHeaderBackgroundProperty);
        set => SetValue(WeekdayHeaderBackgroundProperty, value);
    }

    public Color WeekdayHeaderTextColor
    {
        get => (Color)GetValue(WeekdayHeaderTextColorProperty);
        set => SetValue(WeekdayHeaderTextColorProperty, value);
    }

    public Color SaturdayHeaderTextColor
    {
        get => (Color)GetValue(SaturdayHeaderTextColorProperty);
        set => SetValue(SaturdayHeaderTextColorProperty, value);
    }

    public Color SundayHeaderTextColor
    {
        get => (Color)GetValue(SundayHeaderTextColorProperty);
        set => SetValue(SundayHeaderTextColorProperty, value);
    }

    public Color MonthIndicatorColor
    {
        get => (Color)GetValue(MonthIndicatorColorProperty);
        set => SetValue(MonthIndicatorColorProperty, value);
    }

    public CalendarMarkStyle TodayMarkStyle
    {
        get => (CalendarMarkStyle)GetValue(TodayMarkStyleProperty);
        set => SetValue(TodayMarkStyleProperty, value);
    }

    public CalendarMarkStyle SelectedMarkStyle
    {
        get => (CalendarMarkStyle)GetValue(SelectedMarkStyleProperty);
        set => SetValue(SelectedMarkStyleProperty, value);
    }

    public bool TodayMarkOnSelected
    {
        get => (bool)GetValue(TodayMarkOnSelectedProperty);
        set => SetValue(TodayMarkOnSelectedProperty, value);
    }

    public CalendarDateNumberShape DateNumberShape
    {
        get => (CalendarDateNumberShape)GetValue(DateNumberShapeProperty);
        set => SetValue(DateNumberShapeProperty, value);
    }

    public bool DateNumberBold
    {
        get => (bool)GetValue(DateNumberBoldProperty);
        set => SetValue(DateNumberBoldProperty, value);
    }

    public CalendarGridLineMode GridLineMode
    {
        get => (CalendarGridLineMode)GetValue(GridLineModeProperty);
        set => SetValue(GridLineModeProperty, value);
    }

    public CalendarEventDisplayMode EventDisplayMode
    {
        get => (CalendarEventDisplayMode)GetValue(EventDisplayModeProperty);
        set => SetValue(EventDisplayModeProperty, value);
    }

    public int MaxVisibleDots
    {
        get => (int)GetValue(MaxVisibleDotsProperty);
        set => SetValue(MaxVisibleDotsProperty, value);
    }

    internal CalendarStyle EffectiveStyle
    {
        get => field ??= BuildStyle();
        private set;
    }

    internal static void OnStyleFontChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (ClamCalendarView)bindable;
        view.EffectiveStyle = null!;
        view.renderer?.Dispose();
        view.renderer = null;
        view.CancelInput();
        view.InvalidateLayout();
    }

    internal static void OnStyleChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (ClamCalendarView)bindable;
        view.RefreshStyle();
        view.CancelInput();
        view.InvalidateLayout();
    }

    internal static void OnStyleRenderChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (ClamCalendarView)bindable;
        view.RefreshStyle();
        view.InvalidateSurface();
    }

    private static bool ValidatePositive(object? value, string name) =>
        (value is not double number) || (Double.IsFinite(number) && (number > 0)) ? true : throw new ArgumentOutOfRangeException(name, value, "The value must be a positive finite value.");

    private static bool ValidateSize(object? value, string name) =>
        (value is not double number) || (Double.IsFinite(number) && (number >= 0)) ? true : throw new ArgumentOutOfRangeException(name, value, "The value must be a finite value of zero or more.");

    private static bool ValidateCount(object? value, string name) =>
        (value is not int count) || (count >= 0) ? true : throw new ArgumentOutOfRangeException(name, value, "The value must be zero or more.");

    private void RefreshStyle()
    {
        EffectiveStyle = null!;
        renderer?.UpdateStyle(EffectiveStyle);
    }

    private CalendarStyle BuildStyle() =>
        new()
        {
            FontFamily = FontFamily,
            HeaderFontSize = HeaderFontSize,
            WeekdayHeaderFontSize = WeekdayHeaderFontSize,
            DateNumberFontSize = DateNumberFontSize,
            EventFontSize = EventFontSize,
            DayLabelFontSize = DayLabelFontSize,
            WeekNumberFontSize = WeekNumberFontSize,
            MonthIndicatorFontSize = MonthIndicatorFontSize,
            HeaderHeight = HeaderHeight,
            WeekdayHeaderHeight = WeekdayHeaderHeight,
            DateRowHeight = DateRowHeight,
            SlotRowHeight = SlotRowHeight,
            EventRowHeight = EventRowHeight,
            DateNumberSize = DateNumberSize,
            DateNumberMargin = DateNumberMargin,
            DateNumberMarkWidth = DateNumberMarkWidth,
            StampMarginEdge = StampMarginEdge,
            EventCornerRadius = EventCornerRadius,
            EventPadding = EventPadding,
            EventDotSize = EventDotSize,
            EventDotSpacing = EventDotSpacing,
            NavigationButtonWidth = NavigationButtonWidth,
            TodayButtonWidth = TodayButtonWidth,
            GridLineWidth = GridLineWidth,
            WeekNumberWidth = WeekNumberWidth,
            Background = BackgroundColor ?? Colors.White,
            GridLineColor = GridLineColor,
            WeekdayTextColor = WeekdayTextColor,
            SaturdayTextColor = SaturdayTextColor,
            SundayTextColor = SundayTextColor,
            HolidayTextColor = HolidayTextColor,
            OutsideMonthTextColor = OutsideMonthTextColor,
            OutsideMonthBackground = OutsideMonthBackground,
            WeekendBackground = WeekendBackground,
            HolidayBackground = HolidayBackground,
            TodayBackground = TodayBackground,
            TodayTextColor = TodayTextColor,
            SelectedDayBackground = SelectedDayBackground,
            SelectedDayTextColor = SelectedDayTextColor,
            RangeBackground = RangeBackground,
            DisabledDayTextColor = DisabledDayTextColor,
            DayLabelTextColor = DayLabelTextColor,
            OverflowTextColor = OverflowTextColor,
            StampColor = StampColor,
            WeekNumberTextColor = WeekNumberTextColor,
            HeaderBackground = HeaderBackground,
            HeaderTextColor = HeaderTextColor,
            NavigationButtonColor = NavigationButtonColor,
            WeekdayHeaderBackground = WeekdayHeaderBackground,
            WeekdayHeaderTextColor = WeekdayHeaderTextColor,
            SaturdayHeaderTextColor = SaturdayHeaderTextColor,
            SundayHeaderTextColor = SundayHeaderTextColor,
            MonthIndicatorColor = MonthIndicatorColor,
            TodayMarkStyle = TodayMarkStyle,
            SelectedMarkStyle = SelectedMarkStyle,
            TodayMarkOnSelected = TodayMarkOnSelected,
            DateNumberShape = DateNumberShape,
            DateNumberBold = DateNumberBold,
            GridLineMode = GridLineMode,
            EventDisplayMode = EventDisplayMode,
            MaxVisibleDots = MaxVisibleDots
        };
}
