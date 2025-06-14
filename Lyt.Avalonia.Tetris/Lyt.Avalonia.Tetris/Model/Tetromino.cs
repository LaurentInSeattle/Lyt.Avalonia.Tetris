namespace Lyt.Avalonia.Tetris.Model; 

public enum Direction
{
    Down,
    Left,
    Right,
}

public sealed partial class Tetromino
{
    public enum ShapeKind
    {
        // Real shapes 
        O, L, J, I, S, Z, T,

        // Placeholder to denote an empty slot on the the field matrix 
        // => MUST be last one !!!
        Empty 
    }

    #region Statics 

    private static readonly Random randomNumberGenerator = new(Environment.TickCount);

    public static readonly ShapeKind[] ShapeTypes =
        [.. Enum.GetValues<ShapeKind>().OfType<ShapeKind>()];

    private static readonly Dictionary<ShapeKind, SolidColorBrush> shapeTypeToBrushDict =
        new()
        {
            [ShapeKind.J] = new SolidColorBrush(Color.Parse("#D23734")),
            [ShapeKind.L] = new SolidColorBrush(Color.Parse("#1A73B4")),
            [ShapeKind.S] = new SolidColorBrush(Color.Parse("#92832C")),
            [ShapeKind.Z] = new SolidColorBrush(Color.Parse("#1D9079")),
            [ShapeKind.T] = new SolidColorBrush(Color.Parse("#446524")),
            [ShapeKind.O] = new SolidColorBrush(Color.Parse("#76B03E")),
            [ShapeKind.I] = new SolidColorBrush(Color.Parse("#5A68A1")),
        };

    public static SolidColorBrush ShapeToBrush (ShapeKind shapeKind)
        => shapeTypeToBrushDict[shapeKind];

    private static readonly Dictionary<ShapeKind, bool[,]> shapeTypeToMatrixDict =
        new()
        {
            [ShapeKind.J] = new bool[3, 3]
            {
                { true, false, false },
                { true, true, true },
                { false, false, false },
            },
            [ShapeKind.L] = new bool[3, 3]
            {
                { false, false, false },
                { true, true, true },
                { true, false, false },
            },
            [ShapeKind.S] = new bool[3, 3]
            {
                { false, true, true },
                { true, true, false },
                { false, false, false },
            },
            [ShapeKind.Z] = new bool[3, 3]
            {
                { true, true, false },
                { false, true, true },
                { false, false, false },
            },
            [ShapeKind.T] = new bool[3, 3]
            {
                { false, true, false },
                { true, true, true },
                { false, false, false },
            },
            [ShapeKind.O] = new bool[3, 4]
            {
                { false, true, true, false },
                { false, true, true, false },
                { false, false, false, false },
            },
            [ShapeKind.I] = new bool[4, 4]
            {
                { false, false, false, false },
                { true, true, true, true },
                { false, false, false, false },
                { false, false, false, false },
            },
        };

    public static bool[,] GetMatrix(ShapeKind shape) => shapeTypeToMatrixDict[shape];

    #endregion Statics 

    public Tetromino(Position initialPosition)
    {
        var shapeType = ShapeTypes[randomNumberGenerator.Next(0, ShapeTypes.Length-1)];
        this.TopLeft = initialPosition;
        this.Shape = shapeType;
        this.Brush = shapeTypeToBrushDict[shapeType];
        this.BodyMatrix = shapeTypeToMatrixDict[shapeType];
        this.BodyPositions = GetPositions(this.TopLeft, this.BodyMatrix);
    }

    public ShapeKind Shape { get; private set; }

    public bool[,] BodyMatrix { get; private set; }

    public Brush Brush { get; private set; }

    public Position TopLeft { get; private set; }

    public List<Position> BodyPositions { get; private set; }

    public void Move(Direction direction, int speed)
    {
        var moveContext = this.GetMoveContext(direction, speed);
        this.Move(moveContext);
    }

    private void Move(MoveContext moveContext)
    {
        this.TopLeft = moveContext.TopLeft;
        this.BodyPositions = moveContext.Positions;
    }

    private void Rotate(MoveContext rotationContext)
    {
        this.BodyMatrix = rotationContext.BodyMatrix;
        this.BodyPositions = rotationContext.Positions;
    }

    public static List<Position> GetPositions(Position topLeft, bool[,] bodyMatrix)
    {
        var result = new List<Position>();
        for (int r = 0; r < bodyMatrix.GetLength(0); r++)
        {
            for (int c = 0; c < bodyMatrix.GetLength(1); c++)
            {
                if (bodyMatrix[r, c])
                {
                    result.Add(GetActualPositionByRelativePosition(topLeft, c, r));
                }
            }
        }

        return result;
    }

    private static Position GetActualPositionByRelativePosition(Position topLeft, int relativeX, int relativeY)
    {
        int actualX = topLeft.X + relativeX;
        int actualY = topLeft.Y + relativeY;
        return new Position(actualX, actualY);
    }

    public bool CollisionDetected(ShapeKind[,] fieldMatrix, List<Position> updatedTetrominoPositions)
    {
        int rows = fieldMatrix.GetLength(0);
        int cols = fieldMatrix.GetLength(1);
        var modifiedFieldMatrix = new ShapeKind[rows, cols];
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                modifiedFieldMatrix[r, c] = fieldMatrix[r, c];
            }
        }

        foreach (var position in this.BodyPositions)
        {
            modifiedFieldMatrix[position.Y, position.X] = ShapeKind.Empty;
        }

        foreach (var position in updatedTetrominoPositions)
        {
            int x = position.X;
            int y = position.Y;
            bool isOutsideMatrix = x < 0 || x >= cols || y < 0 || y >= rows;
            if (isOutsideMatrix || modifiedFieldMatrix[y, x] != ShapeKind.Empty)
            {
                return true;
            }
        }

        return false;
    }
}
