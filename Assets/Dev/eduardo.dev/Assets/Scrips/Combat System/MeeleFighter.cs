using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Internal.Execution;
using UnityEngine;

public enum AttackState { Idle, Windup, Impact, Cooldown }

public class MeeleFighter : MonoBehaviour
{
    [SerializeField] List<AttackData> attacks;
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

    AttackState attackState;
    bool doCombo;
    int comboCount = 0;
    public bool InAction {get; private set;} = false;
    public void TryToAttack()
    {
        if (!InAction)
        {
            StartCoroutine(Attack());
        }
        else if (attackState == AttackState.Impact || attackState == AttackState.Cooldown)
        {
            doCombo = true;
        }
    }

    IEnumerator Attack()
    {
        InAction = true;
        attackState = AttackState.Windup;

        animator.CrossFade(attacks[comboCount].AnimName, 0.2f);
        yield return null;

        var animState = animator.GetNextAnimatorStateInfo(1);

        float timer = 0f;
        while (timer <= animState.length)
        {
            timer += Time.deltaTime;
            float normalizedTime = timer / animState.length;

            if (attackState == AttackState.Windup)
            {
                if (normalizedTime >= attacks[comboCount].ImpactStartTime)
                {
                    attackState = AttackState.Impact;
                    axeCollider.enabled = true;
                }
            }
            else if (attackState == AttackState.Impact)
            {
                if (normalizedTime >= attacks[comboCount].ImpactEndTime)
                {
                    attackState = AttackState.Cooldown;
                    axeCollider.enabled = false;
                }
            }
            else if (attackState == AttackState.Cooldown)
            {
                if (doCombo)
                {
                    doCombo = false;
                    comboCount = (comboCount + 1) % attacks.Count;

                    StartCoroutine(Attack());
                    yield break;
                }
            }

            yield return null;
        }

        attackState = AttackState.Idle;
        comboCount = 0;
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

        yield return new WaitForSeconds(animState.length * 0.8f);

        InAction = false;
    }
}
