namespace Lyt.Avalonia.Tetris.Model;

public sealed class TetrisModel : ModelBase
{
    public TetrisModel(ILogger logger) : base(logger) => this.ShouldAutoSave = false;

    public override Task Initialize() => Task.CompletedTask;
}
