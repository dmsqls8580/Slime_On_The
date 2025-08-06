using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss2AttackRange : MonoBehaviour
{
    private Boss2Controller boss2Controller;
    private CircleCollider2D circleCollider2D;
    
    private void Start()
    {
        boss2Controller = GetComponentInParent<Boss2Controller>();
        circleCollider2D = GetComponent<CircleCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out IDamageable iDamageable))
        {
            boss2Controller.SetPlayerInAttackRange(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out IDamageable iDamageable))
        {
            boss2Controller.SetPlayerInAttackRange(false);
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
