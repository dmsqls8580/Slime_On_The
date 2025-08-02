
using UnityEngine;
using UnityEngine.UI;

public class UIKeyRebind : MonoBehaviour
{
    [SerializeField] private Button resetButton;
    
    private void Start()
    {
        // Reset: 기본 설정으로 초기화하고 UI도 갱신
        resetButton.onClick.AddListener(() =>
        {
            KeyBindManager.Instance.ResetToDefault();
        });
    }
}
