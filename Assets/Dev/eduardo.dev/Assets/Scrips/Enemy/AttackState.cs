using UnityEngine;

public class AttackState : State<EnemyController>
{
   EnemyController enemy;

   public override void Enter (EnemyController ownwer)
   {
        enemy = ownwer;
   }
}
