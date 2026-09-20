namespace ClamCalendar;

using System.Globalization;

using ClamCalendar.Layout;
using ClamCalendar.Platform;
using ClamCalendar.Rendering;

using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

public partial class ClamCalendarView : SKCanvasView, IDisposable
{
    public static readonly BindableProperty EventsProperty = BindableProperty.Create(nameof(Events), typeof(IEnumerable<CalendarEvent>), typeof(ClamCalendarView), propertyChanged: OnEventsChanged);
    public static readonly BindableProperty StampsProperty = BindableProperty.Create(nameof(Stamps), typeof(IEnumerable<CalendarStamp>), typeof(ClamCalendarView), propertyChanged: OnStampsChanged);
    public static readonly BindableProperty HolidaysProperty = BindableProperty.Create(nameof(Holidays), typeof(IEnumerable<DateOnly>), typeof(ClamCalendarView), propertyChanged: OnHolidaysChanged);
    public static readonly BindableProperty DayLabelsProperty = BindableProperty.Create(nameof(DayLabels), typeof(IReadOnlyDictionary<DateOnly, string>), typeof(ClamCalendarView), propertyChanged: OnMonthInputChanged);
    public static readonly BindableProperty TodayProperty = BindableProperty.Create(nameof(Today), typeof(DateOnly?), typeof(ClamCalendarView), propertyChanged: OnMonthInputChanged);
    public static readonly BindableProperty FirstDayOfWeekProperty = BindableProperty.Create(nameof(FirstDayOfWeek), typeof(DayOfWeek), typeof(ClamCalendarView), DayOfWeek.Monday, propertyChanged: OnMonthInputChanged);
    public static readonly BindableProperty CultureProperty = BindableProperty.Create(nameof(Culture), typeof(CultureInfo), typeof(ClamCalendarView), propertyChanged: OnRenderInputChanged);
    public static readonly BindableProperty FixedWeekRowsProperty = BindableProperty.Create(nameof(FixedWeekRows), typeof(bool), typeof(ClamCalendarView), true, propertyChanged: OnMonthInputChanged);
    public static readonly BindableProperty OutsideMonthDaysVisibleProperty = BindableProperty.Create(nameof(OutsideMonthDaysVisible), typeof(bool), typeof(ClamCalendarView), true, propertyChanged: OnMonthInputChanged);
    public static readonly BindableProperty MaxVisibleSlotsProperty = BindableProperty.Create(nameof(MaxVisibleSlots), typeof(int), typeof(ClamCalendarView), 0, validateValue: static (_, value) => (int)value >= 0, propertyChanged: OnMonthInputChanged);
    public static readonly BindableProperty HeaderVisibleProperty = BindableProperty.Create(nameof(HeaderVisible), typeof(bool), typeof(ClamCalendarView), true, propertyChanged: OnLayoutInputChanged);
    public static readonly BindableProperty NavigationButtonsVisibleProperty = BindableProperty.Create(nameof(NavigationButtonsVisible), typeof(bool), typeof(ClamCalendarView), true, propertyChanged: OnLayoutInputChanged);
    public static readonly BindableProperty TodayButtonVisibleProperty = BindableProperty.Create(nameof(TodayButtonVisible), typeof(bool), typeof(ClamCalendarView), false, propertyChanged: OnLayoutInputChanged);
    public static readonly BindableProperty HeaderFormatProperty = BindableProperty.Create(nameof(HeaderFormat), typeof(string), typeof(ClamCalendarView), propertyChanged: OnRenderInputChanged);
    public static readonly BindableProperty PrevButtonTextProperty = BindableProperty.Create(nameof(PrevButtonText), typeof(string), typeof(ClamCalendarView), "◀", propertyChanged: OnRenderInputChanged);
    public static readonly BindableProperty NextButtonTextProperty = BindableProperty.Create(nameof(NextButtonText), typeof(string), typeof(ClamCalendarView), "▶", propertyChanged: OnRenderInputChanged);
    public static readonly BindableProperty TodayButtonTextProperty = BindableProperty.Create(nameof(TodayButtonText), typeof(string), typeof(ClamCalendarView), "Today", propertyChanged: OnRenderInputChanged);
    public static readonly BindableProperty WeekdayHeaderVisibleProperty = BindableProperty.Create(nameof(WeekdayHeaderVisible), typeof(bool), typeof(ClamCalendarView), true, propertyChanged: OnLayoutInputChanged);
    public static readonly BindableProperty WeekdayNameFormatProperty = BindableProperty.Create(nameof(WeekdayNameFormat), typeof(CalendarWeekdayNameFormat), typeof(ClamCalendarView), CalendarWeekdayNameFormat.Initial, propertyChanged: OnRenderInputChanged);
    public static readonly BindableProperty MonthIndicatorVisibleProperty = BindableProperty.Create(nameof(MonthIndicatorVisible), typeof(bool), typeof(ClamCalendarView), false, propertyChanged: OnRenderInputChanged);
    public static readonly BindableProperty CalendarStyleProperty = BindableProperty.Create(nameof(CalendarStyle), typeof(CalendarStyle), typeof(ClamCalendarView), defaultValueCreator: static _ => new CalendarStyle(), propertyChanged: OnStyleChanged);

