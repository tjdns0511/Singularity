using UnityEngine;
using System.Collections.Generic;
using static MachineIO;

[CreateAssetMenu(fileName = "Mew Machine", menuName = "Singularity/Data/Block/Machine")]

public class MachineData : BlockData
{
    [Header("Machine Infomation")]
    public BlockDirection Direction = BlockDirection.South;
    public MachineIOPoint[] inputs;
    public MachineIOPoint output;

    public RecipeData setRecipe;
}
