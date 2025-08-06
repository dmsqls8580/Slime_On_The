using _02_Scripts.Manager;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIPauseMenu : UIBase
{
    [SerializeField] private Button blockPanel;

    private void Awake()
    {
        if (blockPanel != null)
        {
            blockPanel.onClick.AddListener(() => UIManager.Instance.Toggle<UIPauseMenu>());
        };
    }

    // 계속하기
    public void OnClickResume()
    {
        UIManager.Instance.Toggle<UIPauseMenu>();
    }
    
    // 설정
    public void OnClickSettings()
    { 
        UIManager.Instance.Toggle<UISettings>();
    }
    
    // 타이틀
    public void OnClickTitle()
    {
        UIConfirmPopup.Show("타이틀로\n돌아갈꺼예요?", () =>
        {
            // 저장기능?
            
            // 타이틀 씬으로 돌아가기
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenuScene");
        });
    }
    
    // 종료하기
    public void OnClickExit()
    {
        UIConfirmPopup.Show("게임을 종료할꺼예요?", () =>
        {
            // 저장기능?
            
            // 종료하기
            #if UNITY_EDITOR    
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        });
    }
    
    public override void Open()
    {
        base.Open();
        Time.timeScale = 0;
    }

    public override void Close()
    {
        Time.timeScale = 1;
        base.Close();
    }
}
