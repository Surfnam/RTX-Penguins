#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Frozen-cave floor: a descending ice stair, a stalactite corridor, stepping stones over lava, a turret-guarded
// ice bridge and a final climb. Build from RTX Penguins > Build Floor 02, check with RTX Penguins > Validate Floor 02.
public static class Floor02Builder
{
    private const string ScenePath = "Assets/Scenes/Floor02.unity";
    private const string MaterialFolder = "Assets/GUIAndLevelDesign/Materials";
    private const string CheckpointPrefab = "Assets/Prefabs/Checkpoint.prefab";
    private const string TurretBallPrefab = "Assets/Prefabs/Turret Ball.prefab";

    [MenuItem("RTX Penguins/Build Floor 02")]
    public static void BuildFloor()
    {
        if (!AssetDatabase.IsValidFolder(MaterialFolder))
        {
            AssetDatabase.CreateFolder("Assets/GUIAndLevelDesign", "Materials");
        }

        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        Material ice = GetMaterial("F2_Ice", new Color(0.55f, 0.85f, 0.95f), 0.8f);
        Material rock = GetMaterial("F2_Rock", new Color(0.12f, 0.14f, 0.2f), 0.2f);
        Material lava = GetMaterial("F2_Lava", new Color(1f, 0.3f, 0.05f), 0.4f);
        Material accent = GetMaterial("F2_Accent", new Color(0.7f, 1f, 1f), 0.9f);
        Material piece = GetMaterial("F2_Piece", new Color(1f, 0.58f, 0.06f), 0.7f);
        Material goal = GetMaterial("F2_Goal", new Color(0.1f, 0.95f, 0.4f), 0.65f);
        Material deepRock = GetMaterial("F2_DeepRock", new Color(0.05f, 0.06f, 0.1f), 0.1f);

        GameObject root = new GameObject("FROZEN_CAVE_FLOOR_02");
        Transform platforms = NewGroup("Platforms", root.transform);
        Transform hazards = NewGroup("Lava_Hazards", root.transform);
        Transform markers = NewGroup("Puzzle_Piece_Placement_Markers", root.transform);
        Transform pickups = NewGroup("Inventory_Pickups", root.transform);

        // Descending ice stair from the cave mouth.
        Box("Start Ledge", new Vector3(0f, 8f, -28f), new Vector3(12f, 1f, 10f), ice, platforms);
        Box("Stair A", new Vector3(0f, 6.8f, -19f), new Vector3(6f, 1f, 5f), ice, platforms);
        Box("Stair B", new Vector3(2f, 5.6f, -13.5f), new Vector3(6f, 1f, 5f), ice, platforms);
        Box("Stair C", new Vector3(-2f, 4.4f, -8f), new Vector3(6f, 1f, 5f), ice, platforms);

        // Stalactite corridor: staggered walls force a zig-zag.
        Box("Cave Floor", new Vector3(0f, 3f, 4f), new Vector3(22f, 1f, 18f), ice, platforms);
        Box("Cave West Wall", new Vector3(-11.5f, 5.5f, 4f), new Vector3(1f, 4f, 18f), rock, platforms);
        Box("Cave East Wall", new Vector3(11.5f, 5.5f, 4f), new Vector3(1f, 4f, 18f), rock, platforms);
        Box("Wall A", new Vector3(-5f, 5.5f, 0f), new Vector3(10f, 4f, 1f), rock, platforms);
        Box("Wall B", new Vector3(5f, 5.5f, 6f), new Vector3(10f, 4f, 1f), rock, platforms);
        Box("Wall C", new Vector3(0f, 5.5f, 10f), new Vector3(8f, 4f, 1f), rock, platforms);
        Box("East Shelf", new Vector3(9.5f, 4.5f, 8f), new Vector3(3f, 1f, 3f), accent, platforms);

        // Stepping stones over the lava pit, each a little higher than the last.
        Box("Stone 1", new Vector3(-3f, 3f, 16.5f), new Vector3(3.5f, 1f, 3.5f), ice, platforms);
        Box("Stone 2", new Vector3(3f, 3.4f, 20.5f), new Vector3(3.5f, 1f, 3.5f), ice, platforms);
        Box("Stone 3", new Vector3(-3f, 3.8f, 24.5f), new Vector3(3.5f, 1f, 3.5f), ice, platforms);
        Box("Stone 4", new Vector3(3f, 4.2f, 28.5f), new Vector3(3.5f, 1f, 3.5f), ice, platforms);
        Box("Stone 5", new Vector3(0f, 4.6f, 32.5f), new Vector3(5f, 1f, 4f), ice, platforms);
        Box("Lava Detour Stone", new Vector3(-8f, 3.8f, 20f), new Vector3(3f, 1f, 3f), accent, platforms);

        // Narrow bridge past a turret, then a checkpoint island.
        Box("Ice Bridge", new Vector3(0f, 5f, 39f), new Vector3(2f, 1f, 10f), ice, platforms);
        Box("Mid Island", new Vector3(0f, 5f, 48f), new Vector3(10f, 1f, 8f), ice, platforms);
        Box("Turret Pillar", new Vector3(-8f, 6f, 40f), new Vector3(2f, 3f, 2f), rock, platforms);

        // Final climb to the summit.
        Box("Lift 1", new Vector3(6f, 6.3f, 53.5f), new Vector3(4f, 1f, 4f), ice, platforms);
        Box("Lift 2", new Vector3(0f, 7.6f, 57.5f), new Vector3(4f, 1f, 4f), ice, platforms);
        Box("Lift 3", new Vector3(-6f, 8.9f, 61.5f), new Vector3(4f, 1f, 4f), ice, platforms);
        Box("Lift Detour", new Vector3(11f, 7.6f, 60f), new Vector3(3f, 1f, 3f), accent, platforms);
        Box("Summit Island", new Vector3(0f, 8.9f, 70f), new Vector3(16f, 1f, 10f), ice, platforms);

        // Lava: the pit itself plus a deep layer that catches any fall. Both respawn the player at the last checkpoint.
        LavaTrigger("Lava Pit", new Vector3(0f, 1.5f, 24f), new Vector3(30f, 0.5f, 24f), lava, hazards);
        LavaTrigger("Deep Lava", new Vector3(0f, -4f, 30f), new Vector3(160f, 1f, 200f), deepRock, hazards);

        // Puzzle pieces: corridor alcove, east shelf, lava detour, lift detour.
        PuzzleMarker("Piece Marker 01 - Corridor Alcove", new Vector3(-9f, 4.7f, 12f), piece, markers);
        PuzzleMarker("Piece Marker 02 - East Shelf", new Vector3(9.5f, 6.2f, 8f), piece, markers);
        PuzzleMarker("Piece Marker 03 - Lava Detour", new Vector3(-8f, 5.5f, 20f), piece, markers);
        PuzzleMarker("Piece Marker 04 - Lift Detour", new Vector3(11f, 8.8f, 60f), piece, markers);

        CreateExit(new Vector3(0f, 10.15f, 72f), rock, goal, root.transform);
        PlacePickups(pickups);
        GameObject player = CreatePlayerAndCamera();
        PlaceCheckpoint(new Vector3(8f, 3.6f, 12.5f), root.transform);
        PlaceCheckpoint(new Vector3(0f, 5.6f, 48f), root.transform);
        CreateTurret(new Vector3(-8f, 8f, 40f), player, rock, root.transform);
        new GameObject("Menu System", typeof(PenguinMenuController));

        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.05f, 0.1f, 0.16f);
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = 0.015f;
        RenderSettings.ambientLight = new Color(0.18f, 0.25f, 0.35f);

        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), ScenePath);
        AddSceneToBuildSettings();
        AssetDatabase.SaveAssets();
        Debug.Log($"Built Floor 02 at {ScenePath}");
    }

    [MenuItem("RTX Penguins/Validate Floor 02")]
    public static void ValidateFloor()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        Require(Object.FindObjectsByType<PuzzlePieceDisplay>(FindObjectsSortMode.None).Length == 4, "Expected 4 puzzle placement markers.");

        int wins = 0;
        foreach (MenuStateTrigger trigger in Object.FindObjectsByType<MenuStateTrigger>(FindObjectsSortMode.None))
        {
            if (trigger.TriggerResult == MenuStateTrigger.Result.Win)
            {
                wins++;
                Require(trigger.requiredPieces == 4, "Win trigger must require 4 pieces.");
                bool hasSolidGate = false;
                foreach (Collider col in trigger.GetComponents<Collider>())
                {
                    if (!col.isTrigger) hasSolidGate = true;
                }
                Require(hasSolidGate, "Win portal needs a solid gate collider.");
            }
        }
        Require(wins == 1, $"Expected 1 win trigger, found {wins}.");

        Require(Object.FindObjectsByType<Hazard>(FindObjectsSortMode.None).Length >= 2, "Expected at least 2 lava hazards.");
        Require(Object.FindObjectsByType<Checkpoint>(FindObjectsSortMode.None).Length >= 2, "Expected at least 2 checkpoints.");
        Require(Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None).Length == 1, "Expected exactly 1 player.");
        Require(Object.FindObjectsByType<PenguinMenuController>(FindObjectsSortMode.None).Length == 1, "Expected exactly 1 menu controller.");

        Turret turret = Object.FindFirstObjectByType<Turret>();
        Require(turret != null && turret.player != null && turret.ballPrefab != null, "Turret is missing its player or ball prefab.");

        ThirdPersonCamera camera = Object.FindFirstObjectByType<ThirdPersonCamera>();
        Require(camera != null && camera.player != null, "Camera is not connected to the player.");

        Debug.Log("FLOOR 02 VALIDATION PASSED");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new System.InvalidOperationException(message);
        }
    }

    private static void PlacePickups(Transform parent)
    {
        (string prefab, Vector3 position)[] pickups =
        {
            ("JumpPotionPickup", new Vector3(3f, 9.3f, -28f)),
            ("CoinPickup", new Vector3(0f, 8f, -19f)),
            ("CoinPickup", new Vector3(2f, 6.8f, -13.5f)),
            ("CoinPickup", new Vector3(-2f, 5.6f, -8f)),
            ("CoinPickup", new Vector3(-3f, 4.6f, 3f)),
            ("CoinPickup", new Vector3(6f, 4.6f, 3f)),
            ("JumpPotionPickup", new Vector3(3f, 4.6f, 12f)),
            ("CoinPickup", new Vector3(-3f, 4.6f, 16.5f)),
            ("CoinPickup", new Vector3(3f, 5f, 20.5f)),
            ("CoinPickup", new Vector3(-3f, 5.4f, 24.5f)),
            ("CoinPickup", new Vector3(3f, 5.8f, 28.5f)),
            ("CoinPickup", new Vector3(0f, 6.5f, 39f)),
            ("ItemPickup", new Vector3(0f, 6.5f, 50f)),
            ("CoinPickup", new Vector3(6f, 7.8f, 53.5f)),
            ("CoinPickup", new Vector3(0f, 9.1f, 57.5f)),
            ("CoinPickup", new Vector3(-6f, 10.4f, 61.5f)),
        };

        foreach ((string prefab, Vector3 position) in pickups)
        {
            GameObject asset = Resources.Load<GameObject>($"Inventory/{prefab}");
            if (asset == null)
            {
                Debug.LogWarning($"Missing pickup prefab {prefab}");
                continue;
            }
            GameObject pickup = (GameObject)PrefabUtility.InstantiatePrefab(asset);
            pickup.transform.SetParent(parent);
            pickup.transform.position = position;
        }
    }

    private static void PlaceCheckpoint(Vector3 position, Transform parent)
    {
        GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(CheckpointPrefab);
        if (asset == null)
        {
            Debug.LogWarning("Missing checkpoint prefab");
            return;
        }
        GameObject checkpoint = (GameObject)PrefabUtility.InstantiatePrefab(asset);
        checkpoint.transform.SetParent(parent);
        checkpoint.transform.position = position;
    }

    private static void CreateTurret(Vector3 position, GameObject player, Material material, Transform parent)
    {
        GameObject head = Box("Turret Head", position, new Vector3(1.2f, 1.2f, 1.2f), material, parent);
        Object.DestroyImmediate(head.GetComponent<Collider>());
        Turret turret = head.AddComponent<Turret>();
        turret.player = player;
        turret.ballPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TurretBallPrefab);
        turret.maxDistance = 14f;
        turret.secondsBetweenShots = 2.5f;
        turret.ballSpeed = 8f;
    }

    private static GameObject CreatePlayerAndCamera()
    {
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "PlayerObject (Sarv Controller Reference)";
        player.transform.position = new Vector3(0f, 9.5f, -28f);
        Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());
        CharacterController character = player.AddComponent<CharacterController>();
        character.height = 2f;
        character.radius = 0.5f;
        player.tag = "Player";
        player.AddComponent<PlayerController>();
        player.AddComponent<Inventory>();
        player.AddComponent<InventoryHUD>();
        player.AddComponent<PlayerPuzzlePiece>();
        player.GetComponent<Renderer>().sharedMaterial = GetMaterial("F2_Player", new Color(0.04f, 0.05f, 0.07f), 0.25f);

        GameObject beak = Box("Beak", player.transform.position + new Vector3(0f, 0.25f, 0.48f), new Vector3(0.35f, 0.2f, 0.35f), GetMaterial("F2_Beak", new Color(1f, 0.55f, 0.05f), 0.3f), player.transform);
        Object.DestroyImmediate(beak.GetComponent<Collider>());

        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 15f, -36f);
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.fieldOfView = 65f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.03f, 0.06f, 0.1f);
        ThirdPersonCamera follow = cameraObject.AddComponent<ThirdPersonCamera>();
        follow.player = player.transform;
        follow.offset = new Vector3(0f, 5.5f, -8f);
        cameraObject.AddComponent<AudioListener>();

        GameObject lightObject = new GameObject("Directional Light");
        lightObject.transform.rotation = Quaternion.Euler(50f, 30f, 0f);
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.1f;
        light.color = new Color(0.7f, 0.88f, 1f);
        return player;
    }

    private static void CreateExit(Vector3 position, Material wall, Material goal, Transform parent)
    {
        Transform frame = NewGroup("Sealed Exit Goal", parent);
        Box("Exit Left", position + new Vector3(-2.3f, 1.8f, 0f), new Vector3(1f, 4.5f, 1f), wall, frame);
        Box("Exit Right", position + new Vector3(2.3f, 1.8f, 0f), new Vector3(1f, 4.5f, 1f), wall, frame);
        Box("Exit Top", position + new Vector3(0f, 4f, 0f), new Vector3(5.6f, 1f, 1f), wall, frame);
        GameObject portal = Box("Exit Portal - Win UI Hook", position + new Vector3(0f, 1.7f, 0f), new Vector3(3.6f, 3.5f, 0.35f), goal, frame);
        portal.GetComponent<BoxCollider>().isTrigger = true;
        // MenuStateTrigger expects a solid gate collider next to the trigger and opens it once the player has all pieces.
        portal.AddComponent<BoxCollider>();
        MenuStateTrigger trigger = portal.AddComponent<MenuStateTrigger>();
        trigger.TriggerResult = MenuStateTrigger.Result.Win;
    }

    private static void LavaTrigger(string name, Vector3 position, Vector3 scale, Material material, Transform parent)
    {
        GameObject lava = Box(name, position, scale, material, parent);
        lava.GetComponent<BoxCollider>().isTrigger = true;
        lava.AddComponent<Hazard>();
    }

    private static void PuzzleMarker(string name, Vector3 position, Material material, Transform parent)
    {
        GameObject marker = Box(name, position, new Vector3(0.75f, 0.75f, 0.18f), material, parent);
        marker.transform.rotation = Quaternion.Euler(35f, 45f, 20f);
        BoxCollider trigger = marker.GetComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(1.5f, 1.5f, 5f);
        marker.AddComponent<PuzzlePieceDisplay>();
        marker.AddComponent<PuzzlePiece>();

        GameObject pedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pedestal.name = $"{name} Pedestal";
        pedestal.transform.SetParent(parent);
        pedestal.transform.position = position - Vector3.up * 1.05f;
        pedestal.transform.localScale = new Vector3(1.4f, 0.15f, 1.4f);
        pedestal.GetComponent<Renderer>().sharedMaterial = material;
        Object.DestroyImmediate(pedestal.GetComponent<Collider>());
    }

    private static GameObject Box(string name, Vector3 position, Vector3 scale, Material material, Transform parent)
    {
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = name;
        box.transform.SetParent(parent);
        box.transform.position = position;
        box.transform.localScale = scale;
        box.GetComponent<Renderer>().sharedMaterial = material;
        return box;
    }

    private static Transform NewGroup(string name, Transform parent)
    {
        GameObject group = new GameObject(name);
        group.transform.SetParent(parent);
        return group.transform;
    }

    private static Material GetMaterial(string name, Color color, float smoothness)
    {
        string path = $"{MaterialFolder}/{name}.mat";
        Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null)
        {
            return existing;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        Material material = new Material(shader) { name = name, color = color };
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    private static void AddSceneToBuildSettings()
    {
        EditorBuildSettingsScene[] current = EditorBuildSettings.scenes;
        foreach (EditorBuildSettingsScene buildScene in current)
        {
            if (buildScene.path == ScenePath)
            {
                return;
            }
        }

        EditorBuildSettingsScene[] updated = new EditorBuildSettingsScene[current.Length + 1];
        current.CopyTo(updated, 0);
        updated[current.Length] = new EditorBuildSettingsScene(ScenePath, true);
        EditorBuildSettings.scenes = updated;
    }
}
#endif
