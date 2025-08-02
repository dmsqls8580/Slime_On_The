using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIRebindButton : MonoBehaviour
{
    [SerializeField] private Button rebindBtn;
    [SerializeField] private string bindingName;

    private void OnEnable()
    {
        rebindBtn.onClick.RemoveAllListeners();
        rebindBtn.onClick.AddListener(() =>
        {
            KeyBindManager.Instance.StartRebind(bindingName);
        });
    }
}