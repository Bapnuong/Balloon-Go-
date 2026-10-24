using UnityEngine;
using UnityEngine.EventSystems;

public static class GameBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (Object.FindFirstObjectByType<GameBootstrapMarker>() != null) return;
        var go = new GameObject("~BalloonGoSystems");
        go.AddComponent<GameBootstrapMarker>();
    }
}

public class GameBootstrapMarker : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        UITheme.Init();

        if (AudioManager.Instance == null) gameObject.AddComponent<AudioManager>();
        if (ShopSystem.Instance == null) gameObject.AddComponent<ShopSystem>();
        if (EffectsManager.Instance == null) gameObject.AddComponent<EffectsManager>();
        if (FogController.Instance == null) gameObject.AddComponent<FogController>();

        if (GameManager.Instance == null)
        {
            var gm = new GameObject("GameManager");
            gm.AddComponent<GameManager>();
        }
        if (ScoreManager.Instance == null)
        {
            var sm = new GameObject("ScoreManager");
            sm.AddComponent<ScoreManager>();
        }
        if (UIManager.Instance == null)
        {
            var ui = new GameObject("UIManager");
            ui.AddComponent<UIManager>();
        }

        if (GameManager.Instance != null && GameManager.Instance.Player == null)
            GameManager.Instance.Player = Object.FindFirstObjectByType<PlayerController>();

        var cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = UITheme.Bg;
            if (cam.GetComponent<CameraShake>() == null)
                cam.gameObject.AddComponent<CameraShake>();
        }

        if (EventSystem.current == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }
    }
}
