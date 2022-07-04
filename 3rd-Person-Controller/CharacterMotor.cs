using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInput))]
public class CharacterMotor : MonoBehaviour
{
    [Header("Movement")]
    public float maxVelocity = 10;
    public float acceleration = 7;
    public float deceleration = 7;
    public float velPower = 0.9f;

    [Header("Jumping")]
    public float jumpVelocity = 32;
    public float gravity = -15;
    public int jumps = 1;

    [Header("Ground Check")]
    public float groundCheckRadius = 0.32f;
    public float groundCheckBuffer = 0.2f;
    public Transform feet;
    public Transform head;
    public LayerMask whatIsGround;

    [Header("Cinemachine")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    public GameObject followTarget;
    public float rotationSpeed = 2f;

    //Jumping and Gravity
    private float _groundedTimer = 0f;
    private bool _grounded;
    private bool _ceiling;
    private float _verticalVelocity = 0f;
    private int _jumpsRemaining = 0;

    //Input
    private Vector2 _input;
    private Vector3 _moveDir;
    private Vector2 _look;

    private Rigidbody _rb;
    private PlayerInput _playerInput;

    public delegate void OnJump();
    public OnJump onJump;

    public delegate void OnLand();
    public OnLand onLand;

    public delegate void OnRoofHit();
    public OnRoofHit onRoofHit;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _playerInput = GetComponent<PlayerInput>();

        _playerInput.onJumpButtonReleased += CutJump;
        _playerInput.onJumpButtonPressed += Jump;

        _jumpsRemaining = jumps;
    }

    void Update()
    {
        GetInput();
        CheckGrounded();
        CheckCeiling();
        CalculateVerticalVelocity();
    }

    private void FixedUpdate()
    {
        ApplyGravity();
        Move();
    }

    private void LateUpdate()
    {
        CameraRotation();
    }

    void GetInput()
    {
        _input = _playerInput.moveInput;
        _look = _playerInput.lookInput;

        //Get the desired movement direction
        _moveDir = new Vector3(_input.x, 0f, _input.y);
        _moveDir.Normalize();

        _moveDir = Quaternion.Euler(transform.rotation.eulerAngles) * _moveDir;
    }

    private void CheckGrounded()
    {
        bool wasGrounded = _grounded;
        _groundedTimer -= Time.deltaTime;

        Collider[] groundHitColliders = Physics.OverlapSphere(feet.position, groundCheckRadius, whatIsGround);
        bool notTrigger = false;

        for (int i = 0; i < groundHitColliders.Length; i++)
        {
            if(!groundHitColliders[i].isTrigger)
            {
                notTrigger = true;
                break;
            }
        }

        _grounded = groundHitColliders.Length > 0 && notTrigger;

        if (_grounded)
        {
            _groundedTimer = groundCheckBuffer;

            if (wasGrounded != _grounded)
            {
                _jumpsRemaining = jumps;

                if(_input == Vector2.zero)
                {
                    //Grab to the floor you are landing on, so the player doesn't slide of the platforms
                    _rb.velocity = new Vector3(0f, _rb.velocity.y, 0f);
                }

                if (onLand != null)
                    onLand();
            }

        }
    }

    private void CheckCeiling()
    {
        bool wasCeiling = _ceiling;

        Collider[] ceilingHitColliders = Physics.OverlapSphere(head.position, groundCheckRadius, whatIsGround);
        bool notTrigger = false;

        for (int i = 0; i < ceilingHitColliders.Length; i++)
        {
            if (!ceilingHitColliders[i].isTrigger)
            {
                notTrigger = true;
                break;
            }
        }

        _ceiling = ceilingHitColliders.Length > 0 && notTrigger;

        if (_ceiling && !wasCeiling)
        {
            _verticalVelocity = 0f;

            if (onRoofHit != null)
                onRoofHit();
        }
    }

    private void CalculateVerticalVelocity()
    {
        if (!_grounded)
        {
            _verticalVelocity += gravity * Time.deltaTime;
        }
        else if (_verticalVelocity < 0f)
        {
            _verticalVelocity = 0f;
        }
    }

    private void ApplyGravity()
    {
        _rb.velocity = new Vector3(_rb.velocity.x, _verticalVelocity, _rb.velocity.z);
    }

    public void Move()
    {
        //Calculate the target velocity for the x and z axis
        float targetVelocityX = _moveDir.x * maxVelocity;
        float targetVelocityZ = _moveDir.z * maxVelocity;

        //Calculate the speed dif between our current velocity and target velocity
        float speedDifX = targetVelocityX - _rb.velocity.x;
        float speedDifZ = targetVelocityZ - _rb.velocity.z;

        //Decide if we want to accelerate or deccelerate (if we are moving or not)
        float accelRate = (Mathf.Abs(targetVelocityX) >= 0.01f) ? acceleration : deceleration;

        //Applies acceleration to speed difference, then raises to set power so acceleration increases with higher speeds
        float movementX = Mathf.Pow(Mathf.Abs(speedDifX) * acceleration, velPower) * Mathf.Sign(speedDifX);
        float movementZ = Mathf.Pow(Mathf.Abs(speedDifZ) * acceleration, velPower) * Mathf.Sign(speedDifZ);

        Vector3 movement = new Vector3(movementX, 0f, movementZ);

        //Applies the new calculated movement vector
        _rb.AddForce(movement);

    }

    public void Jump() 
    {
        bool canJump = _grounded || (_groundedTimer > 0f && _rb.velocity.y <= 0f) || (_jumpsRemaining > 0 && jumps > 1);

        if (canJump)
        {
            _verticalVelocity = jumpVelocity;
            _jumpsRemaining--;

            if (onJump != null)
                onJump();
        }
    }

    private void CutJump()
    {
        if(_verticalVelocity > 0f)
            _verticalVelocity /= 2f;
    }

    private void CameraRotation()
    {

        followTarget.transform.rotation *= Quaternion.AngleAxis(_look.x * rotationSpeed, Vector3.up);
        followTarget.transform.rotation *= Quaternion.AngleAxis(_look.y * rotationSpeed, Vector3.right);

        var angles = followTarget.transform.localEulerAngles;
        angles.z = 0;

        var angle = followTarget.transform.localEulerAngles.x;

        if(angle > 180f && angle < 340f)
        {
            angles.x = 340f;
        }
        else if(angle < 180f && angle > 40f)
        {
            angles.x = 40;
        }

        followTarget.transform.localEulerAngles = angles;

        //Rotates the player towards camera direction
        transform.rotation = Quaternion.Euler(0f, followTarget.transform.rotation.eulerAngles.y, 0f);

        followTarget.transform.localEulerAngles = new Vector3(angles.x, 0f, 0f);

    }
    
    #region Debug

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(feet.position, groundCheckRadius);
    }

    #endregion
}
