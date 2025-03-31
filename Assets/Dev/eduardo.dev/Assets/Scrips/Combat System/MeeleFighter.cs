using System.Collections;
using NUnit.Framework.Internal.Execution;
using UnityEngine;

public enum AttackState { Idle, Windup, Impact, Cooldown }

public class MeeleFighter : MonoBehaviour
{

    [SerializeField] GameObject axe;

    BoxCollider axeCollider;

    Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        if (axe != null)
        {
            axeCollider = axe.GetComponent<BoxCollider>();
            axeCollider.enabled = false;
        }
    } 

    public AttackState attackState;

    public bool InAction {get; private set;} = false;
    public void TryToAttack()
    {
        if (!InAction)
        {
            StartCoroutine(Attack());
        }
    }

    IEnumerator Attack()
    {
        InAction = true;
        attackState = AttackState.Windup;

        float impactStartTime = 0.3f;
        float impactEndTime = 0.5f;


        animator.CrossFade("Horizontal attack", 0.2f);
        yield return null;

        var animState = animator.GetNextAnimatorStateInfo(1);

        float timer = 0f;
        while (timer <= animState.length)
        {
            timer += Time.deltaTime;
            float normalizedTime = timer / animState.length;

            if (attackState == AttackState.Windup)
            {
                if (normalizedTime >= impactStartTime)
                {
                    attackState = AttackState.Impact;
                    axeCollider.enabled = true;
                }
            }
            else if (attackState == AttackState.Impact)
            {
                if (normalizedTime >= impactEndTime)
                {
                    attackState = AttackState.Cooldown;
                    axeCollider.enabled = false;
                }
            }
            else if (attackState == AttackState.Cooldown)
            {
                // To do: Handle combos
            }

            yield return null;
        }

        attackState = AttackState.Idle;
        InAction = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Hitbox" && !InAction)
        {
            StartCoroutine(PlayHitReaction());
        }
    }

    IEnumerator PlayHitReaction()
    {
        InAction = true;
        animator.CrossFade("Impact", 0.2f);
        yield return null;

        var animState = animator.GetNextAnimatorStateInfo(1);

        yield return new WaitForSeconds(animState.length);

        InAction = false;
    }
}
