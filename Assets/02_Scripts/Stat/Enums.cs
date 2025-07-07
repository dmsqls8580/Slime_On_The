public enum StatType
{
    // Common
    MaxHp,
    CurrentHp,
    Attack,
    Defense,
    MoveSpeed,

    // Player
    MaxHunger,
    CurrentHunger,
    MaxSlimeGauge,
    CurrentSlimeGauge,
    
    //Interact
    ActSpeed,
    
    // Enenmy
    AttackCooldown,
    MaxMoveDelay,
    MinMoveDelay,
    WanderRadius,
    SenseRange,
    AttackRange
}

public enum StatModifierType
{
    Base,
    BasePercent,
    Buff,
    BuffPercent,
    Equipment,
}