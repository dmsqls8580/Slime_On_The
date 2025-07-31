using _02_Scripts.Manager;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIWorldSetting : UIBase
{
    [Header("Weather")]
    public Transform weatherParent;
    public List<SettingSO> weatherSettings;

    [Header("Enemy")]
    public Transform enemyParent;
    public List<SettingSO> enemySettings;

    public GameObject settingUIPrefab;

    void Start()
    {
        GenerateSettings(weatherSettings, weatherParent);
        GenerateSettings(enemySettings, enemyParent);
    }
    
    private void GenerateSettings(List<SettingSO> settings, Transform parent)
    {
        foreach (var setting in settings)
        {
            var go = Instantiate(settingUIPrefab, parent);
            go.GetComponent<SettingUIPanel>().Initialize(setting);
        }
    }
    
    public GameSettingSnapshot BuildSnapshot()
    {
        var snapshot = new GameSettingSnapshot();

        foreach (var setting in weatherSettings)
        {
            snapshot.weatherSettings[setting.settingId] = setting.weight;
        }

        foreach (var setting in enemySettings)
        {
            snapshot.enemySettings[setting.settingId] = setting.weight;
        }

        return snapshot;
    }
    
    public void OnClickApply()
    {
        //GameManager.Instance.ApplySettings(BuildSnapshot());
        SceneManager.LoadScene("MVP_Play_Test_Scene");
    }
    
    public void OnClickCancel()
    {
        UIManager.Instance.Toggle<UIWorldSetting>();
    }
}
