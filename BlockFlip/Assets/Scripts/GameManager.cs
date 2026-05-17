using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Runtime")]
    [SerializeField] private int targetFrameRate = 60;
    [SerializeField] private string homeSceneName = "HomeScene";

    [Header("Pause UI")]
    [SerializeField] private PauseMenuView pauseMenuPrefab;
    [SerializeField] private float pauseButtonNormalizedYOffset = 0.05f;
    [SerializeField] private Color pauseButtonColor = new Color(1f, 0.985f, 0.955f, 0.92f);
    [SerializeField] private Color textColor = new Color(0.045f, 0.045f, 0.045f, 1f);

    [Header("Game Over UI")]
    [SerializeField] private GameOverMenuView gameOverMenuPrefab;
    [SerializeField] private string bestScorePrefsKey = "BestScore";

    [Header("Shadow Style")]
    [SerializeField] private Color shadowEffectColor = new Color(0f, 0f, 0f, 0.3f);
    [SerializeField] private Vector2 shadowEffectDistance = new Vector2(0f, -6f);
    [SerializeField] private bool shadowUseGraphicAlpha = true;

    private PauseMenuView pauseMenu;
    private GameOverMenuView gameOverMenu;
    private Coroutine initializeSceneUiCoroutine;
    private bool isPaused;
    private bool isGameOver;
    private static Sprite circleButtonSprite;

    public bool IsPaused => isPaused;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        InitializeSceneUi();
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) &&
            (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
        {
            ShowGameOver();
        }
    }
