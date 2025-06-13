namespace Lyt.Avalonia.Tetris.Shell;

using static Tetromino;

public sealed partial class GameViewModel : ViewModel<GameView>
{
    private void RenderField()
    {
        if (this.gameGrid is null)
        {
            Debug.WriteLine("Game Grid should not be null");
            return;
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

        this.gameGrid.Children.Add(CreateBlockVisual(columnIndex, rowIndex, shapeKind));
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
                    this.nextShapeGrid.Children.Add(CreateBlockVisual(col, row, shapeKind));
                }
            }
        }
    }

    private static Control CreateBlockVisual(int x, int y, ShapeKind shapeKind)
    {
        if (shapeKind == ShapeKind.Empty)
        {
            Debug.WriteLine("Shape Kind should not be empty");
            throw new Exception("Shape Kind should not be empty");
        }

        var border = new Border
        {
            Background = Tetromino.ShapeToBrush(shapeKind),
            BorderBrush = Brushes.BlanchedAlmond,
            BorderThickness = new Thickness(0.5),
        };

        Grid.SetColumn(border, x);
        Grid.SetRow(border, y);
        return border;
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
