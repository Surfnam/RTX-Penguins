
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyNPC : MonoBehaviour
{
    private NavMeshAgent navMeshAgent;

    public GameObject player;

    [Header("Movement Settings")]
    public Vector3 areaCenter;
    public Vector3 areaSize = new Vector3(10f, 5f, 10f);

    [Header("Attack Settings")]
    public float damage = 10f;
    public float bounceDistance = 2f;
    public float captureDistance = 1f;

    private bool bouncing = false;

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();

        // Center the enemy's area around its starting position.
        areaCenter = transform.position;
    }

    void Update()
    {
        if (player == null || !navMeshAgent.isActiveAndEnabled || !navMeshAgent.isOnNavMesh)
        {
            return;
        }

        if (bouncing)
        {
            if (!navMeshAgent.pathPending &&
                (!navMeshAgent.hasPath ||
                 navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance))
            {
                bouncing = false;
            }
        }
        else if (playerInArea())
        {
            Vector3 destination = player.transform.position;
            destination = keepInArea(destination);

            NavMeshHit hit;

            if (NavMesh.SamplePosition(destination, out hit, 2f, NavMesh.AllAreas))
            {
                NavMeshPath path = new NavMeshPath();

                if (navMeshAgent.CalculatePath(hit.position, path) &&
                    path.status == NavMeshPathStatus.PathComplete)
                {
                    navMeshAgent.SetDestination(hit.position);
                }
            }

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
        // Kill the player when caught.
        player.GetComponent<PlayerController>()?.Die();

        bouncing = true;

        Vector3 direction = transform.position - player.transform.position;
        direction.y = 0f;
        direction.Normalize();

        Vector3 destination = transform.position + direction * bounceDistance;
        destination = keepInArea(destination);

        NavMeshHit hit;

        if (NavMesh.SamplePosition(destination, out hit, 2f, NavMesh.AllAreas))
        {
            NavMeshPath path = new NavMeshPath();

            if (navMeshAgent.CalculatePath(hit.position, path) &&
                path.status == NavMeshPathStatus.PathComplete)
            {
                navMeshAgent.SetDestination(hit.position);
            }
            else
            {
                bouncing = false;
            }
        }
        else
        {
            bouncing = false;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(areaCenter, areaSize);
    }
}
