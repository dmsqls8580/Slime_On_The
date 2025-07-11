using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMainMenu : MonoBehaviour
{
    public void OnClickStart()
    {
        UIManager.Instance.Toggle<UIStartGame>();
    }

    public void OnClickSettings()
    {
        
    }
    
    public void OnClickQuit()
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
}
