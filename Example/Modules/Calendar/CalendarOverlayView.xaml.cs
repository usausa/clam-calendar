namespace Example.Modules.Calendar;

[View(ViewId.CalendarOverlay)]
public sealed partial class CalendarOverlayView
{
    public CalendarOverlayView()
    {
        InitializeComponent();
        Calendar.FrameRendered += OnFrameRendered;
    }

    private void OnFrameRendered(object? sender, CalendarFrameEventArgs e)
    {
        if (BindingContext is CalendarOverlayViewModel viewModel)
        {
            viewModel.UpdateStatistics(e);
        }
    }
}
