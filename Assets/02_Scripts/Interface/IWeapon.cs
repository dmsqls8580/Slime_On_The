using UnityEngine;

public interface IWeapon
{
    float AttackPow { get; }
    float AttackSpd { get; }
    GameObject ToolPrefab { get; }
    ToolType  ToolType { get; }
}
