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
            Dictionary<IngredientTag, float> tags = new Dictionary<IngredientTag, float>();
            SumRecipeTags(item.recipe, tags);
            if (IsMatched(_tags, tags))
            {
                _cookingPot.Cook(item, cookingPanel.CookingTime);
                cookingPanel.Initialize();
            }
        }
    }

    private void SumRecipeTags(List<RecipeIngredient> _ingredients, Dictionary<IngredientTag, float> _tags)
    {
        foreach (RecipeIngredient ingredient in _ingredients)
        {
            List<TagValuePair> tags = ingredient.item.cookableData.tags;
            foreach (TagValuePair pair in tags) { _tags[pair.tag] += pair.value; }
        }
    }

    private bool IsMatched(Dictionary<IngredientTag, float> _ingredient, Dictionary<IngredientTag, float> _finished)
    {
        // 완성될 요리가 요구하는 모든 태그(key)에 대해 반복
        foreach (KeyValuePair<IngredientTag, float> pair in _finished)
        {
            IngredientTag tag = pair.Key;
            float value = pair.Value;

            if (!_ingredient.ContainsKey(tag) || _ingredient[tag] < value)
            { return false; }
        }

        // 모든 요구사항을 통과했으면 성공
        return true;
    }
}
