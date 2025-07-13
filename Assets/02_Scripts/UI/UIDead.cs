using _02_Scripts.Manager;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering.Universal;

public class UIDead : UIBase
{
    [SerializeField] private Volume deathVolume;
    [SerializeField] private TextMeshProUGUI dayText;
    [SerializeField] private TextMeshProUGUI reasonText;

    private bool isDead = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) && !isDead)
        {
            TriggerDeath(1, "테스트당함.");
        }
    }

    public void TriggerDeath(int days, string reason)
    {
        isDead = true;
        Time.timeScale = 0f;
        deathVolume.weight = 1f;
        
        dayText.text = $"{days}일 생존";
        reasonText.text = $"사인 : {reason}";
        UIManager.Instance.Toggle<UIDead>();
    }
    
    public override void Open()
    {
        base.Open();
    }

    public override void Close()
    {
        base.Close();
    }
}
