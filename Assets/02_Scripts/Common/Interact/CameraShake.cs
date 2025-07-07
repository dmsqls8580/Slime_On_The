using Cinemachine;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    private CinemachineVirtualCamera virtualCamera;
    private CinemachineBasicMultiChannelPerlin perlin;
    private float shakeTime;

    private void Awake()
    {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
        perlin = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        
    }

    public void Shake(float _duration, float _amplitude, float _frequency)
    {
        if (shakeTime > _duration)
        {
            return;
        }
        
        shakeTime = _duration;
        perlin.m_AmplitudeGain = _amplitude;
        perlin.m_FrequencyGain = _frequency;
    }

    private void Update()
    {
        if (shakeTime > 0)
        {
            shakeTime -= Time.deltaTime;
            if (shakeTime <= 0)
            {
                StopShake();
            }
        }
    }

    public void StopShake()
    {
        shakeTime = 0;
        perlin.m_AmplitudeGain = 0;
        perlin.m_FrequencyGain = 0;
    }
}
