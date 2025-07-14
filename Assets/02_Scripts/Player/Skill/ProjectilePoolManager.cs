using UnityEngine;
using UnityEngine.Pool;

public class ProjectilePoolManager : MonoBehaviour
{
    public static ProjectilePoolManager Instance { get; private set; }

    [SerializeField] private PlayerProjectile projectilePrefab;
    private IObjectPool<PlayerProjectile> pool;

    private void Awake()
    {
        Instance = this;
        pool = new ObjectPool<PlayerProjectile>(
            CreateFunc, OnGet, OnRelease, OnDestroyPoolObject,
            defaultCapacity: 10, maxSize: 100);
    }

    private PlayerProjectile CreateFunc()
    {
        var proj = Instantiate(projectilePrefab, this.transform);
        proj.SetPool(pool);
        return proj;
    }

    private void OnGet(PlayerProjectile _pobj) { _pobj.gameObject.SetActive(true); }
    private void OnRelease(PlayerProjectile _pobj) { _pobj.gameObject.SetActive(false); }
    private void OnDestroyPoolObject(PlayerProjectile _pobj) { Destroy(_pobj.gameObject); }

    public void Spawn(Vector2 _origin, Vector2 _direction, float _speed, float _range)
    {
        var projectile = pool.Get();
        projectile.transform.position = _origin;
        projectile.Init(_direction, _speed, _range);
    }
}