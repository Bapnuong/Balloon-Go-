using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    Canvas canvas;
    RectTransform safeRoot;

    UIPanel menuPanel;
    UIPanel pausePanel;
    UIPanel gameOverPanel;
    UIPanel shopPanel;
    UIPanel storyPanel;
    UIPanel howToPanel;

    GameObject hudRoot;
    GameObject countdownRoot;
    GameObject toastRoot;
    CanvasGroup hudGroup;
    CanvasGroup countdownGroup;
    CanvasGroup toastGroup;
    CanvasGroup hintGroup;

    Text scoreText;
    Text hudBestText;
    Text coinText;
    Text hintText;
    Text countdownText;

    Text menuBestText;
    Text menuCoinText;
    Text menuSoundText;
    Image menuPreview;

    Text pauseSoundText;
    Text pauseScoreText;

    Text gameOverTitle;
    Text gameOverSubtitle;
    Text gameOverScore;
    Text gameOverBest;
    Text gameOverCoins;
    GameObject recordBadge;

    Text shopCoinText;
    Image shopPreviewImage;
    Text shopPreviewName;
    Text shopPreviewStatus;
    Text shopPreviewDesc;
    Button shopColorTab;
    Button shopShapeTab;
    RectTransform shopGrid;
    readonly List<ShopCardUI> shopCards = new List<ShopCardUI>();

    Text storyButtonLabel;
    Button storyButton;
    bool storyFirstTime;

    Text toastText;
    Image damageFlash;

    ShopCategory shopTab = ShopCategory.Color;
    float scorePulse;
    float coinPulse;
    Coroutine hintRoutine;
    Coroutine toastRoutine;
    Coroutine damageRoutine;
    bool ready;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // The prototype scene has a hand-built Canvas. The new UI is generated at runtime.
        var oldCanvas = GameObject.Find("Canvas");
        if (oldCanvas != null)
            Destroy(oldCanvas);
    }

    void OnDestroy()
    {
        Unsubscribe();
        if (Instance == this) Instance = null;
    }

    void Start()
    {
        BuildUI();
        Subscribe();
        HideAllPanels();
    }

    void Update()
    {
        if (scoreText != null)
        {
            scorePulse = Mathf.MoveTowards(scorePulse, 0f, Time.unscaledDeltaTime * 5f);
            scoreText.rectTransform.localScale = Vector3.one * (1f + 0.07f * scorePulse);
        }

        if (coinText != null)
        {
            coinPulse = Mathf.MoveTowards(coinPulse, 0f, Time.unscaledDeltaTime * 5f);
            coinText.rectTransform.localScale = Vector3.one * (1f + 0.08f * coinPulse);
        }

        if (countdownText != null && countdownRoot != null && countdownRoot.activeSelf)
        {
            countdownText.rectTransform.localScale = Vector3.Lerp(
                countdownText.rectTransform.localScale, Vector3.one, Time.unscaledDeltaTime * 12f);
        }
    }

    // ---------------------------------------------------------------- build

    void BuildUI()
    {
        if (ready) return;
        ready = true;
        UITheme.Init();

        var canvasGo = new GameObject("BG_Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        var safe = new GameObject("SafeRoot", typeof(RectTransform));
        safe.transform.SetParent(canvasGo.transform, false);
        safeRoot = (RectTransform)safe.transform;
        UITheme.Stretch(safeRoot);
        safe.AddComponent<SafeArea>();

        BuildMenu();
        BuildHud();
        BuildPause();
        BuildGameOver();
        BuildShop();
        BuildStory();
        BuildHowTo();
        BuildCountdown();
        BuildDamageFlash();
        BuildToast();
    }

    UIPanel MakeFullPanel(string name, float dimAlpha, out RectTransform content)
    {
        var root = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup));
        root.transform.SetParent(safeRoot, false);
        var rootRt = (RectTransform)root.transform;
        UITheme.Stretch(rootRt);

        var dim = UITheme.CreateImage(root.transform, "Dim", null,
            new Color(0.012f, 0.045f, 0.04f, dimAlpha), true);
        UITheme.Stretch(dim.rectTransform);

        content = new GameObject("Content", typeof(RectTransform)).GetComponent<RectTransform>();
        content.SetParent(root.transform, false);
        UITheme.Stretch(content);

        var panel = root.AddComponent<UIPanel>();
        panel.content = content;
        root.SetActive(false);
        return panel;
    }

    UIPanel MakeModal(string name, Vector2 size, out RectTransform content)
    {
        var root = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup));
        root.transform.SetParent(safeRoot, false);
        var rootRt = (RectTransform)root.transform;
        UITheme.Stretch(rootRt);

        var dim = UITheme.CreateImage(root.transform, "Dim", null,
            new Color(0.012f, 0.045f, 0.04f, 0.78f), true);
        UITheme.Stretch(dim.rectTransform);

        content = UITheme.CreatePanel(root.transform, "Content", size, UITheme.Panel, true);
        UITheme.AddOutline(content.GetComponent<Image>(),
            new Color(0.45f, 0.78f, 0.66f, 0.16f), new Vector2(2f, -2f));

        var panel = root.AddComponent<UIPanel>();
        panel.content = content;
        root.SetActive(false);
        return panel;
    }

    static void Place(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    static void TopLeft(RectTransform rt, Vector2 size, Vector2 pos)
    {
        Place(rt, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(pos.x, -pos.y), size);
    }

    static void TopRight(RectTransform rt, Vector2 size, Vector2 pos)
    {
        Place(rt, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-pos.x, -pos.y), size);
    }

    static void BottomLeft(RectTransform rt, Vector2 size, Vector2 pos)
    {
        Place(rt, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), pos, size);
    }

    static void BottomRight(RectTransform rt, Vector2 size, Vector2 pos)
    {
        Place(rt, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-pos.x, pos.y), size);
    }

    static void Centered(RectTransform rt, Vector2 size, Vector2 pos)
    {
        Place(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, size);
    }

    // ---------------------------------------------------------------- menu

    void BuildMenu()
    {
        RectTransform content;
        menuPanel = MakeFullPanel("MenuPanel", 0.34f, out content);

        var title = UITheme.CreateShadowText(content, "Title", "BALLOON GO", 96, UITheme.Cream,
            TextAnchor.UpperLeft, FontStyle.Bold);
        TopLeft(title.rectTransform, new Vector2(1100f, 120f), new Vector2(80f, 50f));

        var accent = UITheme.CreateImage(content, "Accent", UITheme.RoundedSmall, UITheme.Gold, false);
        TopLeft(accent.rectTransform, new Vector2(110f, 6f), new Vector2(84f, 178f));

        var subtitle = UITheme.CreateText(content, "Subtitle", "TÌM ĐƯỜNG VỀ NHÀ", 26, UITheme.Mint,
            TextAnchor.UpperLeft);
        TopLeft(subtitle.rectTransform, new Vector2(800f, 36f), new Vector2(84f, 196f));

        menuBestText = UITheme.CreateText(content, "MenuBest", "KỶ LỤC 0", 22, UITheme.Muted,
            TextAnchor.UpperLeft);
        TopLeft(menuBestText.rectTransform, new Vector2(520f, 30f), new Vector2(84f, 240f));

        var stack = new GameObject("MenuButtons", typeof(RectTransform), typeof(VerticalLayoutGroup));
        stack.transform.SetParent(content, false);
        var stackRt = (RectTransform)stack.transform;
        Place(stackRt, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(84f, -20f), new Vector2(440f, 360f));
        var vlg = stack.GetComponent<VerticalLayoutGroup>();
        vlg.spacing = 12f;
        vlg.childAlignment = TextAnchor.UpperLeft;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = false;
        vlg.childForceExpandHeight = false;

        UITheme.CreateButton(stack.transform, "Play", "CHƠI", new Vector2(440f, 78f),
            UITheme.Gold, UITheme.PanelDark, () => GameManager.Instance.StartGame(), 36);
        UITheme.CreateButton(stack.transform, "Shop", "CỬA HÀNG", new Vector2(440f, 66f),
            UITheme.PanelLight, UITheme.Cream, () => GameManager.Instance.OpenShop(), 30);
        UITheme.CreateButton(stack.transform, "Story", "CÂU CHUYỆN", new Vector2(440f, 58f),
            UITheme.WithAlpha(UITheme.PanelLight, 0.62f), UITheme.Cream,
            () => GameManager.Instance.OpenStory(false), 24);
        UITheme.CreateButton(stack.transform, "HowTo", "HƯỚNG DẪN", new Vector2(440f, 58f),
            UITheme.WithAlpha(UITheme.PanelLight, 0.62f), UITheme.Cream,
            () => GameManager.Instance.OpenHowTo(), 24);

        var preview = new GameObject("Preview", typeof(RectTransform));
        preview.transform.SetParent(content, false);
        var previewRt = (RectTransform)preview.transform;
        Place(previewRt, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
            new Vector2(-120f, 10f), new Vector2(420f, 500f));

        var glow = UITheme.CreateImage(preview.transform, "Glow", UITheme.SoftCircle,
            new Color(0.5f, 0.9f, 0.78f, 0.10f), false);
        Centered(glow.rectTransform, new Vector2(460f, 460f), new Vector2(0f, 20f));

        menuPreview = UITheme.CreateImage(preview.transform, "Balloon", null, Color.white, false);
        menuPreview.preserveAspect = true;
        Centered(menuPreview.rectTransform, new Vector2(280f, 380f), new Vector2(0f, 30f));
        var previewFloat = menuPreview.gameObject.AddComponent<FloatingUI>();
        previewFloat.amplitude = 16f;
        previewFloat.speed = 0.95f;

        AddFamilyBalloon(preview.transform, "Balloons/balloon_3", new Vector2(80f, 110f), new Vector2(-170f, 150f), 0.7f, 12f);
        AddFamilyBalloon(preview.transform, "Balloons/balloon_5", new Vector2(72f, 100f), new Vector2(150f, 175f), 0.8f, 10f);
        AddFamilyBalloon(preview.transform, "Balloons/balloon_8", new Vector2(64f, 90f), new Vector2(160f, -60f), 0.6f, 9f);

        var previewLabel = UITheme.CreateText(preview.transform, "PreviewLabel", "BÓNG CỦA BẠN", 20,
            UITheme.Muted, TextAnchor.MiddleCenter);
        Centered(previewLabel.rectTransform, new Vector2(300f, 30f), new Vector2(0f, -210f));

        var coinPanel = UITheme.CreatePanel(content, "MenuCoins", new Vector2(230f, 60f),
            new Color(0.04f, 0.10f, 0.09f, 0.78f), false);
        TopRight(coinPanel, new Vector2(230f, 60f), new Vector2(60f, 54f));
        coinPanel.GetComponent<Image>().raycastTarget = false;

        var coinIcon = UITheme.CreateImage(coinPanel, "CoinIcon", UITheme.Coin, Color.white, false);
        Place(coinIcon.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(16f, 0f), new Vector2(40f, 40f));

        menuCoinText = UITheme.CreateText(coinPanel, "CoinText", "0", 28, UITheme.Gold,
            TextAnchor.MiddleLeft, FontStyle.Bold);
        Place(menuCoinText.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero);
        menuCoinText.rectTransform.offsetMin = new Vector2(64f, 0f);
        menuCoinText.rectTransform.offsetMax = new Vector2(-16f, 0f);

        var sound = UITheme.CreateButton(content, "MenuSound", "ÂM THANH: BẬT", new Vector2(240f, 50f),
            new Color(0.05f, 0.12f, 0.10f, 0.78f), UITheme.Cream, () =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.ToggleSound();
                UpdateSoundButtons();
            }, 18);
        BottomRight(sound.GetComponent<RectTransform>(), new Vector2(240f, 50f), new Vector2(60f, 44f));
        menuSoundText = sound.GetComponentInChildren<Text>();

        var version = UITheme.CreateText(content, "Version", "v1.0  •  Một dự án indie", 18,
            UITheme.Muted, TextAnchor.LowerLeft);
        BottomLeft(version.rectTransform, new Vector2(500f, 30f), new Vector2(84f, 40f));
    }

    void AddFamilyBalloon(Transform parent, string resource, Vector2 size, Vector2 pos, float speed,
        float amplitude, float alpha = 0.30f)
    {
        var sprite = Resources.Load<Sprite>(resource);
        if (sprite == null) return;
        var img = UITheme.CreateImage(parent, "FamilyBalloon", sprite, new Color(1f, 1f, 1f, alpha), false);
        img.preserveAspect = true;
        Centered(img.rectTransform, size, pos);
        var fl = img.gameObject.AddComponent<FloatingUI>();
        fl.speed = speed;
        fl.amplitude = amplitude;
        fl.rotation = 3f;
    }

    // ---------------------------------------------------------------- hud

    void BuildHud()
    {
        hudRoot = new GameObject("HUD", typeof(RectTransform), typeof(CanvasGroup));
        hudRoot.transform.SetParent(safeRoot, false);
        UITheme.Stretch((RectTransform)hudRoot.transform);
        hudGroup = hudRoot.GetComponent<CanvasGroup>();

        var scorePanel = UITheme.CreatePanel(hudRoot.transform, "ScorePanel", new Vector2(240f, 110f),
            new Color(0.04f, 0.10f, 0.09f, 0.58f), false);
        Place(scorePanel, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -24f), new Vector2(240f, 110f));
        scorePanel.GetComponent<Image>().raycastTarget = false;

        var scoreLabel = UITheme.CreateText(scorePanel, "Label", "ĐIỂM", 18, UITheme.Muted,
            TextAnchor.MiddleCenter);
        Place(scoreLabel.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -12f), new Vector2(200f, 24f));

        scoreText = UITheme.CreateShadowText(scorePanel, "Score", "0", 52, UITheme.Cream,
            TextAnchor.MiddleCenter, FontStyle.Bold);
        Centered(scoreText.rectTransform, new Vector2(220f, 64f), new Vector2(0f, -18f));

        var bestPanel = UITheme.CreatePanel(hudRoot.transform, "BestPanel", new Vector2(230f, 56f),
            new Color(0.04f, 0.10f, 0.09f, 0.46f), false);
        TopLeft(bestPanel, new Vector2(230f, 56f), new Vector2(28f, 24f));
        bestPanel.GetComponent<Image>().raycastTarget = false;

        hudBestText = UITheme.CreateText(bestPanel, "Best", "KỶ LỤC 0", 20, UITheme.Mint,
            TextAnchor.MiddleCenter, FontStyle.Bold);
        UITheme.Stretch(hudBestText.rectTransform, 8f);

        var coinPanel = UITheme.CreatePanel(hudRoot.transform, "CoinPanel", new Vector2(210f, 60f),
            new Color(0.04f, 0.10f, 0.09f, 0.58f), false);
        TopRight(coinPanel, new Vector2(210f, 60f), new Vector2(104f, 24f));
        coinPanel.GetComponent<Image>().raycastTarget = false;

        var coinIcon = UITheme.CreateImage(coinPanel, "CoinIcon", UITheme.Coin, Color.white, false);
        Place(coinIcon.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(14f, 0f), new Vector2(36f, 36f));

        coinText = UITheme.CreateText(coinPanel, "CoinText", "0", 28, UITheme.Gold,
            TextAnchor.MiddleLeft, FontStyle.Bold);
        Place(coinText.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero);
        coinText.rectTransform.offsetMin = new Vector2(58f, 0f);
        coinText.rectTransform.offsetMax = new Vector2(-14f, 0f);

        var pause = UITheme.CreateButton(hudRoot.transform, "PauseButton", "II", new Vector2(64f, 64f),
            new Color(0.05f, 0.12f, 0.10f, 0.86f), UITheme.Cream,
            () => GameManager.Instance.PauseGame(), 26);
        TopRight(pause.GetComponent<RectTransform>(), new Vector2(64f, 64f), new Vector2(28f, 24f));

        hintText = UITheme.CreateShadowText(hudRoot.transform, "Hint", "Nhấn SPACE hoặc chạm để bay",
            26, UITheme.Cream, TextAnchor.MiddleCenter);
        Place(hintText.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0f, 120f), new Vector2(900f, 40f));
        hintGroup = hintText.gameObject.AddComponent<CanvasGroup>();
        hintText.gameObject.SetActive(false);

        hudRoot.SetActive(false);
    }

    // ---------------------------------------------------------------- pause

    void BuildPause()
    {
        RectTransform content;
        pausePanel = MakeModal("PausePanel", new Vector2(560f, 580f), out content);

        var title = UITheme.CreateShadowText(content, "Title", "TẠM DỪNG", 48, UITheme.Gold,
            TextAnchor.MiddleCenter, FontStyle.Bold);
        Centered(title.rectTransform, new Vector2(500f, 64f), new Vector2(0f, 240f));

        var subtitle = UITheme.CreateText(content, "Subtitle", "Hít thở chút nào...", 20, UITheme.Muted,
            TextAnchor.MiddleCenter);
        Centered(subtitle.rectTransform, new Vector2(460f, 32f), new Vector2(0f, 190f));

        pauseScoreText = UITheme.CreateText(content, "Score", "ĐIỂM 0", 24, UITheme.Cream,
            TextAnchor.MiddleCenter, FontStyle.Bold);
        Centered(pauseScoreText.rectTransform, new Vector2(420f, 34f), new Vector2(0f, 140f));

        var stack = new GameObject("Buttons", typeof(RectTransform), typeof(VerticalLayoutGroup));
        stack.transform.SetParent(content, false);
        var stackRt = (RectTransform)stack.transform;
        Place(stackRt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, -50f), new Vector2(420f, 250f));
        var vlg = stack.GetComponent<VerticalLayoutGroup>();
        vlg.spacing = 12f;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = false;
        vlg.childForceExpandHeight = false;

        UITheme.CreateButton(stack.transform, "Resume", "TIẾP TỤC", new Vector2(420f, 64f),
            UITheme.Gold, UITheme.PanelDark, () => GameManager.Instance.ResumeGame(), 30);
        UITheme.CreateButton(stack.transform, "Restart", "CHƠI LẠI", new Vector2(420f, 60f),
            UITheme.PanelLight, UITheme.Cream, () => GameManager.Instance.RestartGame(), 28);
        UITheme.CreateButton(stack.transform, "Menu", "VỀ MENU", new Vector2(420f, 56f),
            UITheme.WithAlpha(UITheme.PanelLight, 0.6f), UITheme.Cream,
            () => GameManager.Instance.ReturnToMenu(), 24);

        var sound = UITheme.CreateButton(content, "Sound", "ÂM THANH: BẬT", new Vector2(260f, 46f),
            new Color(0.05f, 0.12f, 0.10f, 0.75f), UITheme.Cream, () =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.ToggleSound();
                UpdateSoundButtons();
            }, 18);
        Place(sound.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f), new Vector2(0f, 45f), new Vector2(260f, 46f));
        pauseSoundText = sound.GetComponentInChildren<Text>();
    }

    // ---------------------------------------------------------------- game over

    void BuildGameOver()
    {
        RectTransform content;
        gameOverPanel = MakeModal("GameOverPanel", new Vector2(680f, 740f), out content);

        gameOverTitle = UITheme.CreateShadowText(content, "Title", "BẠN ĐÃ LẠC!", 54, UITheme.Cream,
            TextAnchor.MiddleCenter, FontStyle.Bold);
        Centered(gameOverTitle.rectTransform, new Vector2(620f, 72f), new Vector2(0f, 310f));

        gameOverSubtitle = UITheme.CreateText(content, "Subtitle", "Đừng lo, gia đình vẫn đang đợi bạn.",
            20, UITheme.Muted, TextAnchor.MiddleCenter);
        Centered(gameOverSubtitle.rectTransform, new Vector2(580f, 32f), new Vector2(0f, 258f));

        var scoreLabel = UITheme.CreateText(content, "ScoreLabel", "ĐIỂM", 18, UITheme.Muted,
            TextAnchor.MiddleCenter);
        Centered(scoreLabel.rectTransform, new Vector2(200f, 24f), new Vector2(0f, 198f));

        gameOverScore = UITheme.CreateShadowText(content, "Score", "0", 76, UITheme.Gold,
            TextAnchor.MiddleCenter, FontStyle.Bold);
        Centered(gameOverScore.rectTransform, new Vector2(320f, 92f), new Vector2(0f, 150f));

        gameOverBest = UITheme.CreateText(content, "Best", "KỶ LỤC 0", 24, UITheme.Mint,
            TextAnchor.MiddleCenter, FontStyle.Bold);
        Centered(gameOverBest.rectTransform, new Vector2(460f, 34f), new Vector2(0f, 78f));

        gameOverCoins = UITheme.CreateText(content, "Coins", "XU KIẾM ĐƯỢC  +0", 22, UITheme.Gold,
            TextAnchor.MiddleCenter, FontStyle.Bold);
        Centered(gameOverCoins.rectTransform, new Vector2(460f, 32f), new Vector2(0f, 38f));

        var badge = UITheme.CreatePanel(content, "RecordBadge", new Vector2(280f, 44f), UITheme.Gold, false);
        Centered(badge, new Vector2(280f, 44f), new Vector2(0f, -12f));
        badge.GetComponent<Image>().raycastTarget = false;
        var badgeText = UITheme.CreateText(badge, "Text", "KỶ LỤC MỚI!", 20, UITheme.PanelDark,
            TextAnchor.MiddleCenter, FontStyle.Bold);
        UITheme.Stretch(badgeText.rectTransform, 6f);
        recordBadge = badge.gameObject;
        recordBadge.SetActive(false);

        var stack = new GameObject("Buttons", typeof(RectTransform), typeof(VerticalLayoutGroup));
        stack.transform.SetParent(content, false);
        var stackRt = (RectTransform)stack.transform;
        Place(stackRt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, -167f), new Vector2(460f, 240f));
        var vlg = stack.GetComponent<VerticalLayoutGroup>();
        vlg.spacing = 10f;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = false;
        vlg.childForceExpandHeight = false;

        UITheme.CreateButton(stack.transform, "Restart", "CHƠI LẠI", new Vector2(460f, 64f),
            UITheme.Gold, UITheme.PanelDark, () => GameManager.Instance.RestartGame(), 30);
        UITheme.CreateButton(stack.transform, "Shop", "CỬA HÀNG", new Vector2(460f, 60f),
            UITheme.PanelLight, UITheme.Cream, () => GameManager.Instance.OpenShop(), 28);
        UITheme.CreateButton(stack.transform, "Menu", "VỀ MENU", new Vector2(460f, 54f),
            UITheme.WithAlpha(UITheme.PanelLight, 0.6f), UITheme.Cream,
            () => GameManager.Instance.ReturnToMenu(), 24);

        var hint = UITheme.CreateText(content, "Hint", "Nhấn SPACE để chơi lại", 18, UITheme.Muted,
            TextAnchor.MiddleCenter);
        Centered(hint.rectTransform, new Vector2(400f, 24f), new Vector2(0f, -310f));
    }

    // ---------------------------------------------------------------- shop

    void BuildShop()
    {
        RectTransform content;
        shopPanel = MakeModal("ShopPanel", new Vector2(1240f, 780f), out content);

        var title = UITheme.CreateShadowText(content, "Title", "CỬA HÀNG", 46, UITheme.Gold,
            TextAnchor.MiddleLeft, FontStyle.Bold);
        TopLeft(title.rectTransform, new Vector2(500f, 60f), new Vector2(44f, 30f));

        var close = UITheme.CreateButton(content, "Close", "X", new Vector2(52f, 52f),
            UITheme.PanelLight, UITheme.Cream, () => GameManager.Instance.CloseShop(), 24);
        TopRight(close.GetComponent<RectTransform>(), new Vector2(52f, 52f), new Vector2(36f, 30f));

        var coinPanel = UITheme.CreatePanel(content, "Coins", new Vector2(230f, 56f),
            new Color(0.04f, 0.10f, 0.09f, 0.72f), false);
        TopRight(coinPanel, new Vector2(230f, 56f), new Vector2(104f, 30f));
        coinPanel.GetComponent<Image>().raycastTarget = false;

        var coinIcon = UITheme.CreateImage(coinPanel, "CoinIcon", UITheme.Coin, Color.white, false);
        Place(coinIcon.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(14f, 0f), new Vector2(34f, 34f));

        shopCoinText = UITheme.CreateText(coinPanel, "CoinText", "0", 26, UITheme.Gold,
            TextAnchor.MiddleLeft, FontStyle.Bold);
        Place(shopCoinText.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero);
        shopCoinText.rectTransform.offsetMin = new Vector2(56f, 0f);
        shopCoinText.rectTransform.offsetMax = new Vector2(-14f, 0f);

        shopColorTab = UITheme.CreateButton(content, "TabColors", "MÀU SẮC", new Vector2(180f, 48f),
            UITheme.Gold, UITheme.PanelDark, () =>
            {
                shopTab = ShopCategory.Color;
                RefreshShop();
            }, 22);
        TopLeft(shopColorTab.GetComponent<RectTransform>(), new Vector2(180f, 48f), new Vector2(44f, 110f));

        shopShapeTab = UITheme.CreateButton(content, "TabShapes", "HÌNH DÁNG", new Vector2(180f, 48f),
            UITheme.PanelLight, UITheme.Cream, () =>
            {
                shopTab = ShopCategory.Shape;
                RefreshShop();
            }, 22);
        TopLeft(shopShapeTab.GetComponent<RectTransform>(), new Vector2(180f, 48f), new Vector2(236f, 110f));

        var preview = UITheme.CreatePanel(content, "Preview", new Vector2(330f, 520f),
            new Color(0.035f, 0.085f, 0.075f, 0.72f), false);
        TopLeft(preview, new Vector2(330f, 520f), new Vector2(44f, 180f));
        preview.GetComponent<Image>().raycastTarget = false;

        var glow = UITheme.CreateImage(preview, "Glow", UITheme.SoftCircle,
            new Color(0.5f, 0.9f, 0.78f, 0.08f), false);
        Centered(glow.rectTransform, new Vector2(280f, 280f), new Vector2(0f, 80f));

        shopPreviewImage = UITheme.CreateImage(preview, "Balloon", null, Color.white, false);
        shopPreviewImage.preserveAspect = true;
        Centered(shopPreviewImage.rectTransform, new Vector2(210f, 280f), new Vector2(0f, 70f));

        shopPreviewName = UITheme.CreateText(preview, "Name", "", 28, UITheme.Cream,
            TextAnchor.MiddleCenter, FontStyle.Bold);
        Centered(shopPreviewName.rectTransform, new Vector2(290f, 38f), new Vector2(0f, -100f));

        shopPreviewStatus = UITheme.CreateText(preview, "Status", "ĐANG DÙNG", 20, UITheme.Mint,
            TextAnchor.MiddleCenter, FontStyle.Bold);
        Centered(shopPreviewStatus.rectTransform, new Vector2(290f, 28f), new Vector2(0f, -136f));

        shopPreviewDesc = UITheme.CreateText(preview, "Desc", "", 20, UITheme.Muted,
            TextAnchor.UpperCenter);
        shopPreviewDesc.horizontalOverflow = HorizontalWrapMode.Wrap;
        shopPreviewDesc.verticalOverflow = VerticalWrapMode.Overflow;
        shopPreviewDesc.lineSpacing = 1.1f;
        Centered(shopPreviewDesc.rectTransform, new Vector2(290f, 100f), new Vector2(0f, -195f));

        BuildShopGrid(content);

        var footer = UITheme.CreateText(content, "Footer", "Chạm vào vật phẩm để mua hoặc chọn.", 20,
            UITheme.Muted, TextAnchor.LowerLeft);
        BottomLeft(footer.rectTransform, new Vector2(700f, 28f), new Vector2(44f, 40f));
    }

    void BuildShopGrid(RectTransform content)
    {
        var viewportGo = new GameObject("Viewport", typeof(RectTransform), typeof(CanvasRenderer),
            typeof(Image), typeof(RectMask2D));
        viewportGo.transform.SetParent(content, false);
        var viewportRt = (RectTransform)viewportGo.transform;
        TopLeft(viewportRt, new Vector2(790f, 520f), new Vector2(410f, 180f));
        var vpImage = viewportGo.GetComponent<Image>();
        vpImage.color = new Color(0f, 0f, 0f, 0.001f);
        vpImage.raycastTarget = true;

        var gridGo = new GameObject("Grid", typeof(RectTransform));
        gridGo.transform.SetParent(viewportGo.transform, false);
        shopGrid = (RectTransform)gridGo.transform;
        shopGrid.anchorMin = new Vector2(0f, 1f);
        shopGrid.anchorMax = new Vector2(1f, 1f);
        shopGrid.pivot = new Vector2(0.5f, 1f);
        shopGrid.anchoredPosition = Vector2.zero;
        shopGrid.sizeDelta = Vector2.zero;

        var grid = gridGo.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(180f, 150f);
        grid.spacing = new Vector2(14f, 14f);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 4;
        grid.childAlignment = TextAnchor.UpperLeft;

        var fitter = gridGo.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var scroll = viewportGo.AddComponent<ScrollRect>();
        scroll.viewport = viewportRt;
        scroll.content = shopGrid;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 30f;
        scroll.inertia = true;
        scroll.decelerationRate = 0.135f;
    }

    void RefreshShop()
    {
        if (ShopSystem.Instance == null || shopGrid == null) return;

        for (int i = shopGrid.childCount - 1; i >= 0; i--)
        {
            var child = shopGrid.GetChild(i).gameObject;
            child.SetActive(false);
            Destroy(child);
        }
        shopCards.Clear();

        var items = ShopSystem.Instance.GetItems(shopTab);
        for (int i = 0; i < items.Count; i++)
            CreateShopCard(items[i]);

        RefreshTabVisuals();
        RefreshShopCards();
    }

    void CreateShopCard(ShopItem item)
    {
        var go = new GameObject("Card_" + item.Id, typeof(RectTransform), typeof(CanvasRenderer),
            typeof(Image), typeof(Button));
        go.transform.SetParent(shopGrid, false);

        var bg = go.GetComponent<Image>();
        bg.sprite = UITheme.Rounded;
        bg.type = Image.Type.Sliced;
        bg.color = UITheme.PanelDark;

        var button = go.GetComponent<Button>();
        button.targetGraphic = bg;
        var colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.06f, 1.06f, 1.06f, 1f);
        colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
        colors.fadeDuration = 0.08f;
        button.colors = colors;
        button.onClick.AddListener(() => OnShopItemClicked(item));
        go.AddComponent<ButtonFX>();

        var icon = UITheme.CreateImage(go.transform, "Icon", item.Sprite, Color.white, false);
        icon.preserveAspect = true;
        Place(icon.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -12f), new Vector2(64f, 64f));

        var nameText = UITheme.CreateText(go.transform, "Name", item.Name, 20, UITheme.Cream,
            TextAnchor.MiddleCenter, FontStyle.Bold);
        Place(nameText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -82f), new Vector2(160f, 24f));

        var statusText = UITheme.CreateText(go.transform, "Status", "", 18, UITheme.Gold,
            TextAnchor.MiddleCenter);
        Place(statusText.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0f, 12f), new Vector2(160f, 22f));

        var card = go.AddComponent<ShopCardUI>();
        card.Item = item;
        card.background = bg;
        card.icon = icon;
        card.nameText = nameText;
        card.statusText = statusText;
        card.button = button;
        shopCards.Add(card);
    }

    void RefreshShopCards()
    {
        if (ShopSystem.Instance == null) return;
        for (int i = 0; i < shopCards.Count; i++)
        {
            var card = shopCards[i];
            if (card == null || card.Item == null) continue;
            bool owned = ShopSystem.Instance.IsOwned(card.Item.Id);
            bool selected = ShopSystem.Instance.IsSelected(card.Item.Id);
            bool affordable = ShopSystem.Instance.CanAfford(card.Item);
            card.Refresh(card.Item, owned, selected, affordable);
        }
    }

    void RefreshTabVisuals()
    {
        SetTabVisual(shopColorTab, shopTab == ShopCategory.Color);
        SetTabVisual(shopShapeTab, shopTab == ShopCategory.Shape);
    }

    void SetTabVisual(Button tab, bool active)
    {
        if (tab == null) return;
        var img = tab.GetComponent<Image>();
        var label = tab.GetComponentInChildren<Text>();
        img.color = active ? UITheme.Gold : UITheme.PanelLight;
        if (label != null) label.color = active ? UITheme.PanelDark : UITheme.Cream;
    }

    void UpdateShopPreview()
    {
        if (ShopSystem.Instance == null || ShopSystem.Instance.SelectedItem == null) return;
        var item = ShopSystem.Instance.SelectedItem;
        if (shopPreviewImage != null)
        {
            shopPreviewImage.sprite = item.Sprite;
            shopPreviewImage.preserveAspect = true;
        }
        if (shopPreviewName != null) shopPreviewName.text = item.Name;
        if (shopPreviewStatus != null) shopPreviewStatus.text = "ĐANG DÙNG";
        if (shopPreviewDesc != null) shopPreviewDesc.text = item.Description;
    }

    void OnShopItemClicked(ShopItem item)
    {
        if (ShopSystem.Instance == null) return;
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();

        var result = ShopSystem.Instance.BuyOrSelect(item);
        if (result == ShopActionResult.Selected)
        {
            ShowToast("Đã chọn: " + item.Name);
        }
        else if (result == ShopActionResult.Purchased)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.Play(Sfx.Buy, 0.9f);
            ShowToast("Đã mở khóa: " + item.Name + "!");
        }
        else if (result == ShopActionResult.NotEnoughCoins)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.Play(Sfx.Error, 0.7f);
            int missing = item.Price - (ScoreManager.Instance != null ? ScoreManager.Instance.TotalCoins : 0);
            ShowToast("Không đủ xu! Cần thêm " + Mathf.Max(0, missing) + " xu.");
        }

        UpdateShopPreview();
        RefreshShopCards();
        UpdateShopCoins();
    }

    void UpdateShopCoins()
    {
        if (shopCoinText != null && ScoreManager.Instance != null)
            shopCoinText.text = ScoreManager.Instance.TotalCoins.ToString();
    }

    // ---------------------------------------------------------------- story / how to

    void BuildStory()
    {
        RectTransform content;
        storyPanel = MakeModal("StoryPanel", new Vector2(880f, 620f), out content);

        var title = UITheme.CreateShadowText(content, "Title", "CÂU CHUYỆN", 46, UITheme.Gold,
            TextAnchor.MiddleCenter, FontStyle.Bold);
        Centered(title.rectTransform, new Vector2(700f, 60f), new Vector2(0f, 260f));

        var chapter = UITheme.CreateText(content, "Chapter", "CHƯƠNG 1: LẠC TRONG RỪNG SƯƠNG", 20,
            UITheme.Mint, TextAnchor.MiddleCenter, FontStyle.Bold);
        Centered(chapter.rectTransform, new Vector2(760f, 28f), new Vector2(0f, 215f));

        var body = UITheme.CreateText(content, "Body",
            "Gia đình bóng bay vào rừng chơi. Trong lúc mải bay theo những tia nắng, bạn bị lạc trong làn sương mù.\n\n" +
            "Những cành cây và chú chim giật mình bay ra. Hãy giữ bóng bay an toàn, nhặt những đồng xu và tìm đường về với gia đình!",
            24, UITheme.Cream, TextAnchor.UpperCenter);
        body.horizontalOverflow = HorizontalWrapMode.Wrap;
        body.verticalOverflow = VerticalWrapMode.Overflow;
        body.lineSpacing = 1.15f;
        Centered(body.rectTransform, new Vector2(760f, 330f), new Vector2(0f, 30f));

        AddFamilyBalloon(content, "Balloons/balloon_3", new Vector2(52f, 70f),
            new Vector2(-105f, -78f), 0.55f, 8f, 0.55f);
        AddFamilyBalloon(content, "Balloons/balloon_5", new Vector2(46f, 62f),
            new Vector2(0f, -64f), 0.65f, 7f, 0.55f);
        AddFamilyBalloon(content, "Balloons/balloon_8", new Vector2(42f, 58f),
            new Vector2(105f, -82f), 0.5f, 6f, 0.55f);

        var familyCaption = UITheme.CreateText(content, "FamilyCaption",
            "GIA ĐÌNH ĐANG ĐỢI BẠN", 18, UITheme.Mint, TextAnchor.MiddleCenter, FontStyle.Bold);
        Centered(familyCaption.rectTransform, new Vector2(600f, 26f), new Vector2(0f, -138f));

        storyButton = UITheme.CreateButton(content, "Continue", "BẮT ĐẦU HÀNH TRÌNH", new Vector2(440f, 64f),
            UITheme.Gold, UITheme.PanelDark, OnStoryContinue, 24);
        Centered(storyButton.GetComponent<RectTransform>(), new Vector2(440f, 64f), new Vector2(0f, -230f));
        storyButtonLabel = storyButton.GetComponentInChildren<Text>();
    }

    void OnStoryContinue()
    {
        if (GameManager.Instance == null) return;
        if (storyFirstTime) GameManager.Instance.StartFromStory();
        else GameManager.Instance.CloseStory();
    }

    void BuildHowTo()
    {
        RectTransform content;
        howToPanel = MakeModal("HowToPanel", new Vector2(840f, 660f), out content);

        var title = UITheme.CreateShadowText(content, "Title", "CÁCH CHƠI", 46, UITheme.Gold,
            TextAnchor.MiddleCenter, FontStyle.Bold);
        Centered(title.rectTransform, new Vector2(700f, 60f), new Vector2(0f, 280f));

        AddHowToRow(content, "SPACE / CHẠM", "Bay lên", 170f);
        AddHowToRow(content, "CÀNH CÂY & CHIM", "Tránh xa", 105f);
        AddHowToRow(content, "XU", "Nhặt để mua bóng mới", 40f);
        AddHowToRow(content, "RANH GIỚI", "Chạm sương mù trên/dưới là thua", -25f);
        AddHowToRow(content, "ESC / P", "Tạm dừng", -90f);

        var ok = UITheme.CreateButton(content, "Ok", "ĐÃ HIỂU", new Vector2(360f, 60f),
            UITheme.Gold, UITheme.PanelDark, () => GameManager.Instance.CloseHowTo(), 26);
        Centered(ok.GetComponent<RectTransform>(), new Vector2(360f, 60f), new Vector2(0f, -250f));

        var note = UITheme.CreateText(content, "Note", "Chúc bạn tìm được đường về!", 18, UITheme.Muted,
            TextAnchor.MiddleCenter);
        Centered(note.rectTransform, new Vector2(500f, 24f), new Vector2(0f, -310f));
    }

    void AddHowToRow(Transform parent, string key, string desc, float y)
    {
        var keyText = UITheme.CreateText(parent, "Key", key, 22, UITheme.Mint,
            TextAnchor.MiddleRight, FontStyle.Bold);
        Place(keyText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(1f, 0.5f),
            new Vector2(-20f, y), new Vector2(280f, 32f));

        var descText = UITheme.CreateText(parent, "Desc", desc, 22, UITheme.Cream,
            TextAnchor.MiddleLeft);
        Place(descText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(20f, y), new Vector2(380f, 32f));
    }

    // ---------------------------------------------------------------- countdown / toast / fx

    void BuildCountdown()
    {
        countdownRoot = new GameObject("Countdown", typeof(RectTransform), typeof(CanvasGroup));
        countdownRoot.transform.SetParent(safeRoot, false);
        UITheme.Stretch((RectTransform)countdownRoot.transform);
        countdownGroup = countdownRoot.GetComponent<CanvasGroup>();

        var ring = UITheme.CreateImage(countdownRoot.transform, "Ring", UITheme.Ring,
            new Color(1f, 0.76f, 0.30f, 0.16f), false);
        Centered(ring.rectTransform, new Vector2(340f, 340f), Vector2.zero);

        countdownText = UITheme.CreateShadowText(countdownRoot.transform, "Text", "3", 150,
            UITheme.Gold, TextAnchor.MiddleCenter, FontStyle.Bold);
        Centered(countdownText.rectTransform, new Vector2(600f, 220f), Vector2.zero);
        var countdownOutline = countdownText.gameObject.AddComponent<Outline>();
        countdownOutline.effectColor = new Color(0.03f, 0.09f, 0.08f, 0.85f);
        countdownOutline.effectDistance = new Vector2(2f, -2f);
        countdownOutline.useGraphicAlpha = true;

        var readyBg = UITheme.CreatePanel(countdownRoot.transform, "ReadyBg", new Vector2(270f, 46f),
            new Color(0.03f, 0.09f, 0.08f, 0.78f), false);
        Centered(readyBg, new Vector2(270f, 46f), new Vector2(0f, -140f));
        readyBg.GetComponent<Image>().raycastTarget = false;

        var ready = UITheme.CreateShadowText(readyBg, "Ready", "SẴN SÀNG", 22, UITheme.Cream,
            TextAnchor.MiddleCenter, FontStyle.Bold);
        UITheme.Stretch(ready.rectTransform, 8f);

        countdownRoot.SetActive(false);
    }

    void BuildToast()
    {
        toastRoot = new GameObject("Toast", typeof(RectTransform), typeof(CanvasGroup));
        toastRoot.transform.SetParent(safeRoot, false);
        UITheme.Stretch((RectTransform)toastRoot.transform);
        toastGroup = toastRoot.GetComponent<CanvasGroup>();

        var bg = UITheme.CreateImage(toastRoot.transform, "Bg", UITheme.Rounded,
            new Color(0.03f, 0.09f, 0.08f, 0.94f), false);
        Place(bg.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0f, 120f), new Vector2(500f, 64f));

        toastText = UITheme.CreateText(bg.transform, "Text", "", 22, UITheme.Cream,
            TextAnchor.MiddleCenter, FontStyle.Bold);
        UITheme.Stretch(toastText.rectTransform, 12f);

        toastRoot.SetActive(false);
    }

    void BuildDamageFlash()
    {
        damageFlash = UITheme.CreateImage(safeRoot, "DamageFlash", null, new Color(1f, 0.28f, 0.34f, 0f), false);
        UITheme.Stretch(damageFlash.rectTransform);
    }

    // ---------------------------------------------------------------- public API

    public void ShowMainMenu()
    {
        HideAllPanels();
        UpdateMenuStats();
        UpdateMenuPreview();
        UpdateSoundButtons();
        if (menuPanel != null) menuPanel.Show();
    }

    public void ShowHud()
    {
        HideModalPanels();
        HideHint();
        if (hudRoot != null) hudRoot.SetActive(true);
        if (countdownRoot != null) countdownRoot.SetActive(false);
        if (ScoreManager.Instance != null)
        {
            if (scoreText != null) scoreText.text = ScoreManager.Instance.Score.ToString();
            if (coinText != null) coinText.text = ScoreManager.Instance.TotalCoins.ToString();
            if (hudBestText != null)
                hudBestText.text = "KỶ LỤC " +
                    Mathf.Max(ScoreManager.Instance.HighScore, ScoreManager.Instance.Score);
        }
    }

    public void ShowPause()
    {
        HideModalPanels();
        HideHint();
        if (ScoreManager.Instance != null && pauseScoreText != null)
            pauseScoreText.text = "ĐIỂM " + ScoreManager.Instance.Score;
        UpdateSoundButtons();
        if (pausePanel != null) pausePanel.Show();
    }

    public void HidePause()
    {
        if (pausePanel != null) pausePanel.Hide();
    }

    public void ShowGameOver(int score, int best, int coins, bool record)
    {
        HideModalPanels();
        HideHint();
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.LastDeathCause == DeathCause.ScoreLimit)
            {
                gameOverTitle.text = "BẠN ĐÃ VỀ ĐÍCH!";
                gameOverSubtitle.text = "10.000 điểm! Bạn đã tìm được đường về nhà.";
            }
            else if (GameManager.Instance.LastDeathCause == DeathCause.Boundary)
            {
                gameOverTitle.text = "SƯƠNG MÙ QUÁ DÀY!";
                gameOverSubtitle.text = "Bạn chạm vào ranh giới của khu rừng.";
            }
            else
            {
                gameOverTitle.text = "BẠN ĐÃ LẠC!";
                gameOverSubtitle.text = "Đừng lo, gia đình vẫn đang đợi bạn.";
            }
        }

        if (gameOverScore != null) gameOverScore.text = score.ToString();
        if (gameOverBest != null) gameOverBest.text = "KỶ LỤC " + best;
        if (gameOverCoins != null) gameOverCoins.text = "XU KIẾM ĐƯỢC  +" + coins;
        if (recordBadge != null) recordBadge.SetActive(record);
        if (gameOverPanel != null) gameOverPanel.Show();
        UpdateSoundButtons();
    }

    public void ShowShop()
    {
        HideModalPanels();
        HideHint();
        UpdateShopCoins();
        RefreshShop();
        UpdateShopPreview();
        if (shopPanel != null) shopPanel.Show();
    }

    public void ShowStory(bool firstTime)
    {
        HideModalPanels();
        HideHint();
        storyFirstTime = firstTime;
        if (storyButtonLabel != null)
            storyButtonLabel.text = firstTime ? "BẮT ĐẦU HÀNH TRÌNH" : "ĐÓNG";
        if (storyPanel != null) storyPanel.Show();
    }

    public void ShowHowTo()
    {
        HideModalPanels();
        HideHint();
        if (howToPanel != null) howToPanel.Show();
    }

    public void ShowCountdown(string text)
    {
        if (countdownRoot == null) return;
        countdownRoot.SetActive(true);
        countdownText.text = text;
        countdownText.color = text == "BAY!" ? UITheme.Mint : UITheme.Gold;
        countdownText.rectTransform.localScale = Vector3.one * 1.25f;
    }

    public void HideCountdown()
    {
        if (countdownRoot != null) countdownRoot.SetActive(false);
    }

    public void ShowHint()
    {
        if (hintText == null) return;
        hintText.gameObject.SetActive(true);
        if (hintGroup != null) hintGroup.alpha = 1f;
        if (hintRoutine != null) StopCoroutine(hintRoutine);
        hintRoutine = StartCoroutine(HintRoutine());
    }

    public void HideHint()
    {
        if (hintRoutine != null)
        {
            StopCoroutine(hintRoutine);
            hintRoutine = null;
        }
        if (hintGroup != null) hintGroup.alpha = 0f;
        if (hintText != null) hintText.gameObject.SetActive(false);
    }

    IEnumerator HintRoutine()
    {
        yield return new WaitForSecondsRealtime(2.2f);
        float t = 0f;
        while (t < 0.45f)
        {
            t += Time.unscaledDeltaTime;
            if (hintGroup != null) hintGroup.alpha = 1f - Mathf.Clamp01(t / 0.45f);
            yield return null;
        }
        if (hintText != null) hintText.gameObject.SetActive(false);
        hintRoutine = null;
    }

    public void PulseCoins()
    {
        coinPulse = 1f;
    }

    public void ShowToast(string message)
    {
        if (toastRoot == null) return;
        if (toastRoutine != null) StopCoroutine(toastRoutine);
        toastText.text = message;
        toastRoot.SetActive(true);
        toastGroup.alpha = 1f;
        toastRoutine = StartCoroutine(ToastRoutine());
    }

    IEnumerator ToastRoutine()
    {
        yield return new WaitForSecondsRealtime(1.7f);
        float t = 0f;
        while (t < 0.3f)
        {
            t += Time.unscaledDeltaTime;
            toastGroup.alpha = 1f - Mathf.Clamp01(t / 0.3f);
            yield return null;
        }
        toastRoot.SetActive(false);
    }

    public void FlashDamage()
    {
        if (damageFlash == null) return;
        if (damageRoutine != null) StopCoroutine(damageRoutine);
        damageRoutine = StartCoroutine(DamageRoutine());
    }

    IEnumerator DamageRoutine()
    {
        float t = 0f;
        const float duration = 0.4f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / duration);
            damageFlash.color = new Color(1f, 0.28f, 0.34f, Mathf.Lerp(0.34f, 0f, k));
            yield return null;
        }
        damageFlash.color = new Color(1f, 0.28f, 0.34f, 0f);
    }

    public void HideAllPanels()
    {
        HideModalPanels();
        if (hudRoot != null) hudRoot.SetActive(false);
        if (countdownRoot != null) countdownRoot.SetActive(false);
        if (toastRoot != null) toastRoot.SetActive(false);

        if (hintRoutine != null)
        {
            StopCoroutine(hintRoutine);
            hintRoutine = null;
        }
        if (hintGroup != null) hintGroup.alpha = 0f;

        if (toastRoutine != null)
        {
            StopCoroutine(toastRoutine);
            toastRoutine = null;
        }
        if (toastGroup != null) toastGroup.alpha = 0f;

        if (damageRoutine != null)
        {
            StopCoroutine(damageRoutine);
            damageRoutine = null;
        }
        if (damageFlash != null) damageFlash.color = new Color(1f, 0.28f, 0.34f, 0f);
    }

    void HideModalPanels()
    {
        if (menuPanel != null) menuPanel.Hide(true);
        if (pausePanel != null) pausePanel.Hide(true);
        if (gameOverPanel != null) gameOverPanel.Hide(true);
        if (shopPanel != null) shopPanel.Hide(true);
        if (storyPanel != null) storyPanel.Hide(true);
        if (howToPanel != null) howToPanel.Hide(true);
    }

    // ---------------------------------------------------------------- events

    void Subscribe()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged += HandleScore;
            ScoreManager.Instance.OnCoinsChanged += HandleCoins;
            ScoreManager.Instance.OnHighScoreChanged += HandleBest;
        }
        if (ShopSystem.Instance != null)
            ShopSystem.Instance.OnSelectionChanged += HandleSelection;
    }

    void Unsubscribe()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged -= HandleScore;
            ScoreManager.Instance.OnCoinsChanged -= HandleCoins;
            ScoreManager.Instance.OnHighScoreChanged -= HandleBest;
        }
        if (ShopSystem.Instance != null)
            ShopSystem.Instance.OnSelectionChanged -= HandleSelection;
    }

    void HandleScore(int score)
    {
        if (scoreText != null) scoreText.text = score.ToString();
        if (hudBestText != null && ScoreManager.Instance != null)
            hudBestText.text = "KỶ LỤC " +
                Mathf.Max(ScoreManager.Instance.HighScore, score);
        if (pauseScoreText != null && pausePanel != null && pausePanel.IsVisible)
            pauseScoreText.text = "ĐIỂM " + score;
        scorePulse = 1f;
    }

    void HandleCoins(int coins)
    {
        if (coinText != null) coinText.text = coins.ToString();
        if (menuCoinText != null) menuCoinText.text = coins.ToString();
        if (shopCoinText != null) shopCoinText.text = coins.ToString();
        coinPulse = 1f;
        if (shopPanel != null && shopPanel.IsVisible)
            RefreshShopCards();
    }

    void HandleBest(int best)
    {
        if (hudBestText != null) hudBestText.text = "KỶ LỤC " + best;
        if (menuBestText != null) menuBestText.text = "KỶ LỤC " + best;
        if (gameOverBest != null) gameOverBest.text = "KỶ LỤC " + best;
    }

    void HandleSelection()
    {
        UpdateMenuPreview();
        UpdateShopPreview();
        if (shopPanel != null && shopPanel.IsVisible)
            RefreshShopCards();
    }

    void UpdateMenuStats()
    {
        if (menuBestText != null && ScoreManager.Instance != null)
            menuBestText.text = "KỶ LỤC " + ScoreManager.Instance.HighScore;
        if (menuCoinText != null && ScoreManager.Instance != null)
            menuCoinText.text = ScoreManager.Instance.TotalCoins.ToString();
    }

    void UpdateMenuPreview()
    {
        if (menuPreview == null || ShopSystem.Instance == null || ShopSystem.Instance.SelectedItem == null)
            return;
        menuPreview.sprite = ShopSystem.Instance.SelectedItem.Sprite;
        menuPreview.preserveAspect = true;
    }

    void UpdateSoundButtons()
    {
        bool on = AudioManager.Instance != null && AudioManager.Instance.SoundEnabled;
        string label = "ÂM THANH: " + (on ? "BẬT" : "TẮT");
        if (menuSoundText != null) menuSoundText.text = label;
        if (pauseSoundText != null) pauseSoundText.text = label;
    }
}
