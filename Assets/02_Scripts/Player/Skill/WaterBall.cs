using PlayerStates;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "PlayerSkill/WaterBall")]
public class WaterBall : PlayerSkillSO
{
    public float damageInterval = 0.05f;
    public float areaLength = 2f;
    private GameObject sprayAreaPrefab;

    private Coroutine sprayCoroutine;

    public override void Execute(PlayerController _owner, float _damage)
    {
        if (sprayAreaPrefab!=null && sprayAreaPrefab.activeSelf)
        {
            return;
        }
        
        sprayAreaPrefab = ObjectPoolManager.Instance.GetObject("WaterBallArea");
        if (sprayAreaPrefab.IsUnityNull())
        {
            return;
        }

        Vector3 sprayDir = _owner.AnimationController.UpdatePlayerDirectionByMouse();
        sprayAreaPrefab.transform.position = _owner.AttackPivot.position + (Vector3)(sprayDir * areaLength * 0.5f);
        sprayAreaPrefab.transform.rotation = Quaternion.FromToRotation(Vector3.right, sprayDir);
        sprayAreaPrefab.SetActive(true);

        var sprayAreaComponent = sprayAreaPrefab.GetComponent<WaterBallAreaObject>();
        sprayAreaComponent.Init(_owner, _damage, _owner.gameObject, damageInterval, areaLength);
        
        sprayAreaComponent.OnReturned += () => sprayAreaPrefab = null;
    }
}