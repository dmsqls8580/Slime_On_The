using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsUI : MonoBehaviour
{
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    private void Start()
    {
        bgmSlider.onValueChanged.AddListener(SoundManager.Instance.ChangeBGMVolume);
        sfxSlider.onValueChanged.AddListener(SoundManager.Instance.ChangeSFXVolume);

        // 초기 볼륨 설정
        bgmSlider.value = SoundManager.Instance.GetBGMVolume();
        sfxSlider.value = SoundManager.Instance.GetSFXVolume();
    }
}