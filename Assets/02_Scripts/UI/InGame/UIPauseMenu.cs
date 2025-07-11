using UnityEngine;
using UnityEngine.SceneManagement;

public class UIPauseMenu : UIBase
{
    // 계속하기
    public void OnClickResume()
    {
        UIManager.Instance.Toggle<UIPauseMenu>();
    }
    
    // 설정
    public void OnClickSettings()
    { 
        // Setting UI추가로 열기
    }
    
    // 타이틀
    public void OnClickTitle()
    {
        UIConfirmPopup.Show("타이틀로 돌아갈꺼예요?", () =>
        {
            // 저장기능?
            
            // 타이틀 씬으로 돌아가기
            Time.timeScale = 1f;
            SceneManager.LoadScene("TempTitleScene");
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
