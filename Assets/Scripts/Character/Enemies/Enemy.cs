using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : CharacterBehaviour
{
    public EnemyStateMachine stateMachine;
    public Transform target;

    public bool IsSpawning { get; private set; }
    public float chasingDelay;
    public float attackDelay;

    [field: SerializeField] public EnemyInfo Info { get; private set; }
    public Status modifier; // 스탯 가중치 (Player의 경우 장비에 의한 가중치, Enemy의 경우 난이도/층수 에 의한 가중치)

    public Collider Collider { get; private set; }
    public Animator Animator { get; private set; }
    public NavMeshAgent Agent { get; private set; }

    // 나중에 기본 속도와 추가값에 비례해서 리턴하도록 프로퍼티로 수정하면 될듯
    public float animAttackSpeed = 1f;
    public float animMoveSpeed = 1f;

    [SerializeField] Collider[] _meleeAttackColliders;

    [SerializeField] Transform[] _rangeAttackPos;
    [SerializeField] ProjectileID[] _projectileTag;

    private void Awake()
    {
        Info = DataBase.EnemyStats.Get(id);

        Collider = GetComponent<Collider>();
        Animator = GetComponentInChildren<Animator>();
        Agent = GetComponent<NavMeshAgent>();

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

        Init();
    }

    private void OnEnable()
    {
        stateMachine.ChangeState(stateMachine.IdleState);
        StartSpawn();
        Init();
    }

    private void OnDisable()
    {
        EnemyPooler.Instance.ReturnToPull(gameObject);
    }

    void Start()
    {
        //stateMachine.ChangeState(stateMachine.IdleState);

        OnDieEvent += ChangeDieState;
        OnDieEvent += InvokeOnDieActiveFalse;
        OnDieEvent += DropGold;

        OnHitEvent += ChangeHitState;

        target = EnemySpawner.Instance.target; // 임시
    }

    void Update()
    {
        if (DungeonManager.Instance.isStarted == false)
            DeSpawn();

        stateMachine.Update();
    }

    private void FixedUpdate()
    {
        stateMachine.PhysicsUpdate();
    }

    void Init()
    {
        currentStat.InitStatus(Info, modifier);

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

        chasingDelay = 10f; // 그냥 초기값 설정
        attackDelay = 10f;
        knockbackDelay = 10f;
    }

    void ChangeDieState()
    {
        isDie = true;
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

    public void OnChildTriggerEnter(Collider other, SkillType type)
    {
        if (other.gameObject.layer == target.gameObject.layer)
        {
            HealthSystem hs = other.GetComponent<HealthSystem>();
            //이곳에서 자식 콜라이더의 트리거 충돌 처리
            if (type == SkillType.AutoAttack)
            {
                Debug.Log($"AA : {gameObject.name} -> Attack : {other.gameObject.name}");
                hs.TakeDamage(currentStat.meleeAtk);
            }
            else if (type == SkillType.Skill01)
            {
                Debug.Log($"Skill : {gameObject.name} -> Attack : {other.gameObject.name}");
                hs.TakeDamage(currentStat.meleeAtk * 0.4f);
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

    public void RangedAttack(int num)
    {
        //GameObject go = Instantiate(_projectilePrefabs[prfabNum],
        //    _rangeAttackPos[posNum].transform.position, transform.rotation); // 이걸 오브젝트풀에서 가져오게 하면될듯

        GameObject go = ObjectPoolManager.Instance.SpawnFromPool(
            (int)_projectileTag[num],
            _rangeAttackPos[num].transform.position,
            _rangeAttackPos[num].transform.rotation);

        Vector3 directionToTarget = target.position - transform.position;
        directionToTarget.y = 0f;
        directionToTarget.Normalize();

        ProjectileTest projectile = go.GetComponent<ProjectileTest>();

        projectile.AddTarget(LayerData.Player); // Player를 맞춰야함
        projectile.AddExcludeLayer(LayerData.Enemy); // Enemy는 충돌하지 않게 설정

        float value = projectile.GetDamageType == DamageType.Physical ? currentStat.meleeAtk : currentStat.magicAtk;
        projectile.SetValue(value);

        //projectile.rb.AddForce(directionToTarget * 10f, ForceMode.Impulse);
        projectile.SetVelocity(1f); // 속도 배율 설정
    }

    void StartSpawn()
    {
        Invoke(nameof(SpawnEnd), 1f);
    }

    void SpawnEnd()
    {
        IsSpawning = false;
        Collider.enabled = true;
    }

    void DeSpawn()
    {
        gameObject.SetActive(false);
    }

    void InvokeOnDieActiveFalse()
    {
        Invoke(nameof(ActiveFalse), 1.5f);
    }

    void ActiveFalse()
    {
        gameObject.SetActive(false);
    }

    void DropGold()
    {

    }
}
