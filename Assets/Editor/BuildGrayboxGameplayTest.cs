/*****************************************************************
* COMPONENT OF: Unity Editor Tools menu
* REQUIRED DEPENDENCIES: Original Roll-A-Ball scene, Input System, TextMeshPro, prototype scripts
* DESCRIPTION: Builds the four-section graybox course described in the GDD.
* VERSION 1.6: Adds smaller jump targets, varied landing heights and two timed moving platforms.
*****************************************************************/

using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Rendering;

public static class BuildGrayboxGameplayTest
{
    private const string SourceScenePath = "Assets/Scenes/Roll-A-Ball scene.unity";
    private const string OutputScenePath = "Assets/Scenes/Prototype Gameplay Test.unity";
    private const string CollectSoundPath = "Assets/Audio/Sound FX/SFX_Blip.wav";
    private const string WinSoundPath = "Assets/Audio/Sound FX/SFX_Win.wav";
    // Keep the maze pickup on its verified solution route when creating the four collectibles.
    private static Vector3 mazeCollectiblePosition;

    private const string ParticlePrefabPath = "Assets/Source Files/VFX/ConfettiCelebration.prefab";

    [MenuItem("Tools/Build Graybox Gameplay Test")]
    public static void Build()
    {
        EnsureTag("Player");
        EnsureTag("Collectible");

        Scene prototypeScene = EditorSceneManager.OpenScene(SourceScenePath, OpenSceneMode.Single);
        EditorSceneManager.SaveScene(prototypeScene, OutputScenePath);
        SetUpExistingGameObjects();
        CreateGrayboxCourse();
        CreateSectionCollectibles();
        CreateGameplayMechanics();
        AddSceneToBuildSettings();

        EditorSceneManager.MarkSceneDirty(prototypeScene);
        EditorSceneManager.SaveScene(prototypeScene, OutputScenePath);
        Debug.Log("Graybox gameplay test created at " + OutputScenePath);
    }

    // Scene generation must run outside Play mode so its saved references are persistent.
    [MenuItem("Tools/Build Graybox Gameplay Test", true)]
    private static bool CanBuild()
    {
        return !EditorApplication.isPlayingOrWillChangePlaymode;
    }

    private static void SetUpExistingGameObjects()
    {
        GameObject player = GameObject.Find("Player");
        player.tag = "Player";
        player.transform.SetPositionAndRotation(new Vector3(-37f, 1.2f, 0f), Quaternion.identity);

        Rigidbody playerBody = player.GetComponent<Rigidbody>();
        playerBody.linearVelocity = Vector3.zero;
        playerBody.angularVelocity = Vector3.zero;

        ConfigurePlayerJump(player);
        ConfigureFallRestart(player);

        // Tune only the prototype player for the gap distances used in this generated course.
        SerializedObject movementSettings = new SerializedObject(player.GetComponent<PlayerController>());
        movementSettings.FindProperty("force").floatValue = 16f;
        movementSettings.FindProperty("maximumSpeed").floatValue = 6f;
        movementSettings.ApplyModifiedPropertiesWithoutUndo();
        playerBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        playerBody.interpolation = RigidbodyInterpolation.Interpolate;

        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.orthographic = false;

            CameraController cameraController = mainCamera.GetComponent<CameraController>();
            if (cameraController != null)
            {
                cameraController.enabled = false;
            }

            PrototypeRearCameraFollow rearCamera = mainCamera.GetComponent<PrototypeRearCameraFollow>();
            if (rearCamera == null)
            {
                rearCamera = mainCamera.gameObject.AddComponent<PrototypeRearCameraFollow>();
            }

            SerializedObject cameraSettings = new SerializedObject(rearCamera);
            cameraSettings.FindProperty("playerTransform").objectReferenceValue = player.transform;
            cameraSettings.FindProperty("startingForward").vector3Value = Vector3.right;
            cameraSettings.FindProperty("followDistance").floatValue = 6.49f;
            cameraSettings.FindProperty("followHeight").floatValue = 2.78f;
            cameraSettings.FindProperty("lookAtHeight").floatValue = 0.35f;
            cameraSettings.ApplyModifiedPropertiesWithoutUndo();
        }

        GameManager gameManager = Object.FindAnyObjectByType<GameManager>();
        AudioSource managerAudio = gameManager.GetComponent<AudioSource>();
        if (managerAudio == null)
        {
            managerAudio = gameManager.gameObject.AddComponent<AudioSource>();
        }

        SerializedObject managerSettings = new SerializedObject(gameManager);
        managerSettings.FindProperty("winClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(WinSoundPath);
        managerSettings.ApplyModifiedPropertiesWithoutUndo();
        managerAudio.playOnAwake = false;

        GameObject existingGround = GameObject.Find("Ground");
        if (existingGround != null)
        {
            existingGround.name = "Fall Catch Ground";
            existingGround.transform.position = new Vector3(10f, -8f, 0f);
            existingGround.transform.localScale = new Vector3(120f, 1f, 80f);
        }

        GameObject oldCollectiblesParent = GameObject.Find("Collectibles");
        if (oldCollectiblesParent != null)
        {
            Object.DestroyImmediate(oldCollectiblesParent);
        }
    }

