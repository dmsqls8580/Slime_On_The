using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttackRange : MonoBehaviour
{
    private EnemyController enemyController;
    private CircleCollider2D circleCollider2D;
    
    private void Start()
    {
        enemyController = GetComponentInParent<EnemyController>();
        circleCollider2D = GetComponent<CircleCollider2D>();
    }
    
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out IDamageable iDamageable))
        {
            enemyController.SetIDamageableInAttackRange(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out IDamageable iDamageable))
        {
            enemyController.SetIDamageableInAttackRange(false);
        }
    }
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (circleCollider2D == null)
        {
            circleCollider2D = GetComponent<CircleCollider2D>();            
        }
        
        if (circleCollider2D is CircleCollider2D circle)
        {
            Gizmos.color = Color.yellow;
            Vector3 center = circle.transform.TransformPoint(circle.offset);
            Gizmos.DrawWireSphere(center, circle.radius);
        }
    }
#endif

}
