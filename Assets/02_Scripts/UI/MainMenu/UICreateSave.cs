using _02_Scripts.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UICreateSave : UIBase
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void OnClickApply()
    {
        // 맵생성단계로 넘어가야함
        SceneManager.LoadScene("TestPlayerWorldScene");
    }
    
    public void OnClickCancel()
    {
        UIManager.Instance.Toggle<UICreateSave>();
    }
}
