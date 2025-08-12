using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ObjectPoolManager))]
public class ObjectPoolEditor : Editor
{
    public override void OnInspectorGUI()   
    {
        DrawDefaultInspector();

        ObjectPoolManager objectPoolManager = (ObjectPoolManager)target;

        if (GUILayout.Button("ObjectPool 최신화"))
        {
            objectPoolManager.AutoAssignObject();
        }
        
    }
}