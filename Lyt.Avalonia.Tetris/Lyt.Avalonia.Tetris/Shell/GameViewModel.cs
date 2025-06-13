namespace Lyt.Avalonia.Tetris.Shell;

public sealed partial class GameViewModel : ViewModel<GameView>
{
    public enum State
    {
        Running,
        Paused,
        Ended,
    }

    private const int movementSpeed = 1;
    private const int frameRenderingInterval = 500;

    private readonly TetrisModel tetrisModel;
    private readonly DispatcherTimer frameRenderTimer;
    private readonly Field field;

    private State gameState;

    private Grid? gameGrid;
    private Grid? nextShapeGrid;
    private Brush? endGameBlocksBrush;
    private Tetromino? fallingTetromino;
    private Tetromino? nextTetromino;
    // private BackgroundWorker endGameAnimationWorker;

    [ObservableProperty]
    private int level;

    [ObservableProperty]
    private int score;

    [ObservableProperty]
    private int highscore;

    [ObservableProperty]
    private int lines;

    [ObservableProperty]
    private bool isEndGameInfoVisible;

    [ObservableProperty]
    private bool endGameInfoVisibility;

    [ObservableProperty]
    private bool firstStartVisibility;

    #region To please the XAML viewer 

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    // Should never be executed 
    public GameViewModel()
    {
    }
#pragma warning restore CS8618 

    #endregion To please the XAML viewer 

    public GameViewModel(TetrisModel tetrisModel)
    {
        this.tetrisModel = tetrisModel;

        // General init
        this.field = new Field();
        this.gameState = State.Ended;
        this.frameRenderTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(frameRenderingInterval) };
        this.frameRenderTimer.Tick += this.OnFrameRenderTimerTick;
        this.Highscore = Model.Score.GetHighscore();

        // Prepare for end game 
        //this.InitializeEndGameAnimationWorker();
        //this.endGameBlocksBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#20FFFFFF"));
        //this.endGameBlocksBrush.Freeze();

        // Prepare for new game 
        this.IsEndGameInfoVisible = false;
        this.FirstStartVisibility = true;
    }

    public override void OnViewLoaded()
    {
        this.Logger.Debug("OnViewLoaded begins");

        base.OnViewLoaded();
        if (this.View is null)
        {
            throw new Exception("Failed to startup...");
        }

        this.gameGrid = this.View.renderSurface;
        this.nextShapeGrid = this.View.nextShapeRenderSurface;

        GameViewModel.SetupGameSurfaceVisual(
            this.gameGrid,
            this.field.Rows, this.field.Columns, this.field.Width, this.field.Height,
            this.field.CellHeight, this.field.CellWidth);
        this.RenderField();

        this.Logger.Debug("OnViewLoaded complete");
    }

    private bool IsGameRunning => this.gameState == State.Running;

#pragma warning disable IDE0079 
#pragma warning disable CA1822 // Mark members as static

    [RelayCommand]
    public void OnExit()
    {
        Debug.WriteLine("OnExit");
        //var application = App.GetRequiredService<IApplicationBase>();
        //await application.Shutdown();
    }

    [RelayCommand]
    public void OnStartGame()
    {
        Debug.WriteLine("OnStartGame");
        if (this.IsGameRunning)
        {
            return; 
        }

        this.IsEndGameInfoVisible = false;
        this.StartNewGame();
    }

    [RelayCommand]
    public void OnEndGame()
    {
        Debug.WriteLine("OnEndGame");
        if (!this.IsGameRunning)
        {
            return;
        }

        this.EndGame();
    } 

    private void OnGameEnded() { } //  => CommandManager.InvalidateRequerySuggested();

    [RelayCommand]
    public void OnPause()
    {
        if (!this.IsGameRunning)
        {
            return;
        }

        this.PauseGame();
    }


    [RelayCommand]
    public void OnResume()
    {
        if (this.IsGameRunning)
        {
            return;
        }

        this.ResumeGame();
    } 

    [RelayCommand]
    public void OnMove(Direction direction)
    {
        Debug.WriteLine("OnMove: " + direction.ToString());
        if (!this.IsGameRunning)
        {
            return;
        }

        this.MoveTetromino(direction);
        if ((direction == Direction.Down) && this.IsGameRunning)
        {
            this.AddToScore(1);
        }
    }

    [RelayCommand]
    public void OnRotate(bool isCounterClockwise)
    {
        Debug.WriteLine("OnRotate: CCW: " + isCounterClockwise.ToString());

        if (!this.IsGameRunning)
        {
            return;
        }

        this.RotateTetromino(isCounterClockwise);
    }

    [RelayCommand]
    public void OnHideEndGameInfo() => this.IsEndGameInfoVisible = false;

