using UnityEngine;

public class DeadState : State<EnemyController>
{
    override public void Enter(EnemyController owner)
    {
        owner.VisionSensor.gameObject.SetActive(false);
        EnemyManager.i.RemoveEnemyInRange(owner);

        owner.NavAgent.enabled = false;
        owner.CharacterController.enabled = false;
    }
}
