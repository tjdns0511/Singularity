using UnityEngine;

public class PlayerBuildController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private BlockData blockToBuild;

    [Header("Ghost Materials")]
    [SerializeField] private Material ghostValidMaterial;
    [SerializeField] private Material ghostInvalidMaterial;

    private Camera mainCamera;
    private Plane gridPlane = new Plane(Vector3.up, Vector3.zero);

    private GameObject ghostBlockInstance;
    private Renderer ghostRenderer;

    private void Awake()
    {
        mainCamera = Camera.main;
        InitializeGhostBlock();
    }

    private void Update()
    {
        UpdateGhostBlockPosition();
        HandleBuildInputs();
    }

    private void HandleBuildInput()
    {
        Vector3 worldPosition = GetMouseWorldPosition();
        if (!IsValidPosition(worldPosition)) return;

        if (!GameManagers.Instance.ChunkManager.IsPositionInActiveChunk(worldPosition))
        {
            Debug.LogWarning("Cannot build: position is outside of any active chunk.");
            return;
        }

        Vector3Int gridPosition = Vector3Int.RoundToInt(worldPosition);
        GameManagers.Instance.GridSystem.PlaceBlock(blockToBuild, gridPosition, Quaternion.identity);
    }

    private void HandleRemoveInput()
    {
        Vector3 worldPosition = GetMouseWorldPosition();
        if (!IsValidPosition(worldPosition)) return;

        if (!GameManagers.Instance.ChunkManager.IsPositionInActiveChunk(worldPosition))
        {
            Debug.LogWarning("Cannot remove: Position is outside of any active chunk.");
            return;
        }

        Vector3Int gridPosition = Vector3Int.RoundToInt(worldPosition);
        GameManagers.Instance.GridSystem.RemoveBlock(gridPosition);
    }

    private Vector3 GetMouseWorldPosition()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (gridPlane.Raycast(ray, out float enter))
        {
            Vector3 worldPosition = ray.GetPoint(enter);
            return new Vector3(worldPosition.x - 0.5f, worldPosition.y, worldPosition.z - 0.5f);
        }

        return new Vector3(-999, -999, -999);
    }

    private bool IsValidPosition(Vector3 position)
    {
        return position.x != -999;
    }

    private void InitializeGhostBlock()
    {
        if (blockToBuild == null || blockToBuild.prefab == null) return;

        ghostBlockInstance = Instantiate(blockToBuild.prefab);
        ghostRenderer = ghostBlockInstance.GetComponentInChildren<Renderer>();
    }
    
    private void UpdateGhostBlockPosition()
    {
        if (ghostBlockInstance == null) return;

        Vector3 worldPosition = GetMouseWorldPosition();
        if (!IsValidPosition(worldPosition))
        {
            ghostBlockInstance.SetActive(false);
            return;
        }

        ghostBlockInstance.SetActive(true);
        Vector3Int gridPosition = Vector3Int.RoundToInt(worldPosition);
        ghostBlockInstance.transform.position = new Vector3(gridPosition.x + 0.5f, gridPosition.y, gridPosition.z + 0.5f);

        bool canBuild = GameManagers.Instance.ChunkManager.IsPositionInActiveChunk(worldPosition) &&
                        GameManagers.Instance.GridSystem.GetBlockAt(gridPosition) == null;

        ghostRenderer.material = canBuild ? ghostValidMaterial : ghostInvalidMaterial;
    }

    private void HandleBuildInputs()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleBuildInput();
        }

        if (Input.GetMouseButtonDown(1) && !Input.GetKey(KeyCode.LeftControl))
        {
            HandleRemoveInput();
        }
    }
}
