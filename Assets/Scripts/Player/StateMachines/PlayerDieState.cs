public class PlayerDieState : PlayerBaseState
{
    public PlayerDieState(PlayerStateMachine playerstateMachine) : base(playerstateMachine)
    {
    }

    public override void Enter()
    {
        StartAnimation(stateMachine.Player.AnimationData.DieParameterHash);
        player.enabled = false;
        GameManager.Instance.Gameover();
        SoundManager.Instance.PlayAudioClip(player.DieSound);
    }

    public override void Exit()
    {
        base.Exit();
        StopAnimation(stateMachine.Player.AnimationData.DieParameterHash);
        player.enabled = true;
    }


}
