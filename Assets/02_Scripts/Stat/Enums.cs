public enum StatType
{
    // Common
    MaxHp = 0,
    CurrentHp,
    Attack,
    Defense,
    MoveSpeed,
    FinalAtk,

    // Player
    MaxHunger = 20,
    CurrentHunger,
    MaxSlimeGauge,
    CurrentSlimeGauge,
    CriticalChance,
    CriticalMultiplier,
    CurrentStamina,
    
    //Interact
    ActSpeed = 40,
    
    // Enemy
    AttackCooldown = 60,
    MaxMoveDelay,
    MinMoveDelay,
    WanderRadius,
    SenseRange,
    AttackRange,
    AttackRadius,
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