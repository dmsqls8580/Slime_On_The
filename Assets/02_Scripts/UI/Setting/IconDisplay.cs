using TMPro;
using UnityEngine;

public class IconDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI keyText;

    private void OnEnable()
    {
        UpdateDisplay();
        KeyBindManager.OnBindingChanged += UpdateDisplay;
    }

    private void OnDisable()
    {
        KeyBindManager.OnBindingChanged -= UpdateDisplay;
    }

    private void UpdateDisplay()
    {
        if (keyText == null) return;

        string bindingStr = KeyBindManager.Instance.GetBindingText(keyText.name);
        keyText.text = bindingStr;
    }
}
