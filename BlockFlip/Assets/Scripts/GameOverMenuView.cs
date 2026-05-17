using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameOverMenuView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreValueText;
    [SerializeField] private TextMeshProUGUI bestValueText;
    [SerializeField] private TextMeshProUGUI linesValueText;
    [SerializeField] private TextMeshProUGUI maxComboValueText;
    [SerializeField] private GameObject newBestBadge;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button homeButton;

    private void Awake()
    {
        scoreValueText ??= FindText("ScoreValue");
        bestValueText ??= FindText("BestValue");
        linesValueText ??= FindText("LinesValue");
        maxComboValueText ??= FindText("MaxComboValue");
        retryButton ??= FindButton("RetryButton");
        homeButton ??= FindButton("HomeButton");

        if (newBestBadge == null)
        {
            Transform badge = transform.Find("Panel/Stats/BestRow/NewBestBadge");
            newBestBadge = badge != null ? badge.gameObject : null;
        }
    }

    public void Initialize(UnityAction onRetry, UnityAction onHome)
    {
        BindButton(retryButton, onRetry);
        BindButton(homeButton, onHome);
    }

    public void SetStats(int score, int bestScore, int lines, int maxCombo, bool isNewBest)
    {
        SetText(scoreValueText, score.ToString("N0"));
        SetText(bestValueText, bestScore.ToString("N0"));
        SetText(linesValueText, lines.ToString("N0"));
        SetText(maxComboValueText, $"x{maxCombo}");

        if (newBestBadge != null)
        {
            newBestBadge.SetActive(isNewBest);
        }
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

    private static void BindButton(Button button, UnityAction action)
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
}
