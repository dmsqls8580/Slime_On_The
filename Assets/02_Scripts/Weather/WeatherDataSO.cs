using UnityEngine;

[CreateAssetMenu(fileName = "New Weather Data", menuName = "Weather/Weather Data")]
public class WeatherDataSO : ScriptableObject
{
    public WeatherType type;
    public float durationMin;
    public float durationMax;
}
