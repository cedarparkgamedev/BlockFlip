using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableBlock : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private float BlockPosYOffsetWhileDragging;
    [SerializeField] private float dropReleaseDuration = 0.12f;
    [SerializeField] private float dropReleaseScale = 0.72f;

    private BlockShape shape;
    private BlockTray tray;
    private BlockVisual blockVisual;
    private RectTransform rectTransform;
    private Canvas canvas;
    private float trayTileSize;

    private Vector2 originalAnchoredPosition;
    private Transform originalParent;
    private GridDropResolver resolver;

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

        SetDraggingPosition(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        SetDraggingPosition(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        resolver ??= FindAnyObjectByType<GridDropResolver>();

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
