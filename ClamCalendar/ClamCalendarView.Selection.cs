namespace ClamCalendar;

using System.Collections.ObjectModel;

public partial class ClamCalendarView
{
    public static readonly BindableProperty SelectionModeProperty = BindableProperty.Create(nameof(SelectionMode), typeof(CalendarSelectionMode), typeof(ClamCalendarView), CalendarSelectionMode.None, propertyChanged: OnRenderInputChanged);
    public static readonly BindableProperty SelectedDateProperty = BindableProperty.Create(nameof(SelectedDate), typeof(DateOnly?), typeof(ClamCalendarView), defaultBindingMode: BindingMode.TwoWay, propertyChanged: OnSelectionValueChanged);
    public static readonly BindableProperty SelectedDatesProperty = BindableProperty.Create(nameof(SelectedDates), typeof(ObservableCollection<DateOnly>), typeof(ClamCalendarView), defaultValueCreator: static _ => new ObservableCollection<DateOnly>(), propertyChanged: OnSelectedDatesChanged);
    public static readonly BindableProperty SelectedStartDateProperty = BindableProperty.Create(nameof(SelectedStartDate), typeof(DateOnly?), typeof(ClamCalendarView), defaultBindingMode: BindingMode.TwoWay, propertyChanged: OnSelectionValueChanged);
    public static readonly BindableProperty SelectedEndDateProperty = BindableProperty.Create(nameof(SelectedEndDate), typeof(DateOnly?), typeof(ClamCalendarView), defaultBindingMode: BindingMode.TwoWay, propertyChanged: OnSelectionValueChanged);
    public static readonly BindableProperty AllowDeselectProperty = BindableProperty.Create(nameof(AllowDeselect), typeof(bool), typeof(ClamCalendarView), true);
    public static readonly BindableProperty MaxSelectableDaysProperty = BindableProperty.Create(nameof(MaxSelectableDays), typeof(int), typeof(ClamCalendarView), 0, validateValue: static (_, value) => (int)value >= 0);
    public static readonly BindableProperty DisabledDatesProperty = BindableProperty.Create(nameof(DisabledDates), typeof(IEnumerable<DateOnly>), typeof(ClamCalendarView), propertyChanged: OnDisabledDatesChanged);

    private readonly CalendarCollectionWatcher disabledDatesWatcher;
    private readonly CalendarCollectionWatcher selectedDatesWatcher;
    private HashSet<DateOnly>? disabledDateSet;
    private bool selectionUpdating;

    public event EventHandler? SelectionChanged;

    public CalendarSelectionMode SelectionMode
    {
        get => (CalendarSelectionMode)GetValue(SelectionModeProperty);
        set => SetValue(SelectionModeProperty, value);
    }

    public DateOnly? SelectedDate
    {
        get => (DateOnly?)GetValue(SelectedDateProperty);
        set => SetValue(SelectedDateProperty, value);
    }

    // Selection of MultipleDates; bind another collection through the bindable property to share it with a view model
    public ObservableCollection<DateOnly> SelectedDates => (ObservableCollection<DateOnly>)GetValue(SelectedDatesProperty);

    public DateOnly? SelectedStartDate
    {
        get => (DateOnly?)GetValue(SelectedStartDateProperty);
        set => SetValue(SelectedStartDateProperty, value);
    }

    public DateOnly? SelectedEndDate
    {
        get => (DateOnly?)GetValue(SelectedEndDateProperty);
        set => SetValue(SelectedEndDateProperty, value);
    }

    public bool AllowDeselect
    {
        get => (bool)GetValue(AllowDeselectProperty);
        set => SetValue(AllowDeselectProperty, value);
    }

    public int MaxSelectableDays
    {
        get => (int)GetValue(MaxSelectableDaysProperty);
        set => SetValue(MaxSelectableDaysProperty, value);
    }

    public IEnumerable<DateOnly>? DisabledDates
    {
        get => (IEnumerable<DateOnly>?)GetValue(DisabledDatesProperty);
        set => SetValue(DisabledDatesProperty, value);
    }

    public bool IsDateDisabled(DateOnly date) => CalendarSelectionLogic.IsDisabled(date, MinDate, MaxDate, disabledDateSet);

    public bool IsDateSelected(DateOnly date) => SelectionMode switch
    {
        CalendarSelectionMode.SingleDate => SelectedDate == date,
        CalendarSelectionMode.MultipleDates => SelectedDates.Contains(date),
        CalendarSelectionMode.DateRange => CalendarSelectionLogic.IsInRange(date, SelectedStartDate, SelectedEndDate),
        _ => false
    };

    public void ClearSelection()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        selectionUpdating = true;
        try
        {
            SelectedDate = null;
            SelectedDates.Clear();
            SelectedStartDate = null;
            SelectedEndDate = null;
        }
        finally
        {
            selectionUpdating = false;
        }

        OnSelectionChanged();
    }

    // The band behind the date numbers covers the whole range; the selected-day bubble is drawn over its ends
    internal bool IsDateInSelectedRange(DateOnly date) =>
        (SelectionMode == CalendarSelectionMode.DateRange) && CalendarSelectionLogic.IsInRange(date, SelectedStartDate, SelectedEndDate);

    // Applies a tap to the selection and reports whether it changed
    internal bool SelectDay(DateOnly date)
    {
        if (IsDateDisabled(date))
        {
            return false;
        }

        switch (SelectionMode)
        {
            case CalendarSelectionMode.SingleDate:
                var next = CalendarSelectionLogic.TapSingle(SelectedDate, date, AllowDeselect);
                if (next == SelectedDate)
                {
                    return false;
                }

                SelectedDate = next;
                return true;
            case CalendarSelectionMode.MultipleDates:
                bool toggled;
                selectionUpdating = true;
                try
                {
                    toggled = CalendarSelectionLogic.TapMultiple(SelectedDates, date, MaxSelectableDays);
                }
                finally
                {
                    selectionUpdating = false;
                }

                if (toggled)
                {
                    OnSelectionChanged();
                }

                return toggled;
            case CalendarSelectionMode.DateRange:
                var (start, end) = CalendarSelectionLogic.TapRange(SelectedStartDate, SelectedEndDate, date, MaxSelectableDays, IsDateDisabled);
                if ((start == SelectedStartDate) && (end == SelectedEndDate))
                {
                    return false;
                }

                selectionUpdating = true;
                try
                {
                    SelectedStartDate = start;
                    SelectedEndDate = end;
                }
                finally
                {
                    selectionUpdating = false;
                }

                OnSelectionChanged();
                return true;
            default:
                return false;
        }
    }

    private static void OnSelectionValueChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (ClamCalendarView)bindable;
        if (!view.selectionUpdating)
        {
            view.OnSelectionChanged();
        }
    }

    private static void OnSelectedDatesChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (ClamCalendarView)bindable;
        view.selectedDatesWatcher.Watch(newValue);
        view.OnSelectionChanged();
    }

    private static void OnDisabledDatesChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (ClamCalendarView)bindable;
        view.disabledDatesWatcher.Watch(newValue);
        view.UpdateDisabledDates();
    }

    private void OnSelectedDatesCollectionChanged()
    {
        if (!selectionUpdating)
        {
            OnSelectionChanged();
        }
    }

    private void UpdateDisabledDates()
    {
        disabledDateSet = DisabledDates is { } dates ? [.. dates] : null;
        InvalidateSurface();
    }

    private void OnSelectionChanged()
    {
        if (disposed)
        {
            return;
        }

        InvalidateSurface();
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }
}
