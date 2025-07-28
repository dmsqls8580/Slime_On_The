using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

[CustomEditor(typeof(ObjectPoolManager))]
public class ObjectPoolEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ObjectPoolManager objectPoolManager = (ObjectPoolManager)target;

        if (GUILayout.Button("ObjectPool 최신화"))
        {
            objectPoolManager.AutoAssignObject();
        }
        
    }
}

public class ObjectPoolManager : SceneOnlySingleton<ObjectPoolManager>
{
    [SerializeField] private List<GameObject> poolObjectList = new List<GameObject>();
    private List<IPoolObject> pools = new List<IPoolObject>();
    private Dictionary<string, Queue<IPoolObject>> poolObjects = new Dictionary<string, Queue<IPoolObject>>();
    private Dictionary<string, GameObject> registeredObj = new Dictionary<string, GameObject>();
    private Dictionary<string, Transform> parentCache = new Dictionary<string, Transform>();

#if UNITY_EDITOR
    public void AutoAssignObject()
    {
        poolObjectList.Clear();
        string[] guids =
            UnityEditor.AssetDatabase.FindAssets("t:GameObject", new[] { "Assets/03_Prefabs/Pool" });

        foreach (string guid in guids)
        {
            string path  = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            var    asset = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (asset.TryGetComponent<IPoolObject>(out var poolObject))
            {
                if (poolObject != null && !poolObjectList.Contains(asset))
                {
                    poolObjectList.Add(asset);
                }
            }
        }

        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
    protected override void Awake()
    {
        foreach (var obj in poolObjectList)
        {
            if (obj.TryGetComponent<IPoolObject>(out var ipool))
            {
                pools.Add(ipool);
            }
            else
            {
                Debug.LogError($"오브젝트에 IPoolObject이 상속 되어 있지 않습니다. {obj.name}");
            }
        }

        foreach (var pool in pools)
        {
            CreatePool(pool, pool.PoolSize);
        }
    }

    private void Start()
    {
        PrintPoolIDs();
    }

    /// <summary>
    /// 풀에 오브잭트를 등록해주는 메서드
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="poolSize"></param>
    public void CreatePool(IPoolObject iPoolObject, int poolSize)
    {
        if (poolObjects.ContainsKey(iPoolObject.PoolID))
        {
            Debug.LogWarning($"등록된 풀이 있습니다. : {iPoolObject.PoolID}");
            return;
        }

        string poolId = iPoolObject.PoolID;
        GameObject prefab = iPoolObject.GameObject;

        Queue<IPoolObject> newPool = new Queue<IPoolObject>();

        // 🔽 특정 ID는 캔버스 하위로 부모 설정
        Transform parent;
        if (poolId == "SlimeText")
        {
            var canvas = GameObject.Find("SlimeTextCanvas");
            parent = canvas != null ? canvas.transform : transform;
        }
        else
        {
            GameObject parentObj = new GameObject(poolId);
            parentObj.transform.SetParent(transform);
            parent = parentObj.transform;
        }

        parentCache[poolId] = parent;

        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(prefab, parent);
            obj.name = poolId;
            obj.SetActive(false);
            newPool.Enqueue(obj.GetComponent<IPoolObject>());
        }

        poolObjects[poolId] = newPool;
        registeredObj[poolId] = prefab;
    }

    /// <summary>
    /// 풀에서 오브젝트를 꺼내는 메서드
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public GameObject GetObject(string poolId)
    {
        string poolName = poolId.ToString();
        if (!poolObjects.TryGetValue(poolId, out Queue<IPoolObject> pool))
        {
            Debug.LogWarning($"등록된 풀이 없습니다. : {poolId}");
            return null;
        }

        if (pool.Count > 0)
        {
            var        getPool = pool.Dequeue();
            GameObject go      = getPool.GameObject;
            go.SetActive(true);
            return go;
        }
        else
        {
            GameObject prefab = registeredObj[poolId];
            GameObject newObj = Instantiate(prefab, parentCache[poolId]);
            if (newObj.TryGetComponent<IPoolObject>(out var poolObject))
            {
                newObj.name = poolName;
                newObj.SetActive(true);
                return newObj;
            }

            return null;
        }
    }

    /// <summary>
    /// 오브젝트를 풀에 반환하는 함수
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="returnTime"></param>
    /// <param name="action"></param>
    public void ReturnObject(GameObject obj, float returnTime = 0, UnityAction action = null)
    {
        StartCoroutine(DelayedReturnObject(obj, action, returnTime));
    }

    private IEnumerator DelayedReturnObject(GameObject obj, UnityAction action, float returnTime)
    {
        if (!obj.TryGetComponent<IPoolObject>(out IPoolObject iPoolObject))
        {
            Debug.LogError("풀링 오브젝트가 아닙니다.");
            yield break;
        }

        if (!poolObjects.ContainsKey(iPoolObject.PoolID))
        {
            Debug.LogWarning($"등록된 풀이 없습니다. : {iPoolObject.PoolID}");
            CreatePool(iPoolObject, 1);
        }
        
        if (returnTime > 0)
            yield return new WaitForSeconds(returnTime);
        iPoolObject.OnReturnToPool();
        obj.SetActive(false);
        obj.transform.position = Vector3.zero;
        action?.Invoke();
        poolObjects[iPoolObject.PoolID].Enqueue(iPoolObject);
        obj.transform.SetParent(parentCache[iPoolObject.PoolID]);
    }
    
    /// <summary>
    /// 위치를 변경하지 않는 오브젝트를 풀로 반환하는 메서드
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="returnTime"></param>
    /// <param name="action"></param>
    public void ReturnFixedObject(GameObject obj, float returnTime = 0, UnityAction action = null)
    {
        StartCoroutine(DelayedReturnFixedObject(obj, action, returnTime));
    }

    private IEnumerator DelayedReturnFixedObject(GameObject obj, UnityAction action, float returnTime)
    {
        if (!obj.TryGetComponent<IPoolObject>(out IPoolObject iPoolObject))
        {
            Debug.LogError("풀링 오브젝트가 아닙니다.");
            yield break;
        }

        if (!poolObjects.ContainsKey(iPoolObject.PoolID))
        {
            Debug.LogWarning($"등록된 풀이 없습니다. : {iPoolObject.PoolID}");
            CreatePool(iPoolObject, 1);
        }

        if (returnTime > 0)
            yield return new WaitForSeconds(returnTime);
        iPoolObject.OnReturnToPool();
        obj.SetActive(false);
        action?.Invoke();
        poolObjects[iPoolObject.PoolID].Enqueue(iPoolObject);
        obj.transform.SetParent(parentCache[iPoolObject.PoolID]);
    }


    public void RemovePool(string poolId)
    {
        Destroy(parentCache[poolId].gameObject);
        parentCache.Remove(poolId);
        poolObjects.Remove(poolId);
        registeredObj.Remove(poolId);
    }
    
    public bool HasPool(string poolId)
    {
        return poolObjects.ContainsKey(poolId);
    }
    
    public void PrintPoolIDs()
    {
        foreach (var prefab in poolObjectList)
        {
            if (prefab == null)
            {
                Debug.LogWarning("poolObjectList에 null 프리팹이 있습니다.");
                continue;
            }
            if (prefab.TryGetComponent<IPoolObject>(out var poolObject))
            {
                Debug.Log($"Prefab: {prefab.name}, PoolID: {poolObject.PoolID}");
            }
            else
            {
                Debug.LogWarning($"Prefab {prefab.name} does not implement IPoolObject");
            }
        }
    }
}