using UnityEngine;

public class EnemyStatus : MonoBehaviour
{
    private StatManager statManager;
    [SerializeField]private EnemySO enemySO;
    
    public float MaxHealth => statManager.GetValue(StatType.MaxHp);
    public float CurrentHealth => statManager.GetValue(StatType.MaxHp);
    public float AttackDamage => statManager.GetValue(StatType.Attack);
    public float Defense => statManager.GetValue(StatType.Defense);
    public float MoveSpeed => statManager.GetValue(StatType.MoveSpeed);
    public float AttackCooldown => statManager.GetValue(StatType.AttackCooldown);
    public float MaxMoveDelay => statManager.GetValue(StatType.MaxMoveDelay);
    public float MinMoveDelay => statManager.GetValue(StatType.MinMoveDelay);
    public float WanderRadius => statManager.GetValue(StatType.WanderRadius);
    public float SenseRange => statManager.GetValue(StatType.SenseRange);
    public float AttackRange  => statManager.GetValue(StatType.AttackRange);

    private void Start()
    {
        statManager = GetComponent<StatManager>();
        statManager.Init(enemySO);
        EnemyTable enemyTable = TableManager.Instance.GetTable<EnemyTable>();
        enemySO = enemyTable.GetDataByID(1);
    }

    // Collider 초기화 함수
    public void InitCollider(Collider2D _senseCollider, Collider2D _attackCollider)
    {
        // SenseRange를 SenseCollider에 적용
        if (_senseCollider is CircleCollider2D senseCircle)
        {
            senseCircle.radius = SenseRange;
        }
        // AttackRange를 AttackCollider에 적용
        if (_attackCollider is CircleCollider2D attackCircle)
        {
            attackCircle.radius = AttackRange;
        }
    }
    
    // Enemy 현재 체력 초기화 메서드
    public void InitCurrenthealth()
    {
        // 현재 체력 최대 체력으로 회복
        statManager.Recover(StatType.CurrentHp, StatModifierType.Base, MaxHealth);
    }
    
    // Enemy 피격 메서드
    public void TakeDamage(float _damage, StatModifierType _modifierType)
    {
        statManager.Consume(StatType.CurrentHp, _modifierType, _damage);
    }
    

    // Enemy 회복 메서드
    public void RecoverHp(float _amount, StatModifierType _modifierType)
    {
        statManager.Recover(StatType.CurrentHp, _modifierType, _amount);
    }
    
    
    // Enemy 사망 판정
    public bool IsDead => CurrentHealth <= 0;
    
    
    
}
