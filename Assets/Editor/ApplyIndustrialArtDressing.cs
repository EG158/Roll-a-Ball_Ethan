/*****************************************************************
* COMPONENT OF: Unity Editor Tools menu
* REQUIRED DEPENDENCIES: Unity Editor and the Prototype Gameplay Test scene
* DESCRIPTION: Applies a layered industrial science-fiction art pass to the existing prototype course without rebuilding its gameplay layout.
* VERSION 3.8: Connects the factory trusses into the building and improves warm lighting and platform visibility.
*****************************************************************/

using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

[InitializeOnLoad]
public static class ApplyIndustrialArtDressing
{
    private const string ScenePath = "Assets/Scenes/Prototype Gameplay Test.unity";
    private const string MaterialsFolder = "Assets/Prototype Materials";
    private const string DressingRootName = "Industrial Art Dressing (Visual Only) v3.8";
    private const string PreviousDressingRootV37Name = "Industrial Art Dressing (Visual Only) v3.7";
    private const string PreviousDressingRootV36Name = "Industrial Art Dressing (Visual Only) v3.6";
    private const string PreviousDressingRootV35Name = "Industrial Art Dressing (Visual Only) v3.5";
    private const string PreviousDressingRootV34Name = "Industrial Art Dressing (Visual Only) v3.4";
    private const string PreviousDressingRootV33Name = "Industrial Art Dressing (Visual Only) v3.3";
    private const string PreviousDressingRootV32Name = "Industrial Art Dressing (Visual Only) v3.2";
    private const string PreviousDressingRootV31Name = "Industrial Art Dressing (Visual Only) v3.1";
    private const string PreviousDressingRootV30Name = "Industrial Art Dressing (Visual Only) v3.0";
    private const string PreviousDressingRootV29Name = "Industrial Art Dressing (Visual Only) v2.9";
    private const string PreviousDressingRootV28Name = "Industrial Art Dressing (Visual Only) v2.8";
    private const string PreviousDressingRootV27Name = "Industrial Art Dressing (Visual Only) v2.7";
    private const string PreviousDressingRootV26Name = "Industrial Art Dressing (Visual Only) v2.6";
    private const string PreviousDressingRootV25Name = "Industrial Art Dressing (Visual Only) v2.5";
    private const string PreviousDressingRootV24Name = "Industrial Art Dressing (Visual Only) v2.4";
    private const string PreviousDressingRootV23Name = "Industrial Art Dressing (Visual Only) v2.3";
    private const string PreviousDressingRootV22Name = "Industrial Art Dressing (Visual Only) v2.2";
    private const string PreviousDressingRootV21Name = "Industrial Art Dressing (Visual Only) v2.1";
    private const string PreviousDressingRootV2Name = "Industrial Art Dressing (Visual Only) v2.0";
    private const string PreviousDressingRootName = "Industrial Art Dressing (Visual Only) v1.1";
    private const string OriginalDressingRootName = "Industrial Art Dressing (Visual Only)";
    private const string DeckTexturePath = MaterialsFolder + "/Industrial Deck Plate Detailed.png";
    private const string HazardTexturePath = MaterialsFolder + "/Industrial Hazard Stripes.png";
    private const string MazeExitSeamMeshPath = MaterialsFolder + "/Maze Exit Path Seam Mesh.asset";
    private const string IndustrialRingMeshPath = MaterialsFolder + "/Industrial Detail Ring Mesh.asset";
    private const string BandedPlayerMeshPath = MaterialsFolder + "/Industrial Banded Player Ball Mesh v3.3.asset";

    // Apply once after this tool is imported when the target scene is already open in Edit mode.
    static ApplyIndustrialArtDressing()
    {
        EditorApplication.playModeStateChanged += ApplyAfterReturningToEditMode;
    }

    [InitializeOnLoadMethod]
    private static void ScheduleApplicationWhenEditorIsReady()
    {
        EditorApplication.update += ApplyWhenEditorIsReady;
    }

    private static void ApplyWhenEditorIsReady()
    {
        if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode ||
            EditorSceneManager.GetActiveScene().path != ScenePath)
        {
            return;
        }

