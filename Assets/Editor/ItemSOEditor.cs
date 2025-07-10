#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

#if UNITY_EDITOR
[CustomEditor(typeof(ItemSO))]
public class ItemSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var item = (ItemSO)target;

        DrawSafeField("idx");
        DrawSafeField("itemName");
        DrawSafeField("description");
        DrawSafeField("icon");
        DrawSafeField("itemTypes");
        DrawSafeField("stackable");
        DrawSafeField("maxStack");
        DrawSafeField("recipe", true);

        // 타입별 데이터 표시 (초기화 + null 체크 포함)
        if (item.itemTypes.HasFlag(ItemType.Material))
        {
            if (item.materialData == null) item.materialData = new MaterialData();
            DrawSafeField("materialData", true);
        }

        if (item.itemTypes.HasFlag(ItemType.Tool))
        {
            if (item.toolData == null) item.toolData = new ToolData();
            DrawSafeField("toolData", true);
        }

        if (item.itemTypes.HasFlag(ItemType.Equipable))
        {
            if (item.equipableData == null) item.equipableData = new EquipableData();
            DrawSafeField("equipableData", true);
        }

        if (item.itemTypes.HasFlag(ItemType.Eatable))
        {
            if (item.eatableData == null) item.eatableData = new EatableData();
            DrawSafeField("eatableData", true);
        }

        if (item.itemTypes.HasFlag(ItemType.Placeable))
        {
            if (item.placeableData == null) item.placeableData = new PlaceableData();
            DrawSafeField("placeableData", true);
        }

        serializedObject.ApplyModifiedProperties();
    }

    void DrawSafeField(string propertyName, bool includeChildren = false)
    {
        var prop = serializedObject.FindProperty(propertyName);
        if (prop != null)
        {
            EditorGUILayout.PropertyField(prop, includeChildren);
        }
        else
        {
            EditorGUILayout.HelpBox($"[필드 없음] {propertyName}", MessageType.Warning);
        }
    }
}
#endif
