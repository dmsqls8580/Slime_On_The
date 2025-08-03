using _02_Scripts.Manager;
using PlayerStates;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public enum CookingState
{
    Idle,
    Cooking,
    Finished
}

public class CookingPot : MonoBehaviour, IInteractable
{
    private InventoryManager inventoryManager;

    private CookingState currentState = CookingState.Idle;
    private ItemSO finishedItem;

    [Header("Drop Item Data Info(SO, 개수, 확률)")]
    [SerializeField] private List<DropItemData> dropItems;

    [SerializeField] private GameObject dropItemPrefab; //DropItem 스크립트가 붙은 빈 오브젝트 프리팹

    [Header("Drop Animation")]
    [SerializeField] private float dropUpForce = 5f;

    [SerializeField] private float dropSideForce = 2f;
    [SerializeField] private float dropAngleRange = 60f;

    [Header("Drop Item Health")]
    [SerializeField] private float maxHealth;

    private float currentHealth;
    private Rigidbody2D rigid;

    private void Awake()
    {
        inventoryManager = InventoryManager.Instance;
        currentHealth = maxHealth;
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

    public void Interact(InteractionCommandType _type, PlayerController _playerController)
    {
        switch (_type)
        {
            case InteractionCommandType.F:
                switch (currentState)
                {
                    case CookingState.Idle:
                        break;
                    case CookingState.Cooking:
                        Logger.Log("요리 중 입니다.");
                        break;
                    case CookingState.Finished:
                        if (inventoryManager.TryAddItemGlobally(finishedItem, 1))
                        {
                            finishedItem = null;
                            currentState = CookingState.Idle;
                        }
                        else { Logger.Log("인벤토리가 가득 찼습니다"); }
                        break;
                }
                break;
            case InteractionCommandType.Space:
                if (currentState == CookingState.Idle)
                {
                    var toolController = _playerController.GetComponent<ToolController>();
                    float toolPower = toolController != null ? toolController.GetAttackPow() : 1f;
                    TakeInteraction(toolPower);

                    if (currentHealth <= 0)
                    {
                        DropItems(_playerController.transform);
                        Destroy(gameObject);
                    }
                }
                break;
        }
    }

    public void TakeInteraction(float _damage)
    {
        currentHealth -= _damage;
        Logger.Log($"{currentHealth}");
        currentHealth = Mathf.Max(currentHealth, 0);
    }

    public void Cook(ItemSO _item, float _cookingTime)
    {
        if (currentState == CookingState.Idle)
        { StartCoroutine(CookRoutine(_item, _cookingTime)); }
    }

    private IEnumerator CookRoutine(ItemSO _item, float _cookingTime)
    {
        // 상태 변경 및 변수 설정.
        currentState = CookingState.Cooking;
        finishedItem = _item;

        // TODO: 애니메이션 재생(?).

        // 요리 시간만큼 대기.
        yield return new WaitForSeconds(_cookingTime);

        // 요리 완료 처리.
        currentState = CookingState.Finished;

        // TODO: 완료 상태 스프라이트로 변경.
    }
}
