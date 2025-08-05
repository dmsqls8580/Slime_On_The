using System.Collections.Generic;
using UnityEngine;


public enum BGM
{
    WeatherRainSound
}

public enum SFX
{
    Grount,
    SlimeNormalAttack,
    Toggle,
    Click,
    Error,
    WeatherLightningStartSound,
    WeatherStormSound,
    ToolAxe,
    ToolHammer,
    ToolHand,
    ToolPickaxe,
    PlayerDash
}

public class SoundManager : Singleton<SoundManager>
{
    [Header("BGM")]
    [SerializeField] private AudioSource BGMSource;
    [SerializeField] private List<AudioClip> BGMClips;
    
    [Header("SFX")]
    [SerializeField] private AudioSource SFXSource;
    [SerializeField] private List<AudioClip> SFXClips;
    
    public float GetBGMVolume() => BGMSource.volume;
    public float GetSFXVolume() => SFXSource.volume;


    protected override void Awake()
    {
        base.Awake();
    }
    
    public void ChangeBGM(BGM bgm)
    {
        AudioClip clip = BGMClips[(int)bgm];
        BGMSource.clip = clip;
        BGMSource.Play();
    }

    public void PlaySFX(SFX sfx)
    {
        float volumeScale = GetSFXVolumeScale(sfx);
        SFXSource.PlayOneShot(SFXClips[(int)sfx], volumeScale);
    }

    public void ChangeBGMVolume(float volume)
    {
        BGMSource.volume = volume;
    }

    public void ChangeSFXVolume(float volume)
    {
        SFXSource.volume = volume;
    }
    
    private float GetSFXVolumeScale(SFX sfx)
    {
        switch (sfx)
        {
            case SFX.SlimeNormalAttack: return 0.2f;
            case SFX.Grount: return 1.0f;
            case SFX.Toggle: return 0.2f;
            case SFX.Click: return 0.8f;
            case SFX.Error: return 0.5f;
            case SFX.WeatherLightningStartSound: return 0.5f;
            case SFX.WeatherStormSound: return 0.5f;
            case SFX.ToolAxe: return 0.8f;
            case SFX.ToolHammer: return 0.5f;
            case SFX.ToolHand: return 0.5f;
            case SFX.ToolPickaxe: return 0.5f;
            case SFX.PlayerDash: return 0.2f;
            default: return 1.0f;
        }
    }
}
