using _02_Scripts.Manager;
using PlayerStates;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CookingState
{
    Idle,
    Cooking,
    Finishing,
    Finished
}

public class CookPotObject : MonoBehaviour, IInteractable
{
    [SerializeField] private int cookIndex = -1;
    private List<InventorySlot> inputSlots;
    private InventorySlot resultSlot;
    [SerializeField] private CookingState currentState = CookingState.Idle;
    private ItemSO finishedItem = null;
    private float elapsedTime = 0f;
    private float cookingTime = 0f;

    private Coroutine coroutine = null;

    private InventoryManager inventoryManager;
    private UIManager uiManager;

    public int CookIndex => cookIndex;
    public CookingState CurrentState => currentState;
    public void ChangeState(CookingState _state) => currentState = _state;

    private void Awake()
    {
        inventoryManager = InventoryManager.Instance;
        uiManager = UIManager.Instance;
    }

    private void OnDisable()
    {
        inventoryManager.ReleaseCookIndex(cookIndex);
    }

    private void Start()
    {
        if (cookIndex < 0)
        {
            cookIndex = inventoryManager.GetNextAvailableCookIndex();
        }
    }

    public void Initialize(List<InventorySlot> _inputSlots, InventorySlot _resultSlot)
    {
        inputSlots = _inputSlots;
        resultSlot = _resultSlot;
    }

    public void Interact(InteractionCommandType _type, PlayerController _playerController)
    {
        switch (_type)
        {
            case InteractionCommandType.F:
                if (!UIManager.Instance.GetUIComponent<UIInventory>().IsOpen)
                {
                    UIManager.Instance.Toggle<UIInventory>();
                }
                var ui = uiManager.GetUIComponent<UICookPot>();
                ui.Initialize(this);
                break;
            case InteractionCommandType.Space:
                break;
        }
    }

    public void StartCook(ItemSO _item, float _cookingTime)
    {
        if (resultSlot.HasItem() && resultSlot.GetData().ItemData != _item) return;

        finishedItem = _item;
        cookingTime = _cookingTime;
        coroutine = StartCoroutine(CookRoutine());
    }

    private IEnumerator CookRoutine()
    {
        do
        {
            currentState = CookingState.Cooking;
            elapsedTime = 0f;

            while (elapsedTime < cookingTime)
            {
                elapsedTime += Time.deltaTime;
                Logger.Log($"{(elapsedTime / cookingTime * 100f):F2}%");

                yield return null;
            }

            currentState = CookingState.Finishing;

            foreach (var slot in inputSlots)
            {
                inventoryManager.RemoveItem(slot.SlotIndex, 1);
            }
            inventoryManager.TryAddItem(resultSlot.SlotIndex, finishedItem, 1);

        } while (CanCook());

        currentState = CookingState.Finished;
    }

    private bool CanCook()
    {
        foreach (var slot in inputSlots)
        {
            if (!slot.HasItem())
            {
                return false;
            }
        }

        return true;
    }

    public void StopCook()
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            currentState = CookingState.Idle;
            elapsedTime = 0f;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var cookUI = UIManager.Instance.GetUIComponent<UICookPot>();
        if (cookUI != null && cookUI.IsOpen && cookUI.CookIndex == cookIndex)
        {
            UIManager.Instance.Close<UICookPot>();
            if (UIManager.Instance.GetUIComponent<UIInventory>().IsOpen)
            {
                UIManager.Instance.Toggle<UIInventory>();
            }
        }
    }
}
