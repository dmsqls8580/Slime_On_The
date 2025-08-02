using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIDisplaySettings : MonoBehaviour
{
    [Header("Dropdowns")]
    [SerializeField] private TMP_Dropdown displayModeDropdown;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    
    [Header("Sliders")]
    [SerializeField] private Slider brightnessSlider;
    [SerializeField] private TextMeshProUGUI brightnessValueText;

    [SerializeField] private Slider gammaSlider;
    [SerializeField] private TextMeshProUGUI gammaValueText;

    [SerializeField] private Slider uiScaleSlider;
    [SerializeField] private TextMeshProUGUI uiScaleValueText;
    
    [Header("Apply Button")]
    [SerializeField] private Button applyButton;
    
    private void Start()
    {
        InitUIFromCurrentSettings();

        brightnessSlider.onValueChanged.AddListener(value => brightnessValueText.text = Mathf.RoundToInt(value).ToString());
        gammaSlider.onValueChanged.AddListener(value => gammaValueText.text = Mathf.RoundToInt(value).ToString());
        uiScaleSlider.onValueChanged.AddListener(value => uiScaleValueText.text = value.ToString("0.00"));

        applyButton.onClick.AddListener(ApplySettings);
    }
    
    private void InitUIFromCurrentSettings()
    {
        var settings = SettingManager.Instance.CurrentSettings;

        displayModeDropdown.value = (int)settings.mode;
        resolutionDropdown.value = (int)settings.resolution;

        brightnessSlider.value = settings.brightness;
        brightnessValueText.text = Mathf.RoundToInt(settings.brightness).ToString();

        gammaSlider.value = settings.gamma;
        gammaValueText.text = Mathf.RoundToInt(settings.gamma).ToString();

        uiScaleSlider.value = settings.uiScale;
        uiScaleValueText.text = settings.uiScale.ToString("0.00");
    }

    private void ApplySettings()
    {
        var newSettings = new DisplaySettings()
        {
            mode = (DisplayMode)displayModeDropdown.value,
            resolution = (ResolutionRatio)resolutionDropdown.value,
            brightness = brightnessSlider.value,
            gamma = gammaSlider.value,
            uiScale = uiScaleSlider.value
        };

        SettingManager.Instance.ApplySettings(newSettings);
    }
}
