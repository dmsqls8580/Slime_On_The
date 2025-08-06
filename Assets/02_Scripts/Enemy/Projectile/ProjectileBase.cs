using UnityEngine;

public abstract class ProjectileBase : MonoBehaviour, IAttackable, IPoolObject
{
    [SerializeField] protected float speed;
    [SerializeField] protected float lifeTime;
    
    protected Rigidbody2D rigid;
    protected StatBase damage;
    protected bool initialized = false;
    
    /************************ IAttackable ***********************/
    public string AttackerName { get; private set; }
    public virtual StatBase AttackStat => damage;
    public virtual IDamageable Target => null; // OntriggerEnter2D에서 처리하기 때문에 불필요
    public virtual void Attack() { }
    

    /************************ IPoolObject ***********************/
    public virtual GameObject GameObject  => this.gameObject;
    public virtual string PoolID => gameObject.name;
    public virtual int PoolSize => 5;
    
    public virtual void OnSpawnFromPool()
    {
        
    }

    public virtual void OnReturnToPool()
    {
        rigid.velocity = Vector2.zero;
        damage = null;
    }
    
    
    /************************ ProjectileBase ***********************/
    public GameObject projectileHost;
    
    public virtual void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    public virtual void Init(Vector2 dir, StatBase _damage, GameObject _host, float _radius = 0f)
    {
        if (_host.TryGetComponent<IAttackable>(out var attacker))
        {
            AttackerName = attacker.AttackerName;
        }
    }
        

    protected virtual void OnTriggerEnter2D(Collider2D other) {}
    protected virtual void OnTriggerStay2D(Collider2D other) {}
    protected virtual void OnTriggerExit2D(Collider2D other) {}
    
}
