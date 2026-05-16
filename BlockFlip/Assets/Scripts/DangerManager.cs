using UnityEngine;

public class DangerManager : MonoBehaviour
{
    [SerializeField] private int maxDanger = 10;
    [SerializeField] private UnityEngine.UI.Image fillImage;

    private int danger;

    public bool IsGameOver => danger >= maxDanger;

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