    private static void CreateGrayboxCourse()
    {
        Material platformMaterial = CreateMaterial("Prototype Graybox", new Color(0.42f, 0.46f, 0.5f));
        Material startMaterial = CreateMaterial("Prototype Start", new Color(0.25f, 0.65f, 0.35f));
        Material finishMaterial = CreateMaterial("Prototype Finish", new Color(0.08f, 0.14f, 0.18f));
        Material hazardMaterial = CreateMaterial("Prototype Hazard", new Color(0.85f, 0.65f, 0.15f));

        // Stage 1: a connected maze includes genuine choices and dead ends, with solid outer walls.
        CreateBlock("Start Platform", new Vector3(-37f, 0f, 0f), new Vector3(4f, 1f, 6f), startMaterial);
        CreateBlock("Maze Platform", new Vector3(-25f, 0f, 0f), new Vector3(20f, 1f, 18f), platformMaterial);
        CreateBranchingMaze(hazardMaterial);
        CreateBlock("Maze Exit Path", new Vector3(-14f, 0f, 0f), new Vector3(4f, 1f, 3.5f), platformMaterial);
        CreateCheckpoint("Checkpoint 1 - Maze Complete", new Vector3(-14f, 1.2f, 0f), new Vector3(-10.5f, 1.2f, 0f));

        // Stage 2: separate rotor bays and alternating bridges remove the wide, safe bypass.
        CreateMachineryChallenge(platformMaterial, hazardMaterial);
        CreateBlock("Machinery Exit Path", new Vector3(11f, 0f, 0f), new Vector3(2f, 1f, 5f), platformMaterial);

        // Stage 3: small, staggered landing surfaces and moving targets require timed jumps.
        CreateJumpingChallenge(platformMaterial, hazardMaterial);
        CreateCheckpoint("Checkpoint 3 - Jumping Complete", new Vector3(41f, 1.2f, 0f), new Vector3(41f, 1.2f, 0f));

        // Stage 4: narrow zigzags and gentle sideways rocking require speed control and balance.
        CreateBalanceCourse(hazardMaterial);
        CreateBlock("Finish Platform", new Vector3(66f, 0f, 0f), new Vector3(10f, 1f, 9f), finishMaterial);
        CreateCheckpoint("Checkpoint 4 - Balance Complete", new Vector3(64.2f, 1.2f, 0f), new Vector3(64.2f, 1.2f, 0f));

        CreateFinishMarker(finishMaterial);
    }

    private static void CreateBranchingMaze(Material material)
    {
        PrototypeMazeLayout layout = new PrototypeMazeLayout();
        // Put the collectible well inside the required route, rather than in the entrance corridor.
        mazeCollectiblePosition = PrototypeMazeLayout.CellCenter(layout.Solution[layout.Solution.Count * 2 / 3]);
        for (int column = 0; column < PrototypeMazeLayout.Columns; column++)
        {
            for (int row = 0; row < PrototypeMazeLayout.Rows; row++)
            {
                Vector2Int cell = new Vector2Int(column, row);
                Vector3 center = PrototypeMazeLayout.CellCenter(cell);
                center.y = 2f;
                // Each shared wall is built once. Leave an entrance at the west and exit at the east.
                if (!layout.IsOpen(cell, 0) && !(column == 4 && row == 2))
                {
                    CreateMazeObstacle("Maze East Wall " + column + "-" + row,
                        center + Vector3.right * 2f, new Vector3(0.25f, 3f, 3.85f), material);
                }
                if (!layout.IsOpen(cell, 1))
                {
                    CreateMazeObstacle("Maze North Wall " + column + "-" + row,
                        center + Vector3.forward * 1.8f, new Vector3(4.25f, 3f, 0.25f), material);
                }
                if (column == 0 && row != 2)
                {
                    CreateMazeObstacle("Maze Entrance Border " + row,
                        center + Vector3.left * 2f, new Vector3(0.25f, 3f, 3.85f), material);
                }
                if (row == 0)
                {
                    CreateMazeObstacle("Maze South Border " + column,
                        center + Vector3.back * 1.8f, new Vector3(4.25f, 3f, 0.25f), material);
                }
            }
        }
    }

    private static void CreateBalanceCourse(Material material)
    {
        // Offset turns remove the straight run from the landing platform to the finish.
        Vector3[] turns =
        {
            new Vector3(44f, 0.35f, 0f), new Vector3(47f, 0.35f, 0f),
            new Vector3(50f, 0.35f, 2.5f), new Vector3(53f, 0.35f, -1.5f),
            new Vector3(56f, 0.35f, 2f), new Vector3(59f, 0.35f, -2f),
            // Stop the final beam at the finish deck edge so their top faces never overlap.
            new Vector3(60.76f, 0.35f, -0.8267f)
        };
        for (int index = 0; index < turns.Length - 1; index++)
        {
            Vector3 direction = turns[index + 1] - turns[index];
            Vector3 beamDirection = direction.normalized;
            float connectorInset = 0.425f;
            Vector3 beamStart = turns[index] + (index > 0 ? beamDirection * connectorInset : Vector3.zero);
            Vector3 beamEnd = turns[index + 1] - (index < turns.Length - 2 ? beamDirection * connectorInset : Vector3.zero);
            Vector3 beamSegment = beamEnd - beamStart;
            GameObject beam = CreateBlock("Balance Beam " + (index + 1),
                (beamStart + beamEnd) * 0.5f,
                new Vector3(beamSegment.magnitude, 0.3f, 0.85f), material,
                Quaternion.FromToRotation(Vector3.right, direction.normalized));
            // Prevent narrow beam edges from casting dark seams onto adjoining pieces.
            MeshRenderer beamRenderer = beam.GetComponent<MeshRenderer>();
            beamRenderer.shadowCastingMode = ShadowCastingMode.Off;
            beamRenderer.receiveShadows = false;
            Rigidbody body = beam.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            // The last approach is a stable, level beam joining the flat finish floor.
            if (index < turns.Length - 2)
            {
                PrototypeBalanceBeam balance = beam.AddComponent<PrototypeBalanceBeam>();
                SerializedObject settings = new SerializedObject(balance);
                settings.FindProperty("phaseOffset").floatValue = index * 0.9f;
                settings.ApplyModifiedPropertiesWithoutUndo();
            }
            // Slightly raise each small connector to cover the beam ends without coplanar overlap.
            if (index > 0)
            {
                GameObject joint = CreateBlock("Balance Joint " + index, turns[index] + Vector3.up * 0.01f,
                    new Vector3(0.85f, 0.3f, 0.85f), material);
                // Match the beam lighting so the raised connector blends into the walking surface.
                MeshRenderer jointRenderer = joint.GetComponent<MeshRenderer>();
                jointRenderer.shadowCastingMode = ShadowCastingMode.Off;
                jointRenderer.receiveShadows = false;
            }
        }
    }

