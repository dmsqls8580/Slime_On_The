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
            float curAggroValue = 0f;
            if (enemyController.AttackTargets.TryGetValue(other.gameObject, out curAggroValue))
            {
                enemyController.IsAttacked = true;
            }
            // 인식 범위에 들어가면 어그로 수치 +max(value, 10)
            float aggroValue = Mathf.Max(10f, curAggroValue);
            enemyController.SetAggro(other.gameObject, aggroValue);
            enemyController.StartAggroCoroutine();
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            enemyController.IsAttacked = false;
        }
    }

}
