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

public class CookPotObject : MonoBehaviour, IInteractable
{
    [SerializeField] private int cookIndex = -1;
    private List<InventorySlot> inputSlots;
    private InventorySlot resultSlot;
    [SerializeField] private CookingState currentState = CookingState.Idle;
    private ItemSO finishedItem = null;
    private float elapsedTime = 0f;
    private float cookingTime = 0f;
    public float processPercentage = 1f;

    private Coroutine coroutine = null;

    private InventoryManager inventoryManager;
    private UIManager uiManager;
    private UICookPot uiCookPot;

    [Header("Drop Item Health")]
    [SerializeField] private float maxHealth;
    private float currentHealth;
    private Rigidbody2D rigid;

    [Header("Drop Animation")]
    [SerializeField] private float dropUpForce = 5f;
    [SerializeField] private float dropSideForce = 2f;
    [SerializeField] private float dropAngleRange = 60f;

    [Header("Drop Item Data Info(SO, 개수, 확률)")]
    [SerializeField] protected List<DropItemData> dropItems;
    [SerializeField] protected GameObject dropItemPrefab;

    public int CookIndex => cookIndex;
    public CookingState CurrentState => currentState;
    public void ChangeState(CookingState _state) => currentState = _state;

    private void Awake()
    {
        inventoryManager = InventoryManager.Instance;
        uiManager = UIManager.Instance;
        uiCookPot = uiManager.GetUIComponent<UICookPot>();
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
                if (!uiManager.GetUIComponent<UIInventory>().IsOpen)
                {
                    uiManager.Toggle<UIInventory>();
                }
                break;
            case InteractionCommandType.Space:
                var toolController = _playerController.GetComponent<ToolController>();
                float toolPower = toolController != null ? toolController.GetAttackPow() : 1f;
                TakeInteraction(toolPower);

                if (currentHealth <= 0)
                {
                    DropItems(_playerController.transform);
                    InventoryManager.Instance.ReleaseCookIndex(cookIndex);
                    Destroy(gameObject);
                }
                break;
        }
    }

    private void TakeInteraction(float _damage)
    {
        currentHealth -= _damage;
        Logger.Log($"{currentHealth}");
        currentHealth = Mathf.Max(currentHealth, 0);
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
                processPercentage = elapsedTime / cookingTime;
                uiCookPot.RefreshProcessImg(processPercentage);
                Logger.Log($"{(elapsedTime / cookingTime * 100f):F2}%");

                yield return null;
            }

            currentState = CookingState.Finishing;

            foreach (var slot in inputSlots)
            {
                inventoryManager.RemoveItem(slot.SlotIndex, 1);
            }
            inventoryManager.TryAddItem(SlotIndexScheme.GetCookResultIndex(cookIndex), new ItemInstanceData(finishedItem, 1), 1);

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

    private void DropItems(Transform _player)
    {
        float randomChance = Random.value;

        if (dropItems.IsUnityNull() || dropItemPrefab.IsUnityNull())
        {
            return;
        }

        foreach (var item in dropItems)
        {
            for (int i = 0; i < item.amount; i++)
            {
                if (randomChance * 100f > item.dropChance)
                {
                    continue;
                }

                var dropObj = Instantiate(dropItemPrefab, transform.position, Quaternion.identity);
                var itemDrop = dropObj.GetComponent<ItemDrop>();
                if (itemDrop != null)
                {
                    itemDrop.Init(item.itemSo, 1);
                }

                rigid = dropObj.GetComponent<Rigidbody2D>();
                itemDrop.DropAnimation(rigid, dropAngleRange, dropUpForce, dropSideForce);
            }
        }
    }
}
