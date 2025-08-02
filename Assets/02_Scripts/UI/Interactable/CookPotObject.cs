using _02_Scripts.Manager;
using PlayerStates;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CookingState
{
    Idle,
    Cooking,
    Finished
}

public class CookPotObject : MonoBehaviour, IInteractable
{
    [Header("저장 데이터")]
    [SerializeField] private int cookIndex = -1;
    private List<InventorySlot> inputSlots;
    private InventorySlot resultSlot;
    private CookingState currentState = CookingState.Idle;
    private float currentTime = 0f;
    private float cookingTime = 0f;
    private ItemSO finishedItem = null;

    private InventoryManager inventoryManager;
    private UIManager uiManager;

    public int CookIndex => cookIndex;

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
                var ui = uiManager.GetUIComponent<UICookPot>();
                ui.Initialize(this);
                uiManager.Toggle<UICookPot>();
                break;
            case InteractionCommandType.Space:
                break;
        }
    }

    public void StartCook(ItemSO _item, float _cookingTime)
    {
        if (currentState == CookingState.Idle)
        {
            currentState = CookingState.Cooking;
            cookingTime = _cookingTime;
            finishedItem = _item;
            StartCoroutine(CookRoutine());
        }
    }

    private IEnumerator CookRoutine()
    {
        do
        {
            currentTime = 0;
            while (currentTime < cookingTime)
            {
                currentTime += Time.deltaTime;
                yield return null;
            }

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
}
