using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering.Universal;

public class DeathTest : MonoBehaviour
{
    [SerializeField] private Volume deathVolume;
    //[SerializeField] private GameObject deathPanel;
    //[SerializeField] private TextMeshProUGUI reasonText;
    //[SerializeField] private TextMeshProUGUI dayText;

    private bool isDead = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) && !isDead)
        {
            TriggerDeath("죽음 ㅠㅠ", 1);
        }
    }

    public void TriggerDeath(string reason, int days)
    {
        isDead = true;
        Time.timeScale = 0f;
        deathVolume.weight = 1f;
        if (deathVolume.profile.TryGet(out ColorAdjustments ca))
        {
            ca.saturation.overrideState = true;
            ca.saturation.value = -100;
        }
        
        //deathPanel.SetActive(true);
        //reasonText.text = $"사인: {reason}";
        //dayText.text = $"{days}일 생존";
    }
}