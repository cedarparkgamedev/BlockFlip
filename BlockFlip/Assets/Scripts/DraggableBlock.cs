using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableBlock : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [SerializeField] private float BlockPosYOffsetWhileDragging;
    [SerializeField] private float dropReleaseDuration = 0.12f;
    [SerializeField] private float dropReleaseScale = 0.72f;

    [Header("Preview")]
    [SerializeField, Min(0f)] private float previewDelay = 1.25f;
    [SerializeField] private float previewMoveTolerance = 8f;

    private BlockShape shape;
    private BlockTray tray;
    private BlockVisual blockVisual;
    private RectTransform rectTransform;
    private Canvas canvas;
    private float trayTileSize;

    private Vector2 originalAnchoredPosition;
    private Transform originalParent;
    private GridDropResolver resolver;
    private Camera dragCamera;
    private Vector2 lastDragPosition;
    private float stationaryTime;
    private bool isDragging;
    private bool isPreviewVisible;

    public BlockShape Shape => shape;
    public BlockVisual Visual => blockVisual;

    public void Initialize(BlockShape blockShape, BlockTray ownerTray)
    {
        shape = blockShape;
        tray = ownerTray;

        rectTransform = GetComponent<RectTransform>();
        blockVisual = GetComponent<BlockVisual>();
        canvas = GetComponentInParent<Canvas>();
        trayTileSize = blockVisual.TileSize;
        ConfigureDragHitArea();

        // TODO:
        // 여기서 shape.Cells를 기반으로 작은 UI 타일들을 생성해서 블록 모양 표시

        blockVisual.Build(blockShape);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        originalAnchoredPosition = rectTransform.anchoredPosition;
        resolver = FindAnyObjectByType<GridDropResolver>();

        transform.SetParent(canvas.transform, true);

        if (resolver != null)
        {
            blockVisual.Build(shape, resolver.GridCellSize);
        }

        dragCamera = eventData.pressEventCamera;
        lastDragPosition = eventData.position;
        stationaryTime = 0f;
        isDragging = true;
        isPreviewVisible = false;
        SetDraggingPosition(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if ((eventData.position - lastDragPosition).sqrMagnitude > previewMoveTolerance * previewMoveTolerance)
        {
            lastDragPosition = eventData.position;
            stationaryTime = 0f;
            ClearPreview();
        }

        SetDraggingPosition(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        resolver ??= FindAnyObjectByType<GridDropResolver>();
        isDragging = false;
        ClearPreview();

        if (resolver != null && resolver.TryPlaceBlock(this, eventData))
        {
            StartCoroutine(PlayDropRelease());
        }
        else
        {
            transform.SetParent(originalParent, true);
            rectTransform.anchoredPosition = originalAnchoredPosition;
            blockVisual.Build(shape, trayTileSize);
        }
    }

    private void Update()
    {
        if (!isDragging || isPreviewVisible || resolver == null)
            return;

        stationaryTime += Time.deltaTime;
        if (stationaryTime < previewDelay)
            return;

        resolver.ShowPreview(this, dragCamera);
        isPreviewVisible = true;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (transform.parent != originalParent && originalParent != null)
            return;

        shape = shape.Rotated90();
        blockVisual.Build(shape, trayTileSize);
    }

    private IEnumerator PlayDropRelease()
    {
        Vector3 startScale = rectTransform.localScale;
        Vector3 targetScale = startScale * dropReleaseScale;
        float elapsed = 0f;

        while (elapsed < dropReleaseDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / dropReleaseDuration);
            rectTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        tray.ReplaceBlock(this);
    }

    private void ConfigureDragHitArea()
    {
        Image hitArea = GetComponent<Image>();
        if (hitArea == null)
        {
            hitArea = gameObject.AddComponent<Image>();
        }

        hitArea.enabled = true;
        hitArea.raycastTarget = true;
        hitArea.color = Color.clear;
    }

    private void ClearPreview()
    {
        if (resolver != null)
        {
            resolver.ClearPreview();
        }

        isPreviewVisible = false;
    }

    private void SetDraggingPosition(PointerEventData eventData)
    {
        RectTransform parentRect = rectTransform.parent as RectTransform;
        if (parentRect == null)
        {
            rectTransform.position = eventData.position;
            return;
        }

        Camera camera = eventData.pressEventCamera;
        if (!RectTransformUtility.ScreenPointToWorldPointInRectangle(
                parentRect,
                eventData.position,
                camera,
                out Vector3 pointerWorldPosition))
        {
            return;
        }

        rectTransform.position = pointerWorldPosition;

        float targetBottomY = eventData.position.y + BlockPosYOffsetWhileDragging;
        float currentBottomY = blockVisual.GetBottomScreenY(camera);
        Vector2 adjustedPointerPosition = eventData.position + Vector2.up * (targetBottomY - currentBottomY);

        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                parentRect,
                adjustedPointerPosition,
                camera,
                out Vector3 adjustedWorldPosition))
        {
            rectTransform.position = adjustedWorldPosition;
        }
    }
}
