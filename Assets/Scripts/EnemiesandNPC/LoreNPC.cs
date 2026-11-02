using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class LoreNPC : MonoBehaviour
{
    private NavMeshAgent navMeshAgent;

    public GameObject player;

    public Vector3 areaCenter;
    public Vector3 areaSize = new Vector3(10f, 5f, 10f);

    public float moveSpeed = 1.5f;
    public float interactionDistance = 2f;

    public float minWaitTime = 2f;
    public float maxWaitTime = 5f;

    private float waitTimer = 0f;
    private float waitTime;
    private bool waiting = true;

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.speed = moveSpeed;

        waitTime = Random.Range(minWaitTime, maxWaitTime);
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.transform.position);

        if (distance <= interactionDistance)
        {
            navMeshAgent.ResetPath();
            return;
        }

        if (waiting)
        {
            waitTimer += Time.deltaTime;

            if (waitTimer >= waitTime)
            {
                waiting = false;
                waitTimer = 0f;
                moveToRandomPosition();
            }
        }
        else
        {
            if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
            {
                waiting = true;
                waitTime = Random.Range(minWaitTime, maxWaitTime);
            }
        }
    }

    void moveToRandomPosition()
    {
        Vector3 position;

        position.x = Random.Range(areaCenter.x - areaSize.x / 2f, areaCenter.x + areaSize.x / 2f);
        position.y = Random.Range(areaCenter.y - areaSize.y / 2f, areaCenter.y + areaSize.y / 2f);
        position.z = Random.Range(areaCenter.z - areaSize.z / 2f, areaCenter.z + areaSize.z / 2f);

        NavMeshHit hit;

        if (NavMesh.SamplePosition(position, out hit, 2f, NavMesh.AllAreas))
        {
            navMeshAgent.SetDestination(hit.position);
        }
        else
        {
            waiting = true;
            waitTime = Random.Range(minWaitTime, maxWaitTime);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(areaCenter, areaSize);
    }
}
