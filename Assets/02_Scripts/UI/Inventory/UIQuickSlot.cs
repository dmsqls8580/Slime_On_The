using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIQuickSlot : MonoBehaviour
{
    [SerializeField] private QuickSlot[] slots;
    private int selectedIndex = 0;

    private void Start()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].Initialize(i); // 0~9 인덱스는 공유 영역
        }
        
        UpdateSelectedVisual();
    }
    
    private void Update()
    {
        HandleNumberInput();
        HandleScrollInput();
    }

    private void HandleNumberInput()
    {
        for (int i = 0; i <= 9; i++)
        {
            Key key = (Key)((int)Key.Digit0 + i);

            if (Keyboard.current[key].wasPressedThisFrame)
            {
                selectedIndex = i == 0 ? 9 : i - 1; // 0 → 9번 슬롯, 1 → 0번 슬롯
                UpdateSelectedVisual();
                break;
            }
        }
    }
    
    private void HandleScrollInput()
    {
        float scroll = Mouse.current.scroll.ReadValue().y;

        if (scroll > 0f)
            selectedIndex = (selectedIndex - 1 + slots.Length) % slots.Length;
        else if (scroll < 0f)
            selectedIndex = (selectedIndex + 1) % slots.Length;

        if (scroll != 0)
            UpdateSelectedVisual();
    }

    private void UpdateSelectedVisual()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].SetSelected(i == selectedIndex);
        }
    }
}
