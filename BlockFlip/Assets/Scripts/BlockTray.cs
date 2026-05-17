using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockTray : MonoBehaviour
{
    [SerializeField] private DraggableBlock blockPrefab;
    [SerializeField] private Transform[] slots;

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

    public void RefillTray()
    {
        foreach (Transform slot in slots)
        {
            if(slot.childCount > 0)
            {
                continue;
            }

            BlockShape randomShape = GetRandomShape();
            DraggableBlock block = Instantiate(blockPrefab, slot);
            block.Initialize(randomShape, this);
        }
    }

    public void ReplaceBlock(DraggableBlock usedBlock)
    {
        Transform parentSlot = usedBlock.transform.parent;
        Destroy(usedBlock.gameObject);

        RefillTray();
    }

    private BlockShape GetRandomShape()
    {
        int index = Random.Range(0, shapePool.Count);
        return shapePool[index];
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
}
