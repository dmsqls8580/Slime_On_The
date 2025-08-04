using PlayerStates;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "PlayerSkill/PoisonSpray")]
public class PoisonSpray : PlayerSkillSO
{
    public float sprayInterval = 0.05f;
    public float areaLength;
    public GameObject sprayAreaPrefab;
    
    private Coroutine sprayCoroutine;
    
    public override void Execute(PlayerController _owner,float _damage)
    {
        GameObject sprayArea= ObjectPoolManager.Instance.GetObject("PoisonSprayArea");
        if (sprayArea.IsUnityNull())
        {
            return;
        }
        
        Vector3 sprayDir= _owner.AnimationController.UpdatePlayerDirectionByMouse();
        sprayArea.transform.position = _owner.AttackPivot.position + (Vector3)(sprayDir * areaLength * 0.5f);
        sprayArea.transform.rotation = Quaternion.FromToRotation(Vector3.right, sprayDir);
    }
}