    private readonly CalendarCollectionWatcher eventsWatcher;
    private readonly CalendarCollectionWatcher stampsWatcher;
    private readonly CalendarCollectionWatcher holidaysWatcher;
    private CalendarRenderer? renderer;
    private CalendarLayout? currentLayout;
    private ICalendarPlatformBridge? platform;
    private bool layoutDirty = true;
    private bool disposed;

    public IEnumerable<CalendarEvent>? Events
    {
        get => (IEnumerable<CalendarEvent>?)GetValue(EventsProperty);
        set => SetValue(EventsProperty, value);
    }

    public IEnumerable<CalendarStamp>? Stamps
    {
        get => (IEnumerable<CalendarStamp>?)GetValue(StampsProperty);
        set => SetValue(StampsProperty, value);
    }

    public IEnumerable<DateOnly>? Holidays
    {
        get => (IEnumerable<DateOnly>?)GetValue(HolidaysProperty);
        set => SetValue(HolidaysProperty, value);
    }

    public IReadOnlyDictionary<DateOnly, string>? DayLabels
    {
        get => (IReadOnlyDictionary<DateOnly, string>?)GetValue(DayLabelsProperty);
        set => SetValue(DayLabelsProperty, value);
    }

    // Overrides the system date used for the today mark and GoToToday
    public DateOnly? Today
    {
        get => (DateOnly?)GetValue(TodayProperty);
        set => SetValue(TodayProperty, value);
    }

    public DayOfWeek FirstDayOfWeek
    {
        get => (DayOfWeek)GetValue(FirstDayOfWeekProperty);
        set => SetValue(FirstDayOfWeekProperty, value);
    }

    public CultureInfo? Culture
    {
        get => (CultureInfo?)GetValue(CultureProperty);
        set => SetValue(CultureProperty, value);
    }

    public bool FixedWeekRows
    {
        get => (bool)GetValue(FixedWeekRowsProperty);
        set => SetValue(FixedWeekRowsProperty, value);
    }

    public bool OutsideMonthDaysVisible
    {
        get => (bool)GetValue(OutsideMonthDaysVisibleProperty);
        set => SetValue(OutsideMonthDaysVisibleProperty, value);
    }

    public int MaxVisibleSlots
    {
        get => (int)GetValue(MaxVisibleSlotsProperty);
        set => SetValue(MaxVisibleSlotsProperty, value);
    }

    public bool HeaderVisible
    {
        get => (bool)GetValue(HeaderVisibleProperty);
        set => SetValue(HeaderVisibleProperty, value);
    }

    public bool NavigationButtonsVisible
    {
        get => (bool)GetValue(NavigationButtonsVisibleProperty);
        set => SetValue(NavigationButtonsVisibleProperty, value);
    }

    public bool TodayButtonVisible
    {
        get => (bool)GetValue(TodayButtonVisibleProperty);
        set => SetValue(TodayButtonVisibleProperty, value);
    }

    public string? HeaderFormat
    {
        get => (string?)GetValue(HeaderFormatProperty);
        set => SetValue(HeaderFormatProperty, value);
    }

    public string PrevButtonText
    {
        get => (string)GetValue(PrevButtonTextProperty);
        set => SetValue(PrevButtonTextProperty, value);
    }

    public string NextButtonText
    {
        get => (string)GetValue(NextButtonTextProperty);
        set => SetValue(NextButtonTextProperty, value);
    }

