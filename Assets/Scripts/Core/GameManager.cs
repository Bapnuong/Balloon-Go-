using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public const int MaxRunScore = 10000;

    public GameState State { get; private set; } = GameState.Menu;
    public PlayerController Player { get; set; }

    public float RunTime { get; private set; }
    public int LastRunScore { get; private set; }
    public int LastRunCoins { get; private set; }
    public bool LastRunRecord { get; private set; }
    public DeathCause LastDeathCause { get; private set; }

    public float Difficulty01 => Mathf.Clamp01(RunTime / 65f);

    GameState returnState;
    float scoreAccumulator;
    bool busy;
    bool storyFirstTime;

    void Awake()
    {
        Instance = this;
        Player = FindFirstObjectByType<PlayerController>();
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    IEnumerator Start()
    {
        if (Player != null) Player.Hide();
        if (ScoreManager.Instance != null) ScoreManager.Instance.ResetRun();

        // Let UIManager and the runtime systems finish their Start/Awake.
        yield return null;

        if (!SaveSystem.LoadStorySeen())
            OpenStory(true);
        else
            ShowMenu();
    }

    void Update()
    {
        if (State == GameState.Playing)
        {
            RunTime += Time.deltaTime;
            scoreAccumulator += Time.deltaTime * 10f;
            int add = Mathf.FloorToInt(scoreAccumulator);
            if (add > 0)
            {
                scoreAccumulator -= add;
                if (ScoreManager.Instance != null)
                {
                    int remaining = MaxRunScore - ScoreManager.Instance.Score;
                    ScoreManager.Instance.AddScore(Mathf.Min(add, remaining));
                    if (ScoreManager.Instance.Score >= MaxRunScore)
                        EndGame(DeathCause.ScoreLimit);
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
                PauseGame();
        }
        else if (State == GameState.Menu)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                StartGame();
        }
        else if (State == GameState.Paused)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
                ResumeGame();
        }
        else if (State == GameState.GameOver)
        {
            if (Time.unscaledTime - gameOverShownAt > 0.6f &&
                (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)))
                RestartGame();
        }
        else if (State == GameState.Story || State == GameState.HowTo || State == GameState.Shop)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (State == GameState.Story) CloseStory();
                else if (State == GameState.HowTo) CloseHowTo();
                else CloseShop();
            }
            else if (State == GameState.Story &&
                     (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)))
            {
                if (storyFirstTime) StartFromStory();
                else CloseStory();
            }
            else if (State == GameState.HowTo &&
                     (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)))
            {
                CloseHowTo();
            }
        }
    }

    float gameOverShownAt;

    // ---------- Flow ----------
    public void ShowMenu()
    {
        StopAllCoroutines();
        Time.timeScale = 1f;
        State = GameState.Menu;
        ClearSpawns();
        if (Player != null) Player.Hide();
        if (ScoreManager.Instance != null) ScoreManager.Instance.ResetRun();
        if (UIManager.Instance != null) UIManager.Instance.ShowMainMenu();
        if (AudioManager.Instance != null) AudioManager.Instance.SetPaused(false);
        busy = false;
    }

    public void StartGame()
    {
        if (busy) return;
        if (State != GameState.Menu && State != GameState.GameOver) return;
        StartCoroutine(StartRun(false));
    }

    public void RestartGame()
    {
        if (busy) return;
        if (State != GameState.GameOver && State != GameState.Paused) return;
        StartCoroutine(StartRun(true));
    }

    IEnumerator StartRun(bool commitPrevious)
    {
        busy = true;
        Time.timeScale = 1f;
        AudioManager.Instance?.SetPaused(false);

        if (commitPrevious && ScoreManager.Instance != null)
            ScoreManager.Instance.CommitRun();

        ClearSpawns();
        if (ScoreManager.Instance != null) ScoreManager.Instance.ResetRun();
        RunTime = 0f;
        scoreAccumulator = 0f;
        LastRunScore = 0;
        LastRunCoins = 0;
        LastRunRecord = false;

        if (Player != null) Player.PrepareForRun();
        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideAllPanels();
            UIManager.Instance.ShowHud();
            UIManager.Instance.ShowCountdown("3");
        }

        State = GameState.Countdown;
        AudioManager.Instance?.Play(Sfx.Tick, 0.5f);
        yield return new WaitForSecondsRealtime(0.7f);
        if (UIManager.Instance != null) UIManager.Instance.ShowCountdown("2");
        AudioManager.Instance?.Play(Sfx.Tick, 0.5f);
        yield return new WaitForSecondsRealtime(0.7f);
        if (UIManager.Instance != null) UIManager.Instance.ShowCountdown("1");
        AudioManager.Instance?.Play(Sfx.Tick, 0.5f);
        yield return new WaitForSecondsRealtime(0.7f);
        if (UIManager.Instance != null) UIManager.Instance.ShowCountdown("BAY!");
        AudioManager.Instance?.Play(Sfx.Flap, 0.8f);
        yield return new WaitForSecondsRealtime(0.35f);
        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideCountdown();
            UIManager.Instance.ShowHint();
        }

        State = GameState.Playing;
        if (Player != null) Player.BeginRun();
        busy = false;
    }

    public void PauseGame()
    {
        if (State != GameState.Playing) return;
        State = GameState.Paused;
        Time.timeScale = 0f;
        if (UIManager.Instance != null) UIManager.Instance.ShowPause();
        AudioManager.Instance?.SetPaused(true);
    }

    public void ResumeGame()
    {
        if (State != GameState.Paused) return;
        State = GameState.Playing;
        Time.timeScale = 1f;
        if (UIManager.Instance != null) UIManager.Instance.HidePause();
        AudioManager.Instance?.SetPaused(false);
    }

    public void EndGame(DeathCause cause)
    {
        if (State != GameState.Playing) return;
        State = GameState.Dying;
        LastDeathCause = cause;
        bool reachedGoal = cause == DeathCause.ScoreLimit;
        if (reachedGoal)
        {
            AudioManager.Instance?.Play(Sfx.Record, 1f);
            if (EffectsManager.Instance != null && Player != null)
                EffectsManager.Instance.SpawnPop(Player.transform.position, UITheme.Gold);
        }
        else
        {
            AudioManager.Instance?.Play(Sfx.Hit, 1f);
            EffectsManager.Instance?.Shake(0.35f, 0.28f);
            UIManager.Instance?.FlashDamage();
        }
        if (Player != null) Player.Die();
        StartCoroutine(GameOverRoutine());
    }

    IEnumerator GameOverRoutine()
    {
        yield return new WaitForSecondsRealtime(0.16f);
        Time.timeScale = 0f;
        AudioManager.Instance?.SetPaused(true);

        int score = ScoreManager.Instance != null ? ScoreManager.Instance.Score : 0;
        int coins = ScoreManager.Instance != null ? ScoreManager.Instance.RunCoins : 0;
        bool record = ScoreManager.Instance != null && score > ScoreManager.Instance.HighScore;
        if (ScoreManager.Instance != null) ScoreManager.Instance.CommitRun();

        LastRunScore = score;
        LastRunCoins = coins;
        LastRunRecord = record;
        State = GameState.GameOver;
        gameOverShownAt = Time.unscaledTime;

        if (UIManager.Instance != null)
            UIManager.Instance.ShowGameOver(score, ScoreManager.Instance != null ? ScoreManager.Instance.HighScore : 0, coins, record);

        if (record) AudioManager.Instance?.Play(Sfx.Record, 0.9f);
    }

    public void ReturnToMenu()
    {
        StopAllCoroutines();
        if (ScoreManager.Instance != null) ScoreManager.Instance.CommitRun();
        ShowMenu();
    }

    public void OpenShop()
    {
        if (State == GameState.Shop) return;
        returnState = State;
        State = GameState.Shop;
        if (returnState == GameState.Menu) Time.timeScale = 1f;
        else Time.timeScale = 0f;
        AudioManager.Instance?.SetPaused(true);
        if (UIManager.Instance != null) UIManager.Instance.ShowShop();
    }

    public void CloseShop()
    {
        State = returnState;
        if (returnState == GameState.Menu)
        {
            Time.timeScale = 1f;
            if (UIManager.Instance != null) UIManager.Instance.ShowMainMenu();
            AudioManager.Instance?.SetPaused(false);
        }
        else if (returnState == GameState.GameOver)
        {
            Time.timeScale = 0f;
            if (UIManager.Instance != null)
                UIManager.Instance.ShowGameOver(LastRunScore, ScoreManager.Instance != null ? ScoreManager.Instance.HighScore : 0, LastRunCoins, LastRunRecord);
        }
        else if (returnState == GameState.Paused)
        {
            Time.timeScale = 0f;
            if (UIManager.Instance != null) UIManager.Instance.ShowPause();
        }
        else
        {
            ShowMenu();
        }
    }

    public void OpenStory(bool firstTime)
    {
        returnState = State;
        storyFirstTime = firstTime;
        State = GameState.Story;
        Time.timeScale = returnState == GameState.Menu ? 1f : 0f;
        if (UIManager.Instance != null) UIManager.Instance.ShowStory(firstTime);
        AudioManager.Instance?.SetPaused(true);
    }

    public void CloseStory()
    {
        SaveSystem.SaveStorySeen();
        storyFirstTime = false;
        State = GameState.Menu;
        Time.timeScale = 1f;
        if (UIManager.Instance != null) UIManager.Instance.ShowMainMenu();
        AudioManager.Instance?.SetPaused(false);
    }

    public void StartFromStory()
    {
        SaveSystem.SaveStorySeen();
        storyFirstTime = false;
        State = GameState.Menu;
        Time.timeScale = 1f;
        if (UIManager.Instance != null) UIManager.Instance.HideAllPanels();
        StartGame();
    }

    public void OpenHowTo()
    {
        returnState = State;
        State = GameState.HowTo;
        Time.timeScale = returnState == GameState.Menu ? 1f : 0f;
        if (UIManager.Instance != null) UIManager.Instance.ShowHowTo();
    }

    public void CloseHowTo()
    {
        State = GameState.Menu;
        Time.timeScale = 1f;
        if (UIManager.Instance != null) UIManager.Instance.ShowMainMenu();
    }

    public void ClearSpawns()
    {
        var obstacles = FindObjectsByType<ObstacleSpawner>(FindObjectsSortMode.None);
        for (int i = 0; i < obstacles.Length; i++) obstacles[i].ClearAll();

        var birds = FindObjectsByType<BirdSpawn>(FindObjectsSortMode.None);
        for (int i = 0; i < birds.Length; i++) birds[i].ClearAll();

        var coins = FindObjectsByType<CoinSpawner>(FindObjectsSortMode.None);
        for (int i = 0; i < coins.Length; i++) coins[i].ClearAll();

        DestroyTagged("Enemy");
        DestroyTagged("Item");
    }

    static void DestroyTagged(string tag)
    {
        var objects = GameObject.FindGameObjectsWithTag(tag);
        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] != null)
                Destroy(objects[i]);
        }
    }

    void OnApplicationPause(bool pause)
    {
        if (pause && State == GameState.Playing)
            PauseGame();
        if (pause && ScoreManager.Instance != null)
            ScoreManager.Instance.CommitRun();
    }

    void OnApplicationQuit()
    {
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.CommitRun();
    }
}
