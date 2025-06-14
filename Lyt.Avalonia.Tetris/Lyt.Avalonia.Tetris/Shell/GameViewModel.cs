namespace Lyt.Avalonia.Tetris.Shell;

using static Tetromino;

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

    private Tetromino? fallingTetromino;
    private Tetromino? nextTetromino;

    private bool isAnimatingEndGame;

    [ObservableProperty]
    private int level;

    [ObservableProperty]
    private int score;

    [ObservableProperty]
    private int highscore;

    [ObservableProperty]
    private int tetraminos;

    [ObservableProperty]
    private int lines;

    [ObservableProperty]
    private bool isEndGameInfoVisible;

    [ObservableProperty]
    private bool endGameInfoVisibility;

    [ObservableProperty]
    private bool firstStartVisibility;

    public GameViewModel(TetrisModel tetrisModel)
    {
        this.tetrisModel = tetrisModel;

        // General init
        this.field = new Field();
        this.gameState = State.Ended;
        this.frameRenderTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(frameRenderingInterval) };
        this.frameRenderTimer.Tick += this.OnFrameRenderTimerTick;
        this.Highscore = Model.Score.GetHighscore();

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

        this.InitializeRender(); 
        this.Logger.Debug("OnViewLoaded complete");
    }

    private bool IsGameRunning => this.gameState == State.Running;

#pragma warning disable IDE0079 
#pragma warning disable CA1822 // Mark members as static

    [RelayCommand]
    public void OnExit()
    {
        // Debug.WriteLine("OnExit");

        // Fire and forget shutdown 
        var application = App.GetRequiredService<IApplicationBase>();
        _ = application.Shutdown();
    }

#pragma warning restore CA1822
#pragma warning restore IDE0079

    [RelayCommand]
    public void OnStartGame()
    {
        // Debug.WriteLine("OnStartGame");
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
        // Debug.WriteLine("OnEndGame");
        if (!this.IsGameRunning)
        {
            return;
        }

        this.EndGame();
    }

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
        // Debug.WriteLine("OnMove: " + direction.ToString());
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
        // Debug.WriteLine("OnRotate: CCW: " + isCounterClockwise.ToString());
        if (!this.IsGameRunning)
        {
            return;
        }

        this.RotateTetromino(isCounterClockwise);
    }

    [RelayCommand]
    public void OnHideEndGameInfo() => this.IsEndGameInfoVisible = false;

    private void StartNewGame()
    {
        if (this.isAnimatingEndGame)
        {
            return; 
        } 

        this.FirstStartVisibility = false;
        this.EndGameInfoVisibility = false;
        this.Level = 1;
        this.Score = 0;
        this.Tetraminos = 0;
        this.Lines = 0;
        this.field.Clear();
        this.gameState = State.Running;
        this.frameRenderTimer.Interval = TimeSpan.FromMilliseconds(frameRenderingInterval);
        this.frameRenderTimer.Start();
        this.DropNewTetromino();
    }

    private void PauseGame() => this.gameState = State.Paused;

    private void ResumeGame() => this.gameState = State.Running;

    private void EndGame()
    {
        if ((this.gameState == State.Running || this.gameState == State.Paused))
        {
            this.frameRenderTimer.Stop();
            Model.Score.SaveHighscore(this.Highscore);
            this.gameState = State.Ended;
            Schedule.OnUiThread(100, this.OnGameEnded, DispatcherPriority.Normal);
        }
    }

    private void OnFrameRenderTimerTick(object? _, EventArgs e)
    {
        if ((this.gameState != State.Running) ||
            (this.fallingTetromino is null))
        {
            return;
        }

        if (this.frameRenderTimer.IsEnabled)
        {
            this.MoveTetromino(Direction.Down);
        }
    }

    private void DropNewTetromino()
    {
        if (this.gameState != State.Running)
        {
            return;
        }

        if (this.TryDropNewTetromino())
        {
            if (this.nextTetromino is null)
            {
                Debug.WriteLine("nextTetromino should not be null");
                return;
            }

            if (this.fallingTetromino is null)
            {
                Debug.WriteLine("fallingTetromino should not be null");
                return;
            }

            ++ this.Tetraminos; 
            this.fallingTetromino.UpdateField(this.field.Matrix);
            this.RenderField();
            ShapeKind shapeKind = this.nextTetromino.Shape;
            this.RenderNextShape(Tetromino.GetMatrix(shapeKind), shapeKind);
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
        if ((this.gameState != State.Running) ||
            (this.fallingTetromino is null))
        {
            return;
        }

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

                    Debug.WriteLine("Timer Interval: Current: " + currentTimerInterval + "   New: " + newInterval);
                    this.frameRenderTimer.Interval = TimeSpan.FromMilliseconds(newInterval);
                }

                this.AddToScore(Model.Score.GetLineScore(this.Level, clearedRowsCount));
                this.RenderField();
                this.DropNewTetromino();
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
        if ((this.gameState != State.Running) ||
            (this.fallingTetromino is null))
        {
            return;
        }

        if (this.fallingTetromino.Shape == Tetromino.ShapeKind.O)
        {
            // The O shape is invariant for rotation: do nothing 
            return;
        }

        var rotationContext = this.fallingTetromino.GetRotationContext(isCounterClockwise);
        if (!this.fallingTetromino.CollisionDetected(this.field.Matrix, rotationContext.Positions))
        {
            this.fallingTetromino.Rotate(this.field.Matrix, rotationContext);
            this.RenderField();
        }
    }

    private void OnGameEnded() 
    {
        this.isAnimatingEndGame = true;
        this.EndGameInfoVisibility = true;

        Task.Run(async () =>
        {
            int rows = this.field.Matrix.GetLength(0);
            int columns = this.field.Matrix.GetLength(1);
            for (int row = rows - 1; row >= 0; --row)
            {
                for (int col = 0; col < columns; ++col)
                {
                    Dispatch.OnUiThread(() => this.RemoveBlockAt(row, col));    

                    // Sleep * after * capturing row and col
                    await Task.Delay(42);
                }
            }

            this.isAnimatingEndGame = false;
        });
    }
}
