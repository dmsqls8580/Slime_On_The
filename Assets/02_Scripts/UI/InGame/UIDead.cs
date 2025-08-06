using _02_Scripts.Manager;
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class UIDead : UIBase
{
    [SerializeField] private Volume deathVolume;
    [SerializeField] private TextMeshProUGUI dayText;
    [SerializeField] private TextMeshProUGUI reasonText;

    private bool isDead = false;


    private void Update()
    {  
        if (isDead && Input.GetMouseButtonDown(0))
        {
            LoadDeathScene();
        }
    }

    public void TriggerDeath(int _days, string _reason)
    {
        isDead = true;
        dayText.text = $"{_days}일 생존";
        reasonText.text = $"사인 : {_reason}";
        Open();
        Time.timeScale = 0f;
        deathVolume.weight = 1f;
        
        InputController.Instance.SetEnable(true);
    }

    private void LoadDeathScene()
    {
        Time.timeScale = 1f; // Time.timeScale 복구
        UIManager.Instance.CloseAll();
        SceneManager.LoadScene("01_Scenes/MainMenuScene"); // 여기에 이동할 씬 이름 입력
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
