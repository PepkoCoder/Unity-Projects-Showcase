using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMelee : EnemyController {

    [Header("Attack")]
    public bool applyKnockback;
    public float knockbackAmount = 2f;
    public float chargeAttack = 0.3f;
    public LayerMask hittable;

    [Header("Effects")]
    public GameObject impactEffect;

    NavMeshAgent agent;

    public override void Start()
    {
        base.Start();

        agent = GetComponent<NavMeshAgent>();

        agent.speed = movementSpeed;
        agent.stoppingDistance = attackDistance - 0.2f;
    }

    public override void Idle()
    {
        base.Idle();

        if (!agent.isStopped)
        {
            agent.isStopped = true;
        }
    }

    public override void Move()
    {
        base.Move();

        agent.isStopped = false;
        agent.SetDestination(player.position);
    }

    public override void Attack()
    {
        base.Attack();

        agent.isStopped = true;
        StartCoroutine(Charge());
    }

    public void CloseHit()
    {
        Vector3 attackPosition = new Vector3(transform.position.x, 1f, transform.position.z);

        RaycastHit hit;
        if (Physics.Raycast(attackPosition, transform.forward, out hit, attackDistance, hittable))
        {
            Instantiate(impactEffect, hit.point, Quaternion.identity);

            Health hp = hit.transform.GetComponent<Health>();

            if (hp != null)
            {
                hp.TakeDamage(damage);

                if(hit.transform.tag == "Player")
                {
                    if (applyKnockback)
                    {
                        Knockback(knockbackAmount);
                    }
                }
            }
        }
    }

    public void FinishAttack()
    {
        anim.SetBool("Attacking", false);
    }

    IEnumerator Charge()
    {
        attacking = true;

        yield return new WaitForSeconds(chargeAttack);

        anim.SetBool("Attacking", true);

        yield return new WaitForSeconds(attackCooldown);

        attacking = false;
    }
}
