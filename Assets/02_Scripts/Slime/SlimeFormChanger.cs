using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

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
    private PlayerSkillMananger playerSkillManager;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    
    [SerializeField] private List<FormData> formDataList;
    [SerializeField] private int defaultFormId = 0;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        playerSkillManager = GetComponent<PlayerSkillMananger>();
        ResetForm();
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            TestFormChange();
        }
    }

    public void ChangeForm(int _formId)
    {
        var formData = formDataList.Find(x => x.formId == _formId);
        if(formData.IsUnityNull()) return;
        
        animator.runtimeAnimatorController = formData.animator;
        spriteRenderer.color = formData.color;
        if (!spriteRenderer.IsUnityNull() && !formData.mainSprite.IsUnityNull())
        {
            spriteRenderer.sprite = formData.mainSprite;
        }
        
        playerSkillManager.SetSkillSlot(formData.attack0Slot,formData.attack1Slot);
    }

    private void TestFormChange()
    {
        ChangeForm(1);
    }
    
    public void ResetForm()
    {      
        ChangeForm(defaultFormId);
    }
}