#endif

    private void OnDestroy()
    {
        if (Instance != this)
            return;

        SceneManager.sceneLoaded -= OnSceneLoaded;
        Instance = null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (initializeSceneUiCoroutine != null)
        {
            StopCoroutine(initializeSceneUiCoroutine);
        }

        initializeSceneUiCoroutine = StartCoroutine(InitializeSceneUiAfterLayout());
    }

    private IEnumerator InitializeSceneUiAfterLayout()
    {
        yield return null;
        InitializeSceneUi();
        initializeSceneUiCoroutine = null;
    }

    private void InitializeSceneUi()
    {
        Application.targetFrameRate = targetFrameRate;

        if (SceneManager.GetActiveScene().name == homeSceneName)
        {
            isGameOver = false;
            isPaused = false;
            Time.timeScale = 1f;
            return;
        }

        EnsurePauseButton();
        EnsurePauseMenu();
        EnsureGameOverMenu();
        isGameOver = false;
        ResumeGame();
    }

    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        if (isGameOver)
            return;

        isPaused = true;
        Time.timeScale = 0f;

        if (pauseMenu != null)
        {
            pauseMenu.Show();
        }
    }

    public void ResumeGame()
    {
        if (isGameOver)
            return;

        isPaused = false;
        Time.timeScale = 1f;

        if (pauseMenu != null)
        {
            pauseMenu.Hide();
        }
    }

    public void ShowGameOver()
    {
        isGameOver = true;
        isPaused = false;
        Time.timeScale = 0f;

        if (pauseMenu != null)
        {
            pauseMenu.Hide();
        }

        EnsureGameOverMenu();
        if (gameOverMenu == null)
            return;

        int score = ScoreManager.instance != null ? ScoreManager.instance.Score : 0;
        int lines = ScoreManager.instance != null ? ScoreManager.instance.TotalClearedLines : 0;
        int maxCombo = ScoreManager.instance != null ? ScoreManager.instance.MaxCombo : 0;
        int previousBest = PlayerPrefs.GetInt(bestScorePrefsKey, 0);
        bool isNewBest = score > previousBest;
        int bestScore = isNewBest ? score : previousBest;

        if (isNewBest)
        {
            PlayerPrefs.SetInt(bestScorePrefsKey, score);
            PlayerPrefs.Save();
        }

        gameOverMenu.SetStats(score, bestScore, lines, maxCombo, isNewBest);
        gameOverMenu.Show();
    }

    private void EnsurePauseButton()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
            return;

        Transform existing = canvas.transform.Find("PauseButton");
        GameObject buttonObject = existing != null
            ? existing.gameObject
            : new GameObject("PauseButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));

        buttonObject.transform.SetParent(canvas.transform, false);
        buttonObject.transform.SetAsLastSibling();

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0f, 1f);
        buttonRect.anchorMax = new Vector2(0f, 1f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.sizeDelta = new Vector2(92f, 92f);
        RectTransform canvasRect = canvas.transform as RectTransform;
        float safeYOffset = canvasRect != null
            ? canvasRect.rect.height * pauseButtonNormalizedYOffset
            : 0f;
        buttonRect.anchoredPosition = new Vector2(96f, -96f - safeYOffset);

        Image buttonImage = buttonObject.GetComponent<Image>();
        buttonImage.sprite = GetCircleButtonSprite();
        buttonImage.type = Image.Type.Sliced;
        buttonImage.color = pauseButtonColor;
        buttonImage.raycastTarget = true;
        ApplyShadow(buttonObject);

        Button pauseButton = buttonObject.GetComponent<Button>();
        pauseButton.targetGraphic = buttonImage;
        pauseButton.transition = Selectable.Transition.ColorTint;
        ColorBlock colors = pauseButton.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.94f, 0.925f, 0.9f, 1f);
        colors.pressedColor = new Color(0.86f, 0.84f, 0.8f, 1f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(1f, 1f, 1f, 0.35f);
        pauseButton.colors = colors;
        pauseButton.onClick.RemoveListener(TogglePause);
        pauseButton.onClick.AddListener(TogglePause);

        EnsurePauseIcon(buttonObject.transform);
    }

    private void EnsurePauseIcon(Transform parent)
    {
        EnsurePauseIconBar(parent, "PauseBarLeft", new Vector2(-13f, 0f));
        EnsurePauseIconBar(parent, "PauseBarRight", new Vector2(13f, 0f));
    }

    private void EnsurePauseIconBar(Transform parent, string name, Vector2 anchoredPosition)
    {
        Transform existing = parent.Find(name);
        GameObject barObject = existing != null
            ? existing.gameObject
            : new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));

        barObject.transform.SetParent(parent, false);
        RectTransform barRect = barObject.GetComponent<RectTransform>();
        barRect.anchorMin = new Vector2(0.5f, 0.5f);
        barRect.anchorMax = new Vector2(0.5f, 0.5f);
        barRect.pivot = new Vector2(0.5f, 0.5f);
        barRect.sizeDelta = new Vector2(12f, 42f);
        barRect.anchoredPosition = anchoredPosition;

        Image barImage = barObject.GetComponent<Image>();
        barImage.color = textColor;
        barImage.raycastTarget = false;
    }

    private void EnsurePauseMenu()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
            return;

        Transform existing = canvas.transform.Find("PauseMenu");
        pauseMenu = existing != null
            ? existing.GetComponent<PauseMenuView>()
            : null;

        if (pauseMenu == null)
        {
            if (pauseMenuPrefab == null)
            {
                Debug.LogWarning("Pause menu prefab is not assigned to GameManager.");
                return;
            }

            pauseMenu = Instantiate(pauseMenuPrefab, canvas.transform, false);
            pauseMenu.name = "PauseMenu";
        }

        ApplyShadowStyleToHierarchy(pauseMenu.gameObject);
        pauseMenu.transform.SetAsLastSibling();
        pauseMenu.Initialize(ResumeGame, RestartGame, OpenSettings, ReturnToHome);
    }

    private void EnsureGameOverMenu()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
            return;

        Transform existing = canvas.transform.Find("GameOverMenu");
        gameOverMenu = existing != null
            ? existing.GetComponent<GameOverMenuView>()
            : null;

        if (gameOverMenu == null)
        {
            if (gameOverMenuPrefab == null)
            {
                Debug.LogWarning("Game over menu prefab is not assigned to GameManager.");
                return;
            }

            gameOverMenu = Instantiate(gameOverMenuPrefab, canvas.transform, false);
            gameOverMenu.name = "GameOverMenu";
        }

        ApplyShadowStyleToHierarchy(gameOverMenu.gameObject);
        gameOverMenu.transform.SetAsLastSibling();
        gameOverMenu.Initialize(RestartGame, ReturnToHome);
        gameOverMenu.Hide();
    }

    private void OpenSettings()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas != null)
        {
            SettingsMenuPresenter.Show(canvas.transform);
        }
    }

    private void RestartGame()
    {
        isGameOver = false;
        Time.timeScale = 1f;
        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.buildIndex);
    }

    private void ReturnToHome()
    {
        isGameOver = false;
        Time.timeScale = 1f;

        if (!string.IsNullOrWhiteSpace(homeSceneName) &&
            Application.CanStreamedLevelBeLoaded(homeSceneName))
        {
            SceneManager.LoadScene(homeSceneName);
            return;
        }

        SceneManager.LoadScene(0);
    }

    public static Shadow ApplyShadow(GameObject target)
    {
        if (target == null)
            return null;

        Shadow shadow = target.GetComponent<Shadow>();
        if (shadow == null)
        {
            shadow = target.AddComponent<Shadow>();
        }

        ApplyShadow(shadow);
        return shadow;
    }

    public static void ApplyShadow(Shadow shadow)
    {
        if (shadow == null)
            return;

        if (Instance != null)
        {
            shadow.effectColor = Instance.shadowEffectColor;
            shadow.effectDistance = Instance.shadowEffectDistance;
            shadow.useGraphicAlpha = Instance.shadowUseGraphicAlpha;
            return;
        }

        shadow.effectColor = new Color(0f, 0f, 0f, 0.3f);
        shadow.effectDistance = new Vector2(0f, -6f);
        shadow.useGraphicAlpha = true;
    }

    public static void ApplyShadowStyleToHierarchy(GameObject root)
    {
        if (root == null)
            return;

        Shadow[] shadows = root.GetComponentsInChildren<Shadow>(true);
        foreach (Shadow shadow in shadows)
        {
            ApplyShadow(shadow);
        }
    }

    private static Sprite GetCircleButtonSprite()
    {
        if (circleButtonSprite != null)
            return circleButtonSprite;

        const int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.name = "GeneratedCircleButton";
        texture.wrapMode = TextureWrapMode.Clamp;

        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float radius = (size - 2) * 0.5f;
        float feather = 1.5f;
        Color[] pixels = new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float alpha = Mathf.Clamp01((radius - distance) / feather);
                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        circleButtonSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, size, size),
            new Vector2(0.5f, 0.5f),
            100f,
            0,
            SpriteMeshType.FullRect,
            new Vector4(24f, 24f, 24f, 24f)
        );

        return circleButtonSprite;
    }
}
