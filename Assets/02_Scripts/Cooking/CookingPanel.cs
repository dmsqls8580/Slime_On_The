using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CookingPanel : MonoBehaviour
{
    [SerializeField] private CookingManager cookingManager;
    [SerializeField] private List<CookingSlot> cookingSlots;
    [SerializeField] private Button button;

    private Dictionary<IngredientTag, float> tags = new Dictionary<IngredientTag, float>();
    private CookingPot cookingPot;

    private float cookingTime = 0f;
    public float CookingTime => cookingTime;

    private bool canCook = false;

    private void Awake()
    {
        button.onClick.AddListener(OnClickCookButton);
    }

    private void OnClickCookButton()
    {
        if (!canCook) { return; }

        foreach (CookingSlot slot in cookingSlots)
        {
            List<TagValuePair> _tags = slot.Item.cookableData.tags;
            foreach (TagValuePair pair in _tags) { tags[pair.tag] += pair.value; }
            cookingTime += slot.Item.cookableData.cookingTime;
        }
        cookingManager.FindMatchingRecipe(tags, cookingPot);
    }

    public void Initialize()
    {
        tags.Clear();
        cookingTime = 0f;
        foreach (CookingSlot slot in cookingSlots)
        { slot.Initialize(); }
        gameObject.SetActive(false);
    }

    public void Toggle(CookingPot _cookingPot, bool _activeSelf)
    {
        if (_activeSelf)
        {
            // 슬롯에 있는 템들 다시 인벤토리로 옮기면서 초기화.
            gameObject.SetActive(!_activeSelf);
        }
        else
        {
            gameObject.SetActive(!_activeSelf);
            cookingPot = _cookingPot;
        }
    }

    public void CanCook()
    {
        foreach (CookingSlot slot in cookingSlots)
        { if (slot.Item == null) { return; } }
        canCook = true;
    }
}
