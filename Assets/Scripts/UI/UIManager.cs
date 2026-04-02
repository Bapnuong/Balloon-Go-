
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI coinText;
    public GameObject gameOverPanel;

    void Awake()
    {
        Instance = this;
    }

    public void UpdateScore(int score)
    {
        if(scoreText!=null)
            scoreText.text = "Score: " + score;
    }

    public void UpdateCoins(int coins)
    {
        if(coinText!=null)
            coinText.text = "Coins: " + coins;
    }

    public void ShowGameOver()
    {
        if(gameOverPanel!=null)
            gameOverPanel.SetActive(true);
    }
}
