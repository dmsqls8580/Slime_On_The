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
            // 인식 범위에 들어가면 어그로 수치 +max(value, 20)
            float curAggro = 0f;
            enemyController.Aggro.attackTargetsList.TryGetValue(other.gameObject, out curAggro);
            float newAggro = Mathf.Max(curAggro, 20f);
            
            enemyController.Aggro.SetAggro(other.gameObject, newAggro);
            if (enemyController.aggroCoroutine == null)
            {
                enemyController.aggroCoroutine = StartCoroutine(enemyController.DecreaseAggroValue());
            }
            enemyController.SetPlayerInSenseRange(true);
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            enemyController.SetPlayerInSenseRange(false);
        }
    }

}
