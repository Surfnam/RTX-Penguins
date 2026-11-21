using UnityEngine;
using UnityEngine.AI;

public enum NPCRewardType
{
    None,
    Coins,
    JumpBoostPotion
}

[RequireComponent(typeof(NavMeshAgent))]
public class LoreNPC : MonoBehaviour
{
    private NPCDialogueUI dialogueUI;
    private NavMeshAgent navMeshAgent;
    public GameObject player;

    [Header("Patrol Settings")]
    public Vector3 areaCenter;
    public Vector3 areaSize = new Vector3(10f, 5f, 10f);
    public float moveSpeed = 1.5f;
    public float minWaitTime = 2f;
    public float maxWaitTime = 5f;

    [Header("Dialogue Settings")]
    public float interactionDistance = 2f;
    [Min(1)]
    public int dialogueNumber = 1;

    [Header("Reward Settings")]
    public NPCRewardType rewardType = NPCRewardType.None;
    public int coinAmount = 5;
    public Sprite potionIcon;

    private float waitTimer = 0f;
    private float waitTime;
    private bool waiting = true;
    private bool isTalking = false;
    private bool isPlayerNear = false;
    private bool rewardGiven = false;

    public bool IsPlayerNear
    {
        get { return isPlayerNear; }
    }

    public bool IsTalking
    {
        get { return isTalking; }
    }

    public bool HasGivenReward
    {
        get { return rewardGiven; }
    }

    public string dialogueText
    {
        get { return NPCLore.GetDialogue(dialogueNumber); }
    }

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        Debug.Log("NPC on NavMesh: " + navMeshAgent.isOnNavMesh);
        navMeshAgent.speed = moveSpeed;
        dialogueUI = FindFirstObjectByType<NPCDialogueUI>();
        areaCenter = transform.position;
        waitTime = Random.Range(minWaitTime, maxWaitTime);
    }

    void Update()
    {
        if (player == null)
        {
            return;
        }

        float distance = Vector3.Distance(transform.position, player.transform.position);
        isPlayerNear = distance <= interactionDistance;

        if (isTalking)
        {
            return;
        }

        if (isPlayerNear)
        {
            if (navMeshAgent.isActiveAndEnabled && navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.ResetPath();
            }

            Vector3 lookPosition = player.transform.position;
            lookPosition.y = transform.position.y;
            transform.LookAt(lookPosition);

            if (Input.GetKeyDown(KeyCode.E) && !isTalking &&
                dialogueUI != null && !dialogueUI.IsOpen)
            {
                dialogueUI.OpenDialogue(this);
                isTalking = dialogueUI.IsOpen;
            }

            return;
        }

        if (!waiting && !navMeshAgent.pathPending && !navMeshAgent.hasPath)
        {
            waiting = true;
            waitTimer = 0f;
            waitTime = Random.Range(minWaitTime, maxWaitTime);
        }

        if (!isTalking)
        {
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
            else if (navMeshAgent.isActiveAndEnabled && navMeshAgent.isOnNavMesh &&
                     !navMeshAgent.pathPending &&
                     navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
            {
                waiting = true;
                waitTime = Random.Range(minWaitTime, maxWaitTime);
            }
        }
    }

    public void EndDialogue()
    {
        isTalking = false;
    }

    public string GetRewardDialogue()
    {
        if (rewardGiven)
        {
            return "It was lovely talking with you again! Stay safe out there, friend!";
        }

        if (rewardType == NPCRewardType.Coins)
        {
            return "Thanks for stopping to talk! Here, take these coins. They might help you on your journey!";
        }
        else if (rewardType == NPCRewardType.JumpBoostPotion)
        {
            return "It's dangerous out there! Here, take this potion to help you on your journey. Good luck!";
        }

        return "Thanks for listening, friend! Stay safe out there!";
    }

    public bool GiveReward()
    {
        if (rewardGiven)
        {
            return true;
        }

        if (rewardType == NPCRewardType.None)
        {
            rewardGiven = true;
            return true;
        }

        Inventory inventory = player.GetComponent<Inventory>();

        if (inventory == null)
        {
            return false;
        }

        if (rewardType == NPCRewardType.Coins)
        {
            inventory.AddCoins(Mathf.Max(0, coinAmount));
            rewardGiven = true;
            return true;
        }
        else if (rewardType == NPCRewardType.JumpBoostPotion)
        {
            InventorySlot potion = new InventorySlot();
            potion.itemName = "Jump Boost Potion";
            potion.icon = potionIcon;
            potion.isConsumable = true;
            potion.consumable = ConsumableType.JumpBoost;
            potion.count = 1;

            if (inventory.TryAdd(potion))
            {
                rewardGiven = true;
                return true;
            }

            return false;
        }

        return false;
    }

    
    void moveToRandomPosition()
    {
        if (!navMeshAgent.isActiveAndEnabled || !navMeshAgent.isOnNavMesh)
        {
            return;
        }

        for (int i = 0; i < 10; i++)
        {
            Vector3 position;

            position.x = Random.Range(areaCenter.x - areaSize.x / 2f, areaCenter.x + areaSize.x / 2f);
            position.y = Random.Range(areaCenter.y - areaSize.y / 2f, areaCenter.y + areaSize.y / 2f);
            position.z = Random.Range(areaCenter.z - areaSize.z / 2f, areaCenter.z + areaSize.z / 2f);

            NavMeshHit hit;

            if (NavMesh.SamplePosition(position, out hit, 2f, NavMesh.AllAreas))
            {
                NavMeshPath path = new NavMeshPath();

                if (navMeshAgent.CalculatePath(hit.position, path) &&
                    path.status == NavMeshPathStatus.PathComplete)
                {
                    navMeshAgent.SetDestination(hit.position);
                    Debug.Log("NPC found destination: " + hit.position);
                    return;
                }
            }
        }

        Debug.Log("NPC could not find a reachable destination.");
        waiting = true;
        waitTimer = 0f;
        waitTime = Random.Range(minWaitTime, maxWaitTime);
    }


    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(areaCenter, areaSize);
    }
}
