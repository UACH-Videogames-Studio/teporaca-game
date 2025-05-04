using UnityEngine;

public class RetreatAfterAttackState : State<EnemyController>
{
    EnemyController enemy;
    
    [SerializeField] float backwardWalkSpeed = 1.5f;
    [SerializeField] float distanceToRetreat = 3f;

    public override void Enter(EnemyController owner)
    {
        enemy = owner;
        enemy.Animator.SetFloat("forwardSpeed", -0.2f);
    }

    public override void Execute()
    {
        if (Vector3.Distance(enemy.transform.position, enemy.Target.transform.position) >= distanceToRetreat)
        {
            enemy.ChangeState(EnemyStates.CombatMovement);
            return;
        }

        var vecToTarget = enemy.Target.transform.position - enemy.transform.position;
        enemy.NavAgent.Move(-vecToTarget.normalized * backwardWalkSpeed * Time.deltaTime);

        vecToTarget.y = 0;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(vecToTarget), 500 * Time.deltaTime);
    }

    public override void Exit()
    {
        enemy.Animator.SetFloat("forwardSpeed", 0f);
    }

}
