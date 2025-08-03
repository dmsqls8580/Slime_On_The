using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BossSpawner))]
public class BossSpawnerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        BossSpawner spawner = (BossSpawner)target;

        if (GUILayout.Button("보스 스폰"))
        {
            spawner.SpawnBoss();
        }
        
        if (GUILayout.Button("보스 제거"))
        {
            spawner.ClearBoss();
        }
    }
}