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

    private RectTransform rectTransform;
    private Coroutine animationRoutine;
    private Vector3 originalScale;
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

        if (image != null)
        {
            image.color = GetColor(state);
        }
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
        isPreviewing = true;

        if (animationRoutine != null)
        {
            StopCoroutine(animationRoutine);
            animationRoutine = null;
        }

        if (image != null)
        {
            image.color = useSolidPreviewColor
                ? new Color(previewColor.r, previewColor.g, previewColor.b, 1f)
                : Color.Lerp(GetColor(previewState), previewColor, previewColor.a);
        }

        rectTransform.localScale = originalScale * previewScale;
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

        if (rectTransform != null)
        {
            rectTransform.localScale = originalScale;
        }
    }

    private IEnumerator AnimateStateChange(Color targetColor, float duration, float peakScale)
    {
        Color startColor = image != null ? image.color : targetColor;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float scaleT = Mathf.Sin(t * Mathf.PI);

            if (image != null)
            {
                image.color = Color.Lerp(startColor, targetColor, t);
            }

            rectTransform.localScale = Vector3.Lerp(
                originalScale,
                originalScale * peakScale,
                scaleT
            );

            yield return null;
        }

        if (image != null)
        {
            image.color = targetColor;
        }

        rectTransform.localScale = originalScale;
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

        rectTransform.localScale = originalScale;
        animationRoutine = null;
    }

    private IEnumerator AnimateClearHighlight(float duration, float peakScale, Color highlightColor, int flashCount)
    {
        Color baseColor = GetColor(State);
        int safeFlashCount = Mathf.Max(1, flashCount);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float flashT = Mathf.Abs(Mathf.Sin(t * Mathf.PI * safeFlashCount));
            float scaleT = Mathf.Sin(t * Mathf.PI);

            if (image != null)
            {
                image.color = Color.Lerp(baseColor, highlightColor, flashT);
            }

            rectTransform.localScale = Vector3.Lerp(
                originalScale,
                originalScale * peakScale,
                scaleT
            );

            yield return null;
        }

        if (image != null)
        {
            image.color = baseColor;
        }

        rectTransform.localScale = originalScale;
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

        rectTransform.localScale = originalScale;
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
        rectTransform.localScale = originalScale;

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

        Shadow shadow = GetComponent<Shadow>();
        if (shadow == null)
        {
            shadow = gameObject.AddComponent<Shadow>();
        }

        shadow.effectColor = new Color(0f, 0f, 0f, 0.34f);
        shadow.effectDistance = new Vector2(2f, -3f);
        shadow.useGraphicAlpha = true;
    }
}