    public string TodayButtonText
    {
        get => (string)GetValue(TodayButtonTextProperty);
        set => SetValue(TodayButtonTextProperty, value);
    }

    public bool WeekdayHeaderVisible
    {
        get => (bool)GetValue(WeekdayHeaderVisibleProperty);
        set => SetValue(WeekdayHeaderVisibleProperty, value);
    }

    public CalendarWeekdayNameFormat WeekdayNameFormat
    {
        get => (CalendarWeekdayNameFormat)GetValue(WeekdayNameFormatProperty);
        set => SetValue(WeekdayNameFormatProperty, value);
    }

    public bool MonthIndicatorVisible
    {
        get => (bool)GetValue(MonthIndicatorVisibleProperty);
        set => SetValue(MonthIndicatorVisibleProperty, value);
    }

    public CalendarStyle CalendarStyle
    {
        get => (CalendarStyle)GetValue(CalendarStyleProperty);
        set => SetValue(CalendarStyleProperty, value);
    }

    // Month model built from DisplayDate and the data properties
    public CalendarMonth Month { get; private set; } = default!;

    public DateOnly EffectiveToday => Today ?? DateOnly.FromDateTime(DateTime.Today);

    public ClamCalendarView()
    {
        eventsWatcher = new CalendarCollectionWatcher(RebuildMonth);
        stampsWatcher = new CalendarCollectionWatcher(RebuildMonth);
        holidaysWatcher = new CalendarCollectionWatcher(RebuildMonth);
        disabledDatesWatcher = new CalendarCollectionWatcher(UpdateDisabledDates);
        selectedDatesWatcher = new CalendarCollectionWatcher(OnSelectedDatesCollectionChanged);
        selectedDatesWatcher.Watch(SelectedDates);
        BackgroundColor = Colors.White;
        EnableTouchEvents = true;
        Touch += OnCalendarTouch;
        SizeChanged += OnSizeChanged;
        Unloaded += OnUnloaded;
        RebuildMonth();
    }

    //--------------------------------------------------------------------------------
    // Platform
    //--------------------------------------------------------------------------------

    partial void AttachPlatform();

    private void DetachPlatform()
    {
        platform?.Dispose();
        platform = null;
    }

    private void SetParentIntercept(bool allow) => platform?.SetParentIntercept(allow);

