using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableBlock : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private float BlockPosYOffsetWhileDragging;

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
            tray.ReplaceBlock(this);
        }
        else
        {
            transform.SetParent(originalParent, true);
            rectTransform.anchoredPosition = originalAnchoredPosition;
            blockVisual.Build(shape, trayTileSize);
        }
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
