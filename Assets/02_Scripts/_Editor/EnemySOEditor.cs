using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(EnemySO))]
public class EnemySOEditor : EditorWindow
{
    private EnemySO editingSO;
    private SerializedObject soObject;
    // -1은 아직 선택되지 않음을 의미(enum 값에 정의 되지 않은 값)
    private EnemyAttackType prevAttackType = (EnemyAttackType)(-1);

    // AttackType에 따라 자동으로 배열되는 스탯 딕셔너리
    private static readonly Dictionary<EnemyAttackType, StatType[]> StatTemplates = new()
    {
        { EnemyAttackType.None, new[] {
            StatType.MaxHp, StatType.CurrentHp, StatType.Attack, StatType.Defense, StatType.MoveSpeed,
            StatType.AttackCooldown, StatType.MaxMoveDelay, StatType.MinMoveDelay, StatType.WanderRadius,
            StatType.SenseRange, StatType.AttackRange, StatType.FleeDistance
        }},
        { EnemyAttackType.Neutral, new[] {
            StatType.MaxHp, StatType.CurrentHp, StatType.Attack, StatType.Defense, StatType.MoveSpeed,
            StatType.AttackCooldown, StatType.MaxMoveDelay, StatType.MinMoveDelay, StatType.WanderRadius,
            StatType.SenseRange, StatType.AttackRange, StatType.AttackRadius
        }},
        { EnemyAttackType.Aggressive, new[] {
            StatType.MaxHp, StatType.CurrentHp, StatType.Attack, StatType.Defense, StatType.MoveSpeed,
            StatType.AttackCooldown, StatType.MaxMoveDelay, StatType.MinMoveDelay, StatType.WanderRadius,
            StatType.SenseRange, StatType.AttackRange, StatType.AttackRadius
        }},
    };

    [MenuItem("Tools/Enemy SO Editor")]
    public static void ShowWindow()
    {
        GetWindow<EnemySOEditor>("Enemy SO Editor");
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();

        editingSO = (EnemySO)EditorGUILayout.ObjectField("EnemySO", editingSO, typeof(EnemySO), false, GUILayout.Width(400));

        if (editingSO.IsUnityNull())
        {
            if (GUILayout.Button("새 EnemySO 생성"))
            {
                editingSO = CreateInstance<EnemySO>();
                string assetPath = "Assets/10_Tables/ScriptableObj/Enemy/NewEnemySO.asset";
                AssetDatabase.CreateAsset(editingSO, assetPath);
                AssetDatabase.SaveAssets();
            }
            return;
        }

        if (soObject == null || soObject.targetObject != editingSO)
        {
            soObject = new SerializedObject(editingSO);
            prevAttackType = (EnemyAttackType)soObject.FindProperty("AttackType").enumValueIndex;
        }

        soObject.Update();

        EditorGUILayout.LabelField("EnemyID", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(soObject.FindProperty("EnemyID"), GUILayout.Width(400));
        EditorGUILayout.PropertyField(soObject.FindProperty("AttackType"), GUILayout.Width(400));

        // AttackType 변경 감지
        var attackTypeProp = soObject.FindProperty("AttackType");
        var attackType = (EnemyAttackType)attackTypeProp.enumValueIndex;
        if (attackType != prevAttackType)
        {
            // 자동 스탯 세팅
            var statsProp = soObject.FindProperty("EnemyStats");
            statsProp.ClearArray();

            if (StatTemplates.TryGetValue(attackType, out var template))
            {
                foreach (var statType in template)
                {
                    int idx = statsProp.arraySize;
                    statsProp.InsertArrayElementAtIndex(idx);
                    var statProp = statsProp.GetArrayElementAtIndex(idx);
                    statProp.FindPropertyRelative("StatType").enumValueIndex = (int)statType;
                    statProp.FindPropertyRelative("ModifierType").enumValueIndex = (int)StatModifierType.Base;
                    statProp.FindPropertyRelative("Value").floatValue = 0f;
                }
            }
            prevAttackType = attackType;
            soObject.ApplyModifiedProperties();
            soObject.Update();
        }

        if (attackType != EnemyAttackType.None)
        {
            EditorGUILayout.PropertyField(soObject.FindProperty("ProjectileID"), GUILayout.Width(400));
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Enemy Status", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(soObject.FindProperty("EnemyStats"), true);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("드랍 아이템", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(soObject.FindProperty("DropItems"), true);

        soObject.ApplyModifiedProperties();
    }
}

