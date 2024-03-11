using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateMachine : StateMachine
{
    public Enemy Enemy {  get; private set; }

    // States
    public EnemyIdleState IdleState { get; private set; }
    public EnemyChasingState ChasingState { get; private set; }
    public EnemyAttackState AttackState { get; private set; }

    public EnemyStateMachine(Enemy enemy)
    {
        Enemy = enemy;

        IdleState = new(this);
        ChasingState = new(this);
        AttackState = new(this);
    }
}
