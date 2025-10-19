using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class ChunkManager : MonoBehaviour
{

    private HashSet<Vector2Int> activeChunkCoordinates = new HashSet<Vector2Int>();

    [SerializeField] private int chunkSize = 16;

    [SerializeField] private GameObject chunkFloorPrefab;

    private void Start()
    {
        AddChunk(Vector2Int.zero);
    }

    public void AddChunk(Vector2Int chunkCoord)
    {
        if (!activeChunkCoordinates.Contains(chunkCoord))
        {
            activeChunkCoordinates.Add(chunkCoord);
            Debug.Log($"Chunk at {chunkCoord} has been activated.");

            if (chunkFloorPrefab != null)
            {
                GameObject chunkParent = new GameObject($"Chunk ({chunkCoord.x}, {chunkCoord.y})");
                chunkParent.transform.SetParent(this.transform);

                Vector3 chunkOrigin = new Vector3(chunkCoord.x * chunkSize, 0, chunkCoord.y * chunkSize);

                for (int x = 0; x < chunkSize; x++)
                {
                    for (int z = 0; z < chunkSize; z++)
                    {
                        Vector3 tilePosition = chunkOrigin + new Vector3(x, -1, z);
                        Instantiate(chunkFloorPrefab, tilePosition, Quaternion.identity, chunkParent.transform);
                    }
                }
            }
        }
    }

    public bool IsPositionInActiveChunk(Vector3 worldPosition)
    {
        Vector2Int chunkCoord = WorldToChunkCoords(worldPosition);
        return activeChunkCoordinates.Contains(chunkCoord);
    }

    private Vector2Int WorldToChunkCoords(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt(worldPosition.x / chunkSize);
        int z = Mathf.FloorToInt(worldPosition.z / chunkSize);
        return new Vector2Int(x, z);
    }
}
