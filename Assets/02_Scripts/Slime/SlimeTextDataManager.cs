using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class SlimeTextData
{
    public string key;
    public string message;

    // 일정 슬라임게이지 수치일때 ex) (Min = 0, Max = 20) 0~20일때 메세지가 나올 확률 
    public float slimeMinValue;
    public float slimeMaxValue;
    //---------------------------

    public float probability; // 메세지가 나올 확률
}

[System.Serializable]
public class SlimeDataList
{
    public List<SlimeTextData> messages;
}

public class SlimeTextDataManager : SceneOnlySingleton<SlimeTextDataManager>
{
    private List<SlimeTextData> slimeTextDatas = new();

    public void LoadFormJson(TextAsset _json)
    {
        slimeTextDatas.Clear();
        var data = JsonUtility.FromJson<SlimeDataList>(_json.text);
        slimeTextDatas.AddRange(data.messages);
    }

    public string GetRandomText(string _key, float _gauge)
    {
        List<SlimeTextData> textDatas = new();
        foreach (var data in slimeTextDatas)
        {
            if (data.key == _key && _gauge >= data.slimeMinValue - 0.01f && _gauge <= data.slimeMaxValue + 0.01f)
            {
                textDatas.Add(data);
            }
        }

        if (textDatas.Count == 0)
        {
            return string.Empty;
        }

        float chance = 0f;
        foreach (var data in textDatas)
        {
            chance += data.probability;
        }

        float random = Random.value * chance;

        float sum = 0;
        foreach (var data in textDatas)
        {
            sum += data.probability;
            if (random <= sum)
            {
                return data.message;
            }
        }

        return string.Empty;
    }

    public bool HasText(string _key, float _gauge)
    {
        foreach (var data in slimeTextDatas)
        {
            if (data.key == _key &&
                _gauge >= data.slimeMinValue - 0.01f &&
                _gauge <= data.slimeMaxValue + 0.01f)
            {
                return true;
            }
        }

        return false;
    }
}