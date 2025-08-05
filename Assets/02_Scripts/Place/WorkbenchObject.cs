using UnityEngine;

public class WorkbenchObject : MonoBehaviour, IStationType
{
    public CraftingStation GetStationType() => CraftingStation.Workbench;
}
