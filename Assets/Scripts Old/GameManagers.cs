using UnityEngine;

public class GameManagers : MonoBehaviour
{
    public static GameManagers Instance { get; private set; }

    public GridSystem GridSystem { get; private set; }
    public ChunkManager ChunkManager { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            GridSystem = GetComponent<GridSystem>();
            ChunkManager = GetComponent<ChunkManager>();
        }
    }
}
