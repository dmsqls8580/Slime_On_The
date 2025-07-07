using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIConfirmPopup : UIBase
{
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    
    private UnityAction onConfirm;

    public void Setup(string message, UnityAction onConfirmAction)
    {
        messageText.text = message;
        onConfirm = onConfirmAction;

        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(() =>
        {
            onConfirm?.Invoke();
            Close();
        });

        cancelButton.onClick.RemoveAllListeners();
        cancelButton.onClick.AddListener(Close);
    }

    public static void Show(string message, UnityAction onConfirmAction)
    {
        UIManager.Instance.Open<UIConfirmPopup>();

        var popup = UIManager.Instance.GetUIComponent<UIConfirmPopup>();
        if (popup != null)
            popup.Setup(message, onConfirmAction);
    }
}
