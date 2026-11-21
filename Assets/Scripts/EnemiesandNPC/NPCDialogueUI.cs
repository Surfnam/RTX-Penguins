
using UnityEngine;

public class NPCDialogueUI : MonoBehaviour
{
    private LoreNPC currentNPC;
    private PlayerController playerController;
    private PenguinMenuController menu;

    private bool showingRewardDialogue = false;
    private bool rewardFailed = false;

    private GUIStyle panelStyle;
    private GUIStyle titleStyle;
    private GUIStyle textStyle;
    private GUIStyle buttonStyle;

    public bool IsOpen
    {
        get { return currentNPC != null; }
    }

    void Start()
    {
        menu = FindFirstObjectByType<PenguinMenuController>();
    }

    public void OpenDialogue(LoreNPC npc)
    {
        if (IsOpen || (menu != null && !menu.IsGameplayActive))
        {
            return;
        }

        currentNPC = npc;
        showingRewardDialogue = false;
        rewardFailed = false;

        playerController = npc.player.GetComponent<PlayerController>();

        if (playerController != null)
        {
            playerController.isInDialogue = true;
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void CloseDialogue()
    {
        if (playerController != null)
        {
            playerController.isInDialogue = false;
        }

        if (currentNPC != null)
        {
            currentNPC.EndDialogue();
        }

        currentNPC = null;
        showingRewardDialogue = false;
        rewardFailed = false;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void OnGUI()
    {
        if (!IsOpen)
        {
            return;
        }

        BuildStyles();

        Color oldColor = GUI.color;
        GUI.color = new Color(0.01f, 0.03f, 0.06f, 0.85f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = oldColor;

        float width = Mathf.Min(Screen.width * 0.8f, 850f);
        float height = Mathf.Min(Screen.height * 0.75f, 500f);

        Rect panel = new Rect(
            (Screen.width - width) / 2f,
            (Screen.height - height) / 2f,
            width,
            height
        );

        GUI.Box(panel, GUIContent.none, panelStyle);

        GUILayout.BeginArea(new Rect(
            panel.x + 45f,
            panel.y + 35f,
            panel.width - 90f,
            panel.height - 70f
        ));

        GUILayout.Label("THE PENGUIN SPEAKS", titleStyle);

        GUILayout.Space(35f);

        if (rewardFailed)
        {
            GUILayout.Label(
                "Your inventory is full! Make some room and come back for your potion.",
                textStyle
            );
        }
        else if (showingRewardDialogue)
        {
            GUILayout.Label(currentNPC.GetRewardDialogue(), textStyle);
        }
        else
        {
            GUILayout.Label(currentNPC.dialogueText, textStyle);
        }

        GUILayout.FlexibleSpace();

        GUILayout.BeginHorizontal();

        if (rewardFailed)
        {
            if (GUILayout.Button("CLOSE", buttonStyle, GUILayout.Height(55f)))
            {
                CloseDialogue();
            }
        }
        else if (showingRewardDialogue)
        {
            if (currentNPC.HasGivenReward || currentNPC.rewardType == NPCRewardType.None)
            {
                if (GUILayout.Button("FINISH", buttonStyle, GUILayout.Height(55f)))
                {
                    CloseDialogue();
                }
            }
            else
            {
                if (GUILayout.Button("ACCEPT", buttonStyle, GUILayout.Height(55f)))
                {
                    if (currentNPC.GiveReward())
                    {
                        CloseDialogue();
                    }
                    else
                    {
                        rewardFailed = true;
                    }
                }
            }
        }
        else
        {
            if (GUILayout.Button("CONTINUE", buttonStyle, GUILayout.Height(55f)))
            {
                showingRewardDialogue = true;
            }
        }

        GUILayout.Space(20f);

        if (GUILayout.Button("LEAVE", buttonStyle, GUILayout.Height(55f)))
        {
            CloseDialogue();
        }

        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    void BuildStyles()
    {
        if (panelStyle != null)
        {
            return;
        }

        panelStyle = new GUIStyle(GUI.skin.box);
        panelStyle.normal.background = MakeTexture(new Color(0.025f, 0.08f, 0.13f, 0.98f));

        titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.fontSize = 30;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.normal.textColor = new Color(0.25f, 0.9f, 1f);

        textStyle = new GUIStyle(GUI.skin.label);
        textStyle.alignment = TextAnchor.UpperLeft;
        textStyle.fontSize = 22;
        textStyle.wordWrap = true;
        textStyle.normal.textColor = Color.white;

        buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 18;
        buttonStyle.fontStyle = FontStyle.Bold;
        buttonStyle.normal.textColor = Color.white;
    }

    Texture2D MakeTexture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }
}
