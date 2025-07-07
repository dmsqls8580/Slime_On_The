using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolController : MonoBehaviour
{
    private IWeapon _equippedTool;
    private GameObject _equippedToolModel;

    private IWeapon EquippedTool => _equippedTool;

    public void EquipTool(IWeapon tool)
    {
        _equippedTool = tool;

        if (_equippedToolModel != null)
        {
            Destroy(_equippedToolModel);
        }
        
        if (tool.ToolPrefab != null)
        {
            _equippedToolModel = GameObject.Instantiate(tool.ToolPrefab, transform);
        }
    }

    public float GetAttackPow()
    {
        return _equippedTool != null ? _equippedTool.AttackPow : 1f;
    }

    public float GetAttackSpd()
    {
        return _equippedTool != null ? _equippedTool.AttackSpd : 1f;
    }
}
