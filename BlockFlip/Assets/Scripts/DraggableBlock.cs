using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableBlock : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private BlockShape shape;
    private BlockTray tray;
    private RectTransform rectTransform;
    private Canvas canvas;

    private Vector2 originalAnchoredPosition;
    private Transform originalParent;

    public BlockShape Shape => shape;

    public void Initialize(BlockShape blockShape, BlockTray ownerTray)
    {
        shape = blockShape;
        tray = ownerTray;

        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        // TODO:
        // 여기서 shape.Cells를 기반으로 작은 UI 타일들을 생성해서 블록 모양 표시

        GetComponent<BlockVisual>().Build(blockShape);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        originalAnchoredPosition = rectTransform.anchoredPosition;

        transform.SetParent(canvas.transform, true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        GridDropResolver resolver = FindObjectOfType<GridDropResolver>();

        if (resolver != null && resolver.TryPlaceBlock(this, eventData.position))
        {
            tray.ReplaceBlock(this);
        }
        else
        {
            transform.SetParent(originalParent, true);
            rectTransform.anchoredPosition = originalAnchoredPosition;
        }
    }
}