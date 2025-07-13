using TMPro;
using UnityEngine;

public class IngredientSlot : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI amount;

    public void UpdateIngredientSlot(int _having, int _required)
    {
        // 재료 불충분.
        if (_having < _required)
            amount.color = Color.red;
        amount.text = $"{_having} / {_required}";
    }
}
