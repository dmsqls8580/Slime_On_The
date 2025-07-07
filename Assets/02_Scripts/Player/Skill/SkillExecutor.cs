using UnityEngine;
using UnityEngine.EventSystems;

public class SkillExecutor : MonoBehaviour
{
     public void Executor(PlayerSkillSO skill, GameObject player, Vector2 dir)
     {
         if (EventSystem.current.IsPointerOverGameObject())
             return;
         
         Vector2 origin = (Vector2)player.transform.position + dir.normalized * skill.range;

          switch (skill.skillType)
          {
               case PlayerSkillType.Melee:
                    ExecuteMelee(skill, origin);
                    break;
               case PlayerSkillType.Ranged:
                    ExecuteProjectile(skill, origin, dir);
                    break;
          }
     }

     public void ExecuteMelee(PlayerSkillSO skill, Vector2 origin)
     {

     }

     public void ExecuteProjectile(PlayerSkillSO skill, Vector2 origin, Vector2 dir)
     {
          float speed = skill.speed;
          ProjectilePoolManager.Instance.Spawn(origin, dir, speed);
     }

     

}
