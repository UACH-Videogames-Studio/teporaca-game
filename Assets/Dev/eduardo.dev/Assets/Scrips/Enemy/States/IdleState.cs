using Unity.VisualScripting;
using UnityEngine;

public class IdleState : State<EnemyController>
{
    EnemyController enemy;
    public override void Enter(EnemyController owner)
    {
        enemy = owner;

        enemy.Animator.SetInteger("weaponType", 0);
    }

    public override void Execute()
    {
        enemy.Target = enemy.FindTarget();
        if (enemy.Target != null)
            enemy.ChangeState(EnemyStates.CombatMovement);
    }

    public override void Exit()
    {
        
    }
}
