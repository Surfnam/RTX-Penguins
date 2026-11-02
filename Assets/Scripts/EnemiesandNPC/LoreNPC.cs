using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class LoreNPC : MonoBehaviour
{
    private NavMeshAgent navMeshAgent;

    public GameObject player;

    [Header("Patrol Settings")]
    public Vector3 areaCenter;
    public Vector3 areaSize = new Vector3(10f, 5f, 10f);
    public float moveSpeed = 1.5f;
    
    [Header("Dialogue Settings")]
    public float interactionDistance = 2f;
    
    // gives you a nice large text box in the inspector
    [TextArea(3, 5)]
    public string dialogueText = "The labyrinth shifts when you aren't looking...";

    public float minWaitTime = 2f;
    public float maxWaitTime = 5f;

    private float waitTimer = 0f;
    private float waitTime;
    private bool waiting = true;
    
    private bool isPlayerNear = false;
    private GUIStyle dialogueStyle;

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.speed = moveSpeed;

        waitTime = Random.Range(minWaitTime, maxWaitTime);
        
        // setup font style for the dialogue popup
        dialogueStyle = new GUIStyle()
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 24,
            fontStyle = FontStyle.Bold,
            wordWrap = true,
            normal = { textColor = new Color32(234, 246, 252, 255) } 
        };
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.transform.position);
        
        isPlayerNear = (distance <= interactionDistance);

        if (isPlayerNear)
        {
            navMeshAgent.ResetPath();
            
            // make the npc turn to look at the player
            Vector3 lookPosition = player.transform.position;
            lookPosition.y = transform.position.y;
            transform.LookAt(lookPosition);
            
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

    private void OnGUI()
    {
        if (isPlayerNear)
        {
            // draw dialogue safely above the inventory slots
            Rect boxPosition = new Rect((Screen.width - 600f) / 2f, Screen.height - 240f, 600f, 100f);
            GUI.Label(boxPosition, dialogueText, dialogueStyle);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(areaCenter, areaSize);
    }
}