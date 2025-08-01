using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System;
using System.Collections;

[System.Serializable]
public class FormData
{
    public int formId;
    public RuntimeAnimatorController animator;
    public Sprite mainSprite;
    public Color color;
    [Header("0 = 좌클, 1 = 우클")]
    public PlayerSkillSO attack0Slot;
    public PlayerSkillSO attack1Slot;
}

public class SlimeFormChanger : MonoBehaviour
{
    [SerializeField] private List<FormData> formDataList;
    [SerializeField] private GameObject uiBlockerPanel;
    [SerializeField] private int defaultFormId = 0;
    [SerializeField] private float changeCooldown;
    
    private PlayerSkillMananger playerSkillManager;
    private SlimeFormChangeEffect changeEffect;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    
    private Dictionary<int, FormData> formDataDic;

    public event Action<FormData> OnFormChanged;
    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>(); 
        changeEffect = GetComponentInChildren<SlimeFormChangeEffect>();
        animator = GetComponent<Animator>();
        playerSkillManager = GetComponent<PlayerSkillMananger>();
        
        formDataDic = new Dictionary<int, FormData>();
        foreach (FormData formData in formDataList)
        {
            formDataDic[formData.formId] = formData;
        }
        
        ChangeForm(defaultFormId);
    }

    public void RequestFormChange(int _formId)
    {
        if (!changeEffect.IsUnityNull())
        {
            uiBlockerPanel.SetActive(true);
            changeEffect.StartFormChangeEffect(() =>
            {
                ChangeForm(_formId);
                StartCoroutine(UnblockUIAfterCooldown());
            });
        }
        else
        {
            ChangeForm(_formId);
        }
    }

    private IEnumerator UnblockUIAfterCooldown()
    {
        yield return new WaitForSeconds(changeCooldown);
        uiBlockerPanel.SetActive(false);
    }

    public void ChangeForm(int _formId)
    {
        if (!formDataDic.TryGetValue(_formId, out var formData))
        {
            return;
        }
        
        animator.runtimeAnimatorController = formData.animator;
        spriteRenderer.color = formData.color;
        if (!spriteRenderer.IsUnityNull() && !formData.mainSprite.IsUnityNull())
        {
            spriteRenderer.sprite = formData.mainSprite;
        }
        
        playerSkillManager.SetSkillSlot(formData.attack0Slot,formData.attack1Slot);
        OnFormChanged?.Invoke(formData);
    }
    public void ResetForm()
    {      
        RequestFormChange(defaultFormId);
    }
}
