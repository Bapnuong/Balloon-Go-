
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public bool gameOver = false;

    void Awake()
    {
        if(Instance == null)
            Instance = this;
    }

    public void EndGame()
    {
        if(gameOver) return;

        gameOver = true;
        Time.timeScale = 0f;
        UIManager.Instance.ShowGameOver();
        SaveSystem.SaveHighScore(ScoreManager.Instance.score);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
