//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;

//public class CookingPanel : MonoBehaviour
//{
//    [SerializeField] private CookingManager cookingManager;
//    //[SerializeField] private List<CookingSlot> cookingSlots;
//    [SerializeField] private Button button;

//    //private CookingPot cookingPot;

//    private bool canCook = false;

//    private void OnClickCookButton()
//    {
//        if (!canCook) { return; }

//        Dictionary<IngredientTag, float> tags = new Dictionary<IngredientTag, float>();
//        float cookingTime = 0f;

//        // 각 슬롯을 순회하며 태그와 시간을 계산.
//        foreach (CookingSlot slot in cookingSlots)
//        {
//            if (slot.Item == null || slot.Item.cookableData == null) continue;

//            // 태그 합산.
//            List<TagValuePair> _tags = slot.Item.cookableData.tags;
//            foreach (TagValuePair pair in _tags)
//            {
//                tags.TryGetValue(pair.tag, out float currentValue);
//                tags[pair.tag] = currentValue + pair.value;
//            }

//            // 시간 합산.
//            cookingTime += slot.Item.cookableData.contributionTime;
//        }
//        //cookingManager.FindMatchingRecipe(tags, cookingTime, cookingPot);
//    }

//    public void Initialize()
//    {
//        foreach (CookingSlot slot in cookingSlots)
//        { slot.Initialize(); }
//        gameObject.SetActive(false);
//    }

//    //public void Toggle(CookingPot _cookingPot, bool _activeSelf)
//    //{
//    //    if (_activeSelf)
//    //    {
//    //        // 슬롯에 있는 템들 다시 인벤토리로 옮기면서 초기화.
//    //        gameObject.SetActive(!_activeSelf);
//    //    }
//    //    else
//    //    {
//    //        gameObject.SetActive(!_activeSelf);
//    //        cookingPot = _cookingPot;
//    //    }
//    //}

//    public void CanCook()
//    {
//        foreach (CookingSlot slot in cookingSlots)
//        { if (slot.Item == null) { return; } }
//        canCook = true;
//    }
//}
