using UnityEngine;
using UnityEngine.UI;

public class DangerManager : MonoBehaviour
{
    [SerializeField] private int maxDanger = 10;
    [SerializeField] private Image fillImage;

    private int danger;

    public bool IsGameOver => danger >= maxDanger;

    private void Start()
    {
        UpdateUI();
    }

    public void SetFillImage(Image image)
    {
        fillImage = image;

        if (fillImage != null)
        {
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        }

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
        if (fillImage != null)
        {
            fillImage.fillAmount = danger / (float)maxDanger;
        }
    }
}
