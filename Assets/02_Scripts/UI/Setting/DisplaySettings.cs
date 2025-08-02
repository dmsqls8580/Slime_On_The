public enum DisplayMode { Windowed, Fullscreen }
public enum ResolutionRatio { Ratio16_9, Ratio16_10, Ratio21_9 }

[System.Serializable]
public class DisplaySettings
{
    public DisplayMode mode;
    public ResolutionRatio resolution;
    public float brightness;
    public float gamma;
    public float uiScale;
}

