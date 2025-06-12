namespace Lyt.Avalonia.Tetris.Model;

public sealed class Field
{
    public int Rows = 24;
    public int Columns = 10;

    public double CellWidth = 32;
    public double CellHeight = 32;

    public Brush[,] Matrix { get; private set; }

    public double Width => this.CellWidth * this.Columns;

    public double Height => this.CellHeight * this.Rows;

    public Field() => this.Matrix = new Brush[this.Rows, this.Columns];

    public void Clear()
    {
        int rows = this.Matrix.GetLength(0);
        for (int rowIndex = rows - 1; rowIndex >= 0; rowIndex--)
        {
            this.ClearRow(rowIndex);
        }
    }

    public int ClearFullRows()
    {
        int removedRowsCount = 0;
        int rows = this.Matrix.GetLength(0);
        for (int rowIndex = rows - 1; rowIndex >= 0; rowIndex--)
        {
            while (this.ShouldClearRow(rowIndex))
            {
                removedRowsCount++;
                this.ClearRow(rowIndex);
                this.MoveRowsDown(rowIndex - 1, 1);
            }
        }

        return removedRowsCount;
    }

    private bool ShouldClearRow(int rowIndex)
    {
        int columns = this.Matrix.GetLength(1);
        for (int columnIndex = 0; columnIndex < columns; columnIndex++)
        {
            if (this.Matrix[rowIndex, columnIndex] == null)
            {
                return false;
            }
        }

        return true;
    }

    private void ClearRow(int rowIndex)
    {
        int columns = this.Matrix.GetLength(1);
        for (int columnIndex = 0; columnIndex < columns; columnIndex++)
        {
            this.Matrix[rowIndex, columnIndex] = null;
        }
    }

    private void MoveRowsDown(int startRowIndex, int rowIndexOffset)
    {
        int columns = this.Matrix.GetLength(1);
        for (int rowIndex = startRowIndex; rowIndex >= 0; rowIndex--)
        {
            for (int columnIndex = 0; columnIndex < columns; columnIndex++)
            {
                if (this.Matrix[rowIndex, columnIndex] != null)
                {
                    var color = this.Matrix[rowIndex, columnIndex];
                    this.Matrix[rowIndex, columnIndex] = null;
                    this.Matrix[rowIndex + rowIndexOffset, columnIndex] = color;
                }
            }
        }
    }
}
