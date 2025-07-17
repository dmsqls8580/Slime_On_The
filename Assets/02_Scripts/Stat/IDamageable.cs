using System.Collections;
using UnityEngine;

public interface IDamageable
{
    public bool IsDead { get; }
    public Collider2D Collider { get; }
   
    
    /// <summary>
    /// 대미지를 주는 메서드
    /// </summary>
    /// <param name="_attacker">공격을 실행한 대상</param>
    public void TakeDamage(IAttackable _attacker, GameObject _attackerObj);
    public void Dead();
    
}
