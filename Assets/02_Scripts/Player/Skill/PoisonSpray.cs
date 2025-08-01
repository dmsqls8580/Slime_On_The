using PlayerStates;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "PlayerSkill/PoisonSpray")]
public class PoisonSpray : PlayerSkillSO
{

    public float sprayInterval = 0.05f;
    public float sprayAngle = 30f;
    public int bulletCount = 3;
    
    private Coroutine sprayCoroutine;
    
    public override void Execute(PlayerController _owner,float _damage)
    {
        if (sprayCoroutine != null)
        {
            return;
        }

        sprayCoroutine = _owner.StartCoroutine(SprayRoutine(_owner));
    }

    private void Fire(PlayerController _owner)
    {
        Vector2 dir = _owner.AnimationController.UpdatePlayerDirectionByMouse();
        float angleStep = ((bulletCount) > 1) ? sprayAngle / (bulletCount - 1) : 0;
        float startAngle = -sprayAngle * 0.5f;

        for (int i = 0; i < bulletCount; i++)
        {
            float angle = startAngle + angleStep * i;
            Vector2 attackDir = Quaternion.Euler(0,0,angle) * dir;
                
            GameObject projObj = ObjectPoolManager.Instance.GetObject("PlayerProjectile");
            if (!projObj.IsUnityNull())
            {
                projObj.transform.position = _owner.AttackPivot.position;

                var projectile = projObj.GetComponent<PlayerProjectile>();

                projectile.Init(_owner.StatManager, damage, _owner.gameObject, attackDir, speed, range);
                projectile.OnSpawnFromPool();
            }
                
        }
    }
    

    private IEnumerator SprayRoutine(PlayerController _owner)
    {
        float time = 0f;
        while (_owner.IsMouse02Pressed())
        {
            Fire(_owner);
            yield return new WaitForSeconds(sprayInterval);
            time += sprayInterval;
        }
    }
}
