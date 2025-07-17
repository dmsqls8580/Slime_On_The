using PlayerStates;
using UnityEngine;
using UnityEngine.EventSystems;

[CreateAssetMenu(menuName = "PlayerSkill/NormalAttack")]
public class NormalAttack : PlayerSkillSO
{

    public override void Execute(PlayerController _owner)
    {

        // 공격 방향 계산
        Vector2 attackDir = _owner.UpdatePlayerDirectionByMouse();

        // 투사체 풀에서 꺼내서 발사
        ProjectilePoolManager.Instance.Spawn(
            (Vector2)_owner.AttackPivot.position,
            attackDir,
            speed,
            range
        );
        // 쿨타임 등은 SkillMananger에서 통합 관리
    }
}
