namespace Lyt.Avalonia.Tetris.Shell;

using static Tetromino;

public sealed partial class GameViewModel : ViewModel<GameView>
{
    private Grid? gameGrid;
    private Canvas? gameCanvas;
    private Grid? nextShapeGrid;
    private Dictionary<ShapeKind, Queue<Control>>? shapeQueue;

    private void InitializeRender()
    {
        this.gameGrid = this.View.renderSurface;
        this.gameCanvas = this.View.renderCanvas;
        this.nextShapeGrid = this.View.nextShapeRenderSurface;
        this.shapeQueue = [];
        foreach (ShapeKind shapeKind in Tetromino.ShapeTypes)
        {
            if (shapeKind == ShapeKind.Empty)
            {
                continue;
            }

            var queue = new Queue<Control>();
            for (int i = 0; i < 20; ++i)
            {
                queue.Enqueue(CreateBlockVisual(shapeKind));
            }

            this.shapeQueue.Add(shapeKind, queue);
        }

        this.RenderGridLines();
        GameViewModel.SetupGameSurfaceVisual(
            this.gameGrid,
            this.field.Rows, this.field.Columns, this.field.Width, this.field.Height,
            this.field.CellHeight, this.field.CellWidth);
        this.RenderField();
    }

    private Control GetShape(ShapeKind shapeKind)
    {
        if (this.shapeQueue is null)
        {
            throw new Exception("Shape Queue should not be null");
        }

        var queue = this.shapeQueue[shapeKind] ?? 
            throw new Exception("Queue for shape should not be null");
        if (queue.Count == 0)
        {
            return CreateBlockVisual(shapeKind);
        }
        else
        {
            return queue.Dequeue();
        }
    }

    private void ReturnShape(Control control)
    {
        if (this.shapeQueue is null)
        {
            throw new Exception("Shape Queue should not be null");
        }

        if ((control.Tag is ShapeKind shapeKind) && ( shapeKind != ShapeKind.Empty)) 
        {
            var queue = this.shapeQueue[shapeKind] ?? 
                throw new Exception("Queue for shape should not be null");
            queue.Enqueue(control);
        }
    }

    private void RenderField()
    {
        if (this.gameGrid is null)
        {
            Debug.WriteLine("Game Grid should not be null");
            return;
        }

        foreach(Control control in this.gameGrid.Children)
        {
            this.ReturnShape(control);
        }

        this.gameGrid.Children.Clear();
        this.RenderShapes();
    }

    private void RenderShapes()
    {
        int rows = this.field.Matrix.GetLength(0);
        int columns = this.field.Matrix.GetLength(1);
        for (int rowIndex = 0; rowIndex < rows; ++rowIndex)
        {
            for (int columnIndex = 0; columnIndex < columns; ++columnIndex)
            {
                ShapeKind shapeKind = this.field.Matrix[rowIndex, columnIndex];
                if (shapeKind != ShapeKind.Empty)
                {
                    this.RenderBlock(columnIndex, rowIndex, shapeKind);
                }
            }
        }
    }

    private void RenderBlock(int columnIndex, int rowIndex, ShapeKind shapeKind)
    {
        if (this.gameGrid is null)
        {
            Debug.WriteLine("Game Grid should not be null");
            return;
        }

        this.gameGrid.Children.Add(this.PlaceBlockVisual(columnIndex, rowIndex, shapeKind));
    }

    private void RenderGridLines()
    {
        if (this.gameCanvas is null)
        {
            Debug.WriteLine("Game Canvas should not be null");
            return;
        }

        double cellWidth = this.field.CellWidth;
        double cellHeight = this.field.CellHeight;
        var brush = Brushes.BlanchedAlmond;
        for (double linePosition = cellWidth; linePosition <= this.field.Width - cellWidth; linePosition += cellWidth)
        {
            var verticalLine = new Line
            {
                StartPoint = new Point(linePosition, 0.0),
                EndPoint = new Point(linePosition, this.field.Height),
                Stroke = brush,
                StrokeThickness = 1,
                Opacity = 0.15,
            };
            this.gameCanvas.Children.Add(verticalLine);
        }

        for (double linePosition = cellHeight; linePosition <= this.field.Height - cellHeight; linePosition += cellHeight)
        {
            var horizontalLine = new Line
            {
                StartPoint = new Point(0, linePosition),
                EndPoint = new Point(this.field.Width, linePosition),
                Stroke = brush,
                StrokeThickness = 1,
                Opacity = 0.15,
            };
            this.gameCanvas.Children.Add(horizontalLine);
        }
    }