    // Create an invisible trigger that saves a safe spawn position for the next attempt.
    private static void CreateCheckpoint(string checkpointName, Vector3 triggerPosition, Vector3 respawnPosition)
    {
        GameObject checkpoint = new GameObject(checkpointName);
        checkpoint.transform.position = triggerPosition;
        BoxCollider trigger = checkpoint.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(1f, 2.5f, 4f);

        PrototypeCheckpoint checkpointController = checkpoint.AddComponent<PrototypeCheckpoint>();
        SerializedObject settings = new SerializedObject(checkpointController);
        settings.FindProperty("respawnPosition").vector3Value = respawnPosition;
        settings.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreateMazeObstacle(string objectName, Vector3 position, Vector3 scale, Material material)
    {
        GameObject wall = CreateBlock(objectName, position, scale, material);
        // Mark only obstructing walls as fadeable; their physics colliders remain solid.
        wall.AddComponent<PrototypeCameraOccluder>();
    }

    private static void CreateSectionCollectibles()
    {
        Material collectibleMaterial = CreateMaterial("Prototype Collectible", new Color(0.95f, 0.78f, 0.18f));
        AudioClip collectSound = AssetDatabase.LoadAssetAtPath<AudioClip>(CollectSoundPath);
        GameObject particlePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ParticlePrefabPath);
        Vector3[] positions =
        {
            // One pickup for each section: maze, machinery, platform jumping, narrow path.
            mazeCollectiblePosition,
            new Vector3(0f, 1f, 2.2f),
            new Vector3(31f, 1.45f, 1.6f),
            new Vector3(56f, 1.05f, 2f)
        };

        foreach (Vector3 position in positions)
        {
            GameObject collectible = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            collectible.name = "Section Collectible " + (System.Array.IndexOf(positions, position) + 1);
            collectible.tag = "Collectible";
            collectible.transform.position = position;
            collectible.transform.localScale = Vector3.one * 0.75f;
            collectible.GetComponent<Renderer>().sharedMaterial = collectibleMaterial;
            collectible.GetComponent<SphereCollider>().isTrigger = true;

            CollectibleController collectibleController = collectible.AddComponent<CollectibleController>();
            SerializedObject collectibleSettings = new SerializedObject(collectibleController);
            collectibleSettings.FindProperty("collectSound").objectReferenceValue = collectSound;
            collectibleSettings.FindProperty("collectParticlePrefab").objectReferenceValue = particlePrefab;
            collectibleSettings.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void CreateGameplayMechanics()
    {
        Material barrierMaterial = CreateMaterial("Prototype Barrier", new Color(0.7f, 0.28f, 0.2f));
        CreateMachineryRotors(barrierMaterial);

        GameObject finish = new GameObject("Finish Trigger");
        finish.transform.position = new Vector3(65.8f, 1.8f, 0f);
        BoxCollider finishCollider = finish.AddComponent<BoxCollider>();
        finishCollider.isTrigger = true;
        // A thin trigger requires crossing the visible checker line, not merely entering the deck.
        finishCollider.size = new Vector3(0.5f, 3f, 6.8f);
        PrototypeFinishZone finishZone = finish.AddComponent<PrototypeFinishZone>();

        // A visible solid gate closes the approach until the fourth pickup is collected.
        GameObject gate = CreateBlock("Locked Finish Gate", new Vector3(62f, 2.5f, 0f),
            new Vector3(0.5f, 4f, 8f), barrierMaterial);
        gate.AddComponent<PrototypeCameraOccluder>();
        GameManager manager = Object.FindAnyObjectByType<GameManager>();
        SerializedObject managerSettings = new SerializedObject(manager);
        managerSettings.FindProperty("requireFinishZone").boolValue = true;
        managerSettings.FindProperty("finishGate").objectReferenceValue = gate;
        managerSettings.FindProperty("unlockClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(CollectSoundPath);
        managerSettings.ApplyModifiedPropertiesWithoutUndo();

        // Finish validation belongs to the manager so the trigger cannot bypass the pickup requirement.
        SerializedObject finishSettings = new SerializedObject(finishZone);
        finishSettings.FindProperty("gameManager").objectReferenceValue = manager;
        finishSettings.ApplyModifiedPropertiesWithoutUndo();
        ConfigureGameplayUI();
        ConfigureVictoryPresentation(manager);
    }

    // Upgrade only the jump section so checkpoint, maze, machinery and finish edits are preserved.
    [MenuItem("Tools/Upgrade Jumping Challenge")]
    public static void UpgradeJumpingChallenge()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            throw new System.InvalidOperationException("Stop Play mode before upgrading the jump section.");
        }
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != OutputScenePath)
        {
            scene = EditorSceneManager.OpenScene(OutputScenePath, OpenSceneMode.Single);
        }
        string[] replacedObjects = { "Jumping Challenge", "Jump Takeoff Platform", "Jump Platform 1",
            "Jump Platform 2", "Jump Platform 3", "Jump Platform 4", "Jump Landing Platform" };
        foreach (string name in replacedObjects)
        {
            GameObject existing = GameObject.Find(name);
            if (existing != null)
            {
                Object.DestroyImmediate(existing);
            }
        }
        // Reuse existing material assets without changing their appearance in other sections.
        Material floor = AssetDatabase.LoadAssetAtPath<Material>("Assets/Prototype Materials/Prototype Graybox.mat");
        Material hazard = AssetDatabase.LoadAssetAtPath<Material>("Assets/Prototype Materials/Prototype Hazard.mat");
        CreateJumpingChallenge(floor, hazard);
        // The item stays on the raised static platform, so players can always reach and collect it.
        GameObject.Find("Section Collectible 3").transform.position = new Vector3(31f, 1.45f, 1.6f);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Jumping challenge upgraded; other sections and checkpoints preserved.");
    }

