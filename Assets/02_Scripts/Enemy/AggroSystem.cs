using System;
using System.Collections.Generic;
using UnityEngine;

public delegate void AggroTargetChanged(GameObject newTarget, float value);

public class AggroSystem
{
    public bool IsEmpty => attackTargetsList.Count == 0;
    
    public event AggroTargetChanged OnTargetChanged;       // 타겟이 바뀌었을때 실행되는 이벤트
    
    public readonly Dictionary<GameObject, float> attackTargetsList = new();
    
    public GameObject CurrentTarget => currentTarget;      // 현재 타겟을 외부에서 조회 가능
    private GameObject currentTarget;                      // 현재 가장 높은 어그로 수치를 가진 대상
    
    public float CurrentTargetValue => currentTargetValue; // 현재 타겟 어그로 수치를 조회 가능
    private float currentTargetValue;                      // 현재 타겟의 어그로 수치

    private float lastTargetChangeTime = -100f;            // 마지막으로 타겟이 바뀐 시간
    private readonly float stickTime;                      // 타겟 지정 유지되는 최소 시간
    private readonly List<GameObject> toRemove = new();    // 타겟 리스트에서 제거할 대상 리스트
    
    private readonly Func<GameObject, bool> isPlayerInSenseRange;
    private AttackType attackType;
    
    // 생성자: 공격 타입 설정, 타겟 변경 유지시간 초기화
    public AggroSystem(AttackType type,
        Func<GameObject, bool> _isPlayerInSenseRange, float _stickTime = 1f)
    {
        attackType = type;
        isPlayerInSenseRange = _isPlayerInSenseRange;
        stickTime = _stickTime;
    }

    /// <summary>
    /// 어그로 수치를 특정 값만큼 누적시키는 메서드
    /// 어그로 수치가 0 이하면 대상에서 제거
    /// </summary>
    /// <param name="_aggroObject"></param>
    /// <param name="_value"></param>
    public void ModifyAggro(GameObject _aggroObject, float _value)
    {
        if (_aggroObject == null)
        {
            return;
        }

        // 만약 이미 어그로 딕셔너리에 등록되어 있는 경우, 변경
        if (attackTargetsList.ContainsKey(_aggroObject))
        {
            attackTargetsList[_aggroObject] += _value;
        }
        // 등록되어 있지 않은 경우 _value 값으로 등록
        else
        {
            attackTargetsList[_aggroObject] = _value;
        }
        
        // 어그로 수치는 0과 100사이로 제한
        attackTargetsList[_aggroObject] = Mathf.Clamp(attackTargetsList[_aggroObject], 0f, 100f);
        
        // 어그로 수치가 0 이하라면 딕셔너리에서 제거
        if (attackTargetsList[_aggroObject] <= 0f)
        {
            attackTargetsList.Remove(_aggroObject);
        }

        // 어그로 대상 갱신 메서드
        UpdateAggroTarget();

    }

    /// <summary>
    /// 어그로 수치를 지정하여 덮어쓰기하는 메서드
    /// 값이 0이하면 대상에서 제거
    /// </summary>
    /// <param name="_aggroObject"></param>
    /// <param name="_value"></param>
    public void SetAggro(GameObject _aggroObject, float _value)
    {
        if (_aggroObject == null)
        {
            return;
        }
        
        // 어그로 수치는 0과 100사이로 제한
        float curValue = 0f;
        attackTargetsList.TryGetValue(_aggroObject, out curValue);
        float newValue = Mathf.Clamp(Mathf.Max(curValue, _value), 0f, 100f);

        if (newValue <= 0f)
        {
            attackTargetsList.Remove(_aggroObject);
        }
        else
        {
            attackTargetsList[_aggroObject] = newValue;
        }

        // 어그로 대상 갱신 메서드
        UpdateAggroTarget();
    }
    
    /// <summary>
    /// 전체 어그로 수치를 감소시키는 메서드
    /// 코루틴에서 호출해 어그로 수치를 주기적으로 감소
    /// </summary>
    /// <param name="_amount"></param>
    public void DecreaseAllAggro(float _amount)
    {
        var keys = new List<GameObject>(attackTargetsList.Keys);

        foreach (var key in keys)
        {
            ModifyAggro(key, -_amount);
        }
    }

