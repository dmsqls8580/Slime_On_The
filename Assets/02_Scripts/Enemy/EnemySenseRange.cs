using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySenseRange : MonoBehaviour
{
    private EnemyController enemyController;
    
    private void Start()
    {
        enemyController = GetComponentInParent<EnemyController>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            enemyController.ChaseTarget = other.gameObject;
            enemyController.ModifyAggro(other.gameObject, 20f);
        }
    }
    
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            enemyController.ChaseTarget = other.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            float distance = Vector3.Distance(enemyController.transform.position, other.transform.position);
            
            // 콜라이더가 겹치면 ChaseTarget이 null이 되는 오류
            // 플레이어가 감지 범위보다 멀리 있어야 null이 되도록 수정
            if (distance > enemyController.EnemyStatus.SenseRange)
            {
                enemyController.ChaseTarget = null;
            }
            
            enemyController.ModifyAggro(other.gameObject, -20f);
        }
    }

}
