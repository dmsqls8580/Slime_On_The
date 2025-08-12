
public interface IAttackable
{
    public string AttackerName { get; }
    StatBase AttackStat { get; }
    public IDamageable Target { get; }
    public void Attack();
}
