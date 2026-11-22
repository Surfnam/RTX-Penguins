using UnityEngine;
using UnityEngine.SceneManagement;

public class PenguinMenuController : MonoBehaviour
{
    private const int JumpPotionPrice = 5;

    public enum ScreenState
    {
        Start,
        Playing,
        Settings,
        Paused,
        Win,
        Lose,
        Shop
    }
    [SerializeField] private ScreenState currentScreen = ScreenState.Start;

    private ScreenState settingsReturnScreen = ScreenState.Start;
    private ScreenState shopReturnScreen = ScreenState.Win;
    private GUIStyle titleStyle;
    private GUIStyle subtitleStyle;
    private GUIStyle buttonStyle;
    private GUIStyle panelStyle;
    private GUIStyle labelStyle;
    private Inventory shopInventory;
    private Sprite jumpPotionIcon;
    private string shopMessage = "";

    public ScreenState CurrentScreen => currentScreen;
    public bool IsGameplayActive => currentScreen == ScreenState.Playing;

    private void Awake()
    {
        jumpPotionIcon = Resources.Load<Sprite>("Inventory/JumpPotionIcon");
        if (jumpPotionIcon == null)
        {
            Debug.LogError("Jump potion icon is missing at Resources/Inventory/JumpPotionIcon.");
        }

        ApplyScreenState(currentScreen);
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }

