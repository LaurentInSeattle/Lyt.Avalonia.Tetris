namespace Lyt.Avalonia.Tetris.Shell;

public sealed partial class ShellViewModel : ViewModel<ShellView>
{
    private const int MinutesToMillisecs = 60 * 1_000;

    private readonly TetrisModel tetrisModel;
    private readonly IToaster toaster;

    #region To please the XAML viewer 

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    // Should never be executed 
    public ShellViewModel()
    {
    }
#pragma warning restore CS8618 

    #endregion To please the XAML viewer 

    public ShellViewModel(TetrisModel tetrisModel, IToaster toaster)
    {
        this.tetrisModel = tetrisModel;
        this.toaster = toaster;
    }

    public override void OnViewLoaded()
    {
        this.Logger.Debug("OnViewLoaded begins");

        base.OnViewLoaded();
        if (this.View is null)
        {
            throw new Exception("Failed to startup...");
        }

        // Select default language 

        this.Logger.Debug("OnViewLoaded language loaded");


        // Ready : Needed ? 
        // this.toaster.Host = this.View.ToasterHost;
        // this.toaster.Show("Ready!", "Greetings!", 1_600, InformationLevel.Info);

        this.Logger.Debug("OnViewLoaded complete");
    }

    private static async void OnExit()
    {
        var application = App.GetRequiredService<IApplicationBase>();
        await application.Shutdown();
    }

#pragma warning disable IDE0079 
#pragma warning disable CA1822 // Mark members as static

    [RelayCommand]
    public void OnClose() => OnExit();

#pragma warning restore CA1822
#pragma warning restore IDE0079
}
