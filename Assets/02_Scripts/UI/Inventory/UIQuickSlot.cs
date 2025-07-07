using UnityEngine;
using UnityEngine.InputSystem;

public class UIQuickSlot : MonoBehaviour
{
    [SerializeField] private QuickSlot[] slots;
    private int selectedIndex = 0;

    public int SelectedIndex => selectedIndex;

    private void Start()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].Initialize(i, this);
        }

        UpdateSelectedVisual();
    }

    private void Update()
    {
        HandleNumberInput();
        HandleScrollInput();
    }

    public void SelectSlot(int index)
    {
        if (index < 0 || index >= slots.Length)
            return;

        selectedIndex = index;
        UpdateSelectedVisual();
    }

    private void HandleNumberInput()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame) SelectSlot(0);
        else if (Keyboard.current.digit2Key.wasPressedThisFrame) SelectSlot(1);
        else if (Keyboard.current.digit3Key.wasPressedThisFrame) SelectSlot(2);
        else if (Keyboard.current.digit4Key.wasPressedThisFrame) SelectSlot(3);
        else if (Keyboard.current.digit5Key.wasPressedThisFrame) SelectSlot(4);
        else if (Keyboard.current.digit6Key.wasPressedThisFrame) SelectSlot(5);
        else if (Keyboard.current.digit7Key.wasPressedThisFrame) SelectSlot(6);
        else if (Keyboard.current.digit8Key.wasPressedThisFrame) SelectSlot(7);
        else if (Keyboard.current.digit9Key.wasPressedThisFrame) SelectSlot(8);
        else if (Keyboard.current.digit0Key.wasPressedThisFrame) SelectSlot(9);
    }

    private void HandleScrollInput()
    {
        float scroll = Mouse.current.scroll.ReadValue().y;

        if (scroll > 0f)
            SelectSlot((selectedIndex - 1 + slots.Length) % slots.Length);
        else if (scroll < 0f)
            SelectSlot((selectedIndex + 1) % slots.Length);
    }

    private void UpdateSelectedVisual()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].OnSlotSelectedChanged(i == selectedIndex);
        }
    }
}
