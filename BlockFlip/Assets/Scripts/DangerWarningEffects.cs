using UnityEngine;
using UnityEngine.UI;

public class DangerWarningEffects : MonoBehaviour
{
    [SerializeField] private float warningThreshold = 0.7f;
    [SerializeField] private float criticalThreshold = 0.9f;
    [SerializeField] private Color normalGaugeColor = new Color(0.08f, 0.075f, 0.07f, 0.86f);
    [SerializeField] private Color warningGaugeColor = new Color(0.38f, 0.36f, 0.32f, 0.95f);
    [SerializeField] private Color criticalGaugeColor = new Color(0.06f, 0.055f, 0.05f, 1f);

    private Image gaugeFillImage;
    private RectTransform gaugeRoot;
    private RectTransform gridRoot;
    private Image dimOverlay;
    private Image[] vignetteEdges;
    private Vector2 gridBasePosition;
    private float dangerRatio;
    private bool initialized;

    public void Configure(Image fillImage)
    {
        gaugeFillImage = fillImage;
        gaugeRoot = fillImage != null && fillImage.transform.parent != null
            ? fillImage.transform.parent as RectTransform
            : null;
        gridRoot = GameObject.Find("GridRoot")?.GetComponent<RectTransform>();

        if (gridRoot != null)
        {
            gridBasePosition = gridRoot.anchoredPosition;
        }

        EnsureOverlays();
        initialized = true;
        SetDangerRatio(dangerRatio);
    }

    public void SetDangerRatio(float ratio)
    {
        dangerRatio = Mathf.Clamp01(ratio);

        if (!initialized)
        {
            Configure(gaugeFillImage);
        }
    }

    private void Update()
    {
        if (!initialized)
            return;

        float targetDimAlpha = dangerRatio >= criticalThreshold
            ? 0.14f
            : dangerRatio >= warningThreshold
                ? 0.08f
                : 0f;
        SetImageAlpha(dimOverlay, Mathf.Lerp(GetImageAlpha(dimOverlay), targetDimAlpha, Time.unscaledDeltaTime * 4f));

        float targetVignetteAlpha = dangerRatio >= criticalThreshold ? 0.18f : 0f;
        if (vignetteEdges != null)
        {
            foreach (Image edge in vignetteEdges)
            {
                SetImageAlpha(edge, Mathf.Lerp(GetImageAlpha(edge), targetVignetteAlpha, Time.unscaledDeltaTime * 4f));
            }
        }

        UpdateGaugeEffect();
        UpdateGridShake();
    }

    private void UpdateGaugeEffect()
    {
        if (gaugeFillImage == null)
            return;

        if (dangerRatio >= criticalThreshold)
        {
            float pulse = (Mathf.Sin(Time.unscaledTime * 9f) + 1f) * 0.5f;
            gaugeFillImage.color = Color.Lerp(warningGaugeColor, criticalGaugeColor, pulse);

            if (gaugeRoot != null)
            {
                float scale = 1f + pulse * 0.055f;
                gaugeRoot.localScale = new Vector3(scale, scale, 1f);
            }

            return;
        }

        if (gaugeRoot != null)
        {
            gaugeRoot.localScale = Vector3.Lerp(gaugeRoot.localScale, Vector3.one, Time.unscaledDeltaTime * 8f);
        }

        if (dangerRatio >= warningThreshold)
        {
            float blink = (Mathf.Sin(Time.unscaledTime * 2.2f) + 1f) * 0.5f;
            gaugeFillImage.color = Color.Lerp(normalGaugeColor, warningGaugeColor, blink);
            return;
        }

        gaugeFillImage.color = Color.Lerp(gaugeFillImage.color, normalGaugeColor, Time.unscaledDeltaTime * 8f);
    }

    private void UpdateGridShake()
    {
        if (gridRoot == null)
            return;

        if (dangerRatio < warningThreshold)
        {
            gridRoot.anchoredPosition = Vector2.Lerp(gridRoot.anchoredPosition, gridBasePosition, Time.unscaledDeltaTime * 8f);
            return;
        }

        float intensity = dangerRatio >= criticalThreshold ? 3.2f : 1.35f;
        float x = Mathf.Sin(Time.unscaledTime * 19f) * intensity;
        float y = Mathf.Cos(Time.unscaledTime * 23f) * intensity * 0.55f;
        gridRoot.anchoredPosition = gridBasePosition + new Vector2(x, y);
    }

    private void EnsureOverlays()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
            return;

        dimOverlay = EnsureOverlayImage(canvas.transform, "DangerDimOverlay", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, 0f);
        dimOverlay.transform.SetAsFirstSibling();

        vignetteEdges = new[]
        {
            EnsureOverlayImage(canvas.transform, "DangerVignetteTop", new Vector2(0f, 0.88f), Vector2.one, Vector2.zero, Vector2.zero, 0f),
            EnsureOverlayImage(canvas.transform, "DangerVignetteBottom", Vector2.zero, new Vector2(1f, 0.12f), Vector2.zero, Vector2.zero, 0f),
            EnsureOverlayImage(canvas.transform, "DangerVignetteLeft", Vector2.zero, new Vector2(0.1f, 1f), Vector2.zero, Vector2.zero, 0f),
            EnsureOverlayImage(canvas.transform, "DangerVignetteRight", new Vector2(0.9f, 0f), Vector2.one, Vector2.zero, Vector2.zero, 0f)
        };

        foreach (Image edge in vignetteEdges)
        {
            edge.transform.SetAsFirstSibling();
        }
    }

    private Image EnsureOverlayImage(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, float alpha)
    {
        Transform existing = parent.Find(name);
        GameObject overlayObject = existing != null
            ? existing.gameObject
            : new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));

        overlayObject.transform.SetParent(parent, false);
        RectTransform rect = overlayObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;

        Image image = overlayObject.GetComponent<Image>();
        image.color = new Color(0f, 0f, 0f, alpha);
        image.raycastTarget = false;
        return image;
    }

    private static float GetImageAlpha(Image image)
    {
        return image != null ? image.color.a : 0f;
    }

    private static void SetImageAlpha(Image image, float alpha)
    {
        if (image == null)
            return;

        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }
}