#pragma warning restore CA1822
#pragma warning restore IDE0079

    private void StartNewGame()
    {
        this.FirstStartVisibility = false;
        //if (this.endGameAnimationWorker.IsBusy)
        //{
        //    this.endGameAnimationWorker.CancelAsync();
        //}

        this.Level = 1;
        this.Score = 0;
        this.Lines = 0;
        this.field.Clear();
        this.DropNewTetromino();
        this.gameState = State.Running;
        this.frameRenderTimer.Interval = TimeSpan.FromMilliseconds(frameRenderingInterval);
        this.frameRenderTimer.Start();
    }

    private void PauseGame() => this.gameState = State.Paused;

    private void ResumeGame() => this.gameState = State.Running;

    private void EndGame()
    {
        if ((this.gameState == State.Running || this.gameState == State.Paused) && 
            true ) // !this.endGameAnimationWorker.IsBusy)
        {
            this.frameRenderTimer.Stop();
            Model.Score.SaveHighscore(this.Highscore);
            this.gameState = State.Ended;
            Schedule.OnUiThread(100, this.OnGameEnded, DispatcherPriority.Normal);
            // this.endGameAnimationWorker.RunWorkerAsync();
        }
    }

    private void OnFrameRenderTimerTick(object? _, EventArgs e)
    {
        if (this.frameRenderTimer.IsEnabled && this.fallingTetromino != null)
        {
            this.MoveTetromino(Direction.Down);
        }
    }

    private void DropNewTetromino()
    {
        if (this.TryDropNewTetromino())
        {
            this.fallingTetromino.UpdateField(this.field.Matrix);
            this.RenderField();
            this.RenderNextShape(Tetromino.GetMatrix(this.nextTetromino.Shape), this.nextTetromino.Brush);
        }
        else
        {
            this.EndGame();
        }
    }

    private bool TryDropNewTetromino()
    {
        var tetromino = this.nextTetromino ?? new Tetromino(new Position(3, 0));
        this.nextTetromino = new Tetromino(new Position(3, 0));
        var movingContext = tetromino.GetMoveContext(Direction.Down, movementSpeed);
        if (!tetromino.CollisionDetected(this.field.Matrix, movingContext.Positions))
        {
            this.fallingTetromino = tetromino;
            return true;
        }

        return false;
    }

    private void MoveTetromino(Direction direction)
    {
        if (this.fallingTetromino != null && this.gameState == State.Running)
        {
            var movingContext = this.fallingTetromino.GetMoveContext(direction, movementSpeed);
            if (!this.fallingTetromino.CollisionDetected(this.field.Matrix, movingContext.Positions))
            {
                this.fallingTetromino.Move(this.field.Matrix, movingContext);
                this.RenderField();
            }
            else
            {
                if (direction == Direction.Down)
                {
                    int clearedRowsCount = this.field.ClearFullRows();

                    this.Lines += clearedRowsCount;
                    if (this.Lines >= ((this.Level * 10) + 10))
                    {
                        this.Level++;

                        int currentTimerInterval = this.frameRenderTimer.Interval.Milliseconds;
                        int delta = 45 - (3 * this.Level);
                        if (delta < 2)
                        {
                            delta = 2;
                        }

                        int newInterval = currentTimerInterval - delta;
                        if (newInterval < 50)
                        {
                            newInterval = 50;
                        }

                        Debug.WriteLine("Current: " + currentTimerInterval + "   New: " + newInterval);
                        this.frameRenderTimer.Interval = TimeSpan.FromMilliseconds(newInterval);
                    }


                    this.AddToScore(Model.Score.GetLineScore(this.Level, clearedRowsCount));
                    this.RenderField();
                    this.DropNewTetromino();
                }
            }
        }
    }

    private void AddToScore(int score)
    {
        this.Score += score;
        if (this.Score > this.Highscore)
        {
            this.Highscore = this.Score;
        }
    }

    private void RotateTetromino(bool isCounterClockwise = false)
    {
        if (this.fallingTetromino.Shape != Tetromino.ShapeKind.O && this.gameState == State.Running)
        {
            var rotationContext = this.fallingTetromino.GetRotationContext(isCounterClockwise);
            if (!this.fallingTetromino.CollisionDetected(this.field.Matrix, rotationContext.Positions))
            {
                this.fallingTetromino.Rotate(this.field.Matrix, rotationContext);
                this.RenderField();
            }
        }
    }

    private void InitializeEndGameAnimationWorker()
    {
        //this.endGameAnimationWorker = new BackgroundWorker
        //{
        //    WorkerSupportsCancellation = true,
        //};
        //this.endGameAnimationWorker.DoWork += this.EndGameAnimationWorker_DoWork;
        //this.endGameAnimationWorker.RunWorkerCompleted += this.EndGameAnimationWorker_RunWorkerCompleted;
    }

    private void EndGameAnimationWorker_DoWork(object sender, DoWorkEventArgs e)
    {
        int rows = this.field.Matrix.GetLength(0);
        int columns = this.field.Matrix.GetLength(1);
        for (int row = rows - 1; row >= 0; --row)
        {
            for (int col = 0; col < columns; ++col)
            {
                //if (this.endGameAnimationWorker.CancellationPending)
                //{
                //    this.field.Clear();
                //    e.Cancel = true;
                //    return;
                //}

                //if (this.gameState != State.Ended)
                //{
                //    return;
                //}

                //_ = Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                //{
                //    if (this.gameState == State.Ended)
                //    {
                //        this.RenderBlock(col, row, this.endGameBlocksBrush);
                //    }
                //}));

                // Sleep * after * capturing row and col
                Thread.Sleep(15);
            }
        }
    }

    private void EndGameAnimationWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
        //this.field.Clear();
        //Schedule.OnUiThread(200, this.OnEndGameAnimationCompleted);
    }

    private void RenderField()
    {
        this.gameGrid.Children.Clear();
        this.RenderGridLines();
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
                var color = this.field.Matrix[rowIndex, columnIndex];
                if (color != null)
                {
                    this.RenderBlock(columnIndex, rowIndex, color);
                }
            }
        }
    }

    private void RenderBlock(int columnIndex, int rowIndex, Brush brush)
        => this.gameGrid.Children.Add(CreateBlockVisual(columnIndex, rowIndex, brush));

    private void RenderGridLines()
    {
        double cellWidth = this.field.CellWidth;
        double cellHeight = this.field.CellHeight;
        var gridLinesPanel = new Canvas();
        var brush = Brushes.BlanchedAlmond;
        for (double linePosition = cellWidth; linePosition <= this.field.Width - cellWidth; linePosition += cellWidth)
        {
            var verticalLine = new Line
            {
                //X1 = linePosition,
                //X2 = linePosition,
                //Y1 = 0,
                //Y2 = this.field.Height,
                Stroke = brush,
                StrokeThickness = 1,
                Opacity = 0.1,
            };
            gridLinesPanel.Children.Add(verticalLine);
        }

        for (double linePosition = cellHeight; linePosition <= this.field.Height - cellHeight; linePosition += cellHeight)
        {
            var horizontalLine = new Line
            {
                //StartPoint = new Avalonia.Point(0,this.field.Width),
                //EndPoint = new Avalonia.Point(linePosition,linePosition),
                Stroke = brush,
                StrokeThickness = 1,
                Opacity = 0.1,
            };
            gridLinesPanel.Children.Add(horizontalLine);
        }

        this.gameGrid.Children.Insert(0, gridLinesPanel);
    }

    private void RenderNextShape(bool[,] shapeBodyMatrix, Brush brush)
    {
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
                    this.nextShapeGrid.Children.Add(CreateBlockVisual(col, row, brush));
                }
            }
        }
    }

    private static Control CreateBlockVisual(int x, int y, Brush brush)
    {
        var border = new Border
        {
            Background = brush,
            BorderThickness = new Thickness(0),
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
