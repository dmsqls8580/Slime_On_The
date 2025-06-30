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
        if (Keyboard.current.digit1Key.wasPressedThisFrame) { selectedIndex = 0; }
        else if (Keyboard.current.digit2Key.wasPressedThisFrame) { selectedIndex = 1; }
        else if (Keyboard.current.digit3Key.wasPressedThisFrame) { selectedIndex = 2; }
        else if (Keyboard.current.digit4Key.wasPressedThisFrame) { selectedIndex = 3; }
        else if (Keyboard.current.digit5Key.wasPressedThisFrame) { selectedIndex = 4; }
        else if (Keyboard.current.digit6Key.wasPressedThisFrame) { selectedIndex = 5; }
        else if (Keyboard.current.digit7Key.wasPressedThisFrame) { selectedIndex = 6; }
        else if (Keyboard.current.digit8Key.wasPressedThisFrame) { selectedIndex = 7; }
        else if (Keyboard.current.digit9Key.wasPressedThisFrame) { selectedIndex = 8; }
        else if (Keyboard.current.digit0Key.wasPressedThisFrame) { selectedIndex = 9; }
    
        UpdateSelectedVisual();
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
