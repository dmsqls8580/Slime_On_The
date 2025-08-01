using UnityEngine;

public class PlacedObject : MonoBehaviour, IStationType
{
    public CraftingStation GetStationType() => CraftingStation.Workbench;
}
