using UnityEngine;

public interface IBossController
{
    public BossStatus BossStatus { get; set; }
    public Transform Transform { get; }
    public GameObject ChaseTarget { get; set; }
    public GameObject AttackTarget { get; set; }
    public Vector3 SpawnPos      { get;  set; }
    public void SetPlayerInAttackRange(bool inRange);
    public bool IsPlayerInAttackRange { get; }
}