        EditorApplication.update -= ApplyWhenEditorIsReady;
        ApplyOnImportWhenSceneIsReady();
    }

    private static void ApplyAfterReturningToEditMode(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            EditorApplication.delayCall += ApplyOnImportWhenSceneIsReady;
        }
    }

    private static void ApplyOnImportWhenSceneIsReady()
    {
        if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode ||
            EditorSceneManager.GetActiveScene().path != ScenePath ||
            GameObject.Find(DressingRootName) != null)
        {
            return;
        }

        Apply();
    }

    // This menu command changes renderer materials and adds decorative objects only.
    [MenuItem("Tools/Apply Industrial Art Dressing")]
    public static void Apply()
    {
        if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogWarning("Stop Play mode before applying the industrial art dressing.");
            return;
        }

        if (EditorSceneManager.GetActiveScene().path != ScenePath)
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        CreateIndustrialTextures();
        Material steel = CreateTexturedMaterial("Industrial Steel", new Color(0.95f, 0.91f, 0.82f), 0.42f, 0.28f, DeckTexturePath);
        Material darkSteel = CreateMaterial("Industrial Dark Steel", new Color(0.2f, 0.25f, 0.28f), 0.55f, 0.26f);
        Material amber = CreateTexturedMaterial("Industrial Amber", new Color(1f, 0.48f, 0.08f), 0.32f, 0.24f, HazardTexturePath);
        Material trim = CreateMaterial("Industrial Edge Trim", new Color(0.27f, 0.31f, 0.31f), 0.58f, 0.3f);
        Material hazard = CreateTexturedMaterial("Industrial Hazard Stripe", Color.white, 0.38f, 0.3f, HazardTexturePath);
        Material pit = CreateMaterial("Industrial Pit", new Color(0.035f, 0.055f, 0.075f), 0.36f, 0.22f);
        Material lamp = CreateEmissiveMaterial("Industrial Amber Lamp", new Color(1f, 0.66f, 0.27f), 4.0f);
        Material start = CreateTexturedMaterial("Industrial Start Steel", new Color(0.8f, 0.82f, 0.8f), 0.58f, 0.38f, DeckTexturePath);
        Material finish = CreateTexturedMaterial("Industrial Finish Steel", new Color(0.75f, 0.79f, 0.8f), 0.72f, 0.4f, DeckTexturePath);
        Material playerShell = CreateMaterial("Industrial Player Shell", new Color(0.42f, 0.5f, 0.53f), 0.82f, 0.76f);
        Material amberDetail = CreateEmissiveMaterial("Industrial Amber Detail", new Color(1f, 0.34f, 0.06f), 1.6f);
        Material collectibleCore = CreateEmissiveMaterial("Industrial Collectible Core", new Color(1f, 0.45f, 0.08f), 2.2f);
        Mesh detailRing = CreateIndustrialRingMesh();
        Mesh bandedPlayerMesh = CreateBandedPlayerMesh();

        RemoveMazeExitPathVisualOverlap();
        List<Renderer> deckRenderers = StyleExistingRenderers(steel, darkSteel, amber, start, finish, pit);
        CreatePlayerAndCollectibleVisuals(playerShell, collectibleCore, darkSteel, trim, amberDetail,
            detailRing, bandedPlayerMesh);
        GameObject artRoot = CreateFactoryScenery(steel, darkSteel, amber, lamp, trim, hazard);
        CreateDeckDetails(artRoot.transform, deckRenderers, trim, amber, hazard, darkSteel, lamp);
        CreateDepthTrusses(artRoot.transform, darkSteel, trim, lamp);
        CreateBalanceCourseDetails(darkSteel, amber, trim);
        CreateMazeWallDetails(steel, darkSteel, amber, lamp);
        CreateIndustrialPit(artRoot.transform, trim, amber);
        SetIndustrialLighting();
        SetIndustrialCameraBackground();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        Debug.Log("Industrial art pass 3.8 applied. Factory trusses and lighting were refined; course layout, colliders, and gameplay scripts were left unchanged.");
    }

    // Keep this operation unavailable while Unity is playing so visual changes are saved to the scene asset.
    [MenuItem("Tools/Apply Industrial Art Dressing", true)]
    private static bool CanApply()
    {
        return !EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode;
    }

    // Reassign materials only on existing renderers; no transforms or gameplay components are edited.
    private static List<Renderer> StyleExistingRenderers(Material steel, Material darkSteel, Material amber, Material start, Material finish, Material pit)
    {
        List<Renderer> deckRenderers = new List<Renderer>();
        foreach (Renderer renderer in Object.FindObjectsByType<Renderer>())
        {
            // Ignore decoration from every art-pass version so replaced objects are never reused as course surfaces.
            if (renderer.transform.root.name.StartsWith("Industrial Art Dressing (Visual Only)"))
            {
                continue;
            }

            string objectName = renderer.gameObject.name;
            string lowerName = objectName.ToLowerInvariant();

            if (lowerName.Contains("fall catch ground"))
            {
                // Darken the existing safety floor so it reads as the depth below the factory course.
                renderer.sharedMaterial = pit;
            }
            else if (objectName == "Start Platform")
            {
                renderer.sharedMaterial = start;
                deckRenderers.Add(renderer);
            }
            else if (objectName == "Finish Platform")
            {
                renderer.sharedMaterial = finish;
                deckRenderers.Add(renderer);
            }
            else if (lowerName.Contains("finish arch"))
            {
                // Use dark metal for the existing finish frame so its amber accents remain the focal point.
                renderer.sharedMaterial = darkSteel;
            }
            else if (lowerName.Contains("finish left border") || lowerName.Contains("finish right border"))
            {
                renderer.sharedMaterial = amber;
            }
            else if (lowerName.Contains("rotating machine") && lowerName.Contains("arm"))
            {
                renderer.sharedMaterial = amber;
            }
            else if (lowerName.Contains("sliding gate") || lowerName.Contains("balance beam"))
            {
                renderer.sharedMaterial = darkSteel;
            }
            else if (IsCourseSurface(lowerName))
            {
                renderer.sharedMaterial = steel;
                if (!lowerName.Contains("maze east wall") && !lowerName.Contains("maze north wall") &&
                    !lowerName.Contains("maze south border"))
                {
                    deckRenderers.Add(renderer);
                }
            }
            else if (lowerName.Contains("maze east wall") || lowerName.Contains("maze north wall") || lowerName.Contains("maze south border"))
            {
                renderer.sharedMaterial = darkSteel;
            }
        }

        foreach (Renderer deckRenderer in deckRenderers)
        {
            SetDeckTextureTiling(deckRenderer);
        }

        TextMeshPro finishLabel = GameObject.Find("Finish Sign")?.GetComponent<TextMeshPro>();
        if (finishLabel != null)
        {
            finishLabel.color = amber.color;
        }

        return deckRenderers;
    }

    // Restrict surface recoloring to the existing walkable deck objects and preserve their geometry.
    private static bool IsCourseSurface(string lowerName)
    {
        return lowerName.Contains("platform") || lowerName.Contains("exit path") ||
               lowerName.Contains("machinery bay") || lowerName.Contains("machinery bridge") ||
               lowerName.Contains("machinery entrance") || lowerName.Contains("machinery exit apron") ||
               lowerName.Contains("jump takeoff") || lowerName.Contains("jump landing");
    }

    // Trim only the hidden overlap from the maze exit's render mesh; its BoxCollider and route remain unchanged.
    private static void RemoveMazeExitPathVisualOverlap()
    {
        GameObject mazePlatform = GameObject.Find("Maze Platform");
        GameObject mazeExitPath = GameObject.Find("Maze Exit Path");
        if (mazePlatform == null || mazeExitPath == null)
        {
            return;
        }

        MeshFilter exitMeshFilter = mazeExitPath.GetComponent<MeshFilter>();
        MeshFilter platformMeshFilter = mazePlatform.GetComponent<MeshFilter>();
        if (exitMeshFilter == null || platformMeshFilter == null)
        {
            return;
        }

        // Reuse the saved clipped mesh so rerunning the art pass never shrinks the platform a second time.
        Mesh clippedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(MazeExitSeamMeshPath);
        if (clippedMesh == null)
        {
            float mazePlatformRightEdge = mazePlatform.transform.position.x +
                mazePlatform.transform.lossyScale.x * 0.5f;
            float mazeExitLeftEdge = mazeExitPath.transform.position.x -
                mazeExitPath.transform.lossyScale.x * 0.5f;
            float overlapWidth = mazePlatformRightEdge - mazeExitLeftEdge;
            if (overlapWidth <= 0.01f)
            {
                return;
            }

            // The two decks share the same height and overlap by about one world unit in the original layout.
            clippedMesh = Object.Instantiate(exitMeshFilter.sharedMesh);
            clippedMesh.name = "Maze Exit Path Seam Mesh";
            Vector3 localCutPoint = mazeExitPath.transform.InverseTransformPoint(
                new Vector3(mazePlatformRightEdge, mazeExitPath.transform.position.y, mazeExitPath.transform.position.z));
            Vector3[] vertices = clippedMesh.vertices;
            for (int index = 0; index < vertices.Length; index++)
            {
                vertices[index].x = Mathf.Max(vertices[index].x, localCutPoint.x);
            }

            clippedMesh.vertices = vertices;
            clippedMesh.RecalculateBounds();
            clippedMesh.RecalculateNormals();
            AssetDatabase.CreateAsset(clippedMesh, MazeExitSeamMeshPath);
        }

        // Changing the MeshFilter affects only what the camera draws; the original collider is untouched.
        exitMeshFilter.sharedMesh = clippedMesh;
        EditorUtility.SetDirty(exitMeshFilter);
    }

    // Build a separate, collider-free factory environment and replace earlier art passes cleanly.
    private static GameObject CreateFactoryScenery(Material steel, Material darkSteel, Material amber, Material lamp,
        Material trim, Material hazard)
    {
        string[] previousRootNames = { DressingRootName, PreviousDressingRootV37Name, PreviousDressingRootV36Name, PreviousDressingRootV35Name, PreviousDressingRootV34Name, PreviousDressingRootV33Name, PreviousDressingRootV32Name, PreviousDressingRootV31Name, PreviousDressingRootV30Name, PreviousDressingRootV29Name, PreviousDressingRootV28Name, PreviousDressingRootV27Name, PreviousDressingRootV26Name, PreviousDressingRootV25Name, PreviousDressingRootV24Name, PreviousDressingRootV23Name, PreviousDressingRootV22Name, PreviousDressingRootV21Name, PreviousDressingRootV2Name, PreviousDressingRootName, OriginalDressingRootName };
        foreach (string previousRootName in previousRootNames)
        {
            GameObject previous = GameObject.Find(previousRootName);
            if (previous != null)
            {
                Object.DestroyImmediate(previous);
            }
        }

        GameObject root = new GameObject(DressingRootName);
        root.isStatic = true;

        // Large portal frames turn the view ahead into a factory corridor instead of open sky.
        for (int x = -40; x <= 72; x += 8)
        {
            CreateVisualBeam(root.transform, "Factory Portal West " + x,
                new Vector3(x, 5.5f, -18f), new Vector3(0.62f, 13f, 0.62f), darkSteel);
            CreateVisualBeam(root.transform, "Factory Portal East " + x,
                new Vector3(x, 5.5f, 18f), new Vector3(0.62f, 13f, 0.62f), darkSteel);

            // Broad overhead beams and diagonal braces make each portal read as a load-bearing frame.
            CreateVisualBeam(root.transform, "Factory Portal Crossbeam " + x,
                new Vector3(x, 11f, 0f), new Vector3(0.72f, 0.72f, 37f), steel);
            CreateBeamBetween(root.transform, "Factory Portal Brace West " + x,
                new Vector3(x, 2.5f, -18f), new Vector3(x + 5f, 9.5f, -18f), 0.28f, darkSteel);
            CreateBeamBetween(root.transform, "Factory Portal Brace East " + x,
                new Vector3(x, 2.5f, 18f), new Vector3(x + 5f, 9.5f, 18f), 0.28f, darkSteel);
        }

        // Panelled side walls and ribs enclose the course visually without narrowing its playable route.
        for (int x = -44; x <= 80; x += 8)
        {
            CreateVisualBeam(root.transform, "Factory Wall Panel West " + x,
                new Vector3(x, 3.5f, -22f), new Vector3(7.8f, 15f, 0.5f), darkSteel);
            CreateVisualBeam(root.transform, "Factory Wall Panel East " + x,
                new Vector3(x, 3.5f, 22f), new Vector3(7.8f, 15f, 0.5f), darkSteel);
            CreateVisualBeam(root.transform, "Factory Wall Rib West " + x,
                new Vector3(x, 3.5f, -21.65f), new Vector3(0.32f, 15f, 0.22f), steel);
            CreateVisualBeam(root.transform, "Factory Wall Rib East " + x,
                new Vector3(x, 3.5f, 21.65f), new Vector3(0.32f, 15f, 0.22f), steel);
        }

        CreateFactoryInterior(root.transform, steel, darkSteel, amber, lamp, trim);
        CreateFactoryMechanicalBays(root.transform, steel, darkSteel, amber, lamp, trim);

        // Long utility pipes, thinner amber conduits, and a side catwalk add overlapping layers of depth.
        for (int side = -1; side <= 1; side += 2)
        {
            float z = side * 14.5f;
            CreateVisualBeam(root.transform, "Main Utility Pipe " + side,
                new Vector3(16f, 10.8f, z), new Vector3(120f, 0.7f, 0.7f), darkSteel);
            CreateVisualBeam(root.transform, "Amber Utility Conduit " + side,
                new Vector3(16f, 10.35f, z - side * 0.5f), new Vector3(120f, 0.12f, 0.12f), amber);
            CreateVisualBeam(root.transform, "Service Catwalk " + side,
                new Vector3(16f, -0.2f, side * 15.5f), new Vector3(120f, 0.22f, 1.6f), trim);
        }

        // Amber lamps repeat along the walls and portals to establish warm points in the cool metal space.
        for (int x = -36; x <= 68; x += 16)
        {
            CreateVisualBeam(root.transform, "Vertical Amber Wall Light West " + x,
                new Vector3(x, 4f, -13.4f), new Vector3(0.18f, 2.8f, 0.14f), lamp);
            CreateVisualBeam(root.transform, "Vertical Amber Wall Light East " + x,
                new Vector3(x, 4f, 13.4f), new Vector3(0.18f, 2.8f, 0.14f), lamp);
            CreatePointLight(root.transform, "Factory Glow West " + x, new Vector3(x, 5f, -9f));
            CreatePointLight(root.transform, "Factory Glow East " + x, new Vector3(x, 5f, 9f));
        }

        return root;
    }

    // Add large, readable machinery and maintenance structures in the factory shell beyond the playable route.
    private static void CreateFactoryMechanicalBays(Transform root, Material steel, Material darkSteel,
        Material amber, Material lamp, Material trim)
    {
        // Tall columns and side gantries give the room strong scale and a layered industrial silhouette.
        for (int x = -36; x <= 72; x += 12)
        {
            foreach (int side in new[] { -1, 1 })
            {
                float wallSide = side;
                float wallZ = wallSide * 20.8f;

                CreateVisualBeam(root, "Factory Heavy Support Column " + x + " " + side,
                    new Vector3(x, 3.2f, wallSide * 20.1f), new Vector3(0.82f, 17.4f, 0.82f), trim);
                CreateVisualBeam(root, "Factory Column Amber Stripe " + x + " " + side,
                    new Vector3(x, 3.3f, wallSide * 19.62f), new Vector3(0.14f, 14f, 0.08f), amber);

                // Each square fan housing sits on the far wall so it frames the corridor without narrowing it.
                CreateVisualBeam(root, "Factory Ventilation Fan Housing " + x + " " + side,
                    new Vector3(x, 6.3f, wallZ), new Vector3(3.8f, 3.8f, 0.56f), darkSteel);
                CreateVisualBeam(root, "Factory Fan Inner Plate " + x + " " + side,
                    new Vector3(x, 6.3f, wallSide * 20.48f), new Vector3(3.1f, 3.1f, 0.12f), steel);

                // The axial cylinder and three blades make the fan readable from the rear-follow camera.
                CreateVisualCylinder(root, "Factory Ventilation Fan Hub " + x + " " + side,
                    new Vector3(x, 6.3f, wallSide * 20.34f), new Vector3(0.62f, 0.62f, 0.48f), amber)
                    .transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                for (int blade = 0; blade < 3; blade++)
                {
                    float angle = blade * 120f;
                    float radians = angle * Mathf.Deg2Rad;
                    Vector3 bladePosition = new Vector3(x + Mathf.Cos(radians) * 0.9f,
                        6.3f + Mathf.Sin(radians) * 0.9f, wallSide * 20.3f);
                    GameObject fanBlade = CreateVisualBeam(root,
                        "Factory Ventilation Fan Blade " + x + " " + side + " " + blade,
                        bladePosition, new Vector3(1.4f, 0.24f, 0.12f), trim);
                    fanBlade.transform.rotation = Quaternion.Euler(0f, 0f, angle);
                }

                // Short platforms and diagonal braces represent service access around the wall equipment.
                CreateVisualBeam(root, "Factory Service Gantry " + x + " " + side,
                    new Vector3(x, 8.2f, wallSide * 16.7f), new Vector3(7.6f, 0.28f, 1.5f), trim);
                CreateVisualBeam(root, "Factory Gantry Safety Rail " + x + " " + side,
                    new Vector3(x, 8.85f, wallSide * 16.05f), new Vector3(7.6f, 1.05f, 0.12f), darkSteel);
                CreateBeamBetween(root, "Factory Gantry Support " + x + " " + side,
                    new Vector3(x - 2.7f, 6.4f, wallSide * 17.2f),
                    new Vector3(x - 2.7f, 8f, wallSide * 16.6f), 0.2f, steel);

                // Paired lamps mark the work area and add warm highlights against the cool steel shell.
                CreateVisualBeam(root, "Factory Gantry Work Lamp " + x + " " + side,
                    new Vector3(x, 9f, wallSide * 15.7f), new Vector3(1.15f, 0.12f, 0.2f), lamp);
                if (x % 24 == 0)
                {
                    CreatePointLight(root, "Factory Gantry Glow " + x + " " + side,
                        new Vector3(x, 8.5f, wallSide * 14.5f));
                }
            }
        }

        // Roof trusses repeat above the player's head to connect the side frames into one large hall.
        for (int x = -32; x <= 64; x += 16)
        {
            CreateBeamBetween(root, "Factory Roof Truss West " + x,
                new Vector3(x, 10.2f, -18.5f), new Vector3(x + 4f, 12.6f, 0f), 0.34f, trim);
            CreateBeamBetween(root, "Factory Roof Truss East " + x,
                new Vector3(x, 10.2f, 18.5f), new Vector3(x + 4f, 12.6f, 0f), 0.34f, trim);
            CreateVisualBeam(root, "Factory Roof Truss Cross Brace " + x,
                new Vector3(x + 2f, 12.25f, 0f), new Vector3(0.28f, 0.28f, 37f), steel);
        }

        // Large overhead pipes add a visible second ceiling layer and lead the eye toward the finish.
        for (int side = -1; side <= 1; side += 2)
        {
            float wallSide = side;
            CreateVisualBeam(root, "Factory Overhead Main Duct " + side,
                new Vector3(16f, 11.2f, wallSide * 6.4f), new Vector3(120f, 1.2f, 1.2f), darkSteel);
            CreateVisualBeam(root, "Factory Overhead Duct Highlight " + side,
                new Vector3(16f, 11.82f, wallSide * 6.4f), new Vector3(120f, 0.12f, 0.12f), amber);
        }
    }

    // Give the course a recognizable enclosed factory shell without adding anything the player can collide with.
    private static void CreateFactoryInterior(Transform root, Material steel, Material darkSteel,
        Material amber, Material lamp, Material trim)
    {
        // Repeated ceiling panels close the open sky and make the play space read as one large indoor hall.
        for (int x = -40; x <= 76; x += 8)
        {
            CreateVisualBeam(root, "Factory Ceiling Panel " + x,
                new Vector3(x, 13.2f, 0f), new Vector3(7.85f, 0.3f, 43.4f), darkSteel);
            CreateVisualBeam(root, "Factory Ceiling Cross Girder " + x,
                new Vector3(x, 12.95f, 0f), new Vector3(0.52f, 0.5f, 44f), trim);
        }

        // Parallel rails, ducts, and narrow amber conduits create a layered industrial ceiling.
        for (int side = -1; side <= 1; side++)
        {
            float z = side * 10f;
            CreateVisualBeam(root, "Overhead Ventilation Duct " + side,
                new Vector3(16f, 12.25f, z), new Vector3(120f, 0.72f, 1.25f), steel);
            CreateVisualBeam(root, "Overhead Amber Pipe " + side,
                new Vector3(16f, 11.78f, z + 0.9f), new Vector3(120f, 0.12f, 0.12f), amber);
        }

        // Each wall bay gets a deep equipment housing, narrow vent slots, a tank, and a status light.
        for (int x = -32; x <= 64; x += 16)
        {
            foreach (int side in new[] { -1, 1 })
            {
                float wallSide = side;
                float machineZ = wallSide * 19.2f;
                CreateVisualBeam(root, "Wall Mounted Factory Unit " + x + " " + side,
                    new Vector3(x, 3.2f, machineZ), new Vector3(4.6f, 4.2f, 4.8f), darkSteel);
                CreateVisualBeam(root, "Wall Unit Face Plate " + x + " " + side,
                    new Vector3(x, 3.2f, wallSide * 16.72f), new Vector3(3.8f, 3.4f, 0.12f), steel);

                for (int slot = -1; slot <= 1; slot++)
                {
                    CreateVisualBeam(root, "Wall Unit Vent Slot " + x + " " + side + " " + slot,
                        new Vector3(x + slot * 0.82f, 3.25f, wallSide * 16.63f),
                        new Vector3(0.32f, 2.65f, 0.08f), darkSteel);
                }

                CreateVisualBeam(root, "Wall Unit Amber Indicator " + x + " " + side,
                    new Vector3(x + 1.35f, 5.1f, wallSide * 16.6f),
                    new Vector3(0.22f, 0.22f, 0.1f), lamp);
                CreateVisualCylinder(root, "Factory Pressure Tank " + x + " " + side,
                    new Vector3(x + 3.2f, 2.6f, wallSide * 17.4f),
                    new Vector3(1.25f, 5.2f, 1.25f), steel);
                CreateVisualBeam(root, "Wall Unit Pipe " + x + " " + side,
                    new Vector3(x - 2.65f, 5.1f, wallSide * 18.4f),
                    new Vector3(0.24f, 3.1f, 0.24f), amber);
            }
        }

        // Evenly spaced warm strips illuminate the ceiling details without changing the course lighting logic.
        for (int x = -32; x <= 64; x += 16)
        {
            CreateVisualBeam(root, "Ceiling Amber Light Strip " + x,
                new Vector3(x, 12.55f, 0f), new Vector3(2.8f, 0.08f, 0.42f), lamp);
            CreatePointLight(root, "Ceiling Factory Glow " + x, new Vector3(x, 11.7f, 0f));
        }
    }

    // Rotate a cube beam between two world points for diagonal braces and trusses.
    private static void CreateBeamBetween(Transform parent, string objectName, Vector3 start, Vector3 end,
        float thickness, Material material)
    {
        Vector3 direction = end - start;
        GameObject beam = CreateVisualBeam(parent, objectName, (start + end) * 0.5f,
            new Vector3(direction.magnitude, thickness, thickness), material);
        beam.transform.rotation = Quaternion.FromToRotation(Vector3.right, direction.normalized);
    }

    // Add low, collider-free deck edging, plate seams, corner bolts, and supports below each fixed platform.
    private static void CreateDeckDetails(Transform root, List<Renderer> deckRenderers, Material trim,
        Material amber, Material hazard, Material darkSteel, Material lamp)
    {
        foreach (Renderer deckRenderer in deckRenderers)
        {
            if (deckRenderer == null || deckRenderer.gameObject.name.ToLowerInvariant().Contains("jump platform"))
            {
                continue;
            }

            Bounds bounds = deckRenderer.bounds;
            if (bounds.size.x < 1f || bounds.size.z < 1f)
            {
                continue;
            }

            string deckName = deckRenderer.gameObject.name;
            float top = bounds.max.y + 0.014f;
            float longEdgeWidth = Mathf.Min(0.12f, bounds.size.z * 0.04f);
            float endWidth = Mathf.Min(0.2f, bounds.size.x * 0.06f);

            // Dark steel trim outlines each deck while staying below the rolling surface.
            CreateVisualBeam(root, deckName + " Visual Trim North",
                new Vector3(bounds.center.x, top, bounds.max.z - longEdgeWidth * 0.5f),
                new Vector3(bounds.size.x, 0.028f, longEdgeWidth), trim);
            CreateVisualBeam(root, deckName + " Visual Trim South",
                new Vector3(bounds.center.x, top, bounds.min.z + longEdgeWidth * 0.5f),
                new Vector3(bounds.size.x, 0.028f, longEdgeWidth), trim);
            CreateVisualBeam(root, deckName + " Visual Hazard End West",
                new Vector3(bounds.min.x + endWidth * 0.5f, top + 0.008f, bounds.center.z),
                new Vector3(endWidth, 0.025f, bounds.size.z * 0.82f), hazard);
            CreateVisualBeam(root, deckName + " Visual Hazard End East",
                new Vector3(bounds.max.x - endWidth * 0.5f, top + 0.008f, bounds.center.z),
                new Vector3(endWidth, 0.025f, bounds.size.z * 0.82f), hazard);

            // Repeated seams make broad graybox tops read as bolted metal plates.
            for (float seamX = bounds.min.x + 2.5f; seamX < bounds.max.x - 1.5f; seamX += 3f)
            {
                CreateVisualBeam(root, deckName + " Visual Plate Seam",
                    new Vector3(seamX, top + 0.008f, bounds.center.z),
                    new Vector3(0.025f, 0.012f, bounds.size.z * 0.8f), darkSteel);
            }

            // Small square bolt heads sit at deck corners and remain too low to affect the ball.
            float insetX = Mathf.Min(0.22f, bounds.size.x * 0.15f);
            float insetZ = Mathf.Min(0.22f, bounds.size.z * 0.15f);
            foreach (float xSign in new[] { -1f, 1f })
            {
                foreach (float zSign in new[] { -1f, 1f })
                {
                    CreateVisualBeam(root, deckName + " Visual Deck Bolt",
                        new Vector3(bounds.center.x + xSign * (bounds.extents.x - insetX), top + 0.02f,
                            bounds.center.z + zSign * (bounds.extents.z - insetZ)),
                        new Vector3(0.075f, 0.025f, 0.075f), darkSteel);
                }
            }

            // Small recessed-looking amber markers make the platform perimeter visible in the dark factory.
            if (bounds.size.x >= 4f && bounds.size.z >= 3f)
            {
                foreach (float xSign in new[] { -1f, 1f })
                {
                    foreach (float zSign in new[] { -1f, 1f })
                    {
                        Vector3 markerPosition = new Vector3(bounds.center.x + xSign * (bounds.extents.x - 0.42f),
                            top + 0.018f, bounds.center.z + zSign * (bounds.extents.z - 0.20f));
                        CreateVisualBeam(root, deckName + " Edge Light Housing", markerPosition,
                            new Vector3(0.24f, 0.035f, 0.16f), darkSteel);
                        CreateVisualBeam(root, deckName + " Amber Edge Light", markerPosition + Vector3.up * 0.022f,
                            new Vector3(0.14f, 0.015f, 0.08f), lamp);
                    }
                }
            }

            // Structural legs descend toward the catch floor to make floating platforms feel supported.
            float supportBottom = -7.2f;
            if (bounds.size.x >= 4.5f && bounds.size.z >= 4.5f && bounds.min.y > supportBottom)
            {
                float supportHeight = bounds.min.y - supportBottom;
                float supportY = (bounds.min.y + supportBottom) * 0.5f;
                float supportXInset = Mathf.Min(0.45f, bounds.extents.x * 0.5f);
                float supportZInset = Mathf.Min(0.45f, bounds.extents.z * 0.5f);
                foreach (float xSign in new[] { -1f, 1f })
                {
                    foreach (float zSign in new[] { -1f, 1f })
                    {
                        CreateVisualBeam(root, deckName + " Visual Support Leg",
                            new Vector3(bounds.center.x + xSign * (bounds.extents.x - supportXInset), supportY,
                                bounds.center.z + zSign * (bounds.extents.z - supportZInset)),
                            new Vector3(0.34f, supportHeight, 0.34f), darkSteel);
                    }
                }

                // Cross braces below the platform provide a visible structural frame from the rear camera.
                CreateVisualBeam(root, deckName + " Visual Underframe X",
                    new Vector3(bounds.center.x, bounds.min.y - 0.55f, bounds.center.z),
                    new Vector3(bounds.size.x * 0.78f, 0.22f, 0.22f), trim);
                CreateVisualBeam(root, deckName + " Visual Underframe Z",
                    new Vector3(bounds.center.x, bounds.min.y - 0.55f, bounds.center.z),
                    new Vector3(0.22f, 0.22f, bounds.size.z * 0.78f), trim);
            }
        }
    }

    // Open lattice towers below the route add depth without adding colliders or obscuring the playable surface.
    private static void CreateDepthTrusses(Transform root, Material darkSteel, Material trim, Material lamp)
    {
        for (int x = -32; x <= 64; x += 12)
        {
            foreach (int side in new[] { -1, 1 })
            {
                float z = side * 12f;
                for (int post = -1; post <= 1; post += 2)
                {
                    CreateVisualBeam(root, "Depth Truss Post " + x + " " + side + " " + post,
                        new Vector3(x + post * 1.1f, 2.4f, z), new Vector3(0.22f, 19.2f, 0.22f), darkSteel);
                }
                for (int level = 0; level < 9; level++)
                {
                    float y = -6.8f + level * 2f;
                    CreateVisualBeam(root, "Depth Truss Crossbar " + x + " " + side + " " + level,
                        new Vector3(x, y, z), new Vector3(2.3f, 0.14f, 0.16f), trim);
                    CreateBeamBetween(root, "Depth Truss Diagonal " + x + " " + side + " " + level,
                        new Vector3(x - 1.1f, y, z), new Vector3(x + 1.1f, y + 2f, z), 0.12f, trim);
                }
                // A warm fixture breaks up the cool, shadowed supports at a distance.
                CreateVisualBeam(root, "Depth Truss Work Light " + x + " " + side,
                    new Vector3(x, -2.1f, z - side * 0.12f), new Vector3(0.13f, 0.85f, 0.08f), lamp);
            }
        }

        // Long rails join the towers into one continuous structure along each side of the hall.
        foreach (int side in new[] { -1, 1 })
        {
            foreach (float y in new[] { -6.8f, -2.8f, 1.2f, 5.2f, 9.2f })
            {
                CreateVisualBeam(root, "Depth Truss Longitudinal Rail " + side + " " + y,
                    new Vector3(16f, y, side * 12f), new Vector3(100f, 0.18f, 0.18f), darkSteel);
            }
        }
    }

    // Add details only to the six real course beams, never to decorative meshes from an earlier art pass.
    private static void CreateBalanceCourseDetails(Material darkSteel, Material amber, Material trim)
    {
        foreach (Rigidbody beamBody in Object.FindObjectsByType<Rigidbody>())
        {
            if (beamBody == null || !IsBalanceCourseBeam(beamBody.gameObject.name))
            {
                continue;
            }

            Transform beam = beamBody.transform;
            RemoveOldBalanceBeamDetails(beam);

            GameObject detailsRoot = new GameObject("Industrial Balance Beam Details");
            detailsRoot.transform.SetParent(beam, false);
            CreateChildVisualBeam(detailsRoot.transform, "Amber Edge Stripe Left",
                new Vector3(0f, 0.56f, -0.36f), new Vector3(0.9f, 0.08f, 0.08f), amber);
            CreateChildVisualBeam(detailsRoot.transform, "Amber Edge Stripe Right",
                new Vector3(0f, 0.56f, 0.36f), new Vector3(0.9f, 0.08f, 0.08f), amber);
            CreateChildVisualBeam(detailsRoot.transform, "Center Reinforcement",
                new Vector3(0f, 0.56f, 0f), new Vector3(0.025f, 0.07f, 0.76f), trim);
            CreateChildVisualBeam(detailsRoot.transform, "Underframe",
                new Vector3(0f, -0.56f, 0f), new Vector3(0.86f, 0.1f, 0.7f), darkSteel);
        }
    }

    // Accept only the numbered rigidbody segments created for this course, including its stable final connector.
    private static bool IsBalanceCourseBeam(string objectName)
    {
        const string beamPrefix = "Balance Beam ";
        if (!objectName.StartsWith(beamPrefix))
        {
            return false;
        }

        string number = objectName.Substring(beamPrefix.Length);
        return int.TryParse(number, out int beamNumber) && beamNumber > 0;
    }

    // Remove the old repeated decorations under each gameplay beam before rebuilding its four intended details.
    private static void RemoveOldBalanceBeamDetails(Transform beam)
    {
        for (int childIndex = beam.childCount - 1; childIndex >= 0; childIndex--)
        {
            Transform child = beam.GetChild(childIndex);
            string childName = child.name;
            bool isLegacyDetail = childName.StartsWith(beam.name + " Amber Edge Stripe")
                || childName.StartsWith(beam.name + " Center Reinforcement")
                || childName.StartsWith(beam.name + " Underframe");
            bool isCurrentDetailGroup = childName == "Industrial Balance Beam Details";

            // Legacy detail roots contain the recursively multiplied meshes, so removing each root clears its full branch.
            if (isLegacyDetail || isCurrentDetailGroup)
            {
                Object.DestroyImmediate(child.gameObject);
            }
        }
    }

    // Create collider-free decoration in beam-local space so it inherits the beam's gentle rocking motion.
    private static GameObject CreateChildVisualBeam(Transform parent, string objectName, Vector3 localPosition,
        Vector3 localScale, Material material)
    {
        GameObject detail = GameObject.CreatePrimitive(PrimitiveType.Cube);
        detail.name = objectName;
        detail.transform.SetParent(parent, false);
        detail.transform.localPosition = localPosition;
        detail.transform.localRotation = Quaternion.identity;
        detail.transform.localScale = localScale;
        detail.GetComponent<Renderer>().sharedMaterial = material;
        Object.DestroyImmediate(detail.GetComponent<Collider>());
        return detail;
    }

    // Give the player a smooth inlaid belt and replace each pickup's visible sphere with a compact power cell.
    private static void CreatePlayerAndCollectibleVisuals(Material playerShell, Material collectibleCore,
        Material darkSteel, Material trim, Material amberDetail, Mesh detailRing, Mesh bandedPlayerMesh)
    {
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            MeshFilter playerMeshFilter = player.GetComponent<MeshFilter>();
            Renderer playerRenderer = player.GetComponent<Renderer>();
            if (playerMeshFilter != null && playerRenderer != null)
            {
                // Keep the original Player object and its collider; change only the rendered mesh and its three surface materials.
                playerMeshFilter.sharedMesh = bandedPlayerMesh;
                playerRenderer.sharedMaterials = new[] { playerShell, trim, amberDetail };
            }

            Transform oldPlayerVisuals = player.transform.Find("Industrial Player Visuals");
            if (oldPlayerVisuals != null)
            {
                Object.DestroyImmediate(oldPlayerVisuals.gameObject);
            }
        }

        foreach (CollectibleController collectible in Object.FindObjectsByType<CollectibleController>())
        {
            Transform item = collectible.transform;
            Renderer originalRenderer = collectible.GetComponent<Renderer>();
            if (originalRenderer != null)
            {
                // Keep the existing trigger sphere for gameplay while replacing only its visible mesh.
                originalRenderer.enabled = false;
            }

            Transform oldVisuals = item.Find("Industrial Collectible Visuals");
            if (oldVisuals != null)
            {
                Object.DestroyImmediate(oldVisuals.gameObject);
            }

            Transform visuals = CreateVisualRoot(item, "Industrial Collectible Visuals");
            // Enlarge only the decorative model; the collectible's original trigger collider stays unchanged.
            visuals.localScale = Vector3.one * 1.3f;

            // A short glowing body makes each pickup read like a small factory power cell.
            CreateVisualCylinder(visuals, "Amber Power Cell", collectibleCore,
                Vector3.zero, new Vector3(0.26f, 0.18f, 0.26f));

            // Dark metal caps frame the glowing center without hiding it.
            CreateVisualCylinder(visuals, "Power Cell Top Cap", darkSteel,
                new Vector3(0f, 0.22f, 0f), new Vector3(0.28f, 0.045f, 0.28f));
            CreateVisualCylinder(visuals, "Power Cell Bottom Cap", darkSteel,
                new Vector3(0f, -0.22f, 0f), new Vector3(0.28f, 0.045f, 0.28f));

            // Slim side rails add an industrial silhouette while staying inside the original pickup trigger.
            CreateVisualCylinder(visuals, "Power Cell Left Guard", trim,
                new Vector3(-0.19f, 0f, 0f), new Vector3(0.035f, 0.25f, 0.035f));
            CreateVisualCylinder(visuals, "Power Cell Right Guard", trim,
                new Vector3(0.19f, 0f, 0f), new Vector3(0.035f, 0.25f, 0.035f));

            // Thin amber rings mark the cap edges and follow the existing collectible's slow rotation.
            CreateTorusVisual(visuals, "Power Cell Top Amber Seal", detailRing, amberDetail,
                new Vector3(0f, 0.22f, 0f), Vector3.one * 0.22f, Vector3.zero);
            CreateTorusVisual(visuals, "Power Cell Bottom Amber Seal", detailRing, amberDetail,
                new Vector3(0f, -0.22f, 0f), Vector3.one * 0.22f, Vector3.zero);
        }
    }

    // Create a Unity cylinder for decoration, place it in its parent's local space, and remove its collider.
    private static void CreateVisualCylinder(Transform parent, string objectName, Material material,
        Vector3 localPosition, Vector3 localScale)
    {
        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.name = objectName;
        cylinder.transform.SetParent(parent, false);
        cylinder.transform.localPosition = localPosition;
        cylinder.transform.localRotation = Quaternion.identity;
        cylinder.transform.localScale = localScale;
        cylinder.GetComponent<Renderer>().sharedMaterial = material;
        Object.DestroyImmediate(cylinder.GetComponent<Collider>());
    }

    // Create a child holder so all visual pieces follow their existing ball or collectible transform.
    private static Transform CreateVisualRoot(Transform parent, string rootName)
    {
        GameObject root = new GameObject(rootName);
        root.transform.SetParent(parent, false);
        return root.transform;
    }

    // Build a collider-free torus renderer with local placement and rotation controlled by its parent.
    private static void CreateTorusVisual(Transform parent, string objectName, Mesh mesh, Material material,
        Vector3 localPosition, Vector3 localScale, Vector3 localEulerAngles)
    {
        GameObject ring = new GameObject(objectName, typeof(MeshFilter), typeof(MeshRenderer));
        ring.transform.SetParent(parent, false);
        ring.transform.localPosition = localPosition;
        ring.transform.localRotation = Quaternion.Euler(localEulerAngles);
        ring.transform.localScale = localScale;
        ring.GetComponent<MeshFilter>().sharedMesh = mesh;
        ring.GetComponent<MeshRenderer>().sharedMaterial = material;
    }

    // Save a reusable ring mesh so the player and all four collectibles share the same geometry asset.
    private static Mesh CreateIndustrialRingMesh()
    {
        Mesh ringMesh = AssetDatabase.LoadAssetAtPath<Mesh>(IndustrialRingMeshPath);
        if (ringMesh != null)
        {
            return ringMesh;
        }

        const int majorSegments = 48;
        const int tubeSegments = 12;
        const float tubeRadius = 0.1f;
        Vector3[] vertices = new Vector3[majorSegments * tubeSegments];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[majorSegments * tubeSegments * 6];

        for (int major = 0; major < majorSegments; major++)
        {
            float aroundRing = major * Mathf.PI * 2f / majorSegments;
            for (int tube = 0; tube < tubeSegments; tube++)
            {
                float aroundTube = tube * Mathf.PI * 2f / tubeSegments;
                float radius = 1f + tubeRadius * Mathf.Cos(aroundTube);
                int vertexIndex = major * tubeSegments + tube;
                vertices[vertexIndex] = new Vector3(radius * Mathf.Cos(aroundRing),
                    tubeRadius * Mathf.Sin(aroundTube), radius * Mathf.Sin(aroundRing));
                uv[vertexIndex] = new Vector2((float)major / majorSegments, (float)tube / tubeSegments);

                int nextMajor = (major + 1) % majorSegments;
                int nextTube = (tube + 1) % tubeSegments;
                int triangleIndex = vertexIndex * 6;
                triangles[triangleIndex] = vertexIndex;
                triangles[triangleIndex + 1] = nextMajor * tubeSegments + nextTube;
                triangles[triangleIndex + 2] = nextMajor * tubeSegments + tube;
                triangles[triangleIndex + 3] = vertexIndex;
                triangles[triangleIndex + 4] = major * tubeSegments + nextTube;
                triangles[triangleIndex + 5] = nextMajor * tubeSegments + nextTube;
            }
        }

        ringMesh = new Mesh { name = "Industrial Detail Ring Mesh" };
        ringMesh.vertices = vertices;
        ringMesh.uv = uv;
        ringMesh.triangles = triangles;
        ringMesh.RecalculateNormals();
        ringMesh.RecalculateBounds();
        AssetDatabase.CreateAsset(ringMesh, IndustrialRingMeshPath);
        return ringMesh;
    }

    // Build the player sphere as one mesh with steel, dark trim, and a single amber equator band.
    private static Mesh CreateBandedPlayerMesh()
    {
        Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(BandedPlayerMeshPath);
        if (mesh != null)
        {
            return mesh;
        }

        const int latitudeSegments = 96;
        const int longitudeSegments = 64;
        const float radius = 0.5f;
        const float amberBandHalfHeight = 0.115f;
        const float trimBandHalfHeight = 0.145f;
        Vector3[] vertices = new Vector3[(latitudeSegments + 1) * (longitudeSegments + 1)];
        Vector2[] uv = new Vector2[vertices.Length];
        List<int>[] trianglesByMaterial = { new List<int>(), new List<int>(), new List<int>() };

        for (int latitude = 0; latitude <= latitudeSegments; latitude++)
        {
            float angleFromTop = latitude * Mathf.PI / latitudeSegments;
            float verticalPosition = Mathf.Cos(angleFromTop);
            float ringRadius = Mathf.Sin(angleFromTop) * radius;

            for (int longitude = 0; longitude <= longitudeSegments; longitude++)
            {
                float angleAround = longitude * Mathf.PI * 2f / longitudeSegments;
                int vertexIndex = latitude * (longitudeSegments + 1) + longitude;
                vertices[vertexIndex] = new Vector3(ringRadius * Mathf.Cos(angleAround),
                    verticalPosition * radius, ringRadius * Mathf.Sin(angleAround));
                uv[vertexIndex] = new Vector2((float)longitude / longitudeSegments,
                    (float)latitude / latitudeSegments);

            }
        }

        // Build triangles only after every vertex has a real position, so band colors are classified from the complete sphere.
        for (int latitude = 0; latitude < latitudeSegments; latitude++)
        {
            for (int longitude = 0; longitude < longitudeSegments; longitude++)
            {
                int topLeft = latitude * (longitudeSegments + 1) + longitude;
                int bottomLeft = topLeft + longitudeSegments + 1;
                int bottomRight = bottomLeft + 1;
                int topRight = topLeft + 1;

                float triangleCenterHeight = (vertices[topLeft].y + vertices[bottomLeft].y + vertices[bottomRight].y) / 3f;
                int materialIndex = GetPlayerBandMaterialIndex(triangleCenterHeight, amberBandHalfHeight, trimBandHalfHeight);
                trianglesByMaterial[materialIndex].Add(topLeft);
                trianglesByMaterial[materialIndex].Add(bottomRight);
                trianglesByMaterial[materialIndex].Add(bottomLeft);

                triangleCenterHeight = (vertices[topLeft].y + vertices[bottomRight].y + vertices[topRight].y) / 3f;
                materialIndex = GetPlayerBandMaterialIndex(triangleCenterHeight, amberBandHalfHeight, trimBandHalfHeight);
                trianglesByMaterial[materialIndex].Add(topLeft);
                trianglesByMaterial[materialIndex].Add(topRight);
                trianglesByMaterial[materialIndex].Add(bottomRight);
            }
        }

        mesh = new Mesh { name = "Industrial Banded Player Ball Mesh" };
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.subMeshCount = trianglesByMaterial.Length;
        for (int materialIndex = 0; materialIndex < trianglesByMaterial.Length; materialIndex++)
        {
            mesh.SetTriangles(trianglesByMaterial[materialIndex], materialIndex);
        }

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        AssetDatabase.CreateAsset(mesh, BandedPlayerMeshPath);
        return mesh;
    }

    // Map the sphere's vertical position to its steel surface, thin dark borders, or amber equator band.
    private static int GetPlayerBandMaterialIndex(float vertexHeight, float amberBandHalfHeight, float trimBandHalfHeight)
    {
        float absoluteHeight = Mathf.Abs(vertexHeight);
        if (absoluteHeight <= amberBandHalfHeight)
        {
            return 2;
        }

        return absoluteHeight <= trimBandHalfHeight ? 1 : 0;
    }

    // Add collider-free metal face panels to the existing maze walls so the large yellow-gray surfaces read as factory barriers.
    private static void CreateMazeWallDetails(Material steel, Material darkSteel, Material amber, Material lamp)
    {
        foreach (Renderer wallRenderer in Object.FindObjectsByType<Renderer>())
        {
            if (wallRenderer == null)
            {
                continue;
            }

            // Only decorate original camera-fadeable maze walls, not renderers created by this art pass.
            if (wallRenderer.GetComponent<PrototypeCameraOccluder>() == null)
            {
                continue;
            }

            string lowerName = wallRenderer.gameObject.name.ToLowerInvariant();
            if (!lowerName.Contains("maze east wall") && !lowerName.Contains("maze north wall") &&
                !lowerName.Contains("maze south border") && !lowerName.Contains("maze entrance border"))
            {
                continue;
            }

            Bounds bounds = wallRenderer.bounds;
            bool wallRunsAlongX = bounds.size.x >= bounds.size.z;
            float wallLength = wallRunsAlongX ? bounds.size.x : bounds.size.z;
            if (wallLength < 1f || bounds.size.y < 0.5f)
            {
                continue;
            }

            float panelLength = Mathf.Max(0.5f, wallLength - 0.12f);
            float panelHeight = Mathf.Max(0.35f, bounds.size.y * 0.68f);
            float panelY = bounds.center.y;
            float panelDepth = 0.055f;

            // Parent this art group to the original wall so its camera fade also reaches the new renderers.
            const string wallDetailsRootName = "Industrial Wall Details (Camera Fading)";
            Transform existingDetails = wallRenderer.transform.Find(wallDetailsRootName);
            if (existingDetails != null)
            {
                Object.DestroyImmediate(existingDetails.gameObject);
            }

            GameObject wallDetailsRoot = new GameObject(wallDetailsRootName);
            wallDetailsRoot.transform.SetParent(wallRenderer.transform, true);

            // Dress both sides because the player can approach a maze wall from either direction.
            foreach (float faceDirection in new[] { -1f, 1f })
            {
                Vector3 panelPosition;
                Vector3 panelScale;
                Vector3 accentPosition;
                Vector3 accentScale;

                if (wallRunsAlongX)
                {
                    float faceZ = bounds.center.z + faceDirection * (bounds.extents.z + panelDepth * 0.5f + 0.008f);
                    panelPosition = new Vector3(bounds.center.x, panelY, faceZ);
                    panelScale = new Vector3(panelLength, panelHeight, panelDepth);
                    accentPosition = new Vector3(bounds.center.x, bounds.max.y - 0.14f, faceZ - faceDirection * 0.04f);
                    accentScale = new Vector3(panelLength, 0.12f, 0.07f);
                }
                else
                {
                    float faceX = bounds.center.x + faceDirection * (bounds.extents.x + panelDepth * 0.5f + 0.008f);
                    panelPosition = new Vector3(faceX, panelY, bounds.center.z);
                    panelScale = new Vector3(panelDepth, panelHeight, panelLength);
                    accentPosition = new Vector3(faceX - faceDirection * 0.04f, bounds.max.y - 0.14f, bounds.center.z);
                    accentScale = new Vector3(0.07f, 0.12f, panelLength);
                }

                CreateVisualBeam(wallDetailsRoot.transform, wallRenderer.gameObject.name + " Industrial Face Panel " + faceDirection,
                    panelPosition, panelScale, darkSteel);
                CreateVisualBeam(wallDetailsRoot.transform, wallRenderer.gameObject.name + " Amber Safety Band " + faceDirection,
                    accentPosition, accentScale, amber);

                // Raised metal ribs break each long flat panel into readable factory sections.
                int ribCount = Mathf.Max(1, Mathf.FloorToInt(panelLength / 1.8f));
                for (int rib = 1; rib < ribCount; rib++)
                {
                    float alongWall = -panelLength * 0.5f + panelLength * rib / ribCount;
                    Vector3 ribPosition = wallRunsAlongX
                        ? new Vector3(bounds.center.x + alongWall, panelY, panelPosition.z - faceDirection * 0.04f)
                        : new Vector3(panelPosition.x - faceDirection * 0.04f, panelY, bounds.center.z + alongWall);
                    Vector3 ribScale = wallRunsAlongX
                        ? new Vector3(0.09f, panelHeight, 0.07f)
                        : new Vector3(0.07f, panelHeight, 0.09f);
                    CreateVisualBeam(wallDetailsRoot.transform, wallRenderer.gameObject.name + " Steel Wall Rib " + faceDirection,
                        ribPosition, ribScale, steel);
                }

                // Small amber lamps mark the ends of each wall section as visible industrial safety details.
                float lampY = bounds.min.y + Mathf.Min(0.45f, bounds.size.y * 0.25f);
                foreach (float endDirection in new[] { -1f, 1f })
                {
                    Vector3 lampPosition = wallRunsAlongX
                        ? new Vector3(bounds.center.x + endDirection * (panelLength * 0.42f), lampY,
                            panelPosition.z - faceDirection * 0.05f)
                        : new Vector3(panelPosition.x - faceDirection * 0.05f, lampY,
                            bounds.center.z + endDirection * (panelLength * 0.42f));
                    Vector3 lampScale = wallRunsAlongX ? new Vector3(0.14f, 0.24f, 0.07f) : new Vector3(0.07f, 0.24f, 0.14f);
                    CreateVisualBeam(wallDetailsRoot.transform, wallRenderer.gameObject.name + " Safety Lamp " + faceDirection,
                        lampPosition, lampScale, lamp);
                }
            }
        }
    }

    // Replace the plain gray catch-floor appearance with a dark, gridded factory-depth surface.
    private static void CreateIndustrialPit(Transform root, Material grid, Material amber)
    {
        const float floorY = -7.47f;
        for (int z = -40; z <= 40; z += 8)
        {
            Material lineMaterial = z % 32 == 0 ? amber : grid;
            CreateVisualBeam(root, "Pit Floor Long Grid Line " + z,
                new Vector3(10f, floorY, z), new Vector3(120f, 0.018f, 0.026f), lineMaterial);
        }

        for (int x = -50; x <= 70; x += 8)
        {
            Material lineMaterial = x % 32 == 0 ? amber : grid;
            CreateVisualBeam(root, "Pit Floor Cross Grid Line " + x,
                new Vector3(x, floorY, 0f), new Vector3(0.026f, 0.018f, 80f), lineMaterial);
        }
    }

    // Keep the existing rear-follow camera angle and replace only its open-sky background color.
    private static void SetIndustrialCameraBackground()
    {
        Camera sceneCamera = Object.FindAnyObjectByType<Camera>();
        if (sceneCamera != null)
        {
            sceneCamera.clearFlags = CameraClearFlags.SolidColor;
        sceneCamera.backgroundColor = new Color(0.09f, 0.13f, 0.16f);
        }
    }

    // Primitive colliders are removed immediately so art dressing cannot affect rolling, jumping, or falls.
    private static GameObject CreateVisualBeam(Transform parent, string objectName, Vector3 position, Vector3 scale, Material material)
    {
        GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
        beam.name = objectName;
        beam.transform.SetParent(parent, false);
        beam.transform.SetPositionAndRotation(position, Quaternion.identity);
        beam.transform.localScale = scale;
        beam.GetComponent<Renderer>().sharedMaterial = material;
        Object.DestroyImmediate(beam.GetComponent<Collider>());
        beam.isStatic = true;
        return beam;
    }

    // Use a collider-free cylinder for tanks and pressure vessels in the scenery.
    private static GameObject CreateVisualCylinder(Transform parent, string objectName, Vector3 position,
        Vector3 scale, Material material)
    {
        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.name = objectName;
        cylinder.transform.SetParent(parent, false);
        cylinder.transform.SetPositionAndRotation(position, Quaternion.identity);
        cylinder.transform.localScale = scale;
        cylinder.GetComponent<Renderer>().sharedMaterial = material;
        Object.DestroyImmediate(cylinder.GetComponent<Collider>());
        cylinder.isStatic = true;
        return cylinder;
    }

    // These low-intensity point lights add warm reflections but cast no shadows to keep rendering lightweight.
    private static void CreatePointLight(Transform parent, string objectName, Vector3 position)
    {
        GameObject lightObject = new GameObject(objectName);
        lightObject.transform.SetParent(parent, false);
        lightObject.transform.position = position;
        Light pointLight = lightObject.AddComponent<Light>();
        pointLight.type = LightType.Point;
        pointLight.color = new Color(1f, 0.65f, 0.32f);
        pointLight.intensity = 3f;
        pointLight.range = 13f;
        pointLight.shadows = LightShadows.None;
    }

    // A cooler ambient fill and a restrained warm tint establish the reference image's moody factory palette.
    private static void SetIndustrialLighting()
    {
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.55f, 0.57f, 0.58f);
        RenderSettings.ambientIntensity = 1.1f;
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.18f, 0.23f, 0.27f);
        RenderSettings.fogDensity = 0.006f;

        // Find the directional light explicitly because the scene also contains many decorative point lights.
        foreach (Light mainLight in Object.FindObjectsByType<Light>())
        {
            if (mainLight.type == LightType.Directional)
            {
                mainLight.color = new Color(1f, 0.87f, 0.71f);
                mainLight.intensity = 2.0f;
            }
        }
    }

    // Create or reuse a saved URP Lit material so Unity keeps the art style after the editor closes.
    private static Material CreateMaterial(string materialName, Color color, float metallic, float smoothness)
    {
        EnsureMaterialsFolder();
        string assetPath = MaterialsFolder + "/" + materialName + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
        if (material == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            material = new Material(shader);
            AssetDatabase.CreateAsset(material, assetPath);
        }

        material.color = color;
        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
        EditorUtility.SetDirty(material);
        return material;
    }

    // Add a saved texture to URP Lit materials for plate or hazard-strip detail.
    private static Material CreateTexturedMaterial(string materialName, Color color, float metallic,
        float smoothness, string texturePath)
    {
        Material material = CreateMaterial(materialName, color, metallic, smoothness);
        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
        if (texture != null)
        {
            if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", texture);
            if (material.HasProperty("_MainTex")) material.SetTexture("_MainTex", texture);
            EditorUtility.SetDirty(material);
        }

        return material;
    }

    // Scale the plate pattern by platform dimensions so the metal panels keep a believable real-world size.
    private static void SetDeckTextureTiling(Renderer renderer)
    {
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(block);
        Bounds bounds = renderer.bounds;
        Vector4 textureScale = new Vector4(Mathf.Max(1f, bounds.size.x / 2.5f),
            Mathf.Max(1f, bounds.size.z / 2.5f), 0f, 0f);
        block.SetVector("_BaseMap_ST", textureScale);
        block.SetVector("_MainTex_ST", textureScale);
        renderer.SetPropertyBlock(block);
    }

    // Generate reusable seamless plate and warning-stripe textures in the project's material folder.
    private static void CreateIndustrialTextures()
    {
        EnsureMaterialsFolder();
        CreateDeckPlateTextureIfMissing();
        CreateHazardTextureIfMissing();
    }

    private static void CreateDeckPlateTextureIfMissing()
    {
        if (File.Exists(DeckTexturePath)) return;

        const int size = 256;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGB24, false);
        Color32[] pixels = new Color32[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float noise = Mathf.PerlinNoise(x * 0.045f, y * 0.045f);
                float brightness = 0.68f + noise * 0.13f;
                bool edge = x < 5 || y < 5 || x >= size - 5 || y >= size - 5;
                Color color = edge
                    ? new Color(0.11f, 0.14f, 0.16f)
                    : new Color(0.38f * brightness, 0.43f * brightness, 0.45f * brightness);

                // Four recessed corners and bright bolt heads make adjacent repeats form plate joints.
                float rivetDistance = DistanceToNearestPlateCorner(x, y, size);
                if (rivetDistance < 5f) color = new Color(0.08f, 0.1f, 0.11f);
                if (rivetDistance < 2.1f) color = new Color(0.62f, 0.66f, 0.66f);
                pixels[y * size + x] = color;
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply();
        File.WriteAllBytes(DeckTexturePath, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(DeckTexturePath, ImportAssetOptions.ForceSynchronousImport);
        SetRepeatingTextureImportSettings(DeckTexturePath);
    }

    private static float DistanceToNearestPlateCorner(int x, int y, int size)
    {
        int nearestX = x < size / 2 ? 8 : size - 9;
        int nearestY = y < size / 2 ? 8 : size - 9;
        return Vector2.Distance(new Vector2(x, y), new Vector2(nearestX, nearestY));
    }

    private static void CreateHazardTextureIfMissing()
    {
        if (File.Exists(HazardTexturePath)) return;

        const int width = 256;
        const int height = 64;
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGB24, false);
        Color32[] pixels = new Color32[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool amberStripe = (x + y) % 64 < 32;
                float noise = Mathf.PerlinNoise(x * 0.09f, y * 0.09f) * 0.1f;
                Color color = amberStripe
                    ? new Color(0.92f - noise, 0.39f - noise, 0.045f)
                    : new Color(0.035f + noise, 0.045f + noise, 0.05f + noise);
                pixels[y * width + x] = color;
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply();
        File.WriteAllBytes(HazardTexturePath, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(HazardTexturePath, ImportAssetOptions.ForceSynchronousImport);
        SetRepeatingTextureImportSettings(HazardTexturePath);
    }

    private static void SetRepeatingTextureImportSettings(string assetPath)
    {
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null) return;
        importer.wrapMode = TextureWrapMode.Repeat;
        importer.filterMode = FilterMode.Trilinear;
        importer.textureCompression = TextureImporterCompression.Compressed;
        importer.SaveAndReimport();
    }

    // Emission makes the amber fixtures readable as small warm light sources in the gray-blue environment.
    private static Material CreateEmissiveMaterial(string materialName, Color color, float emissionStrength)
    {
        Material material = CreateMaterial(materialName, color, 0.2f, 0.3f);
        material.EnableKeyword("_EMISSION");
        if (material.HasProperty("_EmissionColor"))
        {
            material.SetColor("_EmissionColor", color * emissionStrength);
        }

        EditorUtility.SetDirty(material);
        return material;
    }

    // Keep all generated art materials together with the prototype's existing materials.
    private static void EnsureMaterialsFolder()
    {
        if (!AssetDatabase.IsValidFolder(MaterialsFolder))
        {
            AssetDatabase.CreateFolder("Assets", "Prototype Materials");
        }
    }
}
