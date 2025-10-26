using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    private Dictionary<Vector3Int, BlockObject> chunkBlockGrid = new Dictionary<Vector3Int, BlockObject>();

    [HideInInspector] public Vector2Int chunkCoordinate;

    private int chunksize;
    private GameObject chunkFloorPrefab;

    public void Initialize(Vector2Int coord, int size, GameObject floorPrefab)
    {
        this.chunkCoordinate = coord;
        this.chunksize = size;
        this.chunkFloorPrefab = floorPrefab;

        GenerateFloor();
    }

    private void GenerateFloor()
    {
        
    }



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
