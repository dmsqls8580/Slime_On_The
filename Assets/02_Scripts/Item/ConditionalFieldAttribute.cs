using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class ConditionalFieldAttribute : PropertyAttribute
{
    public string conditionFieldName;
    public object compareValue;

    public ConditionalFieldAttribute(string conditionFieldName)
    {
        this.conditionFieldName = conditionFieldName;
    }

    public ConditionalFieldAttribute(string conditionFieldName, object compareValue)
    {
        this.conditionFieldName = conditionFieldName;
        this.compareValue = compareValue;
    }
}