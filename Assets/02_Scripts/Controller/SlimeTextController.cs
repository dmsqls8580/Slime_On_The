using System;
using System.Collections.Generic;
using UnityEngine;

public class SlimeTextController : MonoBehaviour, ISlimeTextOut
{
    [SerializeField] private Canvas slimeTextCanvas; 
    [SerializeField] private TextAsset slimeTextJson;
    private Dictionary<string, float> lastTime = new();
    private float blockTime = 0.5f;

    private void Start()
    {
        SlimeTextDataManager.Instance.LoadFormJson(slimeTextJson);
    }

    public void TryShowSlimeText(string _key, float _gauge, Vector3 _worldPos)
    {
         if (lastTime.TryGetValue(_key, out float value))
         {
             if(Time.time - value < blockTime)
                 return;
         }
        
       lastTime[_key] = Time.time;
        
        string message= SlimeTextDataManager.Instance.GetRandomText(_key, _gauge);
        if (!string.IsNullOrEmpty(message))
        {
            GameObject poolObject = ObjectPoolManager.Instance.GetObject("SlimeText");
            SlimeTextUI slimeTextUi = poolObject.GetComponent<SlimeTextUI>();
            Logger.Log("슬라임 텍스트 출력메서드");
            slimeTextUi.ShowText(message,_worldPos);
        }
        
    }

    public void OnSlimeGaugeChanged(float _curGauge, float _maxGauge, Vector3 _worldPos)
    {
        float per = (_curGauge / _maxGauge) * 100;

        if (per >= 70f)
        {
            TryShowSlimeText("maxWarning", per, _worldPos);
        }
        Logger.Log("슬라임 텍스트 게이지 변경");
    }
}
