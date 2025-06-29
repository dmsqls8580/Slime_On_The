using UnityEngine;
using UnityEngine.Pool;

public class ProjectilePoolManager : MonoBehaviour
{
    public static ProjectilePoolManager Instance{get; private set;}
    
    [SerializeField] private PlayerProjectile projectilePrefab;
    
    private IObjectPool<PlayerProjectile> _pool;

    private void Awake()
    {
        Instance = this;

        _pool = new ObjectPool<PlayerProjectile>(
            CreateFunc,
            OnGet,
            OnRelease,
            OnDestroyPoolObject,
            defaultCapacity:10,
            maxSize:100
        );
    }

    private PlayerProjectile CreateFunc()
    {
        throw new System.NotImplementedException();
    }

    private void OnGet(PlayerProjectile pobj)
    {
        pobj.gameObject.SetActive(true);
    }

    private void OnRelease(PlayerProjectile pobj)
    {
        pobj.gameObject.SetActive(false);
    }

    private void OnDestroyPoolObject(PlayerProjectile pobj)
    {
        Destroy(pobj.gameObject);
    }

    public void Spawn(Vector2 origin, Vector2 direction, float speed, IAttackable attacker)
    {
        
    }
}
