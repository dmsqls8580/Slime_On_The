using _02_Scripts.Manager;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIStartGame : UIBase
{
    [SerializeField] private Button blockPanel;

    private void Awake()
    {
        if (blockPanel != null)
        {
            blockPanel.onClick.AddListener(() => UIManager.Instance.Toggle<UIStartGame>());
        };
    }
    
    [SerializeField] private AnimationCurve JellyAnimationCurve;
    [SerializeField] private RectTransform content;

    public void OnClickCreateSave()
    {
        GameSettings.seed = 0; // 시드 초기화
        //UIManager.Instance.Toggle<UIWorldSetting>();
        UIManager.Instance.CloseAll();
        SceneManager.LoadScene("InGameScene");
    }
    public void OnClickCancel()
    {
        UIManager.Instance.Toggle<UIStartGame>();
    }
    
    
    public override void Open()
    {
        base.Open();
        content.localScale = Vector3.zero;
        content.DOScale(Vector3.one, 0.3f).SetEase(JellyAnimationCurve);
    }

    public override void Close()
    {
        base.Close();
    }
}
