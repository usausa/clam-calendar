namespace ClamCalendar;

using System.Collections;
using System.ComponentModel;

public class ClamCalendarOverlay : AbsoluteLayout
{
    public static readonly BindableProperty CalendarProperty = BindableProperty.Create(nameof(Calendar), typeof(ClamCalendarView), typeof(ClamCalendarOverlay), propertyChanged: OnCalendarChanged);

    public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(ClamCalendarOverlay), propertyChanged: OnItemsSourceChanged);

    public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(ClamCalendarOverlay), propertyChanged: OnOverlayInputChanged);

    public static readonly BindableProperty PlacementProperty = BindableProperty.Create(nameof(Placement), typeof(CalendarOverlayPlacement), typeof(ClamCalendarOverlay), CalendarOverlayPlacement.Day, propertyChanged: OnOverlayInputChanged);

    public static readonly BindableProperty FollowSlideProperty = BindableProperty.Create(nameof(FollowSlide), typeof(bool), typeof(ClamCalendarOverlay), true, propertyChanged: OnFollowSlideChanged);

    private readonly List<DataTemplate> templates = [];

    private readonly CalendarCollectionWatcher itemsWatcher;

    private ClamCalendarView? calendar;

    private bool attached;

    private bool queued;

    public ClamCalendarView? Calendar
    {
        get => (ClamCalendarView?)GetValue(CalendarProperty);
        set => SetValue(CalendarProperty, value);
    }

    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public DataTemplate? ItemTemplate
    {
        get => (DataTemplate?)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    public CalendarOverlayPlacement Placement
    {
        get => (CalendarOverlayPlacement)GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    public bool FollowSlide
    {
        get => (bool)GetValue(FollowSlideProperty);
        set => SetValue(FollowSlideProperty, value);
    }

    public ClamCalendarOverlay()
    {
        InputTransparent = true;
        itemsWatcher = new CalendarCollectionWatcher(Invalidate);
        Unloaded += OnUnloaded;
    }

    //--------------------------------------------------------------------------------
    // Lifecycle
    //--------------------------------------------------------------------------------

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        if (Handler is not null)
        {
            attached = true;
            Subscribe();
            itemsWatcher.Activate();
            ApplySlide();
            Rebuild();
        }
    }

    private void OnUnloaded(object? sender, EventArgs e)
    {
        attached = false;
        Unsubscribe();
        itemsWatcher.Deactivate();
    }

    private void Subscribe()
    {
        if (attached && (calendar is not null))
        {
            calendar.SizeChanged += OnCalendarSizeChanged;
            calendar.PropertyChanged += OnCalendarPropertyChanged;
        }
    }

    private void Unsubscribe()
    {
        if (calendar is not null)
        {
            calendar.SizeChanged -= OnCalendarSizeChanged;
            calendar.PropertyChanged -= OnCalendarPropertyChanged;
        }
    }

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    private static void OnCalendarChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var overlay = (ClamCalendarOverlay)bindable;
        overlay.Unsubscribe();
        overlay.calendar = newValue as ClamCalendarView;
        overlay.Subscribe();
        overlay.ApplySlide();
        overlay.Invalidate();
    }

    private static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var overlay = (ClamCalendarOverlay)bindable;
        overlay.itemsWatcher.Watch(newValue);
        overlay.Invalidate();
    }

    private static void OnOverlayInputChanged(BindableObject bindable, object oldValue, object newValue) =>
        ((ClamCalendarOverlay)bindable).Invalidate();

    private static void OnFollowSlideChanged(BindableObject bindable, object oldValue, object newValue) =>
        ((ClamCalendarOverlay)bindable).ApplySlide();

    //--------------------------------------------------------------------------------
    // Calendar
    //--------------------------------------------------------------------------------

    private void OnCalendarSizeChanged(object? sender, EventArgs e) => Invalidate();

    private void OnCalendarPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ClamCalendarView.Month))
        {
            Invalidate();
        }
        else if ((e.PropertyName == nameof(ClamCalendarView.SlideOffset)) || (e.PropertyName == nameof(ClamCalendarView.SlideOpacity)))
        {
            ApplySlide();
        }
    }

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private void ApplySlide()
    {
        var follow = FollowSlide && (calendar is not null);
        TranslationX = follow ? calendar!.SlideOffset : 0;
        Opacity = follow ? calendar!.SlideOpacity : 1;
    }

    // The month is rebuilt before the application loads the items of the new range, so the rebuilds of one change are coalesced into one
    private void Invalidate()
    {
        if (queued)
        {
            return;
        }

        queued = true;
        Dispatcher.Dispatch(Rebuild);
    }

    private void Rebuild()
    {
        queued = false;

        var index = 0;
        if ((calendar is not null) && (ItemTemplate is not null) && (ItemsSource is not null))
        {
            foreach (var item in ItemsSource)
            {
                if ((item is not ICalendarOverlayItem overlayItem) || (Resolve(overlayItem.OverlayDate) is not { } bounds))
                {
                    continue;
                }

                var template = ItemTemplate is DataTemplateSelector selector ? selector.SelectTemplate(item, this) : ItemTemplate;
                if (Reuse(index, template) is not { } view)
                {
                    continue;
                }

                view.BindingContext = item;
                SetLayoutBounds((BindableObject)view, bounds);
                view.IsVisible = true;
                index++;
            }
        }

        // The views that are not used stay as hidden children, so the next month reuses them instead of building the templates again
        for (var i = index; i < Children.Count; i++)
        {
            if (Children[i] is View spare)
            {
                spare.IsVisible = false;
            }
        }
    }

    private View? Reuse(int index, DataTemplate template)
    {
        if ((index < Children.Count) && (templates[index] == template))
        {
            return Children[index] as View;
        }

        if (template.CreateContent() is not View view)
        {
            return null;
        }

        if (index < Children.Count)
        {
            Children[index] = view;
            templates[index] = template;
        }
        else
        {
            Children.Add(view);
            templates.Add(template);
        }

        return view;
    }

    private Rect? Resolve(DateOnly date) =>
        Placement == CalendarOverlayPlacement.DateNumber ? calendar!.GetDateNumberBounds(date) : calendar!.GetDayBounds(date);
}
