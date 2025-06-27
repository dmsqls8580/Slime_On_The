using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAttackable
{
    public IDamageable Target     { get; }
    public void        Attack();
}
