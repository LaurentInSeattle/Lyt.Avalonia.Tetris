namespace Lyt.Avalonia.Tetris.Model;

public static class Score
{
    private const string HighscoreFileName = "HighscoreData.txt";

    private static readonly List<int> initialScoresPerNumberOfLines;

    static Score()
    {
        Score.initialScoresPerNumberOfLines = [100, 250, 500, 1500];

        if (!File.Exists(HighscoreFileName))
        {
            var file = File.Create(HighscoreFileName);
            file.Close();
        }
    }

    public static int GetLineScore(int level, int numberOfLines)
    {
        if (numberOfLines < 1 || level < 0)
        {
            return 0;
        }

        int count = Score.initialScoresPerNumberOfLines.Count;
        int initialScore =
            numberOfLines <= count ?
                Score.initialScoresPerNumberOfLines[numberOfLines - 1] :
                Score.initialScoresPerNumberOfLines[count - 1];
        return initialScore * (level + 1);
    }

    public static int GetHighscore()
    {
        int highscore = 0;
        using (var reader = new StreamReader(HighscoreFileName))
        {
            string highscoreString = reader.ReadToEnd().Trim();
            _ = int.TryParse(highscoreString, out highscore);
        }

        return highscore;
    }

    public static void SaveHighscore(int highscore)
    {
        using var writer = new StreamWriter(HighscoreFileName, false);
        writer.WriteLine(highscore);
    }
}