    [MenuItem("Tools/Upgrade Jumping Challenge", true)]
    private static bool CanUpgradeJumping()
    {
        return !EditorApplication.isPlayingOrWillChangePlaymode;
    }

    private static void CreateJumpingChallenge(Material floor, Material hazard)
    {
        Transform root = new GameObject("Jumping Challenge").transform;
        CreateJumpBlock(root, "Jump Takeoff Platform", new Vector3(15f, 0f, 0f), new Vector3(6f, 1f, 8f), floor);
        CreateJumpBlock(root, "Jump Platform 1", new Vector3(21f, 0f, 1.8f), new Vector3(2.6f, 1f, 2.6f), floor);
        CreateMovingJumpBlock(root, "Jump Platform 2", new Vector3(26f, 0.2f, -1.4f), new Vector3(2.2f, 1f, 2.2f),
            1.1f, 4.8f, 0f, hazard);
        CreateJumpBlock(root, "Jump Platform 3", new Vector3(31f, 0.45f, 1.6f), new Vector3(2.2f, 1f, 2.2f), floor);
        CreateMovingJumpBlock(root, "Jump Platform 4", new Vector3(36f, 0.1f, -1.4f), new Vector3(2f, 1f, 2f),
            1.2f, 4f, 1.57f, hazard);
        CreateJumpBlock(root, "Jump Landing Platform", new Vector3(41f, 0f, 0f), new Vector3(6f, 1f, 8f), floor);
    }

    private static GameObject CreateJumpBlock(Transform root, string name, Vector3 position, Vector3 size, Material material)
    {
        GameObject block = CreateBlock(name, position, size, material);
        block.transform.SetParent(root, true);
        return block;
    }

    private static void CreateMovingJumpBlock(Transform root, string name, Vector3 position, Vector3 size,
        float distance, float duration, float phase, Material material)
    {
        GameObject platform = CreateJumpBlock(root, name, position, size, material);
        Rigidbody body = platform.AddComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        PrototypeMovingJumpPlatform movement = platform.AddComponent<PrototypeMovingJumpPlatform>();
        SerializedObject settings = new SerializedObject(movement);
        settings.FindProperty("travelDistance").floatValue = distance;
        settings.FindProperty("cycleDuration").floatValue = duration;
        settings.FindProperty("phaseOffset").floatValue = phase;
        settings.ApplyModifiedPropertiesWithoutUndo();
    }

