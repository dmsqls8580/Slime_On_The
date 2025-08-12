using _02_Scripts.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using DG.Tweening;

public class UIConfirmPopup : UIBase
{
    [SerializeField] private Button blockPanel;

    private void Awake()
    {
        if (blockPanel != null)
        {
            blockPanel.onClick.AddListener(() => UIManager.Instance.Toggle<UIConfirmPopup>());
        };
    }
    
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private AnimationCurve JellyAnimationCurve;
    
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
    
    public override void Open()
    {
        base.Open();
        Contents.localScale = Vector3.zero;
        Contents.DOScale(Vector3.one, 0.3f).SetEase(JellyAnimationCurve).SetUpdate(true);
    }

    public override void Close()
    {
        base.Close();
    }
}
