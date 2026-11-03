using System;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    private CharacterController controller;
    public Animator animator;

    // movement variables
    [Header("Movement Variables")]
    public float moveSpeed = 7f;
    private Vector3 velocity;

    [Header("Crouch Variables")]
    public bool canCrouch = false;

    [Header("Jump Variables")]
    public bool canDoubleJump = false;
    public float baseJumpHeight = 2.2f;
    public float gravity = -20f;
    private bool hasDoubleJumped;

    [Header("Dash Variables")]
    public bool canDash = false;
    private bool isDashing;
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1.0f;
    private float dashTimer = 0f;
    private float nextDashTime = 0f;

    // event actions
    public event Action OnPlayerDeath;
    public event Action OnPlayerRespawn;

    // respawn and death variables
    public Vector3 currentCheckpoint;
    public Vector3 startPoint;
    public float killZThreshold = -15f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // save starting point and checkpoint
        startPoint = transform.position;
        currentCheckpoint = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        // check if player fell off the map
        if (transform.position.y < killZThreshold)
        {
            Die();
        }

        bool grounded = controller.isGrounded;

        if (grounded && velocity.y < 0f)
        {
            velocity.y = -2f;
            hasDoubleJumped = false;
        }

        Vector3 finalVelocity = Vector3.zero;
        
        if (isDashing)
        {
            finalVelocity = HandleDash();
        } 
        else
        {
            HandleDashInput();

            Vector3 wasdVelocity = HandleMovement();
            HandleJump(grounded);
            ApplyGravity(grounded);

            finalVelocity = wasdVelocity + velocity;
        }

        controller.Move(finalVelocity * Time.deltaTime);
    }

    Vector3 HandleMovement()
    {
        float hor = Input.GetAxisRaw("Horizontal");
        float vert = Input.GetAxisRaw("Vertical");

        Vector3 finalMoveVel = new Vector3(0, 0, 0);
        Vector3 newPosition = new Vector3(hor, 0f, vert).normalized;

        if (newPosition.magnitude >= 0.1f)
        {
            transform.forward = Vector3.Slerp(transform.forward, newPosition, Time.deltaTime * 15f);
            finalMoveVel = newPosition * moveSpeed;
        }

        if (animator != null)
        {
            animator.SetFloat("Speed", newPosition.magnitude);
            Debug.Log("new speed: " + newPosition.magnitude);
        }

        return finalMoveVel;
    }
    
    void HandleJump(bool grounded)
    {
        Vector3 finalVertVel = Vector3.zero;

        if (grounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }

        if (Input.GetButtonDown("Jump"))
        {
            if (grounded)
            {
                velocity.y = Mathf.Sqrt(baseJumpHeight * -2f * gravity);
                if (animator != null) animator.SetTrigger("Jump");
                Debug.Log("i am able to jump");
            }
            else if (canDoubleJump && !hasDoubleJumped)
            {
                hasDoubleJumped = true;
                velocity.y = Mathf.Sqrt(baseJumpHeight * -2f * gravity);
                if (animator != null) animator.SetTrigger("Jump");
            }
        }
    }

    private void HandleDashInput()
    {
        if (!canDash)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && Time.time >= nextDashTime)
        {
            isDashing = true;
            dashTimer = dashDuration;
            nextDashTime = Time.time + dashCooldown;

            velocity.y = 0f;

            if (animator != null)
            {
                animator.SetTrigger("Dash");
            }
        }
    }

    private Vector3 HandleDash()
    {
        Vector3 dashVel = Vector3.zero;

        if (dashTimer > 0f)
        {
            dashVel = transform.forward * dashSpeed;
            dashTimer -= Time.deltaTime;
        } 
        else
        {
            isDashing = false;
        }

        return dashVel;
    }

    private void ApplyGravity(bool grounded)
    {
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
    
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("DashPickup"))
        {
            canDash = true;
            Destroy(hit.gameObject);
            Debug.Log("Dash Unlocked!");
        }
    }

    public void Die()
    {
        // trigger death event so inventory drops coins
        OnPlayerDeath?.Invoke();

        // need to disable controller before teleporting
        controller.enabled = false;
        transform.position = currentCheckpoint;
        controller.enabled = true;

        // fire respawn event
        OnPlayerRespawn?.Invoke();
    }

    public void ResetPosition()
    {
        controller.enabled = false;
        transform.position = startPoint;
        controller.enabled = true;
    }

    public void SetCheckpoint(Vector3 newPosition)
    {
        currentCheckpoint = newPosition;
    }
}