    private void RenderNextShape(bool[,] shapeBodyMatrix, ShapeKind shapeKind)
    {
        if (this.nextShapeGrid is null)
        {
            Debug.WriteLine("Next Shape Grid should not be null");
            return;
        }

        foreach (Control control in this.nextShapeGrid.Children)
        {
            this.ReturnShape(control);
        }

        this.nextShapeGrid.Children.Clear();
        int rows = shapeBodyMatrix.GetLength(0);
        int columns = shapeBodyMatrix.GetLength(1);
        double width = this.field.CellWidth * columns;
        double height = this.field.CellHeight * rows;
        GameViewModel.SetupGameSurfaceVisual(
            this.nextShapeGrid, rows, columns, width, height, this.field.CellWidth, this.field.CellHeight);
        for (int row = 0; row < rows; ++row)
        {
            for (int col = 0; col < columns; ++col)
            {
                if (shapeBodyMatrix[row, col])
                {
                    this.nextShapeGrid.Children.Add(this.PlaceBlockVisual(col, row, shapeKind));
                }
            }
        }
    }

    private Control PlaceBlockVisual(int x, int y, ShapeKind shapeKind)
    {
        if (shapeKind == ShapeKind.Empty)
        {
            Debug.WriteLine("Shape Kind should not be empty");
            throw new Exception("Shape Kind should not be empty");
        }

        var control = this.GetShape(shapeKind); 
        Grid.SetColumn(control, x);
        Grid.SetRow(control, y);
        return control;
    }

    private void RemoveBlockAt(int row, int col)
    {
        if (this.gameGrid is null || this.gameGrid.Children.Count == 0)
        {
            return;
        }

        Control? toRemove = null;
        foreach (var control in this.gameGrid.Children)
        {
            if ((Grid.GetRow(control) == row) && (Grid.GetColumn(control) == col))
            {
                toRemove = control;
                break;
            }
        }

        if (toRemove is not null)
        {
            this.gameGrid.Children.Remove(toRemove);
            this.ReturnShape(toRemove);
        }
    }

#pragma warning disable CA1859 
    // Use concrete types when possible for improved performance
    // We may wish in some future to create a control that will be a bit more fancy than
    // just a plain border 
    // So, we return a control, not a border.
    // 
    private static Control CreateBlockVisual(ShapeKind shapeKind)

#pragma warning restore CA1859 
    {
        if (shapeKind == ShapeKind.Empty)
        {
            Debug.WriteLine("Shape Kind should not be empty");
            throw new Exception("Shape Kind should not be empty");
        }

        return new Border
        {
            Background = Tetromino.ShapeToBrush(shapeKind),
            BorderBrush = Tetromino.ShapeToBrushBorder(shapeKind),
            BorderThickness = new Thickness(1.0),
            Margin = new Thickness(1.0),
            Tag= shapeKind,
            CornerRadius=new CornerRadius(2.0),
        };
    }

    private static void SetupGameSurfaceVisual(
        Grid grid, int rows, int columns, double totalWidth, double totalHeight, double cellWidth, double cellHeight)
    {
        grid.Width = totalWidth;
        grid.Height = totalHeight;
        for (int row = 0; row < rows; row++)
        {
            var rowDefinition = new RowDefinition { Height = new GridLength(cellHeight) };
            grid.RowDefinitions.Add(rowDefinition);
        }

        for (int column = 0; column < columns; column++)
        {
            var columnDefinition = new ColumnDefinition { Width = new GridLength(cellWidth) };
            grid.ColumnDefinitions.Add(columnDefinition);
        }
    }
}
