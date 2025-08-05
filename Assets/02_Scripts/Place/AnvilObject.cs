using UnityEngine;

public class AnvilObject : MonoBehaviour, IStationType
{
    public CraftingStation GetStationType() => CraftingStation.Anvil;
}
