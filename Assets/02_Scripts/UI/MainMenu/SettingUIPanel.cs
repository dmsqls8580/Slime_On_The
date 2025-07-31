using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingUIPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI weightText;
    [SerializeField] private Image lessBtn;
    [SerializeField] private Image moreBtn;

    private SettingWeight boundWeight;
    private SettingSO boundSetting;

    public void Initialize(SettingSO setting)
    {
        boundSetting = setting;
        nameText.text = boundSetting.displayName;
        iconImage.sprite = boundSetting.icon;
        weightText.text = boundSetting.weight.ToString();
    }

    public void OnClickLessBtn()
    {
        if (boundSetting.weight > SettingWeight.None)
            boundSetting.weight -= 1;
        UpdateUI();
    }

    public void OnClickMoreBtn()
    {
        if (boundSetting.weight < SettingWeight.More)
            boundSetting.weight += 1;
        UpdateUI();
    }
    
    private void UpdateUI()
    {
        weightText.text = boundSetting.weight.ToString();
        switch (boundSetting.weight)
        {
            case SettingWeight.None:
                lessBtn.gameObject.SetActive(false);
                break;
            case SettingWeight.Less: 
                lessBtn.gameObject.SetActive(true);
                break;
            case SettingWeight.Default:
                moreBtn.gameObject.SetActive(true);
                break;
            case SettingWeight.More:
                moreBtn.gameObject.SetActive(false);
                break;
        }
    }
}