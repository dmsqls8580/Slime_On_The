using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyType
{
    Melee,
    Ranged
}

[CreateAssetMenu(fileName = "EnemyData", menuName = "ScriptableObjects/EnemySO")]
public class EnemySO : ScriptableObject
{
    public int EnemyIDX;            // Enemy IDX
    
    [Header("EnemyStatus")]
    public int Health;              // Enemy 체력
    public int AttackDamage;        // Enemy 공격 데미지
    public float AttackCooldown;    // Enemy 공격 쿨타임
    
    [Header("EnemyMove")]
    public float MoveSpeed;         // EnemyController에서 Agent의 이동 속도
    public float MaxMoveDelay;      // EnemyState에서 Idle 상태 시 최대 지속 시간
    public float MinMoveDelay;      // EnemyState에서 Idle 상태 시 최소 지속 시간
    public float WanderRadius;      // EnemyState에서 Wander상태 시 배회하는 영역 반지름
    public float SenseRange;        // EnemyState에서 player를 감지하는 범위
    public float AttackRange;       // EnemyState에서 공격 State로 넘어가는 범위

}