    /// <summary>
    /// 전체 어그로 수치 초기화
    /// </summary>
    public void Clear()
    {
        attackTargetsList.Clear();
        currentTarget = null;
        currentTargetValue = 0f;
    }
    
    // 공격 타겟 업데이트
    // 딕셔너리 내 가장 높은 어그로 수치를 가진 오브젝트를 AttackTarget으로 설정
    /// <summary>
    /// 어그로가 가장 높은 대상을 찾아 타겟을 갱신하는 메서드
    /// 죽었거나 Key값이 null인 대상은 제거
    /// 변경 최소 시간에 따라 제한 가능
    /// 타겟 변경 시 이벤트 호출
    /// </summary>
    private void UpdateAggroTarget()
    {
        GameObject maxValueTarget = null;
        float maxValue = 0f;
        
        toRemove.Clear();
        
        // 대상 딕셔너리를 순회하며 key 값이 존재하는지 확인
        foreach (var target in attackTargetsList)
        {
            // 대상이 null이면 삭제 리스트에 추가
            if (target.Key == null)
            {
                toRemove.Add(target.Key);
                continue;
            }
    
            // 대상이 IDamageable이고, 사망한 상태라면 삭제 리스트에 추가
            if (target.Key.TryGetComponent<IDamageable>(out var IDamageable))
            {
                if (IDamageable.IsDead)
                {
                    toRemove.Add(target.Key);
                    continue;
                }
            }
            
            // 대상이 플레이어일 경우, 인식 범위 밖으로 나가면 후보에서 제외
            if (isPlayerInSenseRange == null || !isPlayerInSenseRange(target.Key))
            {
                continue;
            }

            // 현재까지 최고 value값으로 어그로 타겟, 수치 갱신
            // 동일한 값을 가지면 마지막으로 순회한 대상을 타겟으로 설정
            if (target.Value >= maxValue)
            {
                maxValueTarget = target.Key;    
                maxValue = target.Value;
            }
        }

        // 삭제 리스트에 있는 대상을 모두 제거
        foreach (var target in toRemove)
        {
            attackTargetsList.Remove(target);
        }
        
        // 전환 여부 판단, 타겟 설정
        bool changed = false;
        float now = Time.time;
        float timeSinceSwitch = now - lastTargetChangeTime;
        
        bool shouldSwitch =
            // 타겟이 없을 경우
            currentTarget == null ||
            // 현재 타겟과 같을 경우
            currentTarget == maxValueTarget ||
            // 타겟 유지 시간 이상 지나고, 새로운 타겟의 어그로 수치가 더 높을 경우
            (timeSinceSwitch >= stickTime &&  maxValue >= currentTargetValue);

        // 타겟이 없거나 최대 어그로 수치가 0이하인 경우
        if (maxValueTarget == null || maxValue <= 0f)
        {
            if (currentTarget != null)
            {
                // 타겟 초기화(제거)
                currentTarget = null;
                currentTargetValue = 0f;
                changed = true;
            }
        }
        // 전환 조건을 만족해 새로운 타겟으로 변경
        else if (shouldSwitch)
        {
            if (currentTarget != maxValueTarget)
            {
                changed = true;
            }
            currentTarget = maxValueTarget;
            currentTargetValue = maxValue;
            lastTargetChangeTime = now;
        }
        // 전환 조건을 만족하지 못한 경우
        else
        {
            if (currentTarget != null && attackTargetsList.TryGetValue(currentTarget, out float value))
            {
                currentTargetValue = value;
            }
            // 기존 타겟이 없어진 경우
            else
            {
                currentTarget = maxValueTarget;
                currentTargetValue = maxValue;
                lastTargetChangeTime = now;
                changed = true;
            }
        }

        if (changed && OnTargetChanged != null)
        {
            OnTargetChanged(currentTarget, currentTargetValue);
        }
    }

}
