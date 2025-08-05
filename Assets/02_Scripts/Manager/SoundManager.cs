using System.Collections.Generic;
using UnityEngine;

public enum BGM
{
    WeatherRainSound,
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
    PlayerDash,
    PlayerWalkLeft,
    PlayerWalkRight,
    SlimeImpactStart,
    ItemPickup,
    Bubble
}

public enum Ambient
{
    
}

public class SoundManager : Singleton<SoundManager>
{
    [Header("BGM")]
    [SerializeField] private AudioSource BGMSource;
    [SerializeField] private List<AudioClip> BGMClips;
    
    [Header("SFX")]
    [SerializeField] private AudioSource SFXSource;
    [SerializeField] private List<AudioClip> SFXClips;
    
    [Header("AmbientSource")]
    [SerializeField] private AudioSource AmbientSource;
    [SerializeField] private List<AudioClip> AmbientClips;

    private BGM? currentBGM = null;

    public float GetBGMVolume() => BGMSource.volume;
    public float GetSFXVolume() => SFXSource.volume;
    public float GetAmbientVolume() => AmbientSource.volume;

    protected override void Awake()
    {
        base.Awake();
    }
    
    public void ChangeBGM(BGM bgm)
    {
        if (currentBGM != null && currentBGM != bgm)
        {
            BGMSource.Stop();
        }

        AudioClip clip = BGMClips[(int)bgm];
        BGMSource.clip = clip;
        BGMSource.loop = true;
        BGMSource.Play();

        currentBGM = bgm;
    }

    public void StopBGM()
    {
        BGMSource.Stop();
        currentBGM = null;
    }

    public void PlaySFX(SFX sfx)
    {
        float volumeScale = GetSFXVolumeScale(sfx);
        SFXSource.PlayOneShot(SFXClips[(int)sfx], volumeScale);
    }
    
    public void PlayAmbient(Ambient ambient)
    {
        AudioClip clip = BGMClips[(int)ambient];
        BGMSource.clip = clip;
        BGMSource.loop = true;
        BGMSource.Play();
    }

    public void ChangeBGMVolume(float volume)
    {
        BGMSource.volume = volume;
    }

    public void ChangeSFXVolume(float volume)
    {
        SFXSource.volume = volume;
    }
    
    public void ChangeAmbientVolume(float volume)
    {
        AmbientSource.volume = volume;
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
            case SFX.PlayerDash: return 0.1f;
            case SFX.PlayerWalkLeft: return 0.2f;
            case SFX.PlayerWalkRight: return 0.2f;
            case SFX.SlimeImpactStart: return 0.2f;
            case SFX.ItemPickup: return 0.1f;
            default: return 1.0f;
        }
    }
}
