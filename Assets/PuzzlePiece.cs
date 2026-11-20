using UnityEngine;

public class PuzzlePiece : MonoBehaviour
{
    private Transform playerTransform;
    private bool isCollected = false;
    
    // original spot and player tracking
    private Vector3 startPosition;
    private PlayerController activePlayer;

    [Header("Orbit Settings")]
    public float orbitRadius = 2.0f;
    public float orbitSpeed = 100f;
    public float verticalOscillationSpeed = 2f;
    public float verticalOscillationMagnitude = 0.2f;

    private float orbitAngle;
    private float startYOffset;

    // checkpoint marker
    public bool savedAtCheckpoint = false;

    private void Start()
    {
        orbitAngle = Random.Range(0f, 360f);
        // save original spot
        startPosition = transform.position;
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
        
        // listen for death to reset piece
        activePlayer = player.GetComponent<PlayerController>();
        if (activePlayer != null) {
            activePlayer.OnPlayerDeath += ResetPiece;
        }

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

    public void SetSavedAtCheckpoint()
    {
        savedAtCheckpoint = true;
        if (activePlayer != null)
        {
            activePlayer.OnPlayerDeath -= ResetPiece;
        }
    }

    private void ResetPiece()
    {
        // do not attempt removing a piece if there are no puzzle pieces
        if (this == null || gameObject == null || playerTransform == null)
        {
            return;
        }

        // do not remove a piece if saved at checkpoint
        if (savedAtCheckpoint)
        {
            return;
        }

        isCollected = false;
        transform.position = startPosition;

        // stop listening to prevent memory leaks
        if (activePlayer != null) {
            activePlayer.OnPlayerDeath -= ResetPiece;
            activePlayer = null;
        }

        // turn visuals back on
        Animator animator = GetComponent<Animator>();
        if (animator != null) {
            animator.enabled = true;
        }
        PuzzlePieceDisplay display = GetComponent<PuzzlePieceDisplay>();
        if (display != null) {
            display.enabled = true;
        }

        // tell player to drop this piece
        PlayerPuzzlePiece inventory = playerTransform.GetComponent<PlayerPuzzlePiece>();
        if (inventory != null) {
            inventory.SendMessage("ClearPieces", SendMessageOptions.DontRequireReceiver);
        }
        
        playerTransform = null;
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