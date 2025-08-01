using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss2SenseRange : MonoBehaviour
{
    private Boss2Controller boss2Controller;
    
    private void Start()
    {
        boss2Controller = GetComponentInParent<Boss2Controller>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            float curAggro = 0f;
            boss2Controller.Aggro.attackTargetsList.TryGetValue(other.gameObject, out curAggro);
            float newAggro = Mathf.Max(curAggro, 20f);
            
            boss2Controller.Aggro.SetAggro(other.gameObject, newAggro);
            if (boss2Controller.aggroCoroutine == null)
            {
                boss2Controller.aggroCoroutine = StartCoroutine(boss2Controller.DecreaseAggroValue());
            }
            boss2Controller.SetPlayerInSenseRange(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            boss2Controller.SetPlayerInSenseRange(false);
        }
    }
}
