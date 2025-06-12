namespace Lyt.Avalonia.Tetris.Model;

public sealed record class MoveContext(
    Position TopLeft, List<Position> Positions, bool[,] BodyMatrix); 
