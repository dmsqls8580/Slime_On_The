using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class HPBarUpdater : MonoBehaviour
{
    [SerializeField] private Image HPBar;
    [FormerlySerializedAs("playerStatus")] [SerializeField] private PlayerStatusManager playerStatusManager;

    private void Start()
    {
        if (playerStatusManager == null)
        {
            playerStatusManager = FindObjectOfType<PlayerStatusManager>();
        }
        
        if (playerStatusManager != null)
        {
            playerStatusManager.OnHpChanged += UpdateHPBar;
            UpdateHPBar(playerStatusManager.MaxHp > 0 ? playerStatusManager.CurrentHp / playerStatusManager.MaxHp : 0f);
        }
    }

    private void OnDestroy()
    {
        if (playerStatusManager != null)
        {
            playerStatusManager.OnHpChanged -= UpdateHPBar;
        }
    }

    private void UpdateHPBar(float _fillAmount)
    {
        HPBar.fillAmount = _fillAmount;
    }
}
