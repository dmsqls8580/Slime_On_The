using UnityEngine;
using UnityEngine.UI;

public class HPBarUpdater : MonoBehaviour
{
    [SerializeField] private Image HPBar;
    [SerializeField] private PlayerStatus playerStatus;

    private void Start()
    {
        if (playerStatus == null)
        {
            playerStatus = FindObjectOfType<PlayerStatus>();
        }
        
        if (playerStatus != null)
        {
            playerStatus.OnHpChanged += UpdateHPBar;
            UpdateHPBar(playerStatus.MaxHp > 0 ? playerStatus.CurrentHp / playerStatus.MaxHp : 0f);
        }
    }

    private void OnDestroy()
    {
        if (playerStatus != null)
        {
            playerStatus.OnHpChanged -= UpdateHPBar;
        }
    }

    private void UpdateHPBar(float fillAmount)
    {
        HPBar.fillAmount = fillAmount;
    }
}
