using UnityEngine;

public class SafeAreaFitter : MonoBehaviour
{
    private Rect lastSafeArea;
    private Vector2Int lastScreenSize;

    private void OnEnable()
    {
        Apply();
    }

    private void Update()
    {
        Vector2Int screenSize = new Vector2Int(Screen.width, Screen.height);
        if (Screen.safeArea != lastSafeArea || screenSize != lastScreenSize)
        {
            Apply();
        }
    }

    private void Apply()
    {
        if (Screen.width <= 0 || Screen.height <= 0)
            return;

        Rect safeArea = Screen.safeArea;
        lastSafeArea = safeArea;
        lastScreenSize = new Vector2Int(Screen.width, Screen.height);

        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        RectTransform rect = transform as RectTransform;
        if (rect == null)
            return;

        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
