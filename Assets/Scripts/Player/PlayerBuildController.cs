using UnityEngine;

public class PlayerBuildController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private BlockData blockToBuild;

    [Header("Ghost Materials")]
    [SerializeField] private Material ghostValidMaterial;
    [SerializeField] private Material ghostInvalidMaterial;

    [Header("Layer Settings")]
    [SerializeField] private int minBuildLayer = 0;
    [SerializeField] private int maxBuildLayer = 19;
    private int currentBuildLayer = 0;

    private Camera mainCamera;
    private Plane gridPlane = new Plane(Vector3.up, Vector3.zero);

    private GameObject ghostBlockInstance;
    private Renderer ghostRenderer;

    private void Awake()
    {
        mainCamera = Camera.main;
        InitializeGhostBlock();
        HandleLayerInput();
        UpdateBuildPlane();
    }

    private void Update()
    {
        HandleLayerInput();
        UpdateGhostBlockPosition();
        HandleBuildInputs();
    }

    private void HandleLayerInput()
    {
        bool layerChanged = false;

        if (Input.GetKeyDown(KeyCode.E))
        {
            currentBuildLayer++;
            layerChanged = true;
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            currentBuildLayer--;
            layerChanged = true;
        }

        if (layerChanged)
        {
            currentBuildLayer = Mathf.Clamp(currentBuildLayer, minBuildLayer, maxBuildLayer);
            UpdateBuildPlane();
            Debug.Log($"Current Build Layer: {currentBuildLayer}");
        }
    }

    private void UpdateBuildPlane()
    {
        gridPlane.SetNormalAndPosition( Vector3.up, new Vector3(0, currentBuildLayer, 0 ));
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

        Vector3Int gridPosition = Vector3Int.FloorToInt(worldPosition);
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

        Vector3Int gridPosition = Vector3Int.FloorToInt(worldPosition);
        GameManagers.Instance.GridSystem.RemoveBlock(gridPosition);
    }

    private Vector3 GetMouseWorldPosition()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (gridPlane.Raycast(ray, out float enter))
        {
            Vector3 worldPosition = ray.GetPoint(enter);
            Debug.Log(Vector3Int.FloorToInt(worldPosition));
            return new Vector3(worldPosition.x, worldPosition.y, worldPosition.z);
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
        Vector3Int gridPosition = Vector3Int.FloorToInt(worldPosition);
        ghostBlockInstance.transform.position = new Vector3(gridPosition.x, gridPosition.y, gridPosition.z);

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
