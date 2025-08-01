using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss1SenseRange : MonoBehaviour
{
    private Boss1Controller boss1Controller;
    
    private void Start()
    {
        boss1Controller = GetComponentInParent<Boss1Controller>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            float curAggro = 0f;
            boss1Controller.Aggro.attackTargetsList.TryGetValue(other.gameObject, out curAggro);
            float newAggro = Mathf.Max(curAggro, 20f);
            
            boss1Controller.Aggro.SetAggro(other.gameObject, newAggro);
            if (boss1Controller.aggroCoroutine == null)
            {
                boss1Controller.aggroCoroutine = StartCoroutine(boss1Controller.DecreaseAggroValue());
            }
            boss1Controller.SetPlayerInSenseRange(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            boss1Controller.SetPlayerInSenseRange(false);
        }
    }

}
