using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingManager : Singleton<SettingManager>
{
    [SerializeField]public DisplaySettings CurrentSettings { get; private set; }
    private readonly Dictionary<string, CanvasScaler> UIDict = new();
    
    protected override void Awake()
    {
        base.Awake();
        if (IsDuplicate)
            return;

        DontDestroyOnLoad(gameObject);
        
        LoadSettings();
        
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        InitializeUIRoot();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InitializeUIRoot();
    }
    
    private void InitializeUIRoot()
    {
        UIDict.Clear();

        Transform uiRoot = GameObject.Find("UIRoot")?.transform;
        if (uiRoot == null)
        {
            Debug.LogWarning("[UIManager] UIRoot를 찾을 수 없습니다.");
            return;
        }

        CanvasScaler[] uiComponents = uiRoot.GetComponentsInChildren<CanvasScaler>(true);
        foreach (CanvasScaler uiComponent in uiComponents)
        {
            UIDict[uiComponent.name] = uiComponent;
        }
        
        ApplySettings(CurrentSettings);
    }
    

    public void ApplySettings(DisplaySettings settings)
    {
        CurrentSettings = settings;

        Screen.fullScreen = settings.mode == DisplayMode.Fullscreen;
        Vector2Int res = GetResolutionFromRatio(settings.resolution);
        Screen.SetResolution(res.x, res.y, Screen.fullScreen);

        Shader.SetGlobalFloat("_Brightness", settings.brightness / 100f);
        Shader.SetGlobalFloat("_Gamma", settings.gamma / 100f);

        foreach (var scaler in UIDict.Values)
        {
            if (scaler != null)
                scaler.referenceResolution = new Vector2(
                    1920f / settings.uiScale, 1080f / settings.uiScale
                );
        }

        SaveSettings();
    }

    private Vector2Int GetResolutionFromRatio(ResolutionRatio ratio)
    {
        switch (ratio)
        {
            case ResolutionRatio.Ratio16_10: return new Vector2Int(1920, 1200);
            case ResolutionRatio.Ratio16_9: return new Vector2Int(1920, 1080);
            case ResolutionRatio.Ratio21_9: return new Vector2Int(2560, 1080);
            default: return new Vector2Int(1920, 1080);
        }
    }

    private void SaveSettings()
    {
        string json = JsonUtility.ToJson(CurrentSettings);
        PlayerPrefs.SetString("DisplaySettings", json);
        PlayerPrefs.Save();
    }

    private void LoadSettings()
    {
        if (PlayerPrefs.HasKey("DisplaySettings"))
        {
            string json = PlayerPrefs.GetString("DisplaySettings");
            CurrentSettings = JsonUtility.FromJson<DisplaySettings>(json);
        }
        else
        {
            CurrentSettings = new DisplaySettings()
            {
                mode = DisplayMode.Windowed,
                resolution = ResolutionRatio.Ratio16_9,
                brightness = 50f,
                gamma = 50f,
                uiScale = 1.0f
            };
        }
    }
    
#if UNITY_EDITOR
    public Dictionary<string, CanvasScaler> DebugUIDict => UIDict;
#endif
}