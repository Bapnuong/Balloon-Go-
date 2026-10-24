using System;
using System.Collections;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Development-only smoke test. It is inert unless the player is launched with
// the "-smoke" command-line argument, so it is safe to keep in a build.
public class RuntimeSmokeTest : MonoBehaviour
{
    static string ShotDir => HasArg("-smoke-fresh")
        ? "C:/Users/ASUS/AppData/Local/Temp/opencode/balloon_shots_fresh"
        : "C:/Users/ASUS/AppData/Local/Temp/opencode/balloon_shots_final";
    static bool started;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (started || !HasSmokeArg()) return;
        started = true;

        if (HasArg("-smoke-fresh"))
        {
            PlayerPrefs.DeleteKey("BalloonGo_StorySeen");
            PlayerPrefs.DeleteKey("BalloonGo_HighScore");
            PlayerPrefs.Save();
        }

        var go = new GameObject("~RuntimeSmokeTest");
        DontDestroyOnLoad(go);
        go.AddComponent<RuntimeSmokeTest>();
    }

    static bool HasSmokeArg()
    {
        try
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
                if (string.Equals(args[i], "-smoke", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(args[i], "-smoke-fresh", StringComparison.OrdinalIgnoreCase))
                    return true;
        }
        catch
        {
            // WebGL and some platforms do not expose command-line arguments.
        }
        return false;
    }

    static bool HasArg(string name)
    {
        try
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
                if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
                    return true;
        }
        catch
        {
        }
        return false;
    }

    void Start()
    {
        Directory.CreateDirectory(ShotDir);
        StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        yield return new WaitForSecondsRealtime(1.5f);

        LogState("initial");
        LogEnvironment();
        LogPanels();
        LogRects();

        if (GameManager.Instance == null)
        {
            Debug.LogError("SMOKE: GameManager missing");
            yield break;
        }

        ResetHighScore();

        if (GameManager.Instance.State == GameState.Story)
        {
            yield return Shot("01_story_first.png");
            GameManager.Instance.StartFromStory();
        }
        else
        {
            yield return Shot("01_menu.png");
            GameManager.Instance.StartGame();
        }

        yield return new WaitForSecondsRealtime(1.2f);
        LogState("countdown");
        yield return Shot("02_countdown.png");

        yield return new WaitForSecondsRealtime(1.5f);
        LogState("gameplay start");
        if (GameManager.Instance.State == GameState.Playing)
            StartCoroutine(Autopilot());
        yield return Shot("03_gameplay_start.png");

        if (GameManager.Instance.State == GameState.Playing)
        {
            GameManager.Instance.PauseGame();
            yield return new WaitForSecondsRealtime(0.5f);
            LogState("paused");
            LogPanels();
            yield return Shot("04_pause.png");
            GameManager.Instance.ResumeGame();
            yield return new WaitForSecondsRealtime(0.25f);
            if (GameManager.Instance.State == GameState.Playing)
                StartCoroutine(Autopilot());
        }

        yield return new WaitForSecondsRealtime(4.5f);
        LogState("autopilot");
        LogSpawns();
        yield return Shot("05_gameplay.png");

        if (GameManager.Instance.State == GameState.Playing)
            GameManager.Instance.EndGame(DeathCause.Obstacle);

        yield return new WaitForSecondsRealtime(0.8f);
        LogState("game over");
        LogPanels();
        yield return Shot("06_gameover.png");

        if (GameManager.Instance.State == GameState.GameOver)
            GameManager.Instance.OpenShop();

        yield return new WaitForSecondsRealtime(0.6f);
        LogState("shop colors");
        Debug.Log("SMOKE: color cards=" + CountCards());
        yield return Shot("07_shop_colors.png");

        if (ScoreManager.Instance != null) ScoreManager.Instance.AddCoin(200);
        var shapeTab = GameObject.Find("TabShapes");
        if (shapeTab != null)
        {
            var b = shapeTab.GetComponent<Button>();
            if (b != null) b.onClick.Invoke();
        }

        yield return new WaitForSecondsRealtime(0.6f);
        LogState("shop shapes");
        Debug.Log("SMOKE: shape cards=" + CountCards());
        yield return Shot("08_shop_shapes.png");

        BuyFirstAffordable();
        yield return new WaitForSecondsRealtime(0.6f);
        LogState("after purchase");
        if (ShopSystem.Instance != null && ShopSystem.Instance.SelectedItem != null)
            Debug.Log("SMOKE: selected=" + ShopSystem.Instance.SelectedItem.Id +
                      " coins=" + (ScoreManager.Instance != null ? ScoreManager.Instance.TotalCoins : -1));
        yield return Shot("09_shop_after_buy.png");

        if (GameManager.Instance.State == GameState.Shop)
            GameManager.Instance.CloseShop();

        yield return new WaitForSecondsRealtime(0.5f);
        LogState("closed shop");
        if (GameManager.Instance.State == GameState.GameOver)
            GameManager.Instance.ReturnToMenu();

        yield return new WaitForSecondsRealtime(0.6f);
        LogState("menu");
        LogPanels();
        LogRects();
        yield return Shot("10_menu.png");

        if (GameManager.Instance.State == GameState.Menu)
        {
            GameManager.Instance.OpenStory(false);
            yield return new WaitForSecondsRealtime(0.5f);
            LogState("story");
            yield return Shot("11_story.png");
            if (GameManager.Instance.State == GameState.Story)
                GameManager.Instance.CloseStory();
            yield return new WaitForSecondsRealtime(0.4f);
        }

        if (GameManager.Instance.State == GameState.Menu)
        {
            GameManager.Instance.OpenHowTo();
            yield return new WaitForSecondsRealtime(0.5f);
            LogState("how to");
            yield return Shot("12_howto.png");
            if (GameManager.Instance.State == GameState.HowTo)
                GameManager.Instance.CloseHowTo();
            yield return new WaitForSecondsRealtime(0.4f);
        }

        LogState("final");
        LogPanels();
        yield return new WaitForSecondsRealtime(0.3f);
        Debug.Log("SMOKE: done");
        Application.Quit();
    }

    IEnumerator Autopilot()
    {
        var player = GameManager.Instance != null ? GameManager.Instance.Player : null;
        if (player == null) yield break;

        while (GameManager.Instance != null && GameManager.Instance.State == GameState.Playing)
        {
            float target = 0.6f;
            Obstacle nearest = null;
            float nearestDx = float.MaxValue;
            var obstacles = FindObjectsByType<Obstacle>(FindObjectsSortMode.None);
            for (int i = 0; i < obstacles.Length; i++)
            {
                float dx = obstacles[i].transform.position.x - player.transform.position.x;
                if (dx < -1f || dx >= nearestDx) continue;
                nearestDx = dx;
                nearest = obstacles[i];
            }

            if (nearest != null && nearestDx < 10f)
            {
                float top = nearest.transform.position.y + 1.95f;
                float bottom = nearest.transform.position.y - 1.95f;
                float above = 5f - top;
                float below = bottom + 5f;
                if (above >= below)
                    target = Mathf.Min(4.2f, top + Mathf.Max(0.6f, above * 0.5f));
                else
                    target = Mathf.Max(-4.2f, bottom - Mathf.Max(0.6f, below * 0.5f));
            }

            var rb = player.GetComponent<Rigidbody2D>();
            if (rb != null && player.transform.position.y < target - 0.1f && rb.linearVelocity.y <= 0.5f)
                player.Flap();

            yield return new WaitForSecondsRealtime(0.08f);
        }
    }

    static void ResetHighScore()
    {
        if (ScoreManager.Instance == null) return;
        var prop = typeof(ScoreManager).GetProperty("HighScore",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (prop != null) prop.SetValue(ScoreManager.Instance, 0);
        SaveSystem.SaveHighScore(0);
        Debug.Log("SMOKE: high score reset");
    }

    static void LogSpawns()
    {
        var obstacles = FindObjectsByType<Obstacle>(FindObjectsSortMode.None);
        var birds = FindObjectsByType<BirdEnemy>(FindObjectsSortMode.None);
        var coins = FindObjectsByType<Coin>(FindObjectsSortMode.None);
        Debug.Log("SMOKE: spawns obstacles=" + obstacles.Length +
                  " birds=" + birds.Length + " coins=" + coins.Length);
    }

    static int CountCards()
    {
        return FindObjectsByType<ShopCardUI>(FindObjectsSortMode.None).Length;
    }

    static void BuyFirstAffordable()
    {
        if (ShopSystem.Instance == null) return;
        var cards = FindObjectsByType<ShopCardUI>(FindObjectsSortMode.None);
        for (int i = 0; i < cards.Length; i++)
        {
            var card = cards[i];
            if (card == null || card.Item == null || card.button == null) continue;
            if (ShopSystem.Instance.IsOwned(card.Item.Id)) continue;
            if (!ShopSystem.Instance.CanAfford(card.Item)) continue;
            Debug.Log("SMOKE: buying " + card.Item.Id + " price=" + card.Item.Price);
            card.button.onClick.Invoke();
            return;
        }
        Debug.Log("SMOKE: no affordable locked card");
    }

    static void LogEnvironment()
    {
        Debug.Log("SMOKE: ui=" + (UIManager.Instance != null) +
                  " font=" + (UITheme.Font != null) +
                  " shop=" + (ShopSystem.Instance != null) +
                  " audio=" + (AudioManager.Instance != null) +
                  " effects=" + (EffectsManager.Instance != null) +
                  " fog=" + (FogController.Instance != null) +
                  " eventSystem=" + (EventSystem.current != null) +
                  " oldCanvas=" + (GameObject.Find("Canvas") != null) +
                  " screen=" + Screen.width + "x" + Screen.height);
    }

    static void LogState(string label)
    {
        Debug.Log("SMOKE: " + label + " state=" +
                  (GameManager.Instance != null ? GameManager.Instance.State.ToString() : "no-game-manager") +
                  " timeScale=" + Time.timeScale +
                  " score=" + (ScoreManager.Instance != null ? ScoreManager.Instance.Score : -1) +
                  " best=" + (ScoreManager.Instance != null ? ScoreManager.Instance.HighScore : -1) +
                  " coins=" + (ScoreManager.Instance != null ? ScoreManager.Instance.TotalCoins : -1));
    }

    static void LogPanels()
    {
        var panels = FindObjectsByType<UIPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < panels.Length; i++)
        {
            var p = panels[i];
            Debug.Log("SMOKE: panel " + p.gameObject.name + " active=" + p.gameObject.activeInHierarchy +
                      " visible=" + p.IsVisible);
        }
        var canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        for (int i = 0; i < canvases.Length; i++)
        {
            Debug.Log("SMOKE: canvas " + canvases[i].gameObject.name +
                      " order=" + canvases[i].sortingOrder +
                      " children=" + canvases[i].transform.childCount);
        }
    }

    static void LogRects()
    {
        var buttons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < buttons.Length; i++)
        {
            var b = buttons[i];
            if (!IsInteresting(b.name)) continue;
            var rt = b.GetComponent<RectTransform>();
            var img = b.GetComponent<Image>();
            var label = b.GetComponentInChildren<Text>(true);
            Debug.Log("SMOKE: button " + b.name +
                      " rect=" + rt.rect.size +
                      " delta=" + rt.sizeDelta +
                      " scale=" + rt.localScale +
                      " anchors=" + rt.anchorMin + "/" + rt.anchorMax +
                      " imgRect=" + (img != null ? img.rectTransform.rect.size.ToString() : "-") +
                      " type=" + (img != null ? img.type.ToString() : "-") +
                      " border=" + (img != null && img.sprite != null ? img.sprite.border.ToString() : "-") +
                      " labelRect=" + (label != null ? label.rectTransform.rect.size.ToString() : "-") +
                      " font=" + (label != null ? label.fontSize : 0));
        }

        var story = FindPanel("StoryPanel");
        if (story != null)
        {
            var content = story.transform.Find("Content") as RectTransform;
            LogRT("story content", content);
            LogRT("story title", content != null ? content.Find("Title") as RectTransform : null);
            LogRT("story body", content != null ? content.Find("Body") as RectTransform : null);
            LogRT("story button", content != null ? content.Find("Continue") as RectTransform : null);
        }

        var menu = FindPanel("MenuPanel");
        if (menu != null)
        {
            var content = menu.transform.Find("Content") as RectTransform;
            LogRT("menu content", content);
            LogRT("menu play", content != null ? content.Find("MenuButtons/Play") as RectTransform : null);
        }

        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas != null)
            Debug.Log("SMOKE: canvas scale=" + canvas.scaleFactor + " pixelRect=" + canvas.pixelRect +
                      " screen=" + Screen.width + "x" + Screen.height);
    }

    static UIPanel FindPanel(string name)
    {
        var panels = FindObjectsByType<UIPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < panels.Length; i++)
            if (panels[i].gameObject.name == name) return panels[i];
        return null;
    }

    static bool IsInteresting(string name)
    {
        return name == "Play" || name == "Shop" || name == "Story" || name == "HowTo" ||
               name == "Continue" || name == "Ok" || name == "Restart" || name == "Menu" ||
               name == "TabColors" || name == "TabShapes" || name == "Close" || name == "Sound";
    }

    static void LogRT(string label, RectTransform rt)
    {
        if (rt == null)
        {
            Debug.Log("SMOKE: " + label + " = null");
            return;
        }
        Debug.Log("SMOKE: " + label + " rect=" + rt.rect.size + " delta=" + rt.sizeDelta +
                  " pos=" + rt.anchoredPosition + " scale=" + rt.localScale +
                  " anchors=" + rt.anchorMin + "/" + rt.anchorMax + " pivot=" + rt.pivot);
    }

    IEnumerator Shot(string name)
    {
        string path = ShotDir + "/" + name;
        ScreenCapture.CaptureScreenshot(path);
        yield return new WaitForSecondsRealtime(0.35f);
        Debug.Log("SMOKE: shot " + path);
    }
}
