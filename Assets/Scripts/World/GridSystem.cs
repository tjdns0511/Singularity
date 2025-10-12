using UnityEngine;
using System.Collections.Generic;

public class GridSystem : MonoBehaviour
{
    public static GridSystem Instance { get; private set; }

    private Dictionary<Vector3Int, BlockObject> blockGrid = new Dictionary<Vector3Int, BlockObject>();

    private void Awake()
    {
        if (Instance == null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public bool PlaceBlock(BlockData data, Vector3Int position, Quaternion rotation)
    {
        if (blockGrid.ContainsKey(position))
        {
            Debug.LogWarning($"GridSystem: Position {position} is already occupied.");
            return false;
        }

        if (data.prefab == null)
        {
            Debug.LogError($"GridSystem: BlockData '{data.blockName}' has no assigned prefab.");
            return false;
        }

        GameObject newBlockInstance = Instantiate(data.prefab, position, rotation);
        BlockObject blockObject = newBlockInstance.GetComponent<BlockObject>();

        if (blockObject == null)
        {
            Debug.LogWarning($"GridSystem: Prefab for '{data.blockName}' is missing a BlockObject-derived component. Adding base BlockObject.");
            blockObject = newBlockInstance.AddComponent<BlockObject>();
        }

        blockObject.Initialize(data, position);
        blockGrid.Add(position, blockObject);

        Debug.Log($"Successfully placed '{data.blockName}' at {position}");

        return true;
    }


    public BlockObject GetBlockAt(Vector3Int position)
    {
        blockGrid.TryGetValue(position, out BlockObject block);
        return block;
    }

    public bool RemoveBlock(Vector3Int position)
    {
        if (blockGrid.TryGetValue(position, out BlockObject blockToRemove))
        {
            Destroy(blockToRemove.gameObject);
            blockGrid.Remove(position);

            Debug.Log($"Successfully removed block at {position}");
            return true;
        }

        Debug.LogWarning($"GridSystem: No block found at {position} to remove.");
        return false;
    }
}
