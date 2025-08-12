using UnityEngine;
using UnityEngine.UI;

public class UISoundSettings : MonoBehaviour
{
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider ambientSlider;

    private void Start()
    {
        if (bgmSlider != null)
        {
            bgmSlider.onValueChanged.AddListener(SoundManager.Instance.ChangeBGMVolume);
            bgmSlider.value = SoundManager.Instance.GetBGMVolume();
        }
        
        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.AddListener(SoundManager.Instance.ChangeSFXVolume);
            sfxSlider.value = SoundManager.Instance.GetSFXVolume();
        }
        
        if (ambientSlider != null)
        {
            ambientSlider.onValueChanged.AddListener(SoundManager.Instance.ChangeAmbientVolume);
            ambientSlider.value = SoundManager.Instance.GetAmbientVolume();
        }
    }
}