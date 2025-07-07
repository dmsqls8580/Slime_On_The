using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestBtn : MonoBehaviour
{
    public void OnToggleInventory()
    {
        UIManager.Instance.Toggle<UIInventory>();
    }
    
    public void OnToggleCrafting()
    {
        UIManager.Instance.Toggle<UICrafting>();
    }
}
