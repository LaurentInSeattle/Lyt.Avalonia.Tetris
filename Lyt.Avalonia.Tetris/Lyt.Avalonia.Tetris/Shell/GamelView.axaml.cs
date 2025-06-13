namespace Lyt.Avalonia.Tetris.Shell;

public partial class GameView : UserControl, IView
{
    public GameView()
    {
        this.InitializeComponent();
        this.Loaded += (s, e) =>
        {
            if (this.DataContext is not null && this.DataContext is ViewModel viewModel)
            {
                viewModel.OnViewLoaded();
            }
        };
    }
}