    private void Update()
    {
        // sent every frame because the AudioManager is created after this object's Awake
        AudioManager.SetMenuOpen(currentScreen != ScreenState.Playing);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentScreen == ScreenState.Playing)
            {
                ApplyScreenState(ScreenState.Paused);
            }
            else if (currentScreen == ScreenState.Paused)
            {
                ApplyScreenState(ScreenState.Playing);
            }
            else if (currentScreen == ScreenState.Settings)
            {
                ApplyScreenState(settingsReturnScreen);
            }
            else if (currentScreen == ScreenState.Shop)
            {
                ApplyScreenState(shopReturnScreen);
            }
        }
    }

    private static bool ClickButton(string text, GUIStyle style, params GUILayoutOption[] options)
    {
        bool pressed = GUILayout.Button(text, style, options);
        if (pressed)
        {
            AudioManager.PlaySfx("ui_click");
        }
        return pressed;
    }

    public void ShowWin()
    {
        if (currentScreen == ScreenState.Playing)
        {
            AudioManager.PlayStinger("win");
            ApplyScreenState(ScreenState.Win);
        }
    }

    public void ShowLose()
    {
        if (currentScreen == ScreenState.Playing)
        {
            AudioManager.PlayStinger("lose");
            ApplyScreenState(ScreenState.Lose);
        }
    }

    private void OnGUI()
    {
        BuildStyles();

        if (currentScreen == ScreenState.Playing)
        {
            DrawHud();
            return;
        }

        DrawDimmer();
        Rect panel = new Rect((Screen.width - 520f) * 0.5f, (Screen.height - 500f) * 0.5f, 520f, 500f);
        GUI.Box(panel, GUIContent.none, panelStyle);

        GUILayout.BeginArea(new Rect(panel.x + 55f, panel.y + 42f, panel.width - 110f, panel.height - 84f));
        switch (currentScreen)
        {
            case ScreenState.Start:
                DrawStartMenu();
                break;
            case ScreenState.Settings:
                DrawSettingsMenu();
                break;
            case ScreenState.Paused:
                DrawPauseMenu();
                break;
            case ScreenState.Win:
                DrawEndMenu(true);
                break;
            case ScreenState.Lose:
                DrawEndMenu(false);
                break;
            case ScreenState.Shop:
                DrawShop();
                break;
        }
        GUILayout.EndArea();
    }

    private void DrawStartMenu()
    {
        GUILayout.Label("RTX PENGUINS", titleStyle);
        GUILayout.Label("LABYRINTH ASCENT", subtitleStyle);
        GUILayout.FlexibleSpace();
        if (ClickButton("BEGIN ASCENT", buttonStyle, GUILayout.Height(58f)))
        {
            ApplyScreenState(ScreenState.Playing);
        }
        GUILayout.Space(14f);
        if (ClickButton("SETTINGS", buttonStyle, GUILayout.Height(58f)))
        {
            OpenSettings(ScreenState.Start);
        }
        GUILayout.FlexibleSpace();
        GUILayout.Label("WASD  MOVE     SPACE  JUMP     ESC  PAUSE", labelStyle);
    }

    private void DrawPauseMenu()
    {
        GUILayout.Label("PAUSED", titleStyle);
        GUILayout.Label("THE LABYRINTH WAITS", subtitleStyle);
        GUILayout.FlexibleSpace();
        if (ClickButton("RESUME", buttonStyle, GUILayout.Height(58f)))
        {
            ApplyScreenState(ScreenState.Playing);
        }
        GUILayout.Space(14f);
        if (ClickButton("SETTINGS", buttonStyle, GUILayout.Height(58f)))
        {
            OpenSettings(ScreenState.Paused);
        }
        GUILayout.Space(14f);
        if (ClickButton("RESTART LEVEL", buttonStyle, GUILayout.Height(58f)))
        {
            ReloadLevel();
        }
        GUILayout.FlexibleSpace();
    }

    private void DrawSettingsMenu()
    {
        GUILayout.Label("SETTINGS", titleStyle);
        GUILayout.Label("TUNE YOUR EXPEDITION", subtitleStyle);
        GUILayout.Space(42f);

        GUILayout.Label($"MASTER VOLUME  {Mathf.RoundToInt(AudioListener.volume * 100f)}%", labelStyle);
        AudioListener.volume = GUILayout.HorizontalSlider(AudioListener.volume, 0f, 1f, GUILayout.Height(32f));
        GUILayout.Space(24f);

        bool fullscreen = GUILayout.Toggle(Screen.fullScreen, "  FULLSCREEN", buttonStyle, GUILayout.Height(52f));
        if (fullscreen != Screen.fullScreen)
        {
            Screen.fullScreen = fullscreen;
        }

        GUILayout.Space(14f);
        string qualityName = QualitySettings.names.Length > 0
            ? QualitySettings.names[QualitySettings.GetQualityLevel()]
            : "DEFAULT";
        if (ClickButton($"QUALITY:  {qualityName.ToUpperInvariant()}", buttonStyle, GUILayout.Height(52f)))
        {
            int count = Mathf.Max(1, QualitySettings.names.Length);
            QualitySettings.SetQualityLevel((QualitySettings.GetQualityLevel() + 1) % count, true);
        }

        GUILayout.FlexibleSpace();
        if (ClickButton("BACK", buttonStyle, GUILayout.Height(58f)))
        {
            ApplyScreenState(settingsReturnScreen);
        }
    }

    private void DrawEndMenu(bool won)
    {
        GUILayout.Label(won ? "FLOOR CLEARED" : "EXPEDITION LOST", titleStyle);
        GUILayout.Label(won ? "THE NEXT FLOOR AWAITS" : "THE LABYRINTH CLAIMS ANOTHER", subtitleStyle);
        GUILayout.FlexibleSpace();
        GUILayout.Label(won
            ? "You reached the sealed exit."
            : "A trap ended this attempt.", labelStyle);
        GUILayout.Space(30f);

        if (won)
        {
            if (GUILayout.Button("OPEN SHOP", buttonStyle, GUILayout.Height(50f)))
            {
                OpenShop();
            }

            GUILayout.Space(10f);

            if (ClickButton("PLAY AGAIN", buttonStyle, GUILayout.Height(60f)))
            {
                ReloadLevel();
            }

            GUILayout.Space(10f);

            if (ClickButton("CONTINUE WITH DASH", buttonStyle, GUILayout.Height(50f)))
            {
                ContinueLevel();
            }
        }
        else
        {
            // close menu and unpause to continue from checkpoint
            if (ClickButton("RETRY (CHECKPOINT)", buttonStyle, GUILayout.Height(60f)))
            {
                ApplyScreenState(ScreenState.Playing);
            }
        }
        
        GUILayout.FlexibleSpace();
    }

    private void DrawShop()
    {
        GUILayout.Label("EXPEDITION SHOP", titleStyle);
        GUILayout.Label("SPEND YOUR COLLECTED COINS", subtitleStyle);
        GUILayout.Space(24f);
        GUILayout.Label($"COINS: {(shopInventory != null ? shopInventory.coins : 0)}", labelStyle);
        GUILayout.Space(16f);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (jumpPotionIcon != null)
        {
            Rect iconRect = GUILayoutUtility.GetRect(22.5f, 31.5f, GUILayout.ExpandWidth(false));
            Rect iconUv = new Rect(3f / 16f, 1f / 16f, 10f / 16f, 14f / 16f);
            GUI.DrawTextureWithTexCoords(iconRect, jumpPotionIcon.texture, iconUv, true);
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Label("JUMP BOOST POTION", subtitleStyle);
        float duration = shopInventory != null ? shopInventory.GetComponent<JumpBoost>().duration : 10f;
        GUILayout.Label($"Boosts jump height for {duration:0.#} seconds.", labelStyle);
        GUILayout.Space(8f);

        if (GUILayout.Button($"BUY  -  {JumpPotionPrice} COINS", buttonStyle, GUILayout.Height(58f)))
        {
            BuyJumpPotion();
        }

        if (!string.IsNullOrEmpty(shopMessage))
        {
            GUILayout.Space(8f);
            GUILayout.Label(shopMessage, labelStyle);
        }

        GUILayout.FlexibleSpace();
        if (GUILayout.Button("BACK TO RESULTS", buttonStyle, GUILayout.Height(54f)))
        {
            ApplyScreenState(shopReturnScreen);
        }
    }

    private void DrawHud()
    {
        GUI.Label(new Rect(24f, 18f, 460f, 36f), "FLOOR 01  •  FIND THE FOUR PIECE MARKERS", labelStyle);
        GUI.Label(new Rect(24f, Screen.height - 42f, 520f, 30f), "WASD MOVE   •   SPACE JUMP   •   ESC PAUSE", labelStyle);
    }

    private void OpenSettings(ScreenState returnScreen)
    {
        settingsReturnScreen = returnScreen;
        ApplyScreenState(ScreenState.Settings);
    }

    private void OpenShop()
    {
        if (currentScreen != ScreenState.Win)
        {
            return;
        }

        shopReturnScreen = currentScreen;
        shopInventory = FindFirstObjectByType<Inventory>();
        shopMessage = "";
        ApplyScreenState(ScreenState.Shop);
    }

    private void BuyJumpPotion()
    {
        if (shopInventory == null)
        {
            shopMessage = "INVENTORY IS UNAVAILABLE.";
            return;
        }

        InventorySlot potion = new InventorySlot
        {
            itemName = "Jump Boost Potion",
            icon = Resources.Load<Sprite>("Inventory/JumpPotionIcon"),
            isConsumable = true,
            consumable = ConsumableType.JumpBoost
        };

        if (shopInventory.TryPurchase(potion, JumpPotionPrice))
        {
            shopMessage = "JUMP BOOST POTION PURCHASED!";
        }
        else if (shopInventory.coins < JumpPotionPrice)
        {
            shopMessage = "NOT ENOUGH COINS.";
        }
        else
        {
            shopMessage = "INVENTORY FULL.";
        }
    }

    private void ReloadLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void ContinueLevel()
    {
        // unlock dash upon completion of level
        PlayerController player = FindFirstObjectByType<PlayerController>();

        if (player != null)
        {
            player.canDash = true;

            PlayerPuzzlePiece inventory = player.GetComponent<PlayerPuzzlePiece>();
            if (inventory != null)
            {
                inventory.ClearInventory();
            }
        }

        ApplyScreenState(ScreenState.Playing);
    }

    public void ApplyScreenState(ScreenState state)
    {
        currentScreen = state;
        bool gameplay = state == ScreenState.Playing;
        Time.timeScale = gameplay ? 1f : 0f;
        Cursor.visible = !gameplay;
        Cursor.lockState = gameplay ? CursorLockMode.Locked : CursorLockMode.None;
    }

    private void DrawDimmer()
    {
        Color previous = GUI.color;
        GUI.color = new Color(0.015f, 0.035f, 0.07f, 0.88f);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = previous;
    }

    private void BuildStyles()
    {
        if (titleStyle != null)
        {
            return;
        }

        titleStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 42,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(0.25f, 0.9f, 1f) }
        };
        subtitleStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 17,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(0.75f, 0.84f, 0.9f) }
        };
        labelStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 15,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };
        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white, background = MakeTexture(new Color(0.05f, 0.2f, 0.3f, 0.98f)) },
            hover = { textColor = Color.white, background = MakeTexture(new Color(0.08f, 0.42f, 0.54f, 1f)) },
            active = { textColor = Color.white, background = MakeTexture(new Color(0.1f, 0.58f, 0.68f, 1f)) }
        };
        panelStyle = new GUIStyle(GUI.skin.box)
        {
            normal = { background = MakeTexture(new Color(0.025f, 0.08f, 0.13f, 0.98f)) },
            border = new RectOffset(2, 2, 2, 2)
        };
    }

    private static Texture2D MakeTexture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }
}