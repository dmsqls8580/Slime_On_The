using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ConditionalFieldAttribute))]
public class ConditionalFieldDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ConditionalFieldAttribute condAttr = (ConditionalFieldAttribute)attribute;

        bool shouldShow = CheckCondition(property, condAttr);

        if (shouldShow)
        {
            EditorGUI.PropertyField(position, property, label, true);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        ConditionalFieldAttribute condAttr = (ConditionalFieldAttribute)attribute;

        bool shouldShow = CheckCondition(property, condAttr);

        return shouldShow ? EditorGUI.GetPropertyHeight(property, label, true) : 0f;
    }

    private bool CheckCondition(SerializedProperty property, ConditionalFieldAttribute condAttr)
    {
        var target = property.serializedObject.targetObject;
        var conditionField = target.GetType().GetField(condAttr.conditionFieldName);
        if (conditionField == null) return true;

        object conditionValue = conditionField.GetValue(target);

        if (condAttr.compareValue == null)
        {
            if (conditionValue is bool b)
                return b;
            return conditionValue != null;
        }

        return Equals(conditionValue, condAttr.compareValue);
    }
}