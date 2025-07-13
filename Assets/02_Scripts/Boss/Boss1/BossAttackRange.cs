using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAttackRange : MonoBehaviour
{
    private BossController bossController;
    private CircleCollider2D circleCollider2D;
    
    private void Start()
    {
        bossController = GetComponentInParent<BossController>();
        circleCollider2D = GetComponent<CircleCollider2D>();
    }

    private void Update()
    {
        if (bossController.IsPlayerInAttackRange)
        {
            // Enemy 공격 범위 가시화
            circleCollider2D.enabled = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && other.TryGetComponent(out IDamageable iDamageable))
        {
            bossController.SetPlayerInAttackRange(true);
            bossController.AttackTarget = other.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && other.TryGetComponent(out IDamageable iDamageable))
        {
            bossController.SetPlayerInAttackRange(false);
            bossController.AttackTarget = null;
        }
    }
    
    void OnDrawGizmos()
    {
        if (circleCollider2D == null)
        {
            circleCollider2D = GetComponent<CircleCollider2D>();            
        }
        
        if (circleCollider2D is CircleCollider2D circle)
        {
            Gizmos.color = Color.red;
            Vector3 center = circle.transform.TransformPoint(circle.offset);
            Gizmos.DrawWireSphere(center, circle.radius);
        }
    }

}
