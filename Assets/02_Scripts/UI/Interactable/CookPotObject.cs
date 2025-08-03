using _02_Scripts.Manager;
using PlayerStates;
using System;
using UnityEngine;

public class CookPotObject : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject dropItemPrefab;
    [SerializeField] private int cookIndex = -1;
    public int GetCookIndex() => cookIndex;
    private bool hasCooked = false;

    [Header("Drop Animation")]
    [SerializeField] private float dropUpForce = 5f;
    [SerializeField] private float dropSideForce = 2f;
    [SerializeField] private float dropAngleRange = 60f;

    [Header("Health")]
    [SerializeField] private float maxHealth;
    private float currentHealth;
    private Rigidbody2D rigid;
    
    private void Start()
    {
        if (cookIndex < 0)
        {
            cookIndex = InventoryManager.Instance.GetNextAvailableCookIndex();
        }
    }

    // TODO: 요리하는거 동작 추가

    private void OnDisable()
    {
        InventoryManager.Instance.ReleaseCookIndex(cookIndex);
    }

    public void TryCook()
    {
        if (hasCooked) return;
        
        hasCooked = true;
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
                var ui = UIManager.Instance.GetUIComponent<UICookPot>();
                ui.Initialize(cookIndex, this);
                UIManager.Instance.Toggle<UICookPot>();
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
    
    public void TakeInteraction(float damage)
    {
        currentHealth -= damage;
        Logger.Log($"{currentHealth}");
        currentHealth = Mathf.Max(0, currentHealth);
    }

    private void DropItems(Transform _player)
    {
        int inputStart = SlotIndexScheme.GetCookInputStart(cookIndex);
        int resultIndex = SlotIndexScheme.GetCookResultIndex(cookIndex);

        // 드롭 인풋 슬롯
        for (int i = 0; i < SlotIndexScheme.CookInputSlotCount; i++)
        {
            int slotIndex = inputStart + i;
            var data = InventoryManager.Instance.GetItem(slotIndex);
            if (data == null || !data.IsValid) continue;

            for (int j = 0; j < data.Quantity; j++)
            {
                var dropObj = Instantiate(dropItemPrefab, transform.position, Quaternion.identity);
                var itemDrop = dropObj.GetComponent<ItemDrop>();
                if (itemDrop != null)
                {
                    itemDrop.Init(data.ItemData, 1);
                    rigid = dropObj.GetComponent<Rigidbody2D>();
                    itemDrop.DropAnimation(rigid, dropAngleRange, dropUpForce, dropSideForce);
                }
            }

            InventoryManager.Instance.ClearItem(slotIndex);
        }

        // 드롭 결과 슬롯
        var result = InventoryManager.Instance.GetItem(resultIndex);
        if (result != null && result.IsValid)
        {
            for (int j = 0; j < result.Quantity; j++)
            {
                var dropObj = Instantiate(dropItemPrefab, transform.position, Quaternion.identity);
                var itemDrop = dropObj.GetComponent<ItemDrop>();
                if (itemDrop != null)
                {
                    itemDrop.Init(result.ItemData, 1);
                    rigid = dropObj.GetComponent<Rigidbody2D>();
                    itemDrop.DropAnimation(rigid, dropAngleRange, dropUpForce, dropSideForce);
                }
            }

            InventoryManager.Instance.ClearItem(resultIndex);
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
