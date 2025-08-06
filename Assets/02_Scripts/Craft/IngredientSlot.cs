using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IngredientSlot : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI amount;
    
    

    public void UpdateIngredientSlot(Sprite _icon, int _having, int _required)
    {
        icon.sprite = _icon;
        // 재료 불충분.
        if (_having < _required)
        {
            amount.color = new Color(0.8f, 0.3f, 0.3f);
        }
        else
        {
            amount.color = new Color(0.3f, 0.8f, 0.3f);
        }
            
        amount.text = $"{_having}/{_required}";
    }
}
