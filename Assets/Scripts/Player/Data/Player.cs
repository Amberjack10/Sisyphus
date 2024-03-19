using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : CharacterBehaviour
{
    [field: Header("References")]
    [field: SerializeField] public PlayerBaseData Data {  get; private set; }
    public Status modifire;

    [field: Header("Animations")]
    [field: SerializeField] public PlayerAnimationData AnimationData {  get; private set; }

    public Rigidbody Rigidbody { get; private set; }
    public Animator Animator { get; private set; }
    public PlayerInput Input { get; private set; }
    public CharacterController Controller { get; private set; }
    public HealthSystem HealthSystem;

    private PlayerStateMachine stateMachine;

    public float hitDelay;

    private void Awake()
    {
        AnimationData.Initialize();
        Data = DataBase.Player.Get(id);
        currentStat.InitStatus(Data, modifire);
        Rigidbody = GetComponent<Rigidbody>();
        Animator = GetComponentInChildren<Animator>();
        Input = GetComponent<PlayerInput>();
        Controller = GetComponent<CharacterController>();
        HealthSystem = GetComponent<HealthSystem>();

        stateMachine = new PlayerStateMachine(this);
    }

    private void Start()
    {
        stateMachine.ChangeState(stateMachine.idleState);
    }

    private void Update()
    {
        stateMachine.HandleInput();
        stateMachine.Update();
    }

    private void FixedUpdate()
    {
        stateMachine.PhysicsUpdate();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer > 6 /*&& player.hitDelay > 0.5f*/)
        {
            HealthSystem.TakeDamage(20f);
            if(isHit)
            {
                stateMachine.ChangeState(stateMachine.hitState);
            }
            if(isDie)
            {
                stateMachine.ChangeState(stateMachine.dieState);
            }
        }
    }

    
}
