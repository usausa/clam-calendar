namespace ClamCalendar.Tests.Input;

public sealed class CalendarGestureTests
{
    [Fact]
    public void SmallMovementRemainsATapButReleasingOutsideDoesNot()
    {
        // Arrange
        var input = new CalendarGestureController();
        input.Press(1, new Point(50, 50), 0);

        // Act & Assert
        Assert.Equal(CalendarGestureAction.None, input.Move(1, new Point(54, 55)).Action);
        Assert.Equal(CalendarGestureAction.Tap, input.Release(1, new Point(54, 55), true).Action);
        Assert.Equal(CalendarGestureState.Idle, input.State);

        // Arrange
        input.Press(1, new Point(50, 50), 100);

        // Act & Assert
        Assert.Equal(CalendarGestureAction.None, input.Release(1, new Point(52, 52), false).Action);
    }

    [Fact]
    public void HorizontalDragOfThirtySixBecomesASwipe()
    {
        // Arrange
        var input = new CalendarGestureController();
        input.Press(1, new Point(100, 100), 0);

        // Act & Assert
        Assert.Equal(CalendarGestureAction.None, input.Move(1, new Point(90, 101)).Action);
        Assert.Equal(CalendarGestureState.Swiping, input.State);
        Assert.Equal(CalendarGestureAction.None, input.Move(1, new Point(75, 102)).Action);
        Assert.Equal(new CalendarGestureResult(CalendarGestureAction.Swipe, 1), input.Move(1, new Point(60, 103)));
        Assert.Equal(CalendarGestureState.Completed, input.State);
        Assert.Equal(CalendarGestureAction.None, input.Move(1, new Point(20, 103)).Action);
        Assert.Equal(CalendarGestureAction.None, input.Release(1, new Point(20, 103), true).Action);
        Assert.Equal(CalendarGestureState.Idle, input.State);
    }

    [Fact]
    public void FlickTriggersTheSwipeBeforeTheDistanceThreshold()
    {
        // Arrange
        var input = new CalendarGestureController();
        input.Press(1, new Point(100, 100), 0);

        // Act & Assert
        Assert.Equal(CalendarGestureAction.None, input.Move(1, new Point(95, 100)).Action);
        Assert.Equal(new CalendarGestureResult(CalendarGestureAction.Swipe, -1), input.Move(1, new Point(118, 101)));
    }

    [Fact]
    public void ReleaseAfterAQuickDragCompletesTheSwipe()
    {
        // Arrange
        var input = new CalendarGestureController();
        input.Press(1, new Point(100, 100), 0);
        input.Move(1, new Point(85, 100));

        // Act & Assert
        Assert.Equal(new CalendarGestureResult(CalendarGestureAction.Swipe, 1), input.Release(1, new Point(60, 100), true));
        Assert.Equal(CalendarGestureState.Idle, input.State);
    }

    [Fact]
    public void VerticalMovementYieldsToTheParent()
    {
        // Arrange
        var input = new CalendarGestureController();
        input.Press(1, new Point(50, 50), 0);

        // Act & Assert
        Assert.Equal(CalendarGestureAction.Yield, input.Move(1, new Point(52, 80)).Action);
        Assert.Equal(CalendarGestureState.Yielded, input.State);
        Assert.Equal(CalendarGestureAction.None, input.Move(1, new Point(100, 80)).Action);
        Assert.Equal(CalendarGestureAction.None, input.Tick(600).Action);
        Assert.Equal(CalendarGestureAction.None, input.Release(1, new Point(100, 80), true).Action);
    }

    [Fact]
    public void HorizontalMovementYieldsWhenSwipingIsDisabled()
    {
        // Arrange
        var input = new CalendarGestureController();
        input.Press(1, new Point(50, 50), 0, false);

        // Act & Assert
        Assert.Equal(CalendarGestureAction.Yield, input.Move(1, new Point(10, 52)).Action);
        Assert.Equal(CalendarGestureAction.None, input.Release(1, new Point(0, 52), true).Action);
    }

    [Fact]
    public void DiagonalMovementIsNeitherTapNorSwipe()
    {
        // Arrange
        var input = new CalendarGestureController();
        input.Press(1, new Point(50, 50), 0);

        // Act & Assert
        Assert.Equal(CalendarGestureAction.None, input.Move(1, new Point(60, 60)).Action);
        Assert.Equal(CalendarGestureState.Moved, input.State);
        Assert.Equal(CalendarGestureAction.None, input.Tick(600).Action);
        Assert.Equal(CalendarGestureAction.None, input.Release(1, new Point(60, 60), true).Action);

        // Arrange
        input.Press(1, new Point(50, 50), 700);
        input.Move(1, new Point(60, 60));

        // Act & Assert
        Assert.Equal(new CalendarGestureResult(CalendarGestureAction.Swipe, 1), input.Move(1, new Point(0, 62)));
    }

    [Fact]
    public void LongPressFiresOnceWhileStill()
    {
        // Arrange
        var input = new CalendarGestureController();
        input.Press(1, new Point(50, 50), 0);

        // Act & Assert
        Assert.Equal(CalendarGestureAction.None, input.Tick(499).Action);
        Assert.Equal(CalendarGestureAction.LongPress, input.Tick(500).Action);
        Assert.Equal(CalendarGestureAction.None, input.Tick(600).Action);
        Assert.Equal(CalendarGestureAction.None, input.Move(1, new Point(0, 50)).Action);
        Assert.Equal(CalendarGestureAction.None, input.Release(1, new Point(0, 50), true).Action);
        Assert.Equal(CalendarGestureState.Idle, input.State);
    }

    [Fact]
    public void SecondPointerBlocksUntilAllPointersLift()
    {
        // Arrange
        var input = new CalendarGestureController();
        input.Press(1, new Point(50, 50), 0);

        // Act & Assert
        Assert.Equal(CalendarGestureAction.Canceled, input.Press(2, new Point(60, 60), 10).Action);
        Assert.Equal(CalendarGestureState.Blocked, input.State);
        Assert.Equal(CalendarGestureAction.None, input.Release(1, new Point(50, 50), true).Action);
        Assert.Equal(CalendarGestureAction.None, input.Move(2, new Point(0, 60)).Action);
        Assert.Equal(CalendarGestureAction.None, input.Release(2, new Point(0, 60), true).Action);
        Assert.Equal(CalendarGestureState.Idle, input.State);

        // Arrange
        input.Press(3, new Point(50, 50), 700);

        // Act & Assert
        Assert.Equal(CalendarGestureAction.Tap, input.Release(3, new Point(50, 50), true).Action);
    }

    [Fact]
    public void BlockAndCancelDiscardTheGesture()
    {
        // Arrange
        var input = new CalendarGestureController();
        input.Press(1, new Point(50, 50), 0);
        input.Block();

        // Act & Assert
        Assert.Equal(CalendarGestureAction.None, input.Release(99, new Point(50, 50), true).Action);
        Assert.Equal(CalendarGestureAction.None, input.Release(1, new Point(50, 50), true).Action);
        Assert.Equal(CalendarGestureState.Idle, input.State);

        // Arrange
        input.Press(1, new Point(50, 50), 100);
        input.Cancel();

        // Act & Assert
        Assert.Equal(CalendarGestureAction.None, input.Tick(1000).Action);
        Assert.Equal(CalendarGestureAction.None, input.Release(1, new Point(50, 50), true).Action);
    }
}
