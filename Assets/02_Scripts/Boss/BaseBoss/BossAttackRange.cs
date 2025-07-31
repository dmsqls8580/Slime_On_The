using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAttackRange : MonoBehaviour
{
    private IBossController iBossController;
    private CircleCollider2D circleCollider2D;
    
    private void Start()
    {
        iBossController = GetComponentInParent<IBossController>();
        circleCollider2D = GetComponent<CircleCollider2D>();
    }

    private void Update()
    {
        if (iBossController.IsPlayerInAttackRange)
        {
            // Enemy 공격 범위 가시화
            circleCollider2D.enabled = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && other.TryGetComponent(out IDamageable iDamageable))
        {
            iBossController.SetPlayerInAttackRange(true);
            iBossController.AttackTarget = other.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && other.TryGetComponent(out IDamageable iDamageable))
        {
            iBossController.SetPlayerInAttackRange(false);
            iBossController.AttackTarget = null;
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
