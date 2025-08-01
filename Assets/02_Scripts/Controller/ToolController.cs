using System;
using Unity.VisualScripting;
using UnityEngine;

public class ToolController : MonoBehaviour
{
    private ITool equippedTool;
    private ITool EquippedTool => equippedTool;
    
    public event Action<ToolType> OnToolTypeChanged;

    public void EquipTool(ITool _tool)
    {
        if(equippedTool==_tool&&!_tool.IsUnityNull())
            return;
        
        equippedTool = _tool;
        OnToolTypeChanged?.Invoke(GetEquippedToolType());
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
