using _02_Scripts.Manager;
using PlayerStates;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum CookingState
{
    Idle,
    Cooking,
    Finishing,
    Finished
}

public class CookPotObject : BaseInteractableObject, IInteractable
{
    [SerializeField] private int cookIndex = -1;
    [SerializeField] private CookingState currentState = CookingState.Idle;
    public ItemSO[] prevItems = new ItemSO[3];
    public ItemSO finishedItem = null;
    private float elapsedTime = 0f;
    private float cookingTime = 0f;
    public float processPercentage = 1f;

    private Coroutine coroutine = null;

    private int inputStart;
    private int inputEnd;

    private InventoryManager inventoryManager;
    private UIManager uiManager;
    private UICookPot uiCookPot;
    
    public int CookIndex => cookIndex;
    public CookingState CurrentState => currentState;
    public void ChangeState(CookingState _state) => currentState = _state;

    protected override void Awake()
    {
        base.Awake();
        inventoryManager = InventoryManager.Instance;
        uiManager = UIManager.Instance;
        uiCookPot = uiManager.GetUIComponent<UICookPot>();
    }

    private void Start()
    {
        if (cookIndex < 0)
        {
            cookIndex = inventoryManager.GetNextAvailableCookIndex();
        }

        inputStart = SlotIndexScheme.GetCookInputStart(cookIndex);
        inputEnd = inputStart + SlotIndexScheme.CookInputSlotCount;
    }

    public override void Interact(InteractionCommandType _type, PlayerController _playerController)
    {
        switch (_type)
        {
            case InteractionCommandType.F:
                var ui = uiManager.GetUIComponent<UICookPot>();
                ui.Initialize(this);
                ui.RefreshTargetImg(cookIndex, finishedItem);
                if (!ui.IsOpen)
                {
                    uiManager.Toggle<UICookPot>();
                    if (!uiManager.GetUIComponent<UIInventory>().IsOpen)
                    {
                        uiManager.Toggle<UIInventory>();
                    }
                }
                else
                {
                    uiManager.Toggle<UICookPot>();
                    if (uiManager.GetUIComponent<UIInventory>().IsOpen)
                    {
                        uiManager.Toggle<UIInventory>();
                    }
                }
                break;
            case InteractionCommandType.Space:
                var toolController = _playerController.GetComponent<ToolController>();
                float toolPower = toolController != null ? toolController.GetAttackPow() : 1f;
                TakeInteraction(toolPower);
                if (currentHealth <= 0)
                {
                    if (uiManager.GetUIComponent<UICookPot>().IsOpen)
                    {
                        uiManager.Toggle<UICookPot>();
                    }
                    StopCook();
                    DropItems(_playerController.transform);
                    inventoryManager.ReleaseCookIndex(cookIndex);
                    Destroy(gameObject);
                }
                break;
        }
    }
    
    public void StartCook(ItemSO _item, float _cookingTime)
    {
        if (inventoryManager.GetItem(inputEnd) != null &&
            inventoryManager.GetItem(inputEnd).ItemData != _item) return;

        if (currentState == CookingState.Cooking) return;
        
        finishedItem = _item;
        cookingTime = _cookingTime;

        uiCookPot.RefreshTargetImg(cookIndex, finishedItem);
        
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
                processPercentage = elapsedTime / cookingTime;
                uiCookPot.RefreshProcessImg(cookIndex, processPercentage);

                yield return null;
            }

            currentState = CookingState.Finishing;

            inventoryManager.TryAddItem(inputEnd, new ItemInstanceData(finishedItem, 1), 1);
            for (int i = inputStart; i < inputEnd; i++)
            {
                inventoryManager.RemoveItem(i, 1);
            }
            
            uiCookPot.RefreshTargetImg(cookIndex, finishedItem);

        } while (CanCook());

        currentState = CookingState.Finished;
    }

    private bool CanCook()
    {
        for (int i = inputStart; i < inputEnd; i++)
        {
            if (inventoryManager.GetItem(i) == null)
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
            uiCookPot.RefreshProcessImg(cookIndex, 1f);
            finishedItem = null;
            uiCookPot.RefreshTargetImg(cookIndex, finishedItem);
        }
    }

    protected override void DropItems(Transform _player)
    {
        base.DropItems(_player);
        
        
        int start = SlotIndexScheme.GetCookInputStart(cookIndex);
        int count = SlotIndexScheme.CookInputSlotCount + 1;
    
        for (int i = 0; i < count; i++)
        {
            int slotIndex = start + i;
            var data = InventoryManager.Instance.GetItem(slotIndex);
            if (data == null || !data.IsValid) continue;
            
            for (int j = 0; j < data.Quantity; j++)
            {
                var dropObj = Instantiate(dropItemPrefab, transform.position, Quaternion.identity);
                var itemDrop = dropObj.GetComponent<ItemDrop>();
                if (itemDrop != null)
                {
                    itemDrop.Init(data.ItemData, 1);
                }
                rigid = dropObj.GetComponent<Rigidbody2D>();
                itemDrop.DropAnimation(rigid, dropAngleRange, dropUpForce, dropSideForce);
            }
            InventoryManager.Instance.ClearItem(slotIndex);
        }
        
    }
}