    // Apply only this stage to an existing prototype, preserving edits made to the other sections.
    [MenuItem("Tools/Upgrade Machinery Challenge")]
    public static void UpgradeMachineryChallenge()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            throw new System.InvalidOperationException("Stop Play mode before upgrading the machinery stage.");
        }
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != OutputScenePath)
        {
            scene = EditorSceneManager.OpenScene(OutputScenePath, OpenSceneMode.Single);
        }
        string[] replacedObjects = { "Machinery Challenge", "Rotating Machinery Platform",
            "Machinery Detour Left", "Machinery Detour Right", "Rotating Machine 1",
            "Rotating Machine 2", "Rotating Machine 3" };
        foreach (string name in replacedObjects)
        {
            GameObject existing = GameObject.Find(name);
            if (existing != null)
            {
                Object.DestroyImmediate(existing);
            }
        }
        // Reuse the current material assets without changing colours shared by other course sections.
        Material floor = AssetDatabase.LoadAssetAtPath<Material>("Assets/Prototype Materials/Prototype Graybox.mat");
        Material hazard = AssetDatabase.LoadAssetAtPath<Material>("Assets/Prototype Materials/Prototype Hazard.mat");
        Material barrier = AssetDatabase.LoadAssetAtPath<Material>("Assets/Prototype Materials/Prototype Barrier.mat");
        CreateMachineryChallenge(floor, hazard);
        CreateMachineryRotors(barrier);
        // Keep the existing collectible component, sound and effects; move only the second-stage item.
        GameObject pickup = GameObject.Find("Section Collectible 2");
        pickup.transform.position = new Vector3(0f, 1f, 2.2f);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Machinery challenge upgraded; other course sections preserved.");
    }

    [MenuItem("Tools/Upgrade Machinery Challenge", true)]
    private static bool CanUpgradeMachinery()
    {
        return !EditorApplication.isPlayingOrWillChangePlaymode;
    }

    private static void CreateMachineryChallenge(Material floor, Material hazard)
    {
        Transform root = new GameObject("Machinery Challenge").transform;
        CreateMachineryBlock(root, "Machinery Entrance", new Vector3(-10.75f, 0f, 0f), new Vector3(2.5f, 1f, 3.5f), floor);
        for (int index = 0; index < 3; index++)
        {
            // Two-metre gaps leave only the offset bridges as a continuous rolling route.
            CreateMachineryBlock(root, "Machinery Bay " + (index + 1), new Vector3(-7f + index * 7f, 0f, 0f),
                new Vector3(5f, 1f, 6f), floor);
        }
        CreateMachineryBlock(root, "Machinery Bridge Left", new Vector3(-3.5f, 0f, 2.2f), new Vector3(2.2f, 1f, 1.3f), hazard);
        CreateMachineryBlock(root, "Machinery Bridge Right", new Vector3(3.5f, 0f, -2.2f), new Vector3(2.2f, 1f, 1.3f), hazard);
        CreateMachineryBlock(root, "Machinery Exit Apron", new Vector3(9.75f, 0f, 0f), new Vector3(0.5f, 1f, 3.5f), floor);
        CreateSlidingGate(root, "Machinery Sliding Gate 1", new Vector3(-3.5f, 1.9f, 2.2f), 3.4f, 0f, floor);
        CreateSlidingGate(root, "Machinery Sliding Gate 2", new Vector3(3.5f, 1.9f, -2.2f), 3f, 1.7f, floor);
    }

    private static GameObject CreateMachineryBlock(Transform root, string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject block = CreateBlock(name, position, scale, material);
        block.transform.SetParent(root, true);
        return block;
    }

    private static void CreateSlidingGate(Transform root, string name, Vector3 position, float duration, float phase, Material material)
    {
        GameObject gate = CreateMachineryBlock(root, name, position, new Vector3(0.35f, 2.8f, 0.55f), material);
        Rigidbody body = gate.AddComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        gate.AddComponent<PrototypeCameraOccluder>();
        PrototypeSlidingGate movement = gate.AddComponent<PrototypeSlidingGate>();
        SerializedObject settings = new SerializedObject(movement);
        settings.FindProperty("cycleDuration").floatValue = duration;
        settings.FindProperty("phaseOffset").floatValue = phase;
        settings.ApplyModifiedPropertiesWithoutUndo();
        // The visible overhead track makes the sliding direction readable before entering the bridge.
        GameObject track = CreateMachineryBlock(root, name + " Track", position + Vector3.up * 1.6f,
            new Vector3(0.4f, 0.25f, 5f), material);
        track.AddComponent<PrototypeCameraOccluder>();
    }

    private static void CreateMachineryRotors(Material material)
    {
        CreateRotatingMachine("Rotating Machine 1", new Vector3(-7f, 1.4f, 0f), -85f, material);
        CreateRotatingMachine("Rotating Machine 2", new Vector3(0f, 1.4f, 0f), 105f, material);
        CreateRotatingMachine("Rotating Machine 3", new Vector3(7f, 1.4f, 0f), -120f, material);
        for (int index = 0; index < 3; index++)
        {
            GameObject rotor = GameObject.Find("Rotating Machine " + (index + 1));
            rotor.transform.SetParent(GameObject.Find("Machinery Challenge").transform, true);
            // Stagger the initial angles as well as the direction and speed of each rotor.
            rotor.transform.rotation = Quaternion.Euler(0f, 15f + index * 40f, 0f);
        }
    }

    private static void CreateRotatingMachine(string objectName, Vector3 position, float rotationSpeed, Material material)
    {
        GameObject pivot = new GameObject(objectName);
        pivot.transform.position = position;
        Rigidbody barrierBody = pivot.AddComponent<Rigidbody>();
        barrierBody.isKinematic = true;
        barrierBody.useGravity = false;

        PrototypeRotatingBarrier rotatingBarrier = pivot.AddComponent<PrototypeRotatingBarrier>();
        SerializedObject barrierSettings = new SerializedObject(rotatingBarrier);
        barrierSettings.FindProperty("rotationSpeed").floatValue = rotationSpeed;
        barrierSettings.ApplyModifiedPropertiesWithoutUndo();

        for (int armIndex = 0; armIndex < 3; armIndex++)
        {
            GameObject arm = CreateBarrierArm(
                pivot.transform,
                objectName + " Arm " + (armIndex + 1),
                new Vector3(1.4f, 0f, 0f),
                new Vector3(3f, 1.8f, 0.4f),
                material);
            // Rotate each arm's offset as well as its orientation to form a centered three-arm rotor.
            Quaternion armRotation = Quaternion.Euler(0f, armIndex * 120f, 0f);
            arm.transform.localRotation = armRotation;
            arm.transform.localPosition = armRotation * new Vector3(1.4f, 0f, 0f);
        }
    }

    // Add fall recovery to the player; the original source scene remains untouched.
    private static void ConfigureFallRestart(GameObject player)
    {
        PrototypeFallRestart restart = player.GetComponent<PrototypeFallRestart>();
        if (restart == null)
        {
            restart = player.AddComponent<PrototypeFallRestart>();
        }
        SerializedObject settings = new SerializedObject(restart);
        // Detect the fall earlier and use a fixed brief pause instead of the entire audio duration.
        settings.FindProperty("fallHeight").floatValue = -1.5f;
        settings.FindProperty("restartDelay").floatValue = 0.2f;
        settings.FindProperty("fallClip").objectReferenceValue =
            AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Sound FX/SFX_Lose.ogg");
        settings.ApplyModifiedPropertiesWithoutUndo();
    }

    // Keep progress readable and put the basic controls where a first-time tester can see them.
    private static void ConfigureGameplayUI()
    {
        TextMeshProUGUI progress = GameObject.Find("Collectibles Remaining Text").GetComponent<TextMeshProUGUI>();
        progress.rectTransform.anchorMin = new Vector2(0f, 1f);
        progress.rectTransform.anchorMax = new Vector2(1f, 1f);
        progress.rectTransform.pivot = new Vector2(0.5f, 1f);
        progress.rectTransform.anchoredPosition = new Vector2(0f, -20f);
        progress.rectTransform.sizeDelta = new Vector2(-40f, 60f);
        progress.fontSize = 28f;
        progress.text = "Collectibles: 0/4  |  Finish locked";
        progress.raycastTarget = false;

        TextMeshProUGUI controls = Object.Instantiate(progress, progress.transform.parent);
        controls.name = "Prototype Controls";
        controls.rectTransform.anchorMin = Vector2.zero;
        controls.rectTransform.anchorMax = Vector2.right;
        controls.rectTransform.pivot = new Vector2(0.5f, 0f);
        controls.rectTransform.anchoredPosition = new Vector2(0f, 18f);
        controls.rectTransform.sizeDelta = new Vector2(-40f, 78f);
        controls.fontSize = 20f;
        controls.text = "WASD / Arrow Keys: Roll   |   Space: Jump\nCollect one item in each section, then reach the finish. Slow down on the rocking balance beams.";
    }

    private static void ConfigurePlayerJump(GameObject player)
    {
        PrototypePlayerJumpController jumpController = player.GetComponent<PrototypePlayerJumpController>();
        if (jumpController == null)
        {
            jumpController = player.AddComponent<PrototypePlayerJumpController>();
        }

        // Connects the existing Space-bound Jump action to the new jump behavior.
        PlayerInput playerInput = player.GetComponent<PlayerInput>();
        SerializedObject inputSettings = new SerializedObject(playerInput);
        SerializedProperty actionEvents = inputSettings.FindProperty("m_ActionEvents");

        for (int eventIndex = 0; eventIndex < actionEvents.arraySize; eventIndex++)
        {
            SerializedProperty actionEvent = actionEvents.GetArrayElementAtIndex(eventIndex);
            string actionName = actionEvent.FindPropertyRelative("m_ActionName").stringValue;
            if (!actionName.StartsWith("Player/Jump"))
            {
                continue;
            }

            SerializedProperty calls = actionEvent.FindPropertyRelative("m_PersistentCalls.m_Calls");
            calls.arraySize = 1;
            SerializedProperty jumpCall = calls.GetArrayElementAtIndex(0);
            // Unity Events need the target component and callback name saved on the PlayerInput.
            jumpCall.FindPropertyRelative("m_Target").objectReferenceValue = jumpController;
            jumpCall.FindPropertyRelative("m_TargetAssemblyTypeName").stringValue = "PrototypePlayerJumpController, Assembly-CSharp";
            jumpCall.FindPropertyRelative("m_MethodName").stringValue = "OnJumpInput";
            jumpCall.FindPropertyRelative("m_Mode").enumValueIndex = 0;
            jumpCall.FindPropertyRelative("m_CallState").intValue = 2;
            inputSettings.ApplyModifiedPropertiesWithoutUndo();
            return;
        }

        Debug.LogWarning("The Player/Jump action was not found, so Space will not trigger jumping.");
    }

    private static void CreateFinishMarker(Material material)
    {
        Material cyan = CreateMaterial("Prototype Finish Cyan", new Color(0.1f, 0.85f, 0.95f));
        Material white = CreateMaterial("Prototype Finish White", new Color(0.92f, 0.96f, 1f));
        Material ink = CreateMaterial("Prototype Finish Ink", new Color(0.025f, 0.035f, 0.05f));
        // All floor graphics are visual-only and sit just above the same flat collision surface.
        for (int row = 0; row < 2; row++)
        {
            for (int square = 0; square < 8; square++)
            {
                CreateFloorGraphic("Finish Checker " + row + "-" + square,
                    new Vector3(65.15f + row * 0.6f, 0.503f, -2.8f + square * 0.8f),
                    new Vector3(0.6f, 0.006f, 0.8f), (row + square) % 2 == 0 ? white : ink);
            }
        }
        CreateFloorGraphic("Finish Left Border", new Vector3(66f, 0.503f, -4.2f), new Vector3(9.5f, 0.006f, 0.12f), cyan);
        CreateFloorGraphic("Finish Right Border", new Vector3(66f, 0.503f, 4.2f), new Vector3(9.5f, 0.006f, 0.12f), cyan);
        // Keep the arch outside the walking area and beyond the finish stripe.
        CreateBlock("Finish Arch Left", new Vector3(68.5f, 2.5f, -3.7f), new Vector3(0.4f, 4f, 0.4f), material);
        CreateBlock("Finish Arch Right", new Vector3(68.5f, 2.5f, 3.7f), new Vector3(0.4f, 4f, 0.4f), material);
        CreateBlock("Finish Arch Header", new Vector3(68.5f, 4.5f, 0f), new Vector3(0.5f, 0.65f, 7.8f), material);
        CreateBlock("Finish Arch Accent Left", new Vector3(68.25f, 2.5f, -3.7f), new Vector3(0.05f, 3.6f, 0.12f), cyan);
        CreateBlock("Finish Arch Accent Right", new Vector3(68.25f, 2.5f, 3.7f), new Vector3(0.05f, 3.6f, 0.12f), cyan);
        GameObject sign = new GameObject("Finish Sign");
        sign.transform.SetPositionAndRotation(new Vector3(68.22f, 4.5f, 0f), Quaternion.Euler(0f, 90f, 0f));
        TextMeshPro label = sign.AddComponent<TextMeshPro>();
        label.text = "F I N I S H";
        label.fontSize = 4.2f;
        label.alignment = TextAlignmentOptions.Center;
        label.color = cyan.color;
        label.rectTransform.sizeDelta = new Vector2(7f, 0.7f);
    }

    private static void CreateFloorGraphic(string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject graphic = CreateBlock(name, position, scale, material);
        // Remove the cube collider so checker squares cannot create bumps or catch the rolling ball.
        Object.DestroyImmediate(graphic.GetComponent<Collider>());
        graphic.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
    }

    private static void ConfigureVictoryPresentation(GameManager manager)
    {
        Transform canvas = GameObject.Find("Canvas").transform;
        TextMeshProUGUI template = GameObject.Find("Collectibles Remaining Text").GetComponent<TextMeshProUGUI>();
        RectTransform hud = CreateUIRect("Gameplay HUD", canvas, Vector2.zero, Vector2.zero, Vector2.zero);
        StretchToParent(hud);
        template.transform.SetParent(hud, false);
        GameObject.Find("Prototype Controls").transform.SetParent(hud, false);

        RectTransform overlay = CreateUIRect("Victory Overlay", canvas, Vector2.zero, Vector2.zero, Vector2.zero);
        StretchToParent(overlay);
        CanvasGroup group = overlay.gameObject.AddComponent<CanvasGroup>();
        group.blocksRaycasts = false;
        group.interactable = false;
        Image dimmer = overlay.gameObject.AddComponent<Image>();
        dimmer.color = new Color(0.01f, 0.025f, 0.04f, 0.18f);
        dimmer.raycastTarget = false;
        RectTransform card = CreateUIRect("Victory Card", overlay, new Vector2(0.5f, 0.77f), new Vector2(660f, 240f), Vector2.zero);
        Image panel = card.gameObject.AddComponent<Image>();
        panel.color = new Color(0.025f, 0.055f, 0.075f, 0.96f);
        panel.raycastTarget = false;
        Outline border = card.gameObject.AddComponent<Outline>();
        border.effectColor = new Color(0.1f, 0.85f, 0.95f, 0.55f);
        border.effectDistance = new Vector2(1f, -1f);
        CreateVictoryLabel(template, card, "Victory Heading", "COURSE COMPLETE", 18f,
            new Color(0.2f, 0.85f, 0.95f), new Vector2(0f, 80f), new Vector2(620f, 28f));
        CreateVictoryLabel(template, card, "Victory Title", "YOU WIN!", 64f,
            Color.white, new Vector2(0f, 24f), new Vector2(620f, 85f));
        CreateVictoryLabel(template, card, "Victory Details", "4 / 4 COLLECTIBLES  |  ALL SECTIONS CLEARED", 18f,
            new Color(0.95f, 0.78f, 0.3f), new Vector2(0f, -36f), new Vector2(620f, 32f));
        TextMeshProUGUI countdown = CreateVictoryLabel(template, card, "Victory Countdown", "Next run in 6 seconds", 16f,
            new Color(0.65f, 0.74f, 0.8f), new Vector2(0f, -86f), new Vector2(620f, 28f));
        overlay.gameObject.SetActive(false);

        GameObject anchor = new GameObject("Victory Celebration Point");
        anchor.transform.position = new Vector3(67f, 1.02f, 0f);
        ParticleSystem left = CreateFinishConfetti("Finish Confetti Left", new Vector3(67.5f, 1f, -3.2f));
        ParticleSystem right = CreateFinishConfetti("Finish Confetti Right", new Vector3(67.5f, 1f, 3.2f));
        PrototypeVictoryPresentation presentation = manager.gameObject.AddComponent<PrototypeVictoryPresentation>();
        SerializedObject settings = new SerializedObject(presentation);
        settings.FindProperty("playerTransform").objectReferenceValue = GameObject.Find("Player").transform;
        settings.FindProperty("celebrationCamera").objectReferenceValue = Camera.main;
        settings.FindProperty("celebrationPoint").objectReferenceValue = anchor.transform;
        settings.FindProperty("gameplayHUD").objectReferenceValue = hud.gameObject;
        settings.FindProperty("victoryOverlay").objectReferenceValue = group;
        settings.FindProperty("victoryCard").objectReferenceValue = card;
        settings.FindProperty("restartText").objectReferenceValue = countdown;
        SerializedProperty emitters = settings.FindProperty("confetti");
        emitters.arraySize = 2;
        emitters.GetArrayElementAtIndex(0).objectReferenceValue = left;
        emitters.GetArrayElementAtIndex(1).objectReferenceValue = right;
        settings.ApplyModifiedPropertiesWithoutUndo();
        SerializedObject managerSettings = new SerializedObject(manager);
        managerSettings.FindProperty("victoryPresentation").objectReferenceValue = presentation;
        managerSettings.ApplyModifiedPropertiesWithoutUndo();
    }

    private static RectTransform CreateUIRect(string name, Transform parent, Vector2 anchor, Vector2 size, Vector2 position)
    {
        GameObject item = new GameObject(name, typeof(RectTransform));
        RectTransform rect = item.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        return rect;
    }

    private static void StretchToParent(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    private static TextMeshProUGUI CreateVictoryLabel(TextMeshProUGUI template, Transform parent,
        string name, string text, float fontSize, Color color, Vector2 position, Vector2 size)
    {
        // Reuse the existing TMP font asset while assigning independent text and layout.
        TextMeshProUGUI label = Object.Instantiate(template, parent);
        label.name = name;
        label.rectTransform.anchorMin = label.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        label.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        label.rectTransform.anchoredPosition = position;
        label.rectTransform.sizeDelta = size;
        label.text = text;
        label.fontSize = fontSize;
        label.color = color;
        label.alignment = TextAlignmentOptions.Center;
        return label;
    }

    private static ParticleSystem CreateFinishConfetti(string name, Vector3 position)
    {
        GameObject item = new GameObject(name);
        item.transform.SetPositionAndRotation(position, Quaternion.Euler(-90f, 0f, 0f));
        ParticleSystem system = item.AddComponent<ParticleSystem>();
        system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = system.main;
        main.playOnAwake = false;
        main.loop = false;
        main.duration = 2.4f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(2.2f, 3.5f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(5f, 7f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.09f, 0.16f);
        main.gravityModifier = 0.6f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 300;
        Gradient palette = new Gradient();
        palette.SetKeys(new[] { new GradientColorKey(new Color(0.1f, 0.85f, 1f), 0f),
            new GradientColorKey(new Color(1f, 0.75f, 0.2f), 0.5f),
            new GradientColorKey(new Color(0.95f, 0.3f, 0.7f), 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
        ParticleSystem.MinMaxGradient colors = new ParticleSystem.MinMaxGradient(palette);
        colors.mode = ParticleSystemGradientMode.RandomColor;
        main.startColor = colors;
        var emission = system.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 90), new ParticleSystem.Burst(1.1f, 55) });
        var shape = system.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 25f;
        shape.radius = 0.15f;
        var rotation = system.rotationOverLifetime;
        rotation.enabled = true;
        rotation.z = new ParticleSystem.MinMaxCurve(-4f, 4f);
        // A particle shader respects the random vertex colors and renders small square confetti.
        string path = "Assets/Prototype Materials/Prototype Celebration Particle.mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            AssetDatabase.CreateAsset(material, path);
        }
        material.color = Color.white;
        EditorUtility.SetDirty(material);
        ParticleSystemRenderer renderer = system.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        return system;
    }

    private static GameObject CreateBarrierArm(Transform parent, string objectName, Vector3 localPosition, Vector3 scale, Material material)
    {
        GameObject arm = GameObject.CreatePrimitive(PrimitiveType.Cube);
        arm.name = objectName;
        arm.transform.SetParent(parent, false);
        arm.transform.localPosition = localPosition;
        arm.transform.localScale = scale;
        arm.GetComponent<Renderer>().sharedMaterial = material;
        arm.AddComponent<PrototypeCameraOccluder>();
        return arm;
    }

    private static GameObject CreateBlock(string objectName, Vector3 position, Vector3 scale, Material material, Quaternion? rotation = null)
    {
        GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
        block.name = objectName;
        block.transform.SetPositionAndRotation(position, rotation ?? Quaternion.identity);
        block.transform.localScale = scale;
        block.GetComponent<Renderer>().sharedMaterial = material;
        return block;
    }

    private static void CreateEdgeRail(string objectName, Vector3 position, Vector3 scale, Material material)
    {
        CreateBlock(objectName, position, scale, material);
    }

    private static Material CreateMaterial(string materialName, Color color)
    {
        string folder = "Assets/Prototype Materials";
        if (!AssetDatabase.IsValidFolder(folder))
        {
            AssetDatabase.CreateFolder("Assets", "Prototype Materials");
        }

        string path = folder + "/" + materialName + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }

        material.color = color;
        EditorUtility.SetDirty(material);
        return material;
    }

    private static void EnsureTag(string tagName)
    {
        if (!System.Array.Exists(InternalEditorUtility.tags, tag => tag == tagName))
        {
            InternalEditorUtility.AddTag(tagName);
        }
    }

    private static void AddSceneToBuildSettings()
    {
        // Put the prototype first so builds launch the gameplay test rather than SampleScene.
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        scenes.RemoveAll(scene => scene.path == OutputScenePath);
        scenes.Insert(0, new EditorBuildSettingsScene(OutputScenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
