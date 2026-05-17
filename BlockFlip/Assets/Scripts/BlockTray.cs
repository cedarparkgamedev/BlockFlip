using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockTray : MonoBehaviour
{
    [SerializeField] private DraggableBlock blockPrefab;
    [SerializeField] private Transform[] slots;

    [Header("Refill Animation")]
    [SerializeField] private float refillSlideDuration = 0.42f;
    [SerializeField] private float refillSlideDistance = 220f;
    [SerializeField] private float refillStagger = 0.08f;
    [SerializeField] private float refillStartScale = 0.92f;
    [SerializeField] private float refillPeakScale = 1.04f;

    private List<BlockShape> shapePool;

    private IEnumerator Start()
    {
        shapePool = BlockShapeLibrary.CreateDefaultShapes();

        yield return null;
        Canvas.ForceUpdateCanvases();

        RefillTray();

        yield return null;
        Canvas.ForceUpdateCanvases();
        RefreshExistingBlocks();
    }

    public List<DraggableBlock> RefillTray(bool prepareForAnimation = false)
    {
        List<DraggableBlock> createdBlocks = new List<DraggableBlock>();

        foreach (Transform slot in slots)
        {
            if(slot.childCount > 0)
            {
                continue;
            }

            BlockShape randomShape = GetRandomShape();
            DraggableBlock block = Instantiate(blockPrefab, slot);
            block.Initialize(randomShape, this);

            if (prepareForAnimation)
            {
                PrepareRefillAnimation(block);
            }

            createdBlocks.Add(block);
        }

        return createdBlocks;
    }

    public void ReplaceBlock(DraggableBlock usedBlock)
    {
        Destroy(usedBlock.gameObject);

        if (AreAllSlotsEmpty())
        {
            StartCoroutine(RefillTrayAfterLayout());
        }
    }

    private BlockShape GetRandomShape()
    {
        int index = Random.Range(0, shapePool.Count);
        return shapePool[index];
    }

    private bool AreAllSlotsEmpty()
    {
        foreach (Transform slot in slots)
        {
            if (slot.childCount > 0)
                return false;
        }

        return true;
    }

    private IEnumerator RefillTrayAfterLayout()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();
        List<DraggableBlock> createdBlocks = RefillTray(true);

        yield return null;
        Canvas.ForceUpdateCanvases();
        RefreshExistingBlocks();

        for (int i = 0; i < createdBlocks.Count; i++)
        {
            StartCoroutine(PlayRefillAnimation(createdBlocks[i], i * refillStagger));
        }
    }

    private void RefreshExistingBlocks()
    {
        foreach (Transform slot in slots)
        {
            foreach (Transform child in slot)
            {
                if (child.TryGetComponent(out DraggableBlock block))
                {
                    block.RefreshTrayVisual();
                }
            }
        }
    }

    private void PrepareRefillAnimation(DraggableBlock block)
    {
        if (block == null)
            return;

        RectTransform blockRect = block.GetComponent<RectTransform>();
        if (blockRect != null)
        {
            blockRect.anchoredPosition = Vector2.left * refillSlideDistance;
            blockRect.localScale = Vector3.one * refillStartScale;
        }

        CanvasGroup canvasGroup = block.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = block.gameObject.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = 0f;
    }

    private IEnumerator PlayRefillAnimation(DraggableBlock block, float delay)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        if (block == null)
            yield break;

        RectTransform blockRect = block.GetComponent<RectTransform>();
        CanvasGroup canvasGroup = block.GetComponent<CanvasGroup>();
        if (blockRect == null || canvasGroup == null)
            yield break;

        Vector2 startPosition = Vector2.left * refillSlideDistance;
        Vector2 targetPosition = Vector2.zero;
        Vector3 startScale = Vector3.one * refillStartScale;
        Vector3 peakScale = Vector3.one * refillPeakScale;
        float elapsed = 0f;

        blockRect.anchoredPosition = startPosition;
        blockRect.localScale = startScale;
        canvasGroup.alpha = 0f;

        while (elapsed < refillSlideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / refillSlideDuration);
            float easedT = Mathf.SmoothStep(0f, 1f, t);
            float scaleT = Mathf.Sin(t * Mathf.PI);

            blockRect.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, easedT);
            blockRect.localScale = Vector3.Lerp(Vector3.one, peakScale, scaleT);
            canvasGroup.alpha = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t * 1.35f));

            yield return null;
        }

        blockRect.anchoredPosition = targetPosition;
        blockRect.localScale = Vector3.one;
        canvasGroup.alpha = 1f;
    }
}
