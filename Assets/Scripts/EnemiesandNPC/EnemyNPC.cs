using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyNPC : MonoBehaviour
{
    private NavMeshAgent navMeshAgent;

    public GameObject player;

    public Vector3 areaCenter;
    public Vector3 areaSize = new Vector3(10f, 5f, 10f);

    public float damage = 10f;
    public float bounceDistance = 2f;
    public float captureDistance = 1f;

    private bool bouncing = false;

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (bouncing)
        {
            if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
            {
                bouncing = false;
            }
        }
        else if (playerInArea())
        {
            Vector3 destination = player.transform.position;
            destination = keepInArea(destination);

            navMeshAgent.SetDestination(destination);

            float distance = Vector3.Distance(transform.position, player.transform.position);

            if (distance <= captureDistance)
            {
                hitPlayer();
            }
        }
        else
        {
            navMeshAgent.ResetPath();
        }
    }

    bool playerInArea()
    {
        Vector3 playerPosition = player.transform.position;

        if (Mathf.Abs(playerPosition.x - areaCenter.x) <= areaSize.x / 2f &&
            Mathf.Abs(playerPosition.y - areaCenter.y) <= areaSize.y / 2f &&
            Mathf.Abs(playerPosition.z - areaCenter.z) <= areaSize.z / 2f)
        {
            return true;
        }

        return false;
    }

    Vector3 keepInArea(Vector3 position)
    {
        position.x = Mathf.Clamp(position.x, areaCenter.x - areaSize.x / 2f, areaCenter.x + areaSize.x / 2f);
        position.y = Mathf.Clamp(position.y, areaCenter.y - areaSize.y / 2f, areaCenter.y + areaSize.y / 2f);
        position.z = Mathf.Clamp(position.z, areaCenter.z - areaSize.z / 2f, areaCenter.z + areaSize.z / 2f);

        return position;
    }

    void hitPlayer()
    {
        // We will add damage here later.

        bouncing = true;

        Vector3 direction = transform.position - player.transform.position;
        direction.y = 0f;
        direction.Normalize();

        Vector3 destination = transform.position + direction * bounceDistance;
        destination = keepInArea(destination);

        navMeshAgent.SetDestination(destination);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(areaCenter, areaSize);
    }
}
