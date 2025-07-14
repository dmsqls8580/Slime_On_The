using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IngredientSlot : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI amount;
    private Image icon;

    private void Awake()
    {
        icon = GetComponent<Image>();
    }

    public void UpdateIngredientSlot(Sprite _icon, int _having, int _required)
    {
        icon.sprite = _icon;
        // 재료 불충분.
        if (_having < _required)
            amount.color = Color.red;
        amount.text = $"{_having} / {_required}";
    }
}
