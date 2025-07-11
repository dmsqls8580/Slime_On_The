using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolController : MonoBehaviour
{
    private IWeapon equippedTool;
    private GameObject equippedToolModel;
    private IWeapon EquippedTool => equippedTool;

    public void EquipTool(IWeapon _tool)
    {
        equippedTool = _tool;
        
        Logger.Log($"{EquippedTool.ToolType}");
        if (equippedToolModel != null)
        {
            Destroy(equippedToolModel);
        }
        
        if (_tool.ToolPrefab != null)
        {
            equippedToolModel = GameObject.Instantiate(_tool.ToolPrefab, transform);
        }
    }

    public float GetAttackPow()
    {
        return equippedTool != null ? equippedTool.AttackPow : 1f;
    }

    public float GetAttackSpd()
    {
        return equippedTool != null ? equippedTool.AttackSpd : 1f;
    }

    public ToolType GetEquippedToolType()
    {
        return equippedTool != null ? equippedTool.ToolType : ToolType.None;
    }
}
