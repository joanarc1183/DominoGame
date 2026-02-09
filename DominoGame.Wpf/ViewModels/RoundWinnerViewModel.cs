namespace DominoGame.Wpf.ViewModels;

public class RoundWinnerViewModel
{
    public RoundWinnerViewModel(string message)
    {
        Message = message;
    }

    public string Message { get; }
}
