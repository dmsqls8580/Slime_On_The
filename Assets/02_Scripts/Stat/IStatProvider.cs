using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IStatProvider
{
    List<StatData> Stats { get; }
}
