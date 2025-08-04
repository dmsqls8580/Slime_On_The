using NPOI.Util.Collections;
using PlayerStates;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PoisonAreaZoneObject : MonoBehaviour, IPoolObject,IAttackable
{
    public GameObject GameObject => gameObject;

    [SerializeField] private string poolID = "PoisonSprayArea";
    public string PoolID => poolID;
    [SerializeField] private int poolSize = 2;
    public int PoolSize => poolSize;

    private StatBase damage;
    
    public IDamageable Target => null;
    public void Attack() { }
    
    public event Action OnReturned;
    
    public StatBase AttackStat => damage;
    
    private PlayerController owner;
    private GameObject projectileHost;
    private float areaLength;
    private float damageInterval;
    private Coroutine damageRoutine;
    private HashSet<IDamageable> targets = new ();

    private bool isActive = false;

    public void Init(PlayerController _owner, float _damage, GameObject  _host, float _damageInterval, float _areaLength)
    {
        owner = _owner;
        projectileHost=_host;
        damageInterval = _damageInterval;
        areaLength = _areaLength;

        isActive = true;

        if (!damageRoutine.IsUnityNull())
        {
            StopCoroutine(damageRoutine);
        }

        damage = new CalculateStat(StatType.Attack, _damage);

      //  Logger.Log($"현재 스킬 대미지: {damage.GetCurrent()}");
        damageRoutine = StartCoroutine(StartDamageRoutine());
    }

    void Update()
    {
        // 플레이어/스킬이 활성 상태일 때만 위치/회전 갱신
        if (isActive && !owner.IsUnityNull() && owner.IsMouse02Pressed())
        {
            Vector3 sprayDir = owner.AnimationController.UpdatePlayerDirectionByMouse();
            transform.position = owner.AttackPivot.position + sprayDir * (areaLength * 0.5f);
            transform.rotation = Quaternion.FromToRotation(Vector3.right, sprayDir);
        }
        else if (isActive)
        {
            // 버튼에서 손을 떼면 비활성화(풀 반환)
            isActive = false;
            ObjectPoolManager.Instance.ReturnObject(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out IDamageable target) && !other.CompareTag("Player"))
        {
            targets.Add(target);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out IDamageable target) && !other.CompareTag("Player"))
        {
            targets.Remove(target);
        }
    }

    private IEnumerator StartDamageRoutine()
    {
        while (gameObject.activeSelf)
        {
            foreach (var target in targets)
            {
                if (target == null)
                {
                    Debug.LogWarning("[PoisonAreaZoneObject] target is null");
                    continue;
                }

                if (owner == null)
                {
                    Debug.LogWarning("[PoisonAreaZoneObject] owner is null");
                    continue;
                }

                target.TakeDamage(this, projectileHost);
            }

            yield return new WaitForSeconds(damageInterval);
        }
    }

    public void OnSpawnFromPool() { }

    public void OnReturnToPool()
    {
        targets.Clear();
        if (!damageRoutine.IsUnityNull())
        {
            StopCoroutine(damageRoutine);
        }

        damageRoutine = null;
        isActive = false;
    }
    
}