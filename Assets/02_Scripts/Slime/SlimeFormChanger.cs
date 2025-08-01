using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System;

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
    [SerializeField] private int defaultFormId = 0;
    
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
            changeEffect.StartFormChangeEffect(() =>
            {
                ChangeForm(_formId);
            });
        }
        else
        {
            ChangeForm(_formId);
        }
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
