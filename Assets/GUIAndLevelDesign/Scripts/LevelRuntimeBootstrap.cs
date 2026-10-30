using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Makes the GUI and level-design scene playable from the repository's default SampleScene even
/// before the editor-only scene generator has been run. The generated scene
/// remains the preferred handoff asset once Unity is available.
/// </summary>
public static class LevelRuntimeBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void BuildInDefaultScene()
    {
        if (SceneManager.GetActiveScene().name != "SampleScene" || GameObject.Find("LABYRINTH_LEVEL_FLOOR_01") != null)
        {
            return;
        }

        foreach (Camera camera in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
        {
            Object.Destroy(camera.gameObject);
        }

        BuildLevel();
    }

    private static void BuildLevel()
    {
        Material ice = Material("Runtime Ice", new Color(0.13f, 0.36f, 0.48f), 0.65f);
        Material darkIce = Material("Runtime Dark Ice", new Color(0.025f, 0.08f, 0.12f), 0.3f);
        Material wall = Material("Runtime Wall", new Color(0.08f, 0.17f, 0.24f), 0.25f);
        Material hazard = Material("Runtime Hazard", new Color(0.9f, 0.08f, 0.1f), 0.4f);
        Material accent = Material("Runtime Accent", new Color(0.04f, 0.78f, 0.95f), 0.8f);
        Material piece = Material("Runtime Puzzle Marker", new Color(1f, 0.58f, 0.05f), 0.7f);
        Material goal = Material("Runtime Goal", new Color(0.08f, 0.95f, 0.38f), 0.7f);

        GameObject root = new GameObject("LABYRINTH_LEVEL_FLOOR_01");
        Transform platforms = Group("Platforms", root.transform);
        Transform traps = Group("Traps_UI_Lose_Hooks", root.transform);
        Transform markers = Group("Puzzle_Piece_Placement_Markers", root.transform);

        Box("Start Plaza", new Vector3(0f, 0f, -24f), new Vector3(14f, 1f, 12f), ice, platforms);
        Box("Jump 01", new Vector3(-3f, 0.4f, -15f), new Vector3(4f, 1f, 4f), ice, platforms);
        Box("Jump 02", new Vector3(2.5f, 0.9f, -10.5f), new Vector3(4f, 1f, 4f), ice, platforms);
        Box("Jump 03", new Vector3(-2.5f, 1.4f, -6f), new Vector3(4f, 1f, 4f), ice, platforms);
        Box("Maze Entry", new Vector3(0f, 1.8f, 0f), new Vector3(15f, 1f, 8f), ice, platforms);
        Box("Labyrinth Floor", new Vector3(0f, 1.8f, 12f), new Vector3(24f, 1f, 20f), ice, platforms);

        Box("West Rim", new Vector3(-11.5f, 3.8f, 12f), new Vector3(1f, 4f, 20f), wall, platforms);
        Box("East Rim", new Vector3(11.5f, 3.8f, 12f), new Vector3(1f, 4f, 20f), wall, platforms);
        Box("Left Split A", new Vector3(-4f, 3.3f, 7f), new Vector3(1f, 3f, 10f), wall, platforms);
        Box("Right Split A", new Vector3(4f, 3.3f, 14f), new Vector3(1f, 3f, 12f), wall, platforms);
        Box("Cross Split", new Vector3(-1f, 3.3f, 17f), new Vector3(10f, 3f, 1f), wall, platforms);
        Box("Final Funnel Left", new Vector3(-7.5f, 3.3f, 21f), new Vector3(8f, 3f, 1f), wall, platforms);
        Box("Final Funnel Right", new Vector3(7.5f, 3.3f, 21f), new Vector3(8f, 3f, 1f), wall, platforms);

        Box("Side Ledge", new Vector3(-8.5f, 3.4f, 14f), new Vector3(4f, 1f, 7f), accent, platforms);
        Box("Ascent 01", new Vector3(0f, 3f, 25f), new Vector3(5f, 1f, 4f), ice, platforms);
        Box("Ascent 02", new Vector3(5f, 4.1f, 29f), new Vector3(5f, 1f, 4f), ice, platforms);
        Box("Ascent 03", new Vector3(0f, 5.2f, 33f), new Vector3(5f, 1f, 4f), ice, platforms);
        Box("Exit Island", new Vector3(0f, 5.2f, 40f), new Vector3(14f, 1f, 10f), ice, platforms);

        LoseTrigger("Start Pit", new Vector3(0f, -2.3f, -12f), new Vector3(18f, 0.6f, 12f), hazard, traps);
        LoseTrigger("Maze Trap Lane A", new Vector3(0f, 2.45f, 8.5f), new Vector3(7f, 0.25f, 1.3f), hazard, traps);
        LoseTrigger("Maze Trap Lane B", new Vector3(-3f, 2.45f, 19.5f), new Vector3(8f, 0.25f, 1.3f), hazard, traps);
        LoseTrigger("Ascent Void", new Vector3(0f, 0f, 31f), new Vector3(25f, 0.5f, 18f), hazard, traps);
        LoseTrigger("World Fall Catcher", new Vector3(0f, -5f, 10f), new Vector3(90f, 1f, 110f), darkIce, traps);

        Marker("Piece Marker 01 - Jump Route", new Vector3(-2.5f, 3.25f, -6f), piece, markers);
        Marker("Piece Marker 02 - Maze Dead End", new Vector3(8f, 3.5f, 8f), piece, markers);
        Marker("Piece Marker 03 - Raised Side Ledge", new Vector3(-8.5f, 5.1f, 14f), piece, markers);
        Marker("Piece Marker 04 - Final Ascent", new Vector3(5f, 5.8f, 29f), piece, markers);

        CreateExit(new Vector3(0f, 6.45f, 42f), wall, goal, root.transform);
        CreatePlayerAndCamera();
        new GameObject("Menu System", typeof(PenguinMenuController));

        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.025f, 0.09f, 0.14f);
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = 0.012f;
        RenderSettings.ambientLight = new Color(0.11f, 0.18f, 0.26f);
    }

    private static void CreatePlayerAndCamera()
    {
        Material playerMaterial = Material("Runtime Player", new Color(0.03f, 0.04f, 0.06f), 0.25f);
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "PlayerObject (Sarv Controller Reference)";
        player.transform.position = new Vector3(0f, 1.5f, -26f);
        Object.Destroy(player.GetComponent<CapsuleCollider>());
        player.GetComponent<Renderer>().sharedMaterial = playerMaterial;
        player.AddComponent<CharacterController>();
        player.AddComponent<PlayerController>();

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

        GameObject lightObject = new GameObject("Level Light");
        lightObject.transform.rotation = Quaternion.Euler(48f, -28f, 0f);
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.25f;
        light.color = new Color(0.72f, 0.9f, 1f);
    }

    private static void CreateExit(Vector3 position, Material wall, Material goal, Transform parent)
    {
        Transform frame = Group("Sealed Exit Goal", parent);
        Box("Exit Left", position + new Vector3(-2.3f, 1.8f, 0f), new Vector3(1f, 4.5f, 1f), wall, frame);
        Box("Exit Right", position + new Vector3(2.3f, 1.8f, 0f), new Vector3(1f, 4.5f, 1f), wall, frame);
        Box("Exit Top", position + new Vector3(0f, 4f, 0f), new Vector3(5.6f, 1f, 1f), wall, frame);
        GameObject portal = Box("Exit Portal - Win UI Hook", position + new Vector3(0f, 1.7f, 0f), new Vector3(3.6f, 3.5f, 0.35f), goal, frame);
        portal.GetComponent<BoxCollider>().isTrigger = true;
        MenuStateTrigger trigger = portal.AddComponent<MenuStateTrigger>();
        trigger.TriggerResult = MenuStateTrigger.Result.Win;
    }

    private static void LoseTrigger(string name, Vector3 position, Vector3 scale, Material material, Transform parent)
    {
        GameObject gameObject = Box(name, position, scale, material, parent);
        gameObject.GetComponent<BoxCollider>().isTrigger = true;
        MenuStateTrigger trigger = gameObject.AddComponent<MenuStateTrigger>();
        trigger.TriggerResult = MenuStateTrigger.Result.Lose;
    }

    private static void Marker(string name, Vector3 position, Material material, Transform parent)
    {
        GameObject marker = Box(name, position, new Vector3(0.75f, 0.75f, 0.18f), material, parent);
        marker.transform.rotation = Quaternion.Euler(35f, 45f, 20f);
        Object.Destroy(marker.GetComponent<Collider>());
        marker.AddComponent<PuzzlePieceDisplay>();
    }

    private static GameObject Box(string name, Vector3 position, Vector3 scale, Material material, Transform parent)
    {
        GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        gameObject.name = name;
        gameObject.transform.SetParent(parent);
        gameObject.transform.position = position;
        gameObject.transform.localScale = scale;
        gameObject.GetComponent<Renderer>().sharedMaterial = material;
        return gameObject;
    }

    private static Transform Group(string name, Transform parent)
    {
        GameObject group = new GameObject(name);
        group.transform.SetParent(parent);
        return group.transform;
    }

    private static Material Material(string name, Color color, float smoothness)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        Material material = new Material(shader) { name = name, color = color };
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
        return material;
    }
}
