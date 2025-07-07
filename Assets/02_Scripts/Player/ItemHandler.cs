using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ItemHandler : MonoBehaviour
{
    [SerializeField] private SpriteRenderer icon;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform itemIconPivot;

    private bool isActive = false;

    private void Awake()
    {
        if (mainCamera.IsUnityNull())
        {
            mainCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (isActive)
            IconRotation();
    }

    public void ShowItemIcon(ItemSO _itemSo)
    {
        Logger.Log($"ShowItemIcon 호출: {_itemSo?.name}, sprite: {_itemSo?.icon}, icon.enabled: {icon.enabled}");
        if (!_itemSo.IsUnityNull() && _itemSo != null)
        {
            icon.sprite = _itemSo.icon;
            icon.enabled = true;
            isActive = true;
            Logger.Log($"아이콘 활성화! sprite={icon.sprite}, enabled={icon.enabled}");
        }
        else
        {
            icon.enabled = false;
            isActive = false;
            Logger.Log("아이콘 비활성화");
        }
        isActive = (_itemSo != null && _itemSo.icon != null);
    }

    public void HideItemIcon()
    {
        icon.sprite = null;
        icon.enabled = false;
        isActive = false;
    }

    private void IconRotation()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(
            new Vector3(mouseScreenPos.x, mouseScreenPos.y, Mathf.Abs(mainCamera.transform.position.z))
        );
        Vector3 dir = mouseWorldPos - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        float spriteOffset = 180f;

        Quaternion targetRot = Quaternion.Euler(0, 0, angle + spriteOffset);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 0.2f);
        
        if (mouseWorldPos.x > transform.position.x)
        {
            icon.flipY = true; 
        }
        else
        {
            icon.flipY = false; 
        }
    }
}