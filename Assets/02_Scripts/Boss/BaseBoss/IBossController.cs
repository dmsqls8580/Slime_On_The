using UnityEngine;

public interface IBossController
{
    public BossStatus BossStatus { get; set; }
    public Transform Transform { get; }
    public GameObject AttackTarget { get; set; }
    public Vector3 SpawnPos      { get;  set; }
    public bool IsPlayerInAttackRange { get; }
    public void SetPlayerInAttackRange(bool inRange);
    public void SetPlayerInSenseRange(bool _inRange);
}