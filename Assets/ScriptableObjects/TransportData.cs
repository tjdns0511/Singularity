using UnityEngine;

public enum TransportType { Conveyor, Pipe }

[CreateAssetMenu(fileName = "New Transport", menuName = "Singularity/Data/Block/Transtort")]
public class TransportData : BlockData
{
    [Header("Transport Infomation")]
    public TransportType TransportType;
    public BlockDirection inputDirection = BlockDirection.North;
    public BlockDirection outputDirection = BlockDirection.South;
    public float transportTime;
    public int transportStack;
}
