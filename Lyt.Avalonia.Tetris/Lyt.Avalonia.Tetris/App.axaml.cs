namespace Lyt.Avalonia.Tetris;

public partial class App : ApplicationBase
{
    public const string Organization = "Lyt";
    public const string Application = "Tetris";
    public const string RootNamespace = "Lyt.Avalonia.Tetris";
    public const string AssemblyName = "Lyt.Avalonia.Tetris";
    public const string AssetsFolder = "Assets";

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public App() : base(
        App.Organization,
        App.Application,
        App.RootNamespace,
        typeof(MainWindow),
        typeof(ApplicationModelBase), // Top level model 
        [
            // Models 
           typeof(TetrisModel),
        ],
        [
           // Singletons
           typeof(GameViewModel),
        ],
        [
            // Services 
            App.LoggerService,
            new Tuple<Type, Type>(typeof(IDialogService), typeof(DialogService)),
            new Tuple<Type, Type>(typeof(IDispatch), typeof(Dispatch)),
            new Tuple<Type, Type>(typeof(IMessenger), typeof(Messenger)),
            new Tuple<Type, Type>(typeof(IProfiler), typeof(Profiler)),
            new Tuple<Type, Type>(typeof(IRandomizer), typeof(Randomizer)),
        ],
        singleInstanceRequested: false,
        splashImageUri: null,
        appSplashWindow: null)
    {
        // This should be empty, use the OnStartup override
        Instance = this;
        Debug.WriteLine("App Instance created");
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public static App Instance { get; private set; }
#pragma warning restore CS8618 

    private static Tuple<Type, Type> LoggerService =>
            Debugger.IsAttached ?
                new Tuple<Type, Type>(typeof(ILogger), typeof(LogViewerWindow)) :
                new Tuple<Type, Type>(typeof(ILogger), typeof(Logger));

    public bool RestartRequired { get; set; }

    //protected override Task OnStartupBegin()
    //{
    //    var logger = App.GetRequiredService<ILogger>();
    //    logger.Debug("OnStartupBegin begins");

    //    logger.Debug("OnStartupBegin complete");
    //    return Task.CompletedTask;
    //}

    //protected override Task OnShutdownComplete()
    //{
    //    var logger = App.GetRequiredService<ILogger>();
    //    logger.Debug("On Shutdown Complete");

    //    if (this.RestartRequired)
    //    {
    //        logger.Debug("On Shutdown Complete: Restart Required");
    //        var process = Process.GetCurrentProcess();
    //        if ((process is not null) && (process.MainModule is not null))
    //        {
    //            Process.Start(process.MainModule.FileName);
    //        }
    //    }

    //    return Task.CompletedTask;
    //}
}
