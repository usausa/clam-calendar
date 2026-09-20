namespace Example.Modules.Calendar;

public sealed class CalendarMenuViewModel : AppViewModelBase
{
    public IObserveCommand ForwardCommand { get; }

    public CalendarMenuViewModel()
    {
        ForwardCommand = MakeAsyncCommand<ViewId>(x => Navigator.ForwardAsync(x));
    }

    protected override Task OnNotifyBackAsync()
    {
        AndroidHelper.MoveTaskToBack();
        return Task.CompletedTask;
    }

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
