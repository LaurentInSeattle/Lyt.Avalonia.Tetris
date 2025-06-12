namespace Lyt.Avalonia.Tetris.Model;

public sealed class TetrisModel : ModelBase
{
    public TetrisModel(IMessenger messenger, ILogger logger) : base(messenger, logger)
    {
        this.ShouldAutoSave = false;
    }

    public override Task Initialize() { return Task.CompletedTask;  }

}
