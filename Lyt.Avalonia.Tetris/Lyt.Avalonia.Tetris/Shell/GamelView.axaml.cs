namespace Lyt.Avalonia.Tetris.Shell;

public partial class GameView : UserControl, IView
{
    public GameView()
    {
        this.InitializeComponent();
        this.Loaded += (s, e) =>
        {
            // Focus so that key bindings are going to work 
            this.Focus(); 
            if (this.DataContext is not null && this.DataContext is ViewModel viewModel)
            {
                viewModel.OnViewLoaded();
            }
        };
    }
}