    //--------------------------------------------------------------------------------
    // Lifecycle
    //--------------------------------------------------------------------------------

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing || disposed)
        {
            return;
        }

        disposed = true;
        CancelInput(true);
        AbortSlide();
        if (inputTimer is not null)
        {
            inputTimer.Tick -= OnInputTick;
            inputTimer = null;
        }

        DeactivateWatchers();
        DetachPlatform();
        renderer?.Dispose();
        renderer = null;
        Touch -= OnCalendarTouch;
        SizeChanged -= OnSizeChanged;
        Unloaded -= OnUnloaded;
    }

    protected override void OnHandlerChanging(HandlerChangingEventArgs args)
    {
        CancelInput(true);
        AbortSlide();
        DeactivateWatchers();
        DetachPlatform();
        renderer?.Dispose();
        renderer = null;
        layoutDirty = true;
        base.OnHandlerChanging(args);
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        if (!disposed && (Handler is not null))
        {
            ActivateWatchers();
            UpdateDisabledDates();
            RebuildMonth(false);
            AnnounceDisplayRange(true);
        }

        AttachPlatform();
        InvalidateSurface();
    }

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if ((propertyName is nameof(IsEnabled) or nameof(IsVisible)) && (!IsEnabled || !IsVisible))
        {
            CancelInput(true);
        }
    }

    // Redraws after the content of the style was changed in place
    public void Invalidate()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        renderer?.Dispose();
        renderer = null;
        InvalidateLayout();
    }

    //--------------------------------------------------------------------------------
    // Render
    //--------------------------------------------------------------------------------

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        EnsureLayout();
        if ((currentLayout is not { } layout) || (renderer is not { } currentRenderer) || (Width <= 0) || (Height <= 0))
        {
            e.Surface.Canvas.Clear();
            return;
        }

        var canvas = e.Surface.Canvas;
        canvas.Save();
        try
        {
            canvas.Scale((float)(e.Info.Width / Width), (float)(e.Info.Height / Height));
            currentRenderer.Render(canvas, layout, CreateRenderState());
        }
        finally
        {
            canvas.Restore();
        }

        base.OnPaintSurface(e);
    }

    internal CalendarRenderState CreateRenderState() => new()
    {
        Culture = Culture ?? CultureInfo.CurrentCulture,
        HeaderFormat = HeaderFormat,
        WeekdayNameFormat = WeekdayNameFormat,
        PrevButtonText = PrevButtonText,
        NextButtonText = NextButtonText,
        TodayButtonText = TodayButtonText,
        CanNavigateBackward = CanNavigate(-1),
        CanNavigateForward = CanNavigate(1),
        MonthIndicatorVisible = MonthIndicatorVisible,
        OutsideMonthDaysVisible = OutsideMonthDaysVisible,
        IsSelected = IsDateSelected,
        IsInRange = IsDateInSelectedRange,
        IsDisabled = IsDateDisabled,
        SlideOffset = SlideOffset,
        SlideOpacity = SlideOpacity
    };

    //--------------------------------------------------------------------------------
    // Month
    //--------------------------------------------------------------------------------

    private void RebuildMonth() => RebuildMonth(true);

    private void RebuildMonth(bool announce)
    {
        if (disposed)
        {
            return;
        }

        var builder = new CalendarMonthBuilder
        {
            FirstDayOfWeek = FirstDayOfWeek,
            FixedWeekRows = FixedWeekRows,
            OutsideMonthDaysVisible = OutsideMonthDaysVisible,
            MaxVisibleSlots = MaxVisibleSlots
        };
        var display = DisplayDate;
        Month = builder.Build(display.Year, display.Month, EffectiveToday, Events, Stamps, Holidays, DayLabels);
        OnPropertyChanged(nameof(Month));
        InvalidateLayout();
        if (announce)
        {
            AnnounceDisplayRange(false);
        }
    }

    private void ActivateWatchers()
    {
        eventsWatcher.Activate();
        stampsWatcher.Activate();
        holidaysWatcher.Activate();
        disabledDatesWatcher.Activate();
        selectedDatesWatcher.Activate();
    }

    private void DeactivateWatchers()
    {
        eventsWatcher.Deactivate();
        stampsWatcher.Deactivate();
        holidaysWatcher.Deactivate();
        disabledDatesWatcher.Deactivate();
        selectedDatesWatcher.Deactivate();
    }

    private static void OnEventsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (ClamCalendarView)bindable;
        view.eventsWatcher.Watch(newValue);
        view.RebuildMonth();
    }

    private static void OnStampsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (ClamCalendarView)bindable;
        view.stampsWatcher.Watch(newValue);
        view.RebuildMonth();
    }

    private static void OnHolidaysChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (ClamCalendarView)bindable;
        view.holidaysWatcher.Watch(newValue);
        view.RebuildMonth();
    }

    private static void OnMonthInputChanged(BindableObject bindable, object oldValue, object newValue) =>
        ((ClamCalendarView)bindable).RebuildMonth();

    private static void OnLayoutInputChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (ClamCalendarView)bindable;
        view.CancelInput();
        view.InvalidateLayout();
    }

    private static void OnRenderInputChanged(BindableObject bindable, object oldValue, object newValue) =>
        ((ClamCalendarView)bindable).InvalidateSurface();

    private static void OnStyleChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (ClamCalendarView)bindable;
        view.renderer?.Dispose();
        view.renderer = null;
        view.CancelInput();
        view.InvalidateLayout();
    }

    private void OnSizeChanged(object? sender, EventArgs e)
    {
        CancelInput();
        InvalidateLayout();
    }

    private void OnUnloaded(object? sender, EventArgs e) => CancelInput(true);

    private void InvalidateLayout()
    {
        layoutDirty = true;
        InvalidateSurface();
    }

    private void EnsureLayout()
    {
        if (disposed || (Width <= 0) || (Height <= 0) || (!layoutDirty && (currentLayout is not null)))
        {
            return;
        }

        renderer ??= new CalendarRenderer(CalendarStyle);
        currentLayout = new CalendarLayout(Month, CalendarStyle, Width, Height, HeaderVisible, NavigationButtonsVisible, TodayButtonVisible, WeekdayHeaderVisible);
        layoutDirty = false;
    }

    internal CalendarHit HitTest(double x, double y)
    {
        EnsureLayout();
        return currentLayout?.HitTest(x, y) ?? CalendarHit.None;
    }
}
