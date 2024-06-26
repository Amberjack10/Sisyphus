public enum EnemyType // 종족
{
    Slime,
    TurtleShell,
    Spider,
    Skeleton,
    Orc,
    Golem,
    Bat,
    Dragon,
    MonsterPlant,
    EvilMage,
    Worm
}

public enum EnemyRank // 등급
{
    Normal,
    Elite,
    Boss
}

public enum EnemyElement // 속성?
{
    Default,
    Type1
}

public enum EnemySize // 기본 3~5종류에 특수한 개체가 있을 경우 해당 개체를 위한 사이즈 추가
{
    Small,
    Medium,
    Large,
    Boss
}

public enum SkillType
{
    AutoAttack,
    Skill01
}

public struct EnemyData
{
    public static readonly float[] ChasingDelay = {
        0.2f, // Normal
        0.1f, // Elite
        0.05f  // Boss
    };

    public static readonly int ChasingPriority = 51;
    public static readonly int DefaultPriority = 50;

    public static readonly float KnockBackDelayTime = 0.2f;
}