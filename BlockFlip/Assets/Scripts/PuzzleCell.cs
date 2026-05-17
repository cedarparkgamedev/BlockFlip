using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum CellState
{
    White,
    Black
}

public class PuzzleCell : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private Color whiteTileColor = new Color(0.94f, 0.925f, 0.9f, 1f);
    [SerializeField] private Color blackTileColor = new Color(0.045f, 0.045f, 0.042f, 1f);
    [SerializeField] private float previewGlowAlpha = 0.68f;
    [SerializeField] private float clearGlowAlpha = 1f;
    [SerializeField] private Color previewInnerTintColor = new Color(0f, 0f, 0f, 0.18f);
    [SerializeField] private Color clearInnerTintColor = new Color(0f, 0f, 0f, 0.34f);

    private RectTransform rectTransform;
    private Image innerTintImage;
    private Image previewGlowImage;
    private Image clearGlowImage;
    private Coroutine animationRoutine;
    private Vector3 originalScale;
    private Quaternion originalRotation;
    private bool isPreviewing;

    public int X { get; private set; }
    public int Y { get; private set; }

    public CellState State { get; private set; }

    public void Initialize(int x, int y, CellState initialState)
    {
        CacheRectTransform();
        ConfigureVisualEffects();

        X = x;
        Y = y;
        SetState(initialState);
    }

    public void Flip()
    {
        SetState(State == CellState.White ? CellState.Black : CellState.White);
    }

    public void FlipAnimated(float duration, float peakScale)
    {
        CellState targetState = State == CellState.White
            ? CellState.Black
            : CellState.White;

        SetStateAnimated(targetState, duration, peakScale);
    }

    public void SetState(CellState state)
    {
        State = state;
        isPreviewing = false;
        SetOverlay(innerTintImage, Color.clear, 0f, 1f);
        SetGlow(previewGlowImage, Color.clear, 0f, 1f);
        SetGlow(clearGlowImage, Color.clear, 0f, 1f);

        if (image != null)
        {
            image.color = GetColor(state);
        }

        ResetTransformVisuals();
    }

    public void SetStateAnimated(CellState state, float duration, float peakScale)
    {
        CacheRectTransform();
        State = state;

        if (animationRoutine != null)
        {
            StopCoroutine(animationRoutine);
        }

        animationRoutine = StartCoroutine(AnimateStateChange(GetColor(state), duration, peakScale));
    }

    public void RegenerateAnimated(
        CellState state,
        float duration,
        float delay,
        float startScale,
        float peakScale)
    {
        CacheRectTransform();
        State = state;

        if (animationRoutine != null)
        {
            StopCoroutine(animationRoutine);
        }

        animationRoutine = StartCoroutine(AnimateRegenerate(
            GetColor(state),
            duration,
            delay,
            startScale,
            peakScale
        ));
    }

    public void RegenerateSlideAnimated(
        CellState state,
        float duration,
        float delay,
        float slideOffset,
        float peakScale)
    {
        RegenerateSlideAnimated(
            state,
            duration,
            delay,
            Vector2.right * slideOffset,
            peakScale
        );
    }

    public void RegenerateSlideAnimated(
        CellState state,
        float duration,
        float delay,
        Vector2 slideOffset,
        float peakScale)
    {
        CacheRectTransform();
        State = state;

        if (animationRoutine != null)
        {
            StopCoroutine(animationRoutine);
        }

        animationRoutine = StartCoroutine(AnimateRegenerateSlide(
            GetColor(state),
            duration,
            delay,
            slideOffset,
            peakScale
        ));
    }

    public void Pulse(float duration, float peakScale)
    {
        CacheRectTransform();

        if (animationRoutine != null)
        {
            StopCoroutine(animationRoutine);
        }

        animationRoutine = StartCoroutine(AnimatePulse(duration, peakScale));
    }

    public void PlayClearHighlight(float duration, float peakScale, Color highlightColor, int flashCount)
    {
        CacheRectTransform();

        if (animationRoutine != null)
        {
            StopCoroutine(animationRoutine);
        }

        animationRoutine = StartCoroutine(AnimateClearHighlight(
            duration,
            peakScale,
            highlightColor,
            flashCount
        ));
    }

    public void ShowPreview(CellState previewState, Color previewColor, float previewScale)
    {
        ShowPreview(previewState, previewColor, previewScale, false);
    }

    public void ShowPreview(CellState previewState, Color previewColor, float previewScale, bool useSolidPreviewColor)
    {
        CacheRectTransform();
        ConfigureVisualEffects();
        isPreviewing = true;

        if (animationRoutine != null)
        {
            StopCoroutine(animationRoutine);
            animationRoutine = null;
        }

        Color glowColor = useSolidPreviewColor
            ? previewColor
            : new Color(previewColor.r, previewColor.g, previewColor.b, previewGlowAlpha);
        float glowAlpha = useSolidPreviewColor
            ? clearGlowAlpha
            : previewGlowAlpha;

        SetGlow(previewGlowImage, glowColor, glowAlpha, previewScale);
        SetOverlay(innerTintImage, useSolidPreviewColor ? clearInnerTintColor : previewInnerTintColor, 1f, 1f);
        ResetTransformVisuals();
    }

    public void ClearPreview()
    {
        if (!isPreviewing)
            return;

        isPreviewing = false;

        if (image != null)
        {
            image.color = GetColor(State);
        }

        SetOverlay(innerTintImage, Color.clear, 0f, 1f);
        SetGlow(previewGlowImage, Color.clear, 0f, 1f);
        SetGlow(clearGlowImage, Color.clear, 0f, 1f);

        ResetTransformVisuals();
    }

    private IEnumerator AnimateStateChange(Color targetColor, float duration, float peakScale)
    {
        float elapsed = 0f;
        float safeDuration = Mathf.Max(0.01f, duration);
        bool colorChanged = false;

        while (elapsed < safeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / safeDuration);
            float scaleT = Mathf.Sin(t * Mathf.PI);
            float foldT = t < 0.5f
                ? t / 0.5f
                : (1f - t) / 0.5f;
            float rotationY = Mathf.Lerp(0f, 90f, foldT);

            if (!colorChanged && t >= 0.5f)
            {
                if (image != null)
                {
                    image.color = targetColor;
                }

                colorChanged = true;
            }

            rectTransform.localScale = Vector3.Lerp(
                originalScale,
                originalScale * peakScale,
                scaleT
            );
            rectTransform.localRotation = originalRotation * Quaternion.Euler(0f, rotationY, 0f);

            yield return null;
        }

        if (image != null)
        {
            image.color = targetColor;
        }

        ResetTransformVisuals();
        animationRoutine = null;
    }

    private IEnumerator AnimatePulse(float duration, float peakScale)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float scaleT = Mathf.Sin(t * Mathf.PI);

            rectTransform.localScale = Vector3.Lerp(
                originalScale,
                originalScale * peakScale,
                scaleT
            );

            yield return null;
        }

        ResetTransformVisuals();
        animationRoutine = null;
    }

    private IEnumerator AnimateClearHighlight(float duration, float peakScale, Color highlightColor, int flashCount)
    {
        int safeFlashCount = Mathf.Max(1, flashCount);
        float elapsed = 0f;
        SetGlow(previewGlowImage, Color.clear, 0f, 1f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float flashT = Mathf.Abs(Mathf.Sin(t * Mathf.PI * safeFlashCount));
            float scaleT = Mathf.Sin(t * Mathf.PI);

            SetOverlay(innerTintImage, clearInnerTintColor, flashT, 1f);
            SetGlow(clearGlowImage, highlightColor, clearGlowAlpha * flashT, Mathf.Lerp(1.02f, peakScale, scaleT));
            rectTransform.localScale = Vector3.Lerp(
                originalScale,
                originalScale * peakScale,
                scaleT
            );

            yield return null;
        }

        SetOverlay(innerTintImage, Color.clear, 0f, 1f);
        SetGlow(clearGlowImage, Color.clear, 0f, 1f);
        ResetTransformVisuals();
        animationRoutine = null;
    }

    private IEnumerator AnimateRegenerate(
        Color targetColor,
        float duration,
        float delay,
        float startScale,
        float peakScale)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        if (image != null)
        {
            image.color = new Color(targetColor.r, targetColor.g, targetColor.b, 0f);
        }

        rectTransform.localRotation = originalRotation;
        rectTransform.localScale = originalScale * startScale;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easedT = 1f - Mathf.Pow(1f - t, 3f);
            float scaleT = Mathf.Sin(t * Mathf.PI);

            if (image != null)
            {
                image.color = Color.Lerp(
                    new Color(targetColor.r, targetColor.g, targetColor.b, 0f),
                    targetColor,
                    easedT
                );
            }

            rectTransform.localScale = Vector3.Lerp(
                originalScale * startScale,
                originalScale,
                easedT
            ) + originalScale * ((peakScale - 1f) * scaleT);

            yield return null;
        }

        if (image != null)
        {
            image.color = targetColor;
        }

        ResetTransformVisuals();
        animationRoutine = null;
    }

    private IEnumerator AnimateRegenerateSlide(
        Color targetColor,
        float duration,
        float delay,
        Vector2 slideOffset,
        float peakScale)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        Vector2 targetPosition = rectTransform.anchoredPosition;
        Vector2 startPosition = targetPosition + slideOffset;

        rectTransform.anchoredPosition = startPosition;
        rectTransform.localRotation = originalRotation;
        rectTransform.localScale = originalScale;

        if (image != null)
        {
            image.color = new Color(targetColor.r, targetColor.g, targetColor.b, 0f);
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easedT = 1f - Mathf.Pow(1f - t, 3f);
            float scaleT = Mathf.Sin(t * Mathf.PI);

            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, easedT);
            rectTransform.localScale = Vector3.Lerp(
                originalScale,
                originalScale * peakScale,
                scaleT
            );

            if (image != null)
            {
                image.color = Color.Lerp(
                    new Color(targetColor.r, targetColor.g, targetColor.b, 0f),
                    targetColor,
                    easedT
                );
            }

            yield return null;
        }

        rectTransform.anchoredPosition = targetPosition;
        ResetTransformVisuals();

        if (image != null)
        {
            image.color = targetColor;
        }

        animationRoutine = null;
    }

    private void CacheRectTransform()
    {
        if (rectTransform != null)
            return;

        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform != null ? rectTransform.localScale : Vector3.one;
        originalRotation = rectTransform != null ? rectTransform.localRotation : Quaternion.identity;
    }

    private void ResetTransformVisuals()
    {
        if (rectTransform == null)
            return;

        rectTransform.localScale = originalScale;
        rectTransform.localRotation = originalRotation;
    }

    private Color GetColor(CellState state)
    {
        return state == CellState.White
            ? whiteTileColor
            : blackTileColor;
    }

    private void ConfigureVisualEffects()
    {
        if (image == null)
            return;

        image.raycastTarget = false;
        innerTintImage = EnsureOverlayImage("InnerTint", 1f, true);
        previewGlowImage = EnsureGlowImage("PreviewGlow", 1.24f);
        clearGlowImage = EnsureGlowImage("ClearGlow", 1.42f);
        SetOverlay(innerTintImage, Color.clear, 0f, 1f);
        SetGlow(previewGlowImage, Color.clear, 0f, 1f);
        SetGlow(clearGlowImage, Color.clear, 0f, 1f);

        GameManager.ApplyShadow(gameObject);
    }

    private Image EnsureOverlayImage(string objectName, float scale, bool fillCenter)
    {
        Transform existing = transform.Find(objectName);
        GameObject overlayObject = existing != null
            ? existing.gameObject
            : new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));

        overlayObject.transform.SetParent(transform, false);
        overlayObject.transform.SetAsLastSibling();

        RectTransform overlayRect = overlayObject.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        overlayRect.localScale = Vector3.one * scale;

        Image overlayImage = overlayObject.GetComponent<Image>();
        overlayImage.sprite = image.sprite;
        overlayImage.type = Image.Type.Sliced;
        overlayImage.fillCenter = fillCenter;
        overlayImage.raycastTarget = false;
        overlayImage.color = Color.clear;

        return overlayImage;
    }

    private Image EnsureGlowImage(string objectName, float scale)
    {
        Image glowImage = EnsureOverlayImage(objectName, scale, false);

        Shadow glowShadow = glowImage.gameObject.GetComponent<Shadow>();
        if (glowShadow == null)
        {
            glowShadow = glowImage.gameObject.AddComponent<Shadow>();
        }

        glowShadow.effectColor = new Color(1f, 1f, 1f, 1f);
        glowShadow.effectDistance = Vector2.zero;
        glowShadow.useGraphicAlpha = true;

        return glowImage;
    }

    private void SetOverlay(Image overlayImage, Color color, float alphaMultiplier, float scale)
    {
        if (overlayImage == null)
            return;

        overlayImage.color = new Color(color.r, color.g, color.b, color.a * alphaMultiplier);
        overlayImage.rectTransform.localScale = Vector3.one * scale;
    }

    private void SetGlow(Image glowImage, Color color, float alpha, float scale)
    {
        if (glowImage == null)
            return;

        glowImage.color = new Color(color.r, color.g, color.b, alpha);
        glowImage.rectTransform.localScale = Vector3.one * scale;
    }
}
