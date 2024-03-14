using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyChasingState : EnemyBaseState
{
    public EnemyChasingState(EnemyStateMachine enemyStateMachine) : base(enemyStateMachine)
    {
        enemy.chasingDelay = 0;
    }

    public override void Enter()
    {
        base.Enter();

        StartAnimation(EnemyAnimationData.MoveParameterHash);

        agent.speed = curStat.moveSpeed;
        agent.isStopped = false;
        agent.SetDestination(enemy.target.position);

        agent.avoidancePriority = EnemyData.ChasingPriority; // 움직일때 다른 상태의 몹들을 밀지않게 하기 위해
    }

    public override void Update()
    {
        base.Update();

        enemy.chasingDelay += Time.deltaTime;

        // 공격이 가능하다면
        if (CanAttack())
        {
            stateMachine.ChangeState(stateMachine.AttackState);
            return;
        }

        // target의 위치 갱신
        if (CanChase())
        {
            enemy.chasingDelay = 0;
            // 추후 Player의 진행방향과 속도를 가지고 보스 or 정예몹은 Player의 움직임을 예측해서 추적하는 방식으로 해도 괜찮을듯
            agent.SetDestination(enemy.target.position);
            return;
        }

        // target이 null이면 (죽었으면?) idle로 전환
        if (HasTarget() == false)
        {
            stateMachine.ChangeState(stateMachine.IdleState);
            return;
        }
    }

    public override void Exit()
    {
        base.Exit();

        StopAnimation(EnemyAnimationData.MoveParameterHash);

        agent.isStopped = true; // 추적을 멈추기 위해서

        agent.avoidancePriority = EnemyData.DefaultPriority;
    }

    bool CanChase()
    {
        // 매 프레임 호출하면 성능적으로 안좋을거 같아서 등급별로 호출 횟수를 다르게 설정
        return enemy.chasingDelay >= EnemyData.ChasingDelay[(int)info.rank];
    }
}
