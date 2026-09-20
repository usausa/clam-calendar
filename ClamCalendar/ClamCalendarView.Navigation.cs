namespace ClamCalendar;

using System.Windows.Input;

public partial class ClamCalendarView
{
    public static readonly BindableProperty DisplayDateProperty = BindableProperty.Create(nameof(DisplayDate), typeof(DateOnly), typeof(ClamCalendarView), defaultValueCreator: static _ => DateOnly.FromDateTime(DateTime.Today), defaultBindingMode: BindingMode.TwoWay, propertyChanged: OnDisplayDateChanged);
    public static readonly BindableProperty MinDateProperty = BindableProperty.Create(nameof(MinDate), typeof(DateOnly?), typeof(ClamCalendarView), propertyChanged: OnRenderInputChanged);
    public static readonly BindableProperty MaxDateProperty = BindableProperty.Create(nameof(MaxDate), typeof(DateOnly?), typeof(ClamCalendarView), propertyChanged: OnRenderInputChanged);
    public static readonly BindableProperty SwipeEnabledProperty = BindableProperty.Create(nameof(SwipeEnabled), typeof(bool), typeof(ClamCalendarView), true);
    public static readonly BindableProperty NavigationAnimationEnabledProperty = BindableProperty.Create(nameof(NavigationAnimationEnabled), typeof(bool), typeof(ClamCalendarView), true);
    public static readonly BindableProperty DisplayDateChangedCommandProperty = BindableProperty.Create(nameof(DisplayDateChangedCommand), typeof(ICommand), typeof(ClamCalendarView), propertyChanged: OnDisplayDateChangedCommandChanged);

    private const string SlideAnimationName = "ClamCalendarViewMonthSlide";
    private const double SlideDistance = 48;
    private const uint SlideLength = 220;

    private int slideDirection;
    private double slideProgress = 1;
    private CalendarDateRange? announcedRange;

    public event EventHandler<CalendarDisplayDateChangedEventArgs>? DisplayDateChanged;

    // Year and month to show; the day is ignored
    public DateOnly DisplayDate
    {
        get => (DateOnly)GetValue(DisplayDateProperty);
        set => SetValue(DisplayDateProperty, value);
    }

    public DateOnly? MinDate
    {
        get => (DateOnly?)GetValue(MinDateProperty);
        set => SetValue(MinDateProperty, value);
    }

    public DateOnly? MaxDate
    {
        get => (DateOnly?)GetValue(MaxDateProperty);
        set => SetValue(MaxDateProperty, value);
    }

    public bool SwipeEnabled
    {
        get => (bool)GetValue(SwipeEnabledProperty);
        set => SetValue(SwipeEnabledProperty, value);
    }

    public bool NavigationAnimationEnabled
    {
        get => (bool)GetValue(NavigationAnimationEnabledProperty);
        set => SetValue(NavigationAnimationEnabledProperty, value);
    }

    public ICommand? DisplayDateChangedCommand
    {
        get => (ICommand?)GetValue(DisplayDateChangedCommandProperty);
        set => SetValue(DisplayDateChangedCommandProperty, value);
    }

    private float SlideOffset => (float)(slideDirection * SlideDistance * (1 - slideProgress));

    private float SlideOpacity => slideDirection == 0 ? 1 : (float)(0.4 + (0.6 * slideProgress));

    // Shows the month of today, or the nearest month inside MinDate and MaxDate
    public void GoToToday()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        DisplayDate = CalendarNavigation.ClampMonth(EffectiveToday, MinDate, MaxDate);
    }

    // Moves by months and stops at the months of MinDate and MaxDate
    public void Navigate(int months)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (CalendarNavigation.OffsetMonth(DisplayDate, months) is { } target)
        {
            DisplayDate = CalendarNavigation.ClampMonth(target, MinDate, MaxDate);
        }
    }

    public bool CanNavigate(int months) =>
        (CalendarNavigation.OffsetMonth(DisplayDate, months) is { } target) && CalendarNavigation.IsMonthWithinLimits(target, MinDate, MaxDate);

    private static void OnDisplayDateChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (ClamCalendarView)bindable;
        var direction = CalendarNavigation.MonthIndex((DateOnly)newValue).CompareTo(CalendarNavigation.MonthIndex((DateOnly)oldValue));
        if (direction == 0)
        {
            return;
        }

        view.CancelInput();
        view.RebuildMonth();
        view.StartSlide(direction);
    }

    // A command assigned while the view is attached receives the current range at once, so a late binding still loads the first month
    private static void OnDisplayDateChangedCommandChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (ClamCalendarView)bindable;
        if (!view.disposed && (view.Handler is not null) && (newValue is ICommand command))
        {
            ExecuteCommand(command, view.CreateDisplayDateChangedEventArgs());
        }
    }

    private void AnnounceDisplayRange(bool force)
    {
        var range = new CalendarDateRange(Month.FirstDate, Month.LastDate);
        if (!force && (announcedRange == range))
        {
            return;
        }

        announcedRange = range;
        var args = CreateDisplayDateChangedEventArgs();
        DisplayDateChanged?.Invoke(this, args);
        if (DisplayDateChangedCommand is { } command)
        {
            ExecuteCommand(command, args);
        }
    }

    private CalendarDisplayDateChangedEventArgs CreateDisplayDateChangedEventArgs() =>
        new(DisplayDate, Month.FirstDate, Month.LastDate);

    private static void ExecuteCommand(ICommand command, CalendarDisplayDateChangedEventArgs args)
    {
        if (command.CanExecute(args))
        {
            command.Execute(args);
        }
    }

    private void StartSlide(int direction)
    {
        AbortSlide();
        if (!NavigationAnimationEnabled || (Handler is null) || (direction == 0))
        {
            return;
        }

        slideDirection = direction;
        slideProgress = 0;
        this.Animate(SlideAnimationName, OnSlideFrame, 16, SlideLength, Easing.CubicOut, OnSlideFinished);
    }

    private void OnSlideFrame(double progress)
    {
        slideProgress = progress;
        InvalidateSurface();
    }

    private void OnSlideFinished(double progress, bool canceled)
    {
        slideDirection = 0;
        slideProgress = 1;
        if (!disposed)
        {
            InvalidateSurface();
        }
    }

    private void AbortSlide()
    {
        if (slideDirection != 0)
        {
            slideDirection = 0;
            slideProgress = 1;
            this.AbortAnimation(SlideAnimationName);
        }
    }
}
