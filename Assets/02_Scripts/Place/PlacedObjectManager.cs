using System.Collections.Generic;
using UnityEngine;

public class PlacedObjectManager : MonoBehaviour, ISavable<PlacedObjectData>
{
    // 모든 설치 가능한 프리팹의 스크립트 등록.
    [SerializeField] private List<PlacedObject> placeableList;

    // Key로 빠르게 찾기 위한 딕셔너리.
    private Dictionary<int, PlacedObject> placeableDictionary = new();

    // 게임 상에 설치된 오브젝트들.
    private List<PlacedObject> placedObjects = new();

    private void Awake()
    {
        // 리스트를 딕셔너리로 변환.
        foreach (var placeable in placeableList)
        {
            if (!placeableDictionary.ContainsKey(placeable.ItemSO.idx))
            {
                placeableDictionary.Add(placeable.ItemSO.idx, placeable);
            }
        }
    }

    public void AddPlacedObject(PlacedObject _obj) => placedObjects.Add(_obj);

    public void RemovePlacedObject(PlacedObject _obj) => placedObjects.Remove(_obj);

    public PlacedObjectData GetSaveData()
    {
        PlacedObjectData data = new();

        foreach (var placedObject in placedObjects)
        {
            PlacedObjectInfo info = new()
            {
                idx = placedObject.ItemSO.idx,
                position = placedObject.gameObject.transform.position,
                rotation = placedObject.gameObject.transform.rotation
            };

            data.datas.Add(info);
        }

        return data;
    }

    public void LoadFromData(PlacedObjectData _data)
    {
        foreach (var info in _data.datas)
        {
            if (placeableDictionary.TryGetValue(info.idx, out PlacedObject prefabComponent))
            {
                GameObject newObj = Instantiate(prefabComponent.gameObject, info.position, info.rotation);
                if (newObj.TryGetComponent(out PlacedObject placedObject))
                {
                    placedObjects.Add(placedObject);
                }
            }
        }
    }
}
