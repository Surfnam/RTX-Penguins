using UnityEngine;

/// <summary>
/// Visual-only marker for Aryan's eventual puzzle-piece system.
/// </summary>
public class PuzzlePieceDisplay : MonoBehaviour
{
    [SerializeField] private float spinSpeed = 55f;
    [SerializeField] private float bobHeight = 0.22f;
    [SerializeField] private float bobSpeed = 2f;

    private Vector3 startPosition;
    private float phase;

    private void Awake()
    {
        startPosition = transform.position;
        phase = transform.position.x + transform.position.z;
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);
        transform.position = startPosition + Vector3.up * (Mathf.Sin(Time.time * bobSpeed + phase) * bobHeight);
    }
}
