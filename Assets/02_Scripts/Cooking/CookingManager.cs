using System.Collections.Generic;
using UnityEngine;

public class CookingManager : MonoBehaviour
{
    [SerializeField] private RecipeDatabase database;
    [SerializeField] private CookingPanel cookingPanel;

    public void FindMatchingRecipe(Dictionary<IngredientTag, float> _tags, CookingPot _cookingPot)
    {
        List<ItemSO> items = database.Items;
        foreach (ItemSO item in items)
        {
            if (IsMatched(_tags, item.cookedData))
            {
                _cookingPot.Cook(item, cookingPanel.CookingTime);
                cookingPanel.Initialize();
                return;
            }
        }
    }

    private bool IsMatched(Dictionary<IngredientTag, float> _tags, CookedData _data)
    {
        // 이 레시피의 필수 조건들을 모두 만족하는지 확인.
        foreach (TagValuePair pair in _data.requiredTags)
        {
            // _tags가 해당 태그를 가지고 있지 않거나, 값이 부족하다면.
            if (!_tags.ContainsKey(pair.tag) || _tags[pair.tag] < pair.value) { return false; }
        }

        // 이 레시피의 금지 조건을 위반하는지 확인.
        foreach (TagValuePair pair in _data.forbiddenTags)
        {
            // _tags가 금지된 태그를 value 이상 가지고 있다면.
            if (_tags.ContainsKey(pair.tag) && _tags[pair.tag] >= pair.value) { return false; }
        }

        return true;
    }
}
