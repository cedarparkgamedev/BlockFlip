using System.Collections.Generic;
using UnityEngine;

public class BlockTray : MonoBehaviour
{
    [SerializeField] private DraggableBlock blockPrefab;
    [SerializeField] private Transform[] slots;

    private List<BlockShape> shapePool;

    private void Start()
    {
        shapePool = BlockShapeLibrary.CreateDefaultShapes();
        RefillTray();
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
}