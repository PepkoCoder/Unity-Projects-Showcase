using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AIController : CharacterController
{
    AIManager aiManager;
    Vector3 moveDir = Vector3.zero;

    [Header("AI")]
    public BotDifficultyPreset botDifficultyPreset;

    public LayerMask whatIsObstacle;
    public LayerMask whatIsTeammate;

    public float maxDistanceForRunning = 30f;
    public float minDistanceForRunning = 8f;
    public float movementSmoothing = 0.1f;
    public float minMovementInput = 0.3f;
    public float jumpOneMinHeight = 10f;
    public float jumpTwoMinHeight = 25f;
    public float timeBetweenJumps = 0.3f;
    public float jumpTimeOut = 3f;
    public float minShootDistance = 10f;
    public float ballTargetOffset = 10f;
    public float minGoalDistance = 50f;
    public float runningTimeOut = 3f;
    public float dirRefreshTime = 0.2f;
    public float minDistanceFromBallToJump = 20f;

    float runningTimer = 0;
    float refreshTimer = 0;
    float jumpTimer = 0;
    float jumpTimeOutTimer = 0;

    Vector3 targetPos;
    Vector3 targetDir;
    List<CharacterController> teammates = new List<CharacterController>();

    protected override void Start()
    {
        base.Start();

        aiManager = GetComponent<AIManager>();
        running = false;

        botDifficultyPreset.LoadVariables(this);

        Invoke("FindTeammates", 0.1f);
    }

    void FindTeammates()
    {
        foreach(CharacterController cc in FindObjectsOfType<CharacterController>())
        {
            if((cc != this && cc.GetComponent<CharacterManager>().team == aiManager.team))
            {
                teammates.Add(cc);
            }
        }
    }

    void Update()
    {
        if(!GameManager.instance.started || ScoreManager.instance.gameOver || GameManager.instance.paused)
        {
            rg.velocity = new Vector3(0f, y, 0f);
            return;
        }

        Falling();
        Running();

        ShowStamina();
        AnimationSetup();
        
        if(aiManager.goalToShoot != null && aiManager.ball != null && !knockback)
        {
            if (Time.time > refreshTimer)
            {
                refreshTimer = Time.time + dirRefreshTime;
                SetNewDirection();
            }

            SetupMovement();
            Rotate(moveDir);

            Jumping();
        }
        else
        {
            if (Time.time >= knockbackTimer)
            {
                knockback = false;
            }
        }

        if (!running && runningTime != 0) runningTime = 0;
        
        rg.velocity = new Vector3(rg.velocity.x, y, rg.velocity.z);
    }

    void Jumping()
    {
        if (Vector3.Distance(transform.position, aiManager.ball.position) <= minDistanceFromBallToJump)
        {
            if(Time.time >= jumpTimeOutTimer && Time.time > jumpTimer)
            {
                if (aiManager.ball.position.y >= jumpOneMinHeight)
                {
                    Jump();
                }

                if (transform.position.y >= jumpOneMinHeight && aiManager.ball.position.y >= jumpTwoMinHeight)
                {
                    Jump();
                }
            }
        }
    }

    void Jump()
    {
        jumpTimer = Time.time + timeBetweenJumps;

        if (currJumps - 1 <= 0)
        {
            jumpTimeOutTimer = Time.time + jumpTimeOut;
            return;
        }

        currJumps--;
        y = jumpSpeed;

        soundManager.Play("Jump_" + currJumps);

        touchedGround = false;

        aiManager.graphicsParent.localScale = scale;
        aiManager.graphicsParent.DOScale(new Vector3(scale.x * 0.6f, scale.y * 1.3f, scale.z * 0.6f), 0.5f).From();

        Instantiate(jumpDustParticles, groundDetection.position, Quaternion.Euler(90, 0f, 0f));
    }

    protected override void StopRunning()
    {
        base.StopRunning();

        runningTimer = Time.time + runningTimeOut;
    }

    void SetNewDirection()
    {
        Vector3 ballToGoalDir = (aiManager.goalToShoot.position - aiManager.ball.position).normalized;
        ballToGoalDir = new Vector3(ballToGoalDir.x, 0f, ballToGoalDir.z);

        Vector3 ballPos = aiManager.ball.position;
        ballPos = new Vector3(ballPos.x, 0f, ballPos.z);

        float currOffset = (TargetInsideWall()) ? ballTargetOffset : ballTargetOffset * 0.2f;

        targetPos = ballPos + (ballToGoalDir * currOffset);

        if (GoingTowardsMyGoal() && Vector3.Distance(aiManager.myGoal.position, transform.position) < minGoalDistance)
        {
            int solution = Random.Range(0, 2);

            if (Vector3.Distance(transform.position, aiManager.ball.position) <= minDistanceFromBallToJump)
            {
                if (Time.time >= jumpTimeOutTimer && Time.time > jumpTimer)
                {

                    if (solution == 1)
                    {
                        Jump();
                    }
                }
            }
            else
            {
                targetPos = ballPos + Vector3.forward * 5f;
            }

            refreshTimer = Time.time + dirRefreshTime * 2f;
        }

        targetPos = AvoidTeammates();
        targetPos = new Vector3(targetPos.x, 5f, targetPos.z);
        targetDir = (targetPos - transform.position).normalized;
        
        if(Vector3.Distance(transform.position, targetPos) <= minShootDistance)
        {
            targetDir = (aiManager.goalToShoot.position - transform.position);
        }

        if (WallInfront())
        {
            Vector3 turnDir = GetTurnDirection();
            targetDir = (turnDir != Vector3.zero) ? turnDir : targetDir;
        }

        targetDir = targetDir.normalized;

        moveDir = new Vector3(targetDir.x, 0f, targetDir.z);
    }

    void SetupMovement()
    {
        if (moveDir.x < -minMovementInput) moveDir.x = -1;
        if (moveDir.x > minMovementInput) moveDir.x = 1;
        if (moveDir.z < -minMovementInput) moveDir.z = -1;
        if (moveDir.z > minMovementInput) moveDir.z = 1;

        float distanceFromBall = Vector3.Distance(transform.position, aiManager.ball.position);

        if (distanceFromBall >= maxDistanceForRunning || distanceFromBall <= minDistanceForRunning)
        {
            if(Time.time >= runningTimer && !running)
            {
                StartRunning();
            }
        }
        else
        {
            if (running)
            {
                StopRunning();
            }
        }

        MoveLerped(moveDir, movementSmoothing);
    }

    Vector3 AvoidTeammates()
    {
        Vector3 target = targetPos;
        Collider[] players = Physics.OverlapSphere(targetPos, 2f, whatIsTeammate);

        foreach(Collider c in players)
        {
            foreach(CharacterController cc in teammates)
            {
                if (c.gameObject == cc.gameObject)
                {
                    targetPos += (Random.Range(0, 2) == 2) ? Vector3.forward * ballTargetOffset : -Vector3.forward * ballTargetOffset;
                }
            }
        }

        return target;
    }

    Vector3 GetTurnDirection()
    {
        Vector3 turn = Vector3.zero;
        Vector3 pos = new Vector3(transform.position.x, 10f, transform.position.z);

        if (Physics.Raycast(pos, -transform.right, 20f, whatIsObstacle))
        {
            turn = transform.right;
        }
        else if (Physics.Raycast(pos, transform.right, 20f, whatIsObstacle))
        {
            turn = -transform.right;
        }

        return turn;
    }

    bool WallInfront()
    {
        Vector3 pos = new Vector3(transform.position.x, 10f, transform.position.z);
        return Physics.Raycast(pos, transform.forward + transform.right * 0.2f, 10f, whatIsObstacle) || Physics.Raycast(pos, transform.forward - transform.right * 0.2f, 10f, whatIsObstacle);
    }

    bool GoingTowardsMyGoal()
    {
        bool going = false;

        float dotProduct = Vector3.Dot(transform.forward, aiManager.myGoal.forward);

        if(dotProduct < 0)
        {
            going = true;
        }

        return going;
    }

    bool TargetInsideWall()
    {
        return (Physics.OverlapSphere(targetPos, 1.5f, whatIsObstacle)) != null;
    }

    void AnimationSetup()
    {
        if (moveDir != Vector3.zero)
        {
            anim.SetInteger("Speed", 1);

            if (running)
            {
                anim.SetInteger("Speed", 2);
            }
        }
        else
        {
            anim.SetInteger("Speed", 0);
        }

        if (!Grounded())
        {
            anim.SetInteger("Speed", 0);
        }
    }
}
