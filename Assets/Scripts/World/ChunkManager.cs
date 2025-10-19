using UnityEngine;
using System.Collections.Generic;

public class ChunkManager : MonoBehaviour
{

    private HashSet<Vector2Int> activeChunkCoordinates = new HashSet<Vector2Int>();

    [SerializeField] private int chunkSize = 16;

    [SerializeField] private GameObject chunkBaseBlockPrefab;

    private const int CHUNK_BLOCK_HEIGHT = 16;

    public HashSet<Vector2Int> GetActiveChunkCoordinates()
    {
        return activeChunkCoordinates;
    }

    public int GetChunkSize()
    {
        return chunkSize;
    }

    private void Start()
    {
        for (int i = -10; i < 10; i++)
        {
            for (int j = -10; j < 10; j++)
            {
                AddChunk(new Vector2Int(i, j));
            }
        }
    }

    public void AddChunk(Vector2Int chunkCoord)
    {
        if (!activeChunkCoordinates.Contains(chunkCoord))
        {
            activeChunkCoordinates.Add(chunkCoord);
            Debug.Log($"Chunk at {chunkCoord} has been activated.");

            if (chunkBaseBlockPrefab != null)
            {
                GameObject chunkParent = new GameObject($"Chunk ({chunkCoord.x}, {chunkCoord.y})");
                chunkParent.transform.SetParent(this.transform);

                Vector3 chunkPosition = new Vector3(
                    chunkCoord.x * chunkSize,
                    -CHUNK_BLOCK_HEIGHT,
                    chunkCoord.y * chunkSize
                );

                Instantiate(chunkBaseBlockPrefab, chunkPosition, Quaternion.identity, chunkParent.transform );
            }
            else
            {
                Debug.LogError("ChunkManager is missing a reference to 'chunkBaseBlockPrefab'");
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
