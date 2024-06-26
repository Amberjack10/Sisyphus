using Constants;
using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : CharacterBehaviour
{
    #region Static Instance Group
    GameManager _gameManager;
    DungeonManager _dungeonManager;
    EnemySpawner _spawner;
    EnemyPooler _enemyPooler;
    ObjectPoolManager _objectPooler;
    FieldItemsPooler _fieldItemsPooler;
    QuestManager _questManager;
    #endregion

    public EnemyStateMachine stateMachine;
    public Transform target;
    Player _player;

    public bool IsSpawning { get; private set; }
    public float chasingDelay;
    public float attackDelay;

    [field: SerializeField] public EnemyInfo Info { get; private set; }
    public Status modifier;

    public Collider Collider { get; private set; }
    public Animator Animator { get; private set; }
    public NavMeshAgent Agent { get; private set; }

    Transform renderTransform;

    // 나중에 기본 속도와 추가값에 비례해서 리턴하도록 프로퍼티로 수정하면 될듯
    public float animAttackSpeed = 1f;
    public float animMoveSpeed = 1f;

    [SerializeField] Collider[] _meleeAttackColliders;

    [SerializeField] Transform[] _rangeAttackPos;
    [SerializeField] ProjectileID[] _projectileTag;
    [SerializeField] ProjectileID[] _areaAttackTag;
    public Action deSpawnEvent;

    [SerializeField] int dropGoldValue;

    [Header("Sound Tag")]
    public string hitSound = "hit20";

    public Action<float, float> changeHealth;

    protected override void Awake()
    {
        base.Awake();

        Info = DataBase.EnemyStats.Get(id);

        Collider = GetComponent<Collider>();
        Animator = GetComponentInChildren<Animator>();
        Agent = GetComponent<NavMeshAgent>();

        renderTransform = transform.GetChild(0);

        _gameManager = GameManager.Instance;
        _dungeonManager = DungeonManager.Instance;

        _player = _gameManager.Player;

        stateMachine = new(this);

        switch (Info.rank) // 등급별로 동적 장애물 회피 성능을 조절해서 최적화?
        {
            case EnemyRank.Normal:
                Agent.obstacleAvoidanceType = ObstacleAvoidanceType.MedQualityObstacleAvoidance;
                break;
            case EnemyRank.Elite:
                Agent.obstacleAvoidanceType = ObstacleAvoidanceType.GoodQualityObstacleAvoidance;
                break;
            case EnemyRank.Boss:
                Agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
                break;
        }
    }

    private void OnEnable()
    {
        Init();
        _spawner.onEnemiesDeSpawn += DeSpawn;
        _gameManager.onGameOverEvent += ChangeVictory;

        StartSpawn();

        Animator.SetFloat(EnemyAnimData.IdleFloatParameterHash, 0f);

        if (DungeonManager.Instance.gameState == DungeonState.Playing)
            stateMachine.ChangeState(stateMachine.IdleState);
        else if (DungeonManager.Instance.gameState == DungeonState.Clear)
            stateMachine.ChangeState(stateMachine.DieState);


        InvokeChangeHealth();
    }

    private void OnDisable()
    {
        target = null;

        _spawner.onEnemiesDeSpawn -= DeSpawn;

        _gameManager.onGameOverEvent -= ChangeVictory;
        _enemyPooler.ReturnToPull(gameObject);
    }

    void Start()
    {
        //stateMachine.ChangeState(stateMachine.IdleState);

        OnDieEvent += ChangeDieState;
        OnDieEvent += InvokeActiveFalse;
        OnDieEvent += DropItem;
        OnDieEvent += DropExp;
        if (Info.rank == EnemyRank.Boss)
        {
            OnDieEvent += DropRune;
            OnDieEvent += ChangeComplete;
            OnDieEvent += () => { _spawner.arriveBoss = false; };
        }

        OnHitEvent += ChangeHitState;

        deSpawnEvent += ChangeDieState;
        deSpawnEvent += InvokeActiveFalse;
    }

    void Update()
    {
        if (isDieTrigger)
        {
            if (isDie)
            {
                _questManager.NotifyQuest(QuestType.KillMonster, 30, 1);
                OnDieEvent?.Invoke();
                isDieTrigger = false;
                return;
            }
        }


        stateMachine.Update();
    }

    private void FixedUpdate()
    {
        stateMachine.PhysicsUpdate();
    }

    void Init()
    {
        modifier.Init_EnemyModifier(Info, Info.rank);
        currentStat.InitStatus(Info, modifier, false);
        currentStat.SyncHealth();

        StaticInstanceInit();

        dropGoldValue = Info.gold + ((_dungeonManager.currnetstage / 2) * EnemyStageModifier.gold); // 2스테이지당 증가
        dropGoldValue = dropGoldValue > EnemyStageModifier.maxDropGold ? EnemyStageModifier.maxDropGold : dropGoldValue;

        #region AnimatorOverrideController 으로 시도했던것
        // OverrideAnimator는 속도 조절에는 사용하지 않아도 되지만 시도해본 방법중 하나였음.
        // OverrideAnimator는 애니메이션 클립을 부분적으로만 변경을 하려 할 때 사용하기 좋을 것 같음
        // ex)특정 조건 만족시 공격 모션이나 그런게 변할 때?
        //OverrideAnimator = new AnimatorOverrideController(Animator.runtimeAnimatorController);
        //Animator.runtimeAnimatorController = OverrideAnimator;
        #endregion
        Animator.SetFloat(EnemyAnimData.AttackSpeedParameterHash, animAttackSpeed); // 각 객체의 속도별로 Animation의 속도를 조절하기 위해서
        Animator.SetFloat(EnemyAnimData.MoveSpeedParameterHash, animMoveSpeed); // 현재 프로젝트에서는 필요 없을 것 같지만 그냥 해보고싶었음.

        Agent.stoppingDistance = Info.attackRange - .2f; // 사거리보다 살짝 더 들어가게끔 하지 않으면 멈춰서 이상한짓함
        Agent.angularSpeed = 240f;

        isDie = false;
        isHit = false;

        isDieTrigger = false;

        chasingDelay = 10f; // 그냥 초기값 설정
        attackDelay = 10f;
        knockbackDelay = 10f;

        isInvincibility = false;

        target = _gameManager.Player.transform;
    }

    void StaticInstanceInit()
    {
        if (_gameManager == null)
            _gameManager = GameManager.Instance;

        if (_dungeonManager == null)
            _dungeonManager = DungeonManager.Instance;

        if (_spawner == null)
            _spawner = EnemySpawner.Instance;

        if (_questManager == null)
            _questManager = QuestManager.Instance;

        if (_enemyPooler == null)
            _enemyPooler = EnemyPooler.Instance;

        if (_objectPooler == null)
            _objectPooler = ObjectPoolManager.Instance;

        if (_fieldItemsPooler == null)
            _fieldItemsPooler = FieldItemsPooler.Instance;
    }

    void ChangeDieState()
    {
        stateMachine.ChangeState(stateMachine.DieState);
    }

    void ChangeHitState()
    {
        stateMachine.ChangeState(stateMachine.HitState);
    }

    public void InvokeEvent(Action action)
    {
        action?.Invoke();
    }

    public void InvokeChangeHealth()
    {
        changeHealth?.Invoke(currentStat.health, currentStat.maxHealth);
    }

    public void OnChildTriggerEnter(Collider other, SkillType type)
    {
        if (target == null)
            return;

        if (other.gameObject.layer == target.gameObject.layer)
        {
            HealthSystem hs = other.GetComponent<HealthSystem>();
            //이곳에서 자식 콜라이더의 트리거 충돌 처리
            if (type == SkillType.AutoAttack)
            {
                //Debug.Log($"AA : {gameObject.name} -> Attack : {other.gameObject.name}");
                hs.TakeDamage(currentStat.physicalAtk, DamageType.Physical);
            }
            else if (type == SkillType.Skill01)
            {
                //Debug.Log($"Skill : {gameObject.name} -> Attack : {other.gameObject.name}");
                hs.TakeDamage(currentStat.physicalAtk * 0.4f, DamageType.Physical);
            }
        }
    }

    public void AttackStart(int num)
    {
        //Debug.Log("Attack Start");
        _meleeAttackColliders[num].enabled = true;
    }

    public void AttackEnd(int num)
    {
        _meleeAttackColliders[num].enabled = false;
        //Debug.Log("Attack End");
    }

    public void ActiveMeleeRange(int index, bool active)
    {
        _meleeAttackColliders[index].gameObject.SetActive(active);
    }

    public void RangedAttack(int num)
    {
        if (target == null)
            return;

        GameObject go = _objectPooler.SpawnFromPool(
            (int)_projectileTag[num],
            _rangeAttackPos[num].transform.position,
            _rangeAttackPos[num].transform.rotation);

        ProjectileTest projectile = go.GetComponent<ProjectileTest>();

        SetProjectileTarget(projectile);
        projectile.AddExcludeLayer(LayerData.Enemy); // Enemy는 충돌하지 않게 설정

        float value = projectile.GetDamageType == DamageType.Physical ? currentStat.physicalAtk : currentStat.magicAtk;
        projectile.SetValue(value);

        projectile.SetVelocity(1f); // 속도 배율 설정
    }

    public void AreaAttack(int num)
    {
        GameObject go = _objectPooler.SpawnFromPool(
        (int)_areaAttackTag[num],
        target.transform.position,
        Quaternion.identity);

        AreaAttack areaAttack = go.GetComponent<AreaAttack>();

        areaAttack.AddTarget(LayerData.Player);

        float value = areaAttack.GetDamageType == DamageType.Physical ? currentStat.physicalAtk : currentStat.magicAtk;
        areaAttack.SetValue(value);

        areaAttack.AttackStart();
    }

    void SetProjectileTarget(ProjectileTest projectile)
    {
        if (projectile is Projectile_Guided)
        {
            projectile.AddTarget(LayerData.Player, target);
        }
        else
        {
            projectile.AddTarget(LayerData.Player); // Player를 맞춰야함
        }
    }

    void StartSpawn()
    {
        IsSpawning = true;
        //Collider.enabled = false;

        renderTransform.localPosition += Vector3.down * Agent.height;
        renderTransform.DOLocalMoveY(0, 1f).OnComplete(SpawnEnd);
    }

    void SpawnEnd()
    {
        IsSpawning = false;
        //Collider.enabled = true;
    }

    public void DeSpawn()
    {
        if (gameObject.activeSelf == true && isDie == false)
            deSpawnEvent?.Invoke();
    }

    void InvokeActiveFalse()
    {
        Invoke(nameof(ActiveFalse), 3f);
    }

    void ActiveFalse()
    {
        renderTransform.DOLocalMoveY(-Agent.height, 0.5f).OnComplete(() => { gameObject.SetActive(false); });
    }

    void DropItem()
    {
        DropFielIdtems(FieldItemType.Gold, dropGoldValue + _dungeonManager.currnetstage / 5);

        float rand = UnityEngine.Random.value;
        if (rand < EnemyStageModifier.fieldItemDropPer)
        {
            float rand2 = UnityEngine.Random.value;

            if (rand2 > 0.5f)
            {
                DropFielIdtems(FieldItemType.Heart, 10);
            }
            else
            {
                DropFielIdtems(FieldItemType.Shield, 20);
            }
        }
    }

    void DropFielIdtems(FieldItemType type, int value)
    {
        Quaternion rot;
        Vector3 _itemPos;

        if (type == FieldItemType.Heart)
        {
            rot = Quaternion.Euler(-90, 0, 0);
            _itemPos = transform.position;
            _itemPos.y = 1f;
        }
        else if (type == FieldItemType.Shield)
        {
            rot = Quaternion.Euler(-30, 0, 0);
            _itemPos = transform.position;
            _itemPos.y = 1f;
        }
        else
        {
            rot = Quaternion.identity;
            _itemPos = transform.position;
        }

        FieldItems go = _fieldItemsPooler.SpawnFromPool(
            type.ToString(),
            _itemPos,
            rot).GetComponent<FieldItems>();

        go.SetValue(value);
    }


    void DropRune()
    {
        _player.GetComponent<Player>().ChangeRune(_dungeonManager.currnetstage % 5);
    }

    void DropExp()
    {
        _player.GetEXP(10 + _dungeonManager.currnetstage % 3);
    }

    void ChangeComplete()
    {
        _dungeonManager.isStageCompleted = true;
    }

    void ChangeVictory()
    {
        target = null;
        Animator.SetFloat(EnemyAnimData.IdleFloatParameterHash, 1f);
        stateMachine.ChangeState(stateMachine.IdleState);
    }
}
