using Newtonsoft.Json;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    [Header("저장 데이터 참조")]
    [SerializeField] private PlacedObjectManager placedObjectManager;

    private string savePath => Application.persistentDataPath + "/save.json";

    public void Save()
    {
        SaveData data = new()
        {
            placedObjectData = placedObjectManager.GetSaveData(),
            // TODO: 다른것들 추가.
        };

        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(savePath, json);
    }

    public void Load()
    {
        if (!File.Exists(savePath))
        {
            Debug.LogWarning("No save file found!");
            return;
        }

        string json = File.ReadAllText(savePath);
        SaveData data = JsonConvert.DeserializeObject<SaveData>(json);

        placedObjectManager.LoadFromData(data.placedObjectData);
        //inventoryManager.LoadFromData(data.inventoryData);
    }
}
