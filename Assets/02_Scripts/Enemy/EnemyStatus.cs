using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStatus : BaseStatus
{
    [SerializeField] private EnemySO enemySO;
    
    private void Start()
    {
        ChangeHealth(MaxHealth.FinalValue);
    }

    private void SetEnemySOData()
    {
        
    }
}
