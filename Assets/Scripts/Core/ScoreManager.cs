
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    public int score;
    public int coins;

    void Awake()
    {
        Instance = this;
    }

    public void AddScore(int amount)
    {
        score += amount;
        UIManager.Instance.UpdateScore(score);
    }

    public void AddCoin(int amount)
    {
        coins += amount;
        UIManager.Instance.UpdateCoins(coins);
    }
}
