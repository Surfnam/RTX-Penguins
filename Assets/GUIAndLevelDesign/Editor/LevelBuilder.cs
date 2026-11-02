#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Reflection;

public static class LevelBuilder
{
    private const string ScenePath = "Assets/Scenes/LabyrinthFloor01.unity";
    private const string MaterialFolder = "Assets/GUIAndLevelDesign/Materials";

    [MenuItem("RTX Penguins/Build Labyrinth Level")]
    public static void BuildLevel()
    {
        EnsureFolder("Assets/GUIAndLevelDesign", "Materials");
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        Material ice = GetMaterial("Ice", new Color(0.13f, 0.36f, 0.48f), 0.65f, 0.35f);
        Material darkIce = GetMaterial("DarkIce", new Color(0.035f, 0.11f, 0.18f), 0.35f, 0.15f);
        Material wall = GetMaterial("Wall", new Color(0.1f, 0.19f, 0.25f), 0.2f, 0.1f);
        Material hazard = GetMaterial("Hazard", new Color(0.9f, 0.1f, 0.12f), 0.4f, 0.25f);
        Material accent = GetMaterial("Accent", new Color(0.05f, 0.8f, 0.95f), 0.85f, 0.3f);
        Material piece = GetMaterial("PuzzlePiece", new Color(1f, 0.58f, 0.06f), 0.7f, 0.45f);
        Material goal = GetMaterial("Goal", new Color(0.1f, 0.95f, 0.4f), 0.65f, 0.3f);

        CreateEnvironment(darkIce);
        GameObject levelRoot = new GameObject("LABYRINTH_LEVEL_FLOOR_01");
        CreateCourse(levelRoot.transform, ice, wall, hazard, accent, piece, goal);
        CreatePlayerAndCamera();
        new GameObject("Menu System", typeof(PenguinMenuController));

        EditorSceneManager.SaveScene(scene, ScenePath);
        AddSceneToBuildSettings();
        AssetDatabase.SaveAssets();
        Debug.Log($"Built labyrinth level at {ScenePath}");
    }

    [MenuItem("RTX Penguins/Validate Labyrinth Level")]
    public static void ValidateLevel()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        if (!scene.IsValid())
        {
            throw new System.InvalidOperationException("Labyrinth level scene could not be opened.");
        }

        Require(Object.FindFirstObjectByType<PlayerController>() != null, "PlayerController is missing.");
        ThirdPersonCamera camera = Object.FindFirstObjectByType<ThirdPersonCamera>();
        Require(camera != null && camera.player != null, "Third-person camera is not connected to the player.");

        PuzzlePieceDisplay[] pieces = Object.FindObjectsByType<PuzzlePieceDisplay>(FindObjectsSortMode.None);
        Require(pieces.Length == 4, $"Expected 4 puzzle placement markers, found {pieces.Length}.");

        MenuStateTrigger[] triggers = Object.FindObjectsByType<MenuStateTrigger>(FindObjectsSortMode.None);
        int wins = 0;
        int losses = 0;
        foreach (MenuStateTrigger trigger in triggers)
        {
            Require(trigger.GetComponent<Collider>() != null && trigger.GetComponent<Collider>().isTrigger,
                $"{trigger.name} does not have a trigger collider.");
            if (trigger.TriggerResult == MenuStateTrigger.Result.Win) wins++;
            else losses++;
        }
        Require(wins == 1, $"Expected 1 win trigger, found {wins}.");
        Require(losses >= 4, $"Expected at least 4 lose triggers, found {losses}.");

        PenguinMenuController menu = Object.FindFirstObjectByType<PenguinMenuController>();
        Require(menu != null, "Menu controller is missing.");
        MethodInfo applyState = typeof(PenguinMenuController).GetMethod("ApplyScreenState", BindingFlags.Instance | BindingFlags.NonPublic);
        Require(applyState != null, "Menu state transition method is missing.");
        applyState.Invoke(menu, new object[] { PenguinMenuController.ScreenState.Playing });
        menu.ShowWin();
        Require(menu.CurrentScreen == PenguinMenuController.ScreenState.Win, "Win menu transition failed.");
        applyState.Invoke(menu, new object[] { PenguinMenuController.ScreenState.Playing });
        menu.ShowLose();
        Require(menu.CurrentScreen == PenguinMenuController.ScreenState.Lose, "Lose menu transition failed.");
        Time.timeScale = 1f;

