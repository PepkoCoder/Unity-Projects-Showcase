using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyRanged : EnemyController {

    [Header("Shooting")]
    public bool mustHavePlayerInSight = true;
    public GameObject projectile;
    public Transform shootPosition;
    public LayerMask detectable;

    [Header("Movement")]
    public float timeBtwnMovement = 5f;
    bool canMove = true;
    NavMeshAgent agent;

    Vector3 dirToPlayer;

    public override void Start()
    {
        base.Start();

        agent = GetComponent<NavMeshAgent>();
        agent.speed = movementSpeed;
    }

    #region Movement
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

        if (canMove)
        {
            agent.isStopped = false;

            canMove = false;

            Vector3 curPos = transform.position;
            Vector3 newPos = new Vector3(curPos.x + Random.Range(-3f, 3f), curPos.y, curPos.z + Random.Range(-3f, 3f));

            agent.SetDestination(newPos);

            StartCoroutine(Wait());
        }
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(timeBtwnMovement);
        canMove = true;
    }
    #endregion

    #region Attack
    public override void Attack()
    {
        base.Attack();

        if (mustHavePlayerInSight)
        {
            if (!PlayerInSight()) return;
        }

        if (!attacking)
        {
            StartCoroutine(Shoot());
        }
    }

    IEnumerator Shoot()
    {
        attacking = true;

        anim.SetBool("Attacking", true);

        GameObject p = Instantiate(projectile, shootPosition.position, Quaternion.LookRotation(dirToPlayer));
        p.GetComponent<Projectile>().damage = damage;

        yield return new WaitForSeconds(attackCooldown);

        attacking = false;

        anim.SetBool("Attacking", false);
    }

    bool PlayerInSight()
    {
        dirToPlayer = player.position - shootPosition.position;
        dirToPlayer.y = 0f;

        RaycastHit hit;
        if (Physics.Raycast(shootPosition.position, dirToPlayer, out hit, attackDistance, detectable))
        {
            if (hit.transform.tag == "Player")
            {
                return true;
            }
        }
        

        return false;
    }
    #endregion
}
