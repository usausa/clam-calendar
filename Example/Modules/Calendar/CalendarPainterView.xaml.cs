namespace Example.Modules.Calendar;

[View(ViewId.CalendarPainter)]
public sealed partial class CalendarPainterView
{
    public CalendarPainterView()
    {
        InitializeComponent();
        Calendar.FrameRendered += OnFrameRendered;
        Calendar.DayPainter = PaintDay;
    }

    private void OnFrameRendered(object? sender, CalendarFrameEventArgs e)
    {
        if (BindingContext is CalendarPainterViewModel viewModel)
        {
            viewModel.UpdateStatistics(e);
        }
    }

    private void PaintDay(SKCanvas canvas, CalendarDayPaintContext context)
    {
        if (BindingContext is CalendarPainterViewModel viewModel)
        {
            viewModel.PaintDay(canvas, context);
        }
    }
}
