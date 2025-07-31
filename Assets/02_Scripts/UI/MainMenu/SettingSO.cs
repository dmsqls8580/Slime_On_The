using UnityEngine;

public enum SettingWeight
{
    None,
    Less,
    Default,
    More
}

[CreateAssetMenu(menuName = "SettingSO")]
public class SettingSO : ScriptableObject
{
    public string settingId; //weather_rain, 
    public string displayName;
    public Sprite icon;
    public SettingWeight weight = SettingWeight.Default;
}