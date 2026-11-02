using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    public Transform player;

    private Animator animator;
    private Vector3 lastPosition;

    void Start()
    {
        animator = GetComponent<Animator>();
        lastPosition = player.position;
    }

    void Update()
    {
        Vector3 movement = player.position - lastPosition;
        movement.y = 0f;

        float speed = movement.magnitude / Time.deltaTime;

        bool isMoving = speed > 0.1f;

        animator.SetBool("isMoving", isMoving);

        lastPosition = player.position;
    }
}