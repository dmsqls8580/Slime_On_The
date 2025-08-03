using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnemySpawner))]
public class EnemySpawnerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EnemySpawner spawner = (EnemySpawner)target;

        if (GUILayout.Button("몬스터 스폰"))
        {
            spawner.SpawnEnemies();
        }
        
        if (GUILayout.Button("몬스터 제거"))
        {
            spawner.ClearEnemies();
        }
    }
}