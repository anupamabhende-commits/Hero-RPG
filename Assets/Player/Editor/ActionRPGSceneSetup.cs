using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ActionRPGSceneSetup
{
    private const string ScenePath = "Assets/Scenes/ActionRPGPrototype.unity";
    private const string CharacterPrefabPath = "Assets/Character/character.prefab";

    [MenuItem("Magic Adventure/Setup Action RPG Prototype Scene")]
    public static void CreatePrototypeSceneFromMenu()
    {
        CreatePrototypeScene();
    }

    public static void CreatePrototypeSceneBatch()
    {
        CreatePrototypeScene();
        EditorApplication.Exit(0);
    }

    private static void CreatePrototypeScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "ActionRPGPrototype";

        Material groundMaterial = CreateRuntimeMaterial("Prototype Ground Material", new Color(0.22f, 0.28f, 0.24f));
        Material obstacleMaterial = CreateRuntimeMaterial("Prototype Obstacle Material", new Color(0.27f, 0.25f, 0.32f));
        Material targetMaterial = CreateRuntimeMaterial("Prototype Target Material", new Color(0.65f, 0.18f, 0.14f));
        Material portalMaterial = CreateRuntimeMaterial("Prototype Portal Destination Material", new Color(0.12f, 0.4f, 0.78f));

        CreateLighting();
        CreateGround(groundMaterial);
        CreateObstacleCourse(obstacleMaterial);

        GameObject playerObject = CreatePlayer();
        PlayerController playerController = ConfigurePlayer(playerObject);
        Transform cameraTarget = CreateCameraTarget(playerObject.transform);
        Transform lockOnTarget = CreateLockOnTarget(targetMaterial);
        Transform portalDestination = CreatePortalDestination(portalMaterial);
        Camera sceneCamera = CreateCamera(cameraTarget, playerController, lockOnTarget);

        WireReferences(playerController, sceneCamera, cameraTarget, lockOnTarget, portalDestination);
        AddSceneToBuildSettings();

        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = playerObject;
        Debug.Log($"Action RPG prototype scene created at {ScenePath}.");
    }

    private static void CreateLighting()
    {
        GameObject lightObject = new GameObject("Directional Light");
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 2.2f;
        light.shadows = LightShadows.Soft;
        lightObject.transform.rotation = Quaternion.Euler(48f, -34f, 0f);

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.43f, 0.48f, 0.56f);
        RenderSettings.ambientEquatorColor = new Color(0.25f, 0.27f, 0.3f);
        RenderSettings.ambientGroundColor = new Color(0.13f, 0.13f, 0.12f);
    }

    private static void CreateGround(Material material)
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Combat Test Arena";
        ground.transform.localScale = new Vector3(4f, 1f, 4f);
        ground.GetComponent<Renderer>().sharedMaterial = material;
    }

    private static void CreateObstacleCourse(Material material)
    {
        CreateObstacle("Camera Collision Wall", new Vector3(0f, 1.25f, -4f), new Vector3(5.5f, 2.5f, 0.35f), material);
        CreateObstacle("Dodge Pillar A", new Vector3(-3.5f, 1.1f, 3.2f), new Vector3(0.8f, 2.2f, 0.8f), material);
        CreateObstacle("Dodge Pillar B", new Vector3(3.2f, 1.1f, 2.4f), new Vector3(0.8f, 2.2f, 0.8f), material);
    }

    private static void CreateObstacle(string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obstacle.name = name;
        obstacle.transform.position = position;
        obstacle.transform.localScale = scale;
        obstacle.GetComponent<Renderer>().sharedMaterial = material;
    }

    private static GameObject CreatePlayer()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CharacterPrefabPath);

        if (prefab == null)
        {
            GameObject fallback = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            fallback.name = "Player";
            fallback.transform.position = Vector3.zero;
            return fallback;
        }

        GameObject playerObject = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        playerObject.name = "Player";
        playerObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        DisableEmbeddedCameras(playerObject);
        return playerObject;
    }

    private static PlayerController ConfigurePlayer(GameObject playerObject)
    {
        Animator animator = playerObject.GetComponentInChildren<Animator>();

        if (animator != null)
        {
            animator.applyRootMotion = false;
        }

        CharacterController characterController = GetOrAdd<CharacterController>(playerObject);
        characterController.height = 1.85f;
        characterController.radius = 0.32f;
        characterController.center = new Vector3(0f, 0.93f, 0f);
        characterController.stepOffset = 0.35f;
        characterController.slopeLimit = 55f;

        PlayerStats stats = GetOrAdd<PlayerStats>(playerObject);
        stats.walkSpeed = 4.2f;
        stats.sprintSpeed = 7.2f;
        stats.acceleration = 18f;
        stats.deceleration = 22f;
        stats.useRootMotion = false;

        PlayerInput input = GetOrAdd<PlayerInput>(playerObject);
        input.portalKey = KeyCode.F;

        PlayerMovement movement = GetOrAdd<PlayerMovement>(playerObject);
        PlayerRotation rotation = GetOrAdd<PlayerRotation>(playerObject);
        PlayerJump jump = GetOrAdd<PlayerJump>(playerObject);
        PlayerDash dash = GetOrAdd<PlayerDash>(playerObject);
        PlayerCombat combat = GetOrAdd<PlayerCombat>(playerObject);
        PlayerAnimator playerAnimator = GetOrAdd<PlayerAnimator>(playerObject);
        GroundChecker groundChecker = GetOrAdd<GroundChecker>(playerObject);
        PlayerController playerController = GetOrAdd<PlayerController>(playerObject);
        PlayerSpellController spellController = playerObject.GetComponent<PlayerSpellController>();

        playerAnimator.animator = animator;
        groundChecker.characterController = characterController;
        combat.spellController = spellController;

        playerController.stats = stats;
        playerController.input = input;
        playerController.movement = movement;
        playerController.rotation = rotation;
        playerController.jump = jump;
        playerController.dash = dash;
        playerController.combat = combat;
        playerController.playerAnimator = playerAnimator;
        playerController.groundChecker = groundChecker;

        if (spellController != null)
        {
            spellController.animator = animator;
            spellController.portalKey = KeyCode.F;
            spellController.debugLogs = false;
        }

        return playerController;
    }

    private static Transform CreateCameraTarget(Transform player)
    {
        GameObject target = new GameObject("CameraTarget");
        target.transform.SetParent(player, false);
        target.transform.localPosition = new Vector3(0f, 1.45f, 0f);
        return target.transform;
    }

    private static Transform CreateLockOnTarget(Material material)
    {
        GameObject target = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        target.name = "Lock-On Test Target";
        target.transform.position = new Vector3(4.5f, 1f, 5.5f);
        target.transform.localScale = new Vector3(0.85f, 1f, 0.85f);
        target.GetComponent<Renderer>().sharedMaterial = material;
        return target.transform;
    }

    private static Transform CreatePortalDestination(Material material)
    {
        GameObject destination = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        destination.name = "Portal Destination";
        destination.transform.position = new Vector3(0f, 0.05f, 12f);
        destination.transform.localScale = new Vector3(1.35f, 0.1f, 1.35f);
        destination.GetComponent<Renderer>().sharedMaterial = material;

        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        marker.name = "Portal Destination Marker";
        marker.transform.SetParent(destination.transform, false);
        marker.transform.localPosition = new Vector3(0f, 1f, 0f);
        marker.transform.localScale = new Vector3(0.35f, 1f, 0.35f);
        marker.GetComponent<Renderer>().sharedMaterial = material;

        return destination.transform;
    }

    private static Camera CreateCamera(Transform target, PlayerController player, Transform lockOnTarget)
    {
        GameObject cameraObject = new GameObject("Action RPG Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = target.position + new Vector3(0f, 1.4f, -5.5f);
        cameraObject.transform.LookAt(target.position + Vector3.up);

        Camera camera = cameraObject.AddComponent<Camera>();
        camera.fieldOfView = 60f;
        camera.nearClipPlane = 0.03f;
        camera.farClipPlane = 500f;
        cameraObject.AddComponent<AudioListener>();

        ActionRPGCamera actionCamera = cameraObject.AddComponent<ActionRPGCamera>();
        actionCamera.target = target;
        actionCamera.player = player;
        actionCamera.lockOnTarget = lockOnTarget;

        return camera;
    }

    private static void WireReferences(
        PlayerController playerController,
        Camera camera,
        Transform cameraTarget,
        Transform lockOnTarget,
        Transform portalDestination)
    {
        playerController.playerCamera = camera;
        playerController.cameraTarget = cameraTarget;
        playerController.lockOnTarget = lockOnTarget;

        PlayerSpellController spellController = playerController.GetComponent<PlayerSpellController>();

        if (spellController != null)
        {
            spellController.portalDestination = portalDestination;
        }
    }

    private static void DisableEmbeddedCameras(GameObject playerObject)
    {
        foreach (Camera camera in playerObject.GetComponentsInChildren<Camera>(true))
        {
            camera.gameObject.tag = "Untagged";
            camera.gameObject.SetActive(false);
        }

        foreach (AudioListener listener in playerObject.GetComponentsInChildren<AudioListener>(true))
        {
            listener.enabled = false;
        }
    }

    private static T GetOrAdd<T>(GameObject gameObject) where T : Component
    {
        T component = gameObject.GetComponent<T>();
        return component != null ? component : gameObject.AddComponent<T>();
    }

    private static Material CreateRuntimeMaterial(string name, Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");

        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        Material material = new Material(shader);
        material.name = name;
        material.color = color;
        return material;
    }

    private static void AddSceneToBuildSettings()
    {
        List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes.ToList();

        if (scenes.Any(scene => scene.path == ScenePath))
        {
            return;
        }

        scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
