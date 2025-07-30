using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossSenseRange : MonoBehaviour
{
    private IBossController iBossController;
    
    private void Start()
    {
        iBossController = GetComponentInParent<IBossController>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            iBossController.ChaseTarget = other.gameObject;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            iBossController.ChaseTarget = other.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            float distance = Vector3.Distance(iBossController.Transform.position, other.transform.position);
            
            // 콜라이더가 겹치면 ChaseTarget이 null이 되는 오류
            // 플레이어가 감지 범위보다 멀리 있어야 null이 되도록 수정
            if (distance > iBossController.BossStatus.SenseRange)
            {
                iBossController.ChaseTarget = null;
            }
        }
    }

}
