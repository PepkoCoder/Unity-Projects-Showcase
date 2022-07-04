using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour {

    [Header("Movement")]
    public float movementSpeed;
    public float lookRadious = 15f;

    [Header("Attack")]
    public float damage = 10f;
    public float attackDistance = 1f;
    public float attackCooldown = 1f;

    [HideInInspector] public bool canAttack;
    [HideInInspector] public bool attacking;

    protected Transform player;
    protected Health playerHP;
    protected Animator anim;

    protected State state = State.IDLE;

    // Use this for initialization
    public virtual void Start () {

        player = PlayerManager.instance.player.transform;
        anim = GetComponentInChildren<Animator>();
        playerHP = player.GetComponent<Health>();
    }

    // Update is called once per frame
    public virtual void Update () {

        float distance = Vector3.Distance(transform.position, player.position);

        if(state == State.IDLE)
        {
            anim.SetBool("Running", false);

            Idle();

            if(distance <= lookRadious)
            {
                state = State.MOVE;
            }
        }
        else if(state == State.MOVE)
        {
            anim.SetBool("Running", true);

            Move();

            if (distance <= attackDistance && !attacking)
            {
                state = State.ATTACK;
            }
            
            if(distance > lookRadious)
            {
                state = State.IDLE;
            }
        } 
        else if (state == State.ATTACK)
        {
            if (!attacking)
            {
                Attack();
            }

            if (distance > attackDistance || attacking)
            {
                state = State.MOVE;
            }
        }

        FaceTarget(player);
    }
    #region Movement
    protected void FaceTarget(Transform target)
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0f, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 5f * Time.deltaTime);
    }

    public virtual void Move()
    {
        // Movement
    }

    public virtual void Idle()
    {
        //Idle
    }
    #endregion

    #region Attack
    public virtual void Attack()
    {
        //Do attack stuff
    }

    public void Knockback(float amount)
    {
        Vector3 direction = (player.position - transform.position);
        Vector3 dir = new Vector3(direction.x, direction.y + 2, direction.z);

        player.GetComponent<PlayerMotor>().Knockback(dir, amount);
    }
    #endregion
}

public enum State
{
    IDLE,
    MOVE,
    ATTACK
}
