namespace Lyt.Avalonia.Tetris.Model; 

public sealed partial class Tetromino
{
    public void Move(Brush[,] fieldMatrix, MoveContext context)
    {
        this.RemoveTetrominoFromFieldMatrix(fieldMatrix);
        this.Move(context);
        this.UpdateField(fieldMatrix);
    }

    public void Rotate(Brush[,] fieldMatrix, MoveContext context)
    {
        this.RemoveTetrominoFromFieldMatrix(fieldMatrix);
        this.Rotate(context);
        this.UpdateField(fieldMatrix);
    }

    private void RemoveTetrominoFromFieldMatrix(Brush[,] matrix)
    {
        for (int i = 0; i < this.BodyPositions.Count; i++)
        {
            var position = this.BodyPositions[i];
            matrix[position.Y, position.X] = null;
        }
    }

    public void UpdateField(Brush[,] fieldMatrix)
    {
        for (int i = 0; i < this.BodyPositions.Count; i++)
        {
            var position = this.BodyPositions[i];
            fieldMatrix[position.Y, position.X] = this.Brush;
        }
    }

    public MoveContext GetRotationContext(bool isCounterClockwise = false)
    {
        bool[,] newMatrix = Tetromino.GetRotatedMatrix(this.BodyMatrix, isCounterClockwise);
        var rotatedPositions = Tetromino.GetPositions(this.TopLeft, newMatrix);
        return new MoveContext(this.TopLeft, rotatedPositions, newMatrix);
    }

    public MoveContext GetMoveContext(Direction direction, int speed)
    {
        var newTopLeft = this.TopLeft;
        if (direction == Direction.Down)
        {
            newTopLeft = new Position(this.TopLeft.X, this.TopLeft.Y + speed);
        }
        else if (direction == Direction.Left)
        {
            newTopLeft = new Position(this.TopLeft.X - speed, this.TopLeft.Y);
        }
        else if (direction == Direction.Right)
        {
            newTopLeft = new Position(this.TopLeft.X + speed, this.TopLeft.Y);
        }

        return new MoveContext(newTopLeft, Tetromino.GetPositions(newTopLeft, this.BodyMatrix), this.BodyMatrix);
    }

    public static bool[,] GetRotatedMatrix(bool[,] originalMatrix, bool isCounterClockwise)
        => isCounterClockwise ?
            Tetromino.RotateMatrixCounterClockwise(originalMatrix) :
            Tetromino.RotateMatrixClockwise(originalMatrix);

    private static bool[,] RotateMatrixCounterClockwise(bool[,] originalMatrix)
    {
        int rows = originalMatrix.GetLength(0);
        int columns = originalMatrix.GetLength(1);
        int nextRow = 0;
        bool[,] rotatedMatrix = new bool[columns, rows];
        for (int oldColumn = columns - 1; oldColumn >= 0; --oldColumn)
        {
            for (int oldRow = 0; oldRow < rows; ++oldRow)
            {
                rotatedMatrix[nextRow, oldRow] = originalMatrix[oldRow, oldColumn];
            }

            ++nextRow;
        }

        return rotatedMatrix;
    }

    private static bool[,] RotateMatrixClockwise(bool[,] originalMatrix)
    {
        int columns = originalMatrix.GetLength(1);
        int rows = originalMatrix.GetLength(0);
        bool[,] rotatedMatrix = new bool[columns, rows];
        for (int oldColumn = 0; oldColumn < columns; ++oldColumn)
        {
            for (int oldRow = 0; oldRow < rows; ++oldRow)
            {
                rotatedMatrix[oldColumn, oldRow] = originalMatrix[rows - oldRow - 1, oldColumn];
            }
        }

        return rotatedMatrix;
    }
}
