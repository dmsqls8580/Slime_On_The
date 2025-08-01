using _02_Scripts.Manager;
using PlayerStates;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

[System.Serializable]
public enum ObjectType
{
    Tree,
    Ore ,
    Placed
}

[System.Serializable]
public class DropItemData
{
    public ItemSO itemSo; //드랍될 아이템 데이터
    public int amount = 1; // 드랍 개수
    [Range(0f, 100f)] public float dropChance = 100f; //아이템 드랍확률 100%
}

public class DestroyableObject : MonoBehaviour, IInteractable
{
    [Header("Drop Item Data Info(SO, 개수, 확률)")]
    [SerializeField] private List<DropItemData> dropItems;
    [FormerlySerializedAs("resourceType")] [SerializeField] private ObjectType objectType;

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
                    itemDrop.Init(item.itemSo, 1, _player);
                }

                rigid = dropObj.GetComponent<Rigidbody2D>();
                itemDrop.DropAnimation(rigid, dropAngleRange, dropUpForce, dropSideForce);
            }
        }
    }

    public void Interact(InteractionCommandType _type, PlayerController _playerController)
    {
        var toolController = _playerController.GetComponent<ToolController>();
        float toolPower = toolController.IsUnityNull() ? toolController.GetAttackPow() : 1f;
        TakeInteraction(toolPower);

        if (_type == InteractionCommandType.F)
            UIManager.Instance.Toggle<UICrafting>();
        
        if (currentHealth <= 0)
        {
            DropItems(_playerController.transform);
            Destroy(gameObject);
        }
    }
    public ToolType GetRequiredToolType()
    {
        switch (objectType)
        {
            case ObjectType.Tree:
                return ToolType.Axe;
            case ObjectType.Ore:
                return ToolType.Pickaxe;
            case ObjectType.Placed:
                return ToolType.Shovel;
            default:
                return ToolType.None;
        }
    }

    private void TakeInteraction(float _damage)
    {
        currentHealth -= _damage;
        Logger.Log($"{currentHealth}");
        currentHealth = Mathf.Max(currentHealth, 0);
    }
}