        Debug.Log($"LEVEL VALIDATION PASSED: 4 piece markers, {losses} traps, 1 exit, menus operational.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new System.InvalidOperationException(message);
        }
    }

    private static void CreateCourse(Transform root, Material ice, Material wall, Material hazard, Material accent, Material piece, Material goal)
    {
        Transform geometry = NewGroup("Platforms", root);
        Transform traps = NewGroup("Traps_UI_Lose_Hooks", root);
        Transform markers = NewGroup("Puzzle_Piece_Placement_Markers", root);

        // Start arena and a readable zig-zag jump tutorial.
        Platform("Start Plaza", new Vector3(0f, 0f, -24f), new Vector3(14f, 1f, 12f), ice, geometry);
        Platform("Jump 01", new Vector3(-3f, 0.4f, -15f), new Vector3(4f, 1f, 4f), ice, geometry);
        Platform("Jump 02", new Vector3(2.5f, 0.9f, -10.5f), new Vector3(4f, 1f, 4f), ice, geometry);
        Platform("Jump 03", new Vector3(-2.5f, 1.4f, -6f), new Vector3(4f, 1f, 4f), ice, geometry);
        Platform("Maze Entry", new Vector3(0f, 1.8f, 0f), new Vector3(15f, 1f, 8f), ice, geometry);

        // Compact labyrinth: walls create two loops and reward looking down side paths.
        Platform("Labyrinth Floor", new Vector3(0f, 1.8f, 12f), new Vector3(24f, 1f, 20f), ice, geometry);
        Wall("West Rim", new Vector3(-11.5f, 3.8f, 12f), new Vector3(1f, 4f, 20f), wall, geometry);
        Wall("East Rim", new Vector3(11.5f, 3.8f, 12f), new Vector3(1f, 4f, 20f), wall, geometry);
        Wall("Left Split A", new Vector3(-4f, 3.3f, 7f), new Vector3(1f, 3f, 10f), wall, geometry);
        Wall("Right Split A", new Vector3(4f, 3.3f, 14f), new Vector3(1f, 3f, 12f), wall, geometry);
        Wall("Cross Split", new Vector3(-1f, 3.3f, 17f), new Vector3(10f, 3f, 1f), wall, geometry);
        Wall("Final Funnel Left", new Vector3(-7.5f, 3.3f, 21f), new Vector3(8f, 3f, 1f), wall, geometry);
        Wall("Final Funnel Right", new Vector3(7.5f, 3.3f, 21f), new Vector3(8f, 3f, 1f), wall, geometry);

        // Raised side route and final ascent.
        Platform("Side Ledge", new Vector3(-8.5f, 3.4f, 14f), new Vector3(4f, 1f, 7f), accent, geometry);
        Platform("Ascent 01", new Vector3(0f, 3f, 25f), new Vector3(5f, 1f, 4f), ice, geometry);
        Platform("Ascent 02", new Vector3(5f, 4.1f, 29f), new Vector3(5f, 1f, 4f), ice, geometry);
        Platform("Ascent 03", new Vector3(0f, 5.2f, 33f), new Vector3(5f, 1f, 4f), ice, geometry);
        Platform("Exit Island", new Vector3(0f, 5.2f, 40f), new Vector3(14f, 1f, 10f), ice, geometry);

        // Static trap lanes. These only route to the lose menu; death/checkpoints remain integration work.
        Hazard("Start Pit", new Vector3(0f, -2.3f, -12f), new Vector3(18f, 0.6f, 12f), hazard, traps);
        Hazard("Maze Trap Lane A", new Vector3(0f, 2.45f, 8.5f), new Vector3(7f, 0.25f, 1.3f), hazard, traps);
        Hazard("Maze Trap Lane B", new Vector3(-3f, 2.45f, 19.5f), new Vector3(8f, 0.25f, 1.3f), hazard, traps);
        Hazard("Ascent Void", new Vector3(0f, 0f, 31f), new Vector3(25f, 0.5f, 18f), hazard, traps);

        // Four intentionally distinct placements: tutorial high point, maze dead end, side ledge, final ascent.
        PuzzleMarker("Piece Marker 01 - Jump Route", new Vector3(-2.5f, 3.25f, -6f), piece, markers);
        PuzzleMarker("Piece Marker 02 - Maze Dead End", new Vector3(8f, 3.5f, 8f), piece, markers);
        PuzzleMarker("Piece Marker 03 - Raised Side Ledge", new Vector3(-8.5f, 5.1f, 14f), piece, markers);
        PuzzleMarker("Piece Marker 04 - Final Ascent", new Vector3(5f, 5.8f, 29f), piece, markers);

        CreateExit(new Vector3(0f, 6.45f, 42f), wall, goal, root);
        LevelRuntimeBootstrap.PlacePickups(NewGroup("Inventory_Pickups", root), prefab => (GameObject)PrefabUtility.InstantiatePrefab(prefab));
    }

    private static void CreateExit(Vector3 position, Material wall, Material goal, Transform parent)
    {
        GameObject frame = new GameObject("Sealed Exit Goal");
        frame.transform.SetParent(parent);
        Wall("Exit Left", position + new Vector3(-2.3f, 1.8f, 0f), new Vector3(1f, 4.5f, 1f), wall, frame.transform);
        Wall("Exit Right", position + new Vector3(2.3f, 1.8f, 0f), new Vector3(1f, 4.5f, 1f), wall, frame.transform);
        Wall("Exit Top", position + new Vector3(0f, 4f, 0f), new Vector3(5.6f, 1f, 1f), wall, frame.transform);
        GameObject portal = Primitive("Exit Portal - Win UI Hook", PrimitiveType.Cube, position + new Vector3(0f, 1.7f, 0f), new Vector3(3.6f, 3.5f, 0.35f), goal, frame.transform);
        BoxCollider collider = portal.GetComponent<BoxCollider>();
        collider.isTrigger = true;
        MenuStateTrigger trigger = portal.AddComponent<MenuStateTrigger>();
        trigger.TriggerResult = MenuStateTrigger.Result.Win;
    }

    private static void CreatePlayerAndCamera()
    {
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "PlayerObject (Sarv Controller Reference)";
        player.transform.position = new Vector3(0f, 1.5f, -26f);
        Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());
        CharacterController character = player.AddComponent<CharacterController>();
        character.height = 2f;
        character.radius = 0.5f;
        player.tag = "Player";
        player.AddComponent<PlayerController>();
        player.AddComponent<Inventory>();
        player.AddComponent<InventoryHUD>();
        player.AddComponent<PlayerPuzzlePiece>();
        Renderer renderer = player.GetComponent<Renderer>();
        renderer.sharedMaterial = GetMaterial("PlayerPlaceholder", new Color(0.04f, 0.05f, 0.07f), 0.25f, 0.1f);

        GameObject beak = Primitive("Beak", PrimitiveType.Sphere, player.transform.position + new Vector3(0f, 0.25f, 0.48f), new Vector3(0.35f, 0.2f, 0.35f), GetMaterial("Beak", new Color(1f, 0.55f, 0.05f), 0.3f, 0.1f), player.transform);
        Object.DestroyImmediate(beak.GetComponent<Collider>());

        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 6f, -33f);
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.fieldOfView = 65f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.015f, 0.035f, 0.07f);
        ThirdPersonCamera follow = cameraObject.AddComponent<ThirdPersonCamera>();
        follow.player = player.transform;
        follow.offset = new Vector3(0f, 5.5f, -8f);
        cameraObject.AddComponent<AudioListener>();

        GameObject lightObject = new GameObject("Directional Light");
        lightObject.transform.rotation = Quaternion.Euler(48f, -28f, 0f);
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.25f;
        light.color = new Color(0.72f, 0.9f, 1f);
        RenderSettings.ambientLight = new Color(0.11f, 0.18f, 0.26f);
    }

    private static void CreateEnvironment(Material material)
    {
        Primitive("Distant Ice Shelf", PrimitiveType.Cube, new Vector3(0f, -4f, 10f), new Vector3(90f, 2f, 100f), material, null);
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.025f, 0.09f, 0.14f);
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = 0.012f;
    }

    private static GameObject Platform(string name, Vector3 position, Vector3 scale, Material material, Transform parent)
    {
        return Primitive(name, PrimitiveType.Cube, position, scale, material, parent);
    }

    private static GameObject Wall(string name, Vector3 position, Vector3 scale, Material material, Transform parent)
    {
        return Primitive(name, PrimitiveType.Cube, position, scale, material, parent);
    }

    private static void Hazard(string name, Vector3 position, Vector3 scale, Material material, Transform parent)
    {
        GameObject hazard = Primitive(name, PrimitiveType.Cube, position, scale, material, parent);
        hazard.GetComponent<BoxCollider>().isTrigger = true;
        MenuStateTrigger trigger = hazard.AddComponent<MenuStateTrigger>();
        trigger.TriggerResult = MenuStateTrigger.Result.Lose;
    }

    private static void PuzzleMarker(string name, Vector3 position, Material material, Transform parent)
    {
        GameObject marker = Primitive(name, PrimitiveType.Cube, position, new Vector3(0.75f, 0.75f, 0.18f), material, parent);
        marker.transform.rotation = Quaternion.Euler(35f, 45f, 20f);
        BoxCollider trigger = marker.GetComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(1.5f, 1.5f, 5f);
        marker.AddComponent<PuzzlePieceDisplay>();
        marker.AddComponent<PuzzlePiece>();

        GameObject pedestal = Primitive($"{name} Pedestal", PrimitiveType.Cylinder, position - Vector3.up * 1.05f, new Vector3(1.4f, 0.15f, 1.4f), material, parent);
        Object.DestroyImmediate(pedestal.GetComponent<Collider>());
    }

    private static GameObject Primitive(string name, PrimitiveType type, Vector3 position, Vector3 scale, Material material, Transform parent)
    {
        GameObject gameObject = GameObject.CreatePrimitive(type);
        gameObject.name = name;
        gameObject.transform.SetParent(parent);
        gameObject.transform.position = position;
        gameObject.transform.localScale = scale;
        gameObject.GetComponent<Renderer>().sharedMaterial = material;
        return gameObject;
    }

    private static Transform NewGroup(string name, Transform parent)
    {
        GameObject group = new GameObject(name);
        group.transform.SetParent(parent);
        return group.transform;
    }

    private static Material GetMaterial(string name, Color color, float smoothness, float metallic)
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
        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    private static void EnsureFolder(string parent, string child)
    {
        string path = $"{parent}/{child}";
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder(parent, child);
        }
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
