using PlayerStates;
using UnityEngine;
using UnityEngine.EventSystems;

[CreateAssetMenu(menuName = "PlayerSkill/NormalAttack")]
public class NormalAttack : PlayerSkillSO
{

    public override void Execute(PlayerController _owner, float _damage)
    {

        // 공격 방향 계산
        Vector2 attackDir = _owner.AnimationController.UpdatePlayerDirectionByMouse();

        // 투사체 풀에서 꺼내서 발사

        GameObject projObj = ObjectPoolManager.Instance.GetObject("PlayerProjectile");
        if (projObj != null)
        {
            projObj.transform.position = _owner.AttackPivot.position;

            // 투사체 초기화
            var projectile = projObj.GetComponent<PlayerProjectile>();
            projectile.Init(_owner.StatManager, _damage, attackDir, speed, range);

            // 풀에서 꺼낼 때 필요한 추가 초기화(예: 이펙트 등) 있으면 OnSpawnFromPool 호출
            projectile.OnSpawnFromPool();
        }
        // 쿨타임 등은 SkillMananger에서 통합 관리
    }
}
