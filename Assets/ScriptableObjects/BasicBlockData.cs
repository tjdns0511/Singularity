using UnityEngine;

[CreateAssetMenu(fileName = "New BasicBlock", menuName = "Singularity/Data/Block/BasicBlock")]
public class BasicBlockData : BlockData
{
    [Header("Basic Block Information")]
    public BlockDirection Direction;
}
