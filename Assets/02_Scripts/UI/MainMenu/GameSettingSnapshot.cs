using System.Collections.Generic;

[System.Serializable]
public class GameSettingSnapshot
{
    // string: settingID
    // SettingWeight: Less,default....
    
    public Dictionary<string, SettingWeight> weatherSettings = new();
    public Dictionary<string, SettingWeight> enemySettings = new();
}