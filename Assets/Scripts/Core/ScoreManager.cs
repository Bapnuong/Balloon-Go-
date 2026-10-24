using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    public int Score { get; private set; }
    public int RunCoins { get; private set; }
    public int TotalCoins { get; private set; }
    public int HighScore { get; private set; }

    public event Action<int> OnScoreChanged;
    public event Action<int> OnCoinsChanged;
    public event Action<int> OnHighScoreChanged;

    void Awake()
    {
        Instance = this;
        HighScore = SaveSystem.LoadHighScore();
        TotalCoins = SaveSystem.LoadCoins();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void ResetRun()
    {
        Score = 0;
        RunCoins = 0;
        OnScoreChanged?.Invoke(Score);
        OnCoinsChanged?.Invoke(TotalCoins);
    }

    public void AddScore(int amount)
    {
        if (amount <= 0) return;
        Score += amount;
        OnScoreChanged?.Invoke(Score);
    }

    public void AddCoin(int amount)
    {
        if (amount <= 0) return;
        RunCoins += amount;
        TotalCoins += amount;
        SaveSystem.SaveCoins(TotalCoins);
        OnCoinsChanged?.Invoke(TotalCoins);
    }

    public bool SpendCoins(int amount)
    {
        if (amount < 0) return false;
        if (TotalCoins < amount) return false;
        TotalCoins -= amount;
        SaveSystem.SaveCoins(TotalCoins);
        OnCoinsChanged?.Invoke(TotalCoins);
        return true;
    }

    public void CommitRun()
    {
        if (Score > HighScore)
        {
            HighScore = Score;
            SaveSystem.SaveHighScore(HighScore);
            OnHighScoreChanged?.Invoke(HighScore);
        }
        SaveSystem.SaveCoins(TotalCoins);
    }
}
