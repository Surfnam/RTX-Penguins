using UnityEngine;

public class TurretBall : MonoBehaviour
{
    public Vector3 direction;
    public float speed;
    public float maxDistance;
    public float damage;

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;

        float distance = Vector3.Distance(startPosition, transform.position);

        if (distance >= maxDistance)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // We will add damage here later.
            Destroy(gameObject);
        }
    }
}
