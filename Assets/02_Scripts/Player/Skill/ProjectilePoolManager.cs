using UnityEngine;
using UnityEngine.Pool;

public class ProjectilePoolManager : MonoBehaviour
{
    public static ProjectilePoolManager Instance{get; private set;}
    
    [SerializeField] private PlayerProjectile projectilePrefab;
    
    private IObjectPool<PlayerProjectile> pool;

    private void Awake()
    {
        Instance = this;

        pool = new ObjectPool<PlayerProjectile>(
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
        var proj=  Instantiate(projectilePrefab);
        proj.SetPool(pool);
        return proj;
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

    public void Spawn(Vector2 origin, Vector2 direction, float speed, float damage, IAttackable attacker)
    {
        var projectile = pool.Get();
        projectile.transform.position = origin;
        projectile.Init(direction, speed, damage, attacker);
    }
}
