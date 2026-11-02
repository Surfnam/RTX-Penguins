using System;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    private CharacterController controller;
    public Animator animator;

    // movement variables
    public float moveSpeed = 7f;
    private Vector3 velocity;

    // crouch variables
    public bool canCrouch = false;

    // jump variables
    public bool canDoubleJump = false;
    public float baseJumpHeight = 2.2f;
    public float gravity = -20f;
    private bool hasDoubleJumped;

    // dash variables
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

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        bool grounded = controller.isGrounded;
        if (grounded)
        {
            hasDoubleJumped = false;
        }

        Vector3 dashVelocity = HandleDashInput();

        if (isDashing)
        {
            HandleDash();
            return;
        }

        Vector3 wasdVelocity = HandleMovement();
        Vector3 verticalVelocity = HandleJump(grounded);
        Vector3 gravVelocity = ApplyGravity(grounded);

        Vector3 finalVelocity = dashVelocity + wasdVelocity + verticalVelocity + gravVelocity;
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
        }

        return finalMoveVel;
    }

    Vector3 HandleJump(bool grounded)
    {
        Vector3 finalVertVel = Vector3.zero;
        if (grounded && velocity.y < 0f)
        {
            finalVertVel.y = -2f;
        }

        if (Input.GetButtonDown("Jump"))
        {
            if (grounded)
            {
                finalVertVel.y = Mathf.Sqrt(baseJumpHeight * -2f * gravity);
                if (animator != null) animator.SetTrigger("Jump");
                Debug.Log("i am able to jump");
            }
            else if (canDoubleJump && !hasDoubleJumped)
            {
                hasDoubleJumped = true;
                finalVertVel.y = Mathf.Sqrt(baseJumpHeight * -2f * gravity);
                if (animator != null) animator.SetTrigger("Jump");
            }
        }

        return finalVertVel;

    }

    private Vector3 HandleDashInput()
    {
        Vector3 finalDashVel = new Vector3(0, -2, 0);
        
        if (Input.GetKeyDown(KeyCode.LeftShift) && Time.time >= nextDashTime)
        {
            isDashing = true;
            dashTimer = dashDuration;
            nextDashTime = Time.time + dashCooldown;

            finalDashVel.y = 0f;

            if (animator != null)
            {
                animator.SetTrigger("Dash");
            }
        }

        return finalDashVel;
    }

    private void HandleDash()
    {
        if (dashTimer > 0f)
        {
            dashTimer -= Time.deltaTime;
        } 
        else
        {
            isDashing = false;
        }
    }

    private Vector3 ApplyGravity(bool grounded)
    {
        Vector3 finalGravVel = Vector3.zero;
        finalGravVel.y += gravity * Time.deltaTime;

        return finalGravVel;
    }
}
