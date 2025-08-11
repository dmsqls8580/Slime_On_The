using PlayerStates;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public enum ObjectType
{
    UnDestroyed= -1,
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

public abstract class BaseInteractableObject : MonoBehaviour, IInteractable
{
    [Header("Drop Item Data Info(SO, 개수, 확률)")]
    [Header("Drop Item Data")]
    [SerializeField] protected List<DropItemData> dropItemsOnFirstBreak; // 나무 → 밑둥
    [SerializeField] protected List<DropItemData> dropItemsOnFinalBreak; // 밑둥 파괴 시

    [SerializeField] protected ObjectType objectType;

    [SerializeField] protected GameObject dropItemPrefab; //DropItem 스크립트가 붙은 빈 오브젝트 프리팹

    [Header("Drop Animation")]
    [SerializeField] protected float dropUpForce = 5f;

    [SerializeField] protected float dropSideForce = 2f;
    [SerializeField] protected float dropAngleRange = 60f;
    
    [Header("Resource Health")]
    [SerializeField] protected float maxHealth;

    protected float currentHealth;
    public float CurrentHealth => currentHealth;

    protected Rigidbody2D rigid;
    
    protected bool isInteracted;
    public bool IsInteracted => isInteracted;
    public ObjectType ObjectType => objectType;
    
    protected virtual void Awake()
    {
        currentHealth = maxHealth;
    }
    
    public abstract void Interact(InteractionCommandType _type, PlayerController _playerController);

    public virtual ToolType GetRequiredToolType()
    {
        return objectType switch
        {
            ObjectType.Tree => ToolType.Axe,
            ObjectType.Ore => ToolType.Pickaxe,
            ObjectType.Placed => ToolType.Hammer,
            ObjectType.UnDestroyed => ToolType.None,
        };
    }

    protected virtual void DropItems(Transform _player)
    {
        DropItems(_player, dropItemsOnFinalBreak);
    }

    protected virtual void DropItems(Transform _player, List<DropItemData> itemList)
    {
        if (itemList.IsUnityNull() || dropItemPrefab.IsUnityNull()) return;

        foreach (var item in itemList)
        {
            for (int i = 0; i < item.amount; i++)
            {
                float chance = Random.value * 100f;
                if (chance > item.dropChance) continue;

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

    protected void TakeInteraction(float _damage)
    {
        currentHealth -= _damage;
        currentHealth = Mathf.Max(currentHealth, 0);
    }
}