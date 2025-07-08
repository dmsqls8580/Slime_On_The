public enum StatType
{
    // Common
    MaxHp = 0,
    CurrentHp,
    Attack,
    Defense,
    MoveSpeed,

    // Player
    MaxHunger =20,
    CurrentHunger,
    MaxSlimeGauge,
    CurrentSlimeGauge,
    
    //Interact
    ActSpeed = 40,
    
    // Enemy
    AttackCooldown = 60,
    MaxMoveDelay,
    MinMoveDelay,
    WanderRadius,
    SenseRange,
    AttackRange,
    FleeDistance
}

public enum StatModifierType
{
    Base,
    BasePercent,
    Buff,
    BuffPercent,
    Equipment,
}