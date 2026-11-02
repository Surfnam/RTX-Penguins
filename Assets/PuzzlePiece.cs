using UnityEngine;

public class PuzzlePiece : MonoBehaviour
{
    private Transform playerTransform;
    private bool isCollected = false;

    [Header("Orbit Settings")]
    public float orbitRadius = 2.0f;
    public float orbitSpeed = 100f;
    public float verticalOscillationSpeed = 2f;
    public float verticalOscillationMagnitude = 0.2f;

    private float orbitAngle;
    private float startYOffset;

    private void Start()
    {
        orbitAngle = Random.Range(0f, 360f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isCollected) {
            CollectPiece(other.transform);
        }
    }

    private void CollectPiece(Transform player)
    {
        isCollected = true;
        playerTransform = player;
        startYOffset = transform.position.y - playerTransform.position.y;
        Animator animator = GetComponent<Animator>();
        if (animator != null){
            animator.enabled = false;
        }
        PuzzlePieceDisplay display = GetComponent<PuzzlePieceDisplay>();
        if (display != null) {
            display.enabled = false;
        }
        transform.SetParent(null);
        PlayerPuzzlePiece inventory = player.GetComponent<PlayerPuzzlePiece>();
        if (inventory != null) {
            inventory.AddPiece(this);
        }
    }

    private void Update()
    {
        if (isCollected && playerTransform != null) {
            OrbitAroundPlayer();
        }
    }

    private void OrbitAroundPlayer()
    {
        orbitAngle += orbitSpeed * Time.deltaTime;
        float radians = orbitAngle * Mathf.Deg2Rad;
        float x = Mathf.Cos(radians) * orbitRadius;
        float z = Mathf.Sin(radians) * orbitRadius;
        float yOscillation = Mathf.Sin(Time.time * verticalOscillationSpeed) * verticalOscillationMagnitude;
        Vector3 targetPosition = playerTransform.position + new Vector3(x, startYOffset + yOscillation, z);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, orbitSpeed * Time.deltaTime);
        transform.Rotate(Vector3.up, 100f * Time.deltaTime);
    }
}
