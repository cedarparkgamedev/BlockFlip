using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BestMenuView : MonoBehaviour
{
    private const string LegacyBestScoreKey = "BestScore";
    private const string ClassicBestScoreKey = "Best_Classic_Score";
    private const string ClassicLinesKey = "Best_Classic_Lines";
    private const string ClassicMaxComboKey = "Best_Classic_MaxCombo";
    private const string ClassicLongestRunKey = "Best_Classic_LongestRunSeconds";
    private const string ZenLongestSessionKey = "Best_Zen_LongestSessionSeconds";
    private const string ZenLinesKey = "Best_Zen_Lines";
    private const string ZenMaxComboKey = "Best_Zen_MaxCombo";
    private const string ZenFlowStreakKey = "Best_Zen_FlowStreak";

    [SerializeField] private Button backButton;
    [SerializeField] private Button doneButton;
    [SerializeField] private TextMeshProUGUI classicBestScoreText;
    [SerializeField] private TextMeshProUGUI classicLinesText;
    [SerializeField] private TextMeshProUGUI classicMaxComboText;
    [SerializeField] private TextMeshProUGUI classicLongestRunText;
    [SerializeField] private TextMeshProUGUI zenLongestSessionText;
    [SerializeField] private TextMeshProUGUI zenLinesText;
    [SerializeField] private TextMeshProUGUI zenMaxComboText;
    [SerializeField] private TextMeshProUGUI zenFlowStreakText;

    private void Awake()
    {
        ResolveReferences();
    }

    public void Initialize()
    {
        ResolveReferences();
        BindButton(backButton, Hide);
        BindButton(doneButton, Hide);
        RefreshStats();
    }

    public void Show()
    {
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void RefreshStats()
    {
        int classicScore = PlayerPrefs.GetInt(ClassicBestScoreKey, PlayerPrefs.GetInt(LegacyBestScoreKey, 0));
        SetText(classicBestScoreText, classicScore.ToString("N0"));
        SetText(classicLinesText, PlayerPrefs.GetInt(ClassicLinesKey, 0).ToString("N0"));
        SetText(classicMaxComboText, $"x{PlayerPrefs.GetInt(ClassicMaxComboKey, 0)}");
        SetText(classicLongestRunText, FormatClock(PlayerPrefs.GetInt(ClassicLongestRunKey, 0)));

        SetText(zenLongestSessionText, FormatClock(PlayerPrefs.GetInt(ZenLongestSessionKey, 0)));
        SetText(zenLinesText, PlayerPrefs.GetInt(ZenLinesKey, 0).ToString("N0"));
        SetText(zenMaxComboText, $"x{PlayerPrefs.GetInt(ZenMaxComboKey, 0)}");
        SetText(zenFlowStreakText, PlayerPrefs.GetInt(ZenFlowStreakKey, 0).ToString("N0"));
    }

    private void ResolveReferences()
    {
        backButton ??= FindButton("BackButton");
        doneButton ??= FindButton("DoneButton");
        classicBestScoreText ??= FindText("ClassicBestScoreValue");
        classicLinesText ??= FindText("ClassicLinesValue");
        classicMaxComboText ??= FindText("ClassicMaxComboValue");
        classicLongestRunText ??= FindText("ClassicLongestRunValue");
        zenLongestSessionText ??= FindText("ZenLongestSessionValue");
        zenLinesText ??= FindText("ZenLinesValue");
        zenMaxComboText ??= FindText("ZenMaxComboValue");
        zenFlowStreakText ??= FindText("ZenFlowStreakValue");
    }

    private Button FindButton(string objectName)
    {
        Transform[] children = GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            if (child.name == objectName && child.TryGetComponent(out Button button))
            {
                return button;
            }
        }

        return null;
    }

    private TextMeshProUGUI FindText(string objectName)
    {
        Transform[] children = GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            if (child.name == objectName && child.TryGetComponent(out TextMeshProUGUI text))
            {
                return text;
            }
        }

        return null;
    }

    private static void BindButton(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null)
            return;

        button.onClick.RemoveAllListeners();
        button.interactable = action != null;

        if (action != null)
        {
            button.onClick.AddListener(action);
        }
    }

    private static void SetText(TextMeshProUGUI text, string value)
    {
        if (text != null)
        {
            text.text = value;
        }
    }

    private static string FormatClock(int seconds)
    {
        seconds = Mathf.Max(0, seconds);
        int minutes = seconds / 60;
        int remainingSeconds = seconds % 60;
        return $"{minutes:00}:{remainingSeconds:00}";
    }
}
