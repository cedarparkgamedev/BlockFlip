using UnityEngine;
using UnityEngine.UI;

public class DangerManager : MonoBehaviour
{
    [SerializeField] private int maxDanger = 10;
    [SerializeField] private Image fillImage;

    private int danger;
    private bool gameOverNotified = false;
    private RectTransform fillRect;
    private DangerWarningEffects warningEffects;

    public bool IsGameOver => danger >= maxDanger;

    private void Start()
    {
        UpdateUI();
    }

    public void SetFillImage(Image image)
    {
        fillImage = image;
        fillRect = fillImage != null
            ? fillImage.rectTransform
            : null;

        if (fillImage != null)
        {
            fillImage.type = Image.Type.Simple;
        }

        warningEffects = FindAnyObjectByType<DangerWarningEffects>();
        if (warningEffects == null)
        {
            warningEffects = new GameObject("DangerWarningEffects").AddComponent<DangerWarningEffects>();
        }

        warningEffects.Configure(fillImage);
        UpdateUI();
    }

    public void OnMoveResolved(int clearedRows)
    {
        if (clearedRows > 0)
        {
            danger = Mathf.Max(0, danger - clearedRows);
        }
        else
        {
            danger++;
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        float dangerRatio = maxDanger > 0
            ? Mathf.Clamp01(danger / (float)maxDanger)
            : 1f;

        if (fillImage != null)
        {
            fillImage.fillAmount = dangerRatio;
        }

        if (fillRect != null)
        {
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(dangerRatio, 1f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            fillRect.gameObject.SetActive(dangerRatio > 0f);
        }

        if (warningEffects != null)
        {
            warningEffects.SetDangerRatio(dangerRatio);
        }

        if (IsGameOver && !gameOverNotified)
        {
            gameOverNotified = true;
            GameManager.Instance?.ShowGameOver();
        }
    }
}
