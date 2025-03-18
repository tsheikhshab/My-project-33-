using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// VR-Optimized Suhrawardi Cosmology Visualizer
/// Fixed for reliable rendering across all VR headsets
/// </summary>
public class SuhrawardiVRCosmology : MonoBehaviour
{
    [Header("VR Settings")]
    [Tooltip("Set this to add a guarantee that objects will be visible in VR")]
    [SerializeField] private bool attachVisualizersToVRCamera = true;
    [SerializeField] private float directAttachmentDistance = 0.7f; // Closer for better visibility
    [SerializeField] private bool findVRCameraAutomatically = true;

    [Header("Player Reference")]
    [SerializeField] private Transform cameraTransform; // Main Camera or VR Camera
    [SerializeField] private bool trackCameraPosition = true;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI interactionText;
    [SerializeField] private GameObject textPanel;

    [Header("Scale & Positioning")]
    [SerializeField] private float worldScale = 0.8f; // Increased for better VR visibility
    [SerializeField] private float spawnDistance = 2f; // Closer to camera for VR
    [SerializeField] private float spawnHeightOffset = -0.5f; // Less vertical offset

    [Header("Rendering Options")]
    [SerializeField] private bool forceOpaqueRendering = true; // Always true for VR
    [SerializeField] private float emissionMultiplier = 2.0f; // Reduced for VR compatibility
    [SerializeField] private int maxLightsActive = 4; // Limited for mobile VR

    [Header("Debug Options")]
    [SerializeField] private bool showDebugSphere = true;
    [SerializeField] private Color debugSphereColor = Color.magenta;
    [SerializeField] private float debugSphereSize = 1.5f; // Smaller size
    [SerializeField] private bool logPositioningInfo = true;

    // Generated object references
    private GameObject mountQafContainer;
    private GameObject[] mountainLayers = new GameObject[11];
    private GameObject fountainOfLife;
    private GameObject tubaTree;
    private GameObject nightIlluminatingJewel;
    private GameObject[] jewelPhases = new GameObject[4];
    private GameObject[] twelveWorkshops = new GameObject[12];
    private GameObject davidsArmor;
    private GameObject debugSphere;
    private GameObject vrAttachedVisualizer; // For direct VR camera attachment

    // Materials
    private Material[] mountainMaterials = new Material[11];
    private Material treeMaterial;
    private Material jewelMaterial;
    private Material fountainMaterial;
    private Material waterMaterial;
    private Material armorMaterial;
    private Material[] workshopMaterials = new Material[12];

    // Shader reference - VR compatible
    private Shader vrShader;

    // Colors directly from SVG
    private readonly Color[] mountainColors = new Color[]
    {
        new Color(0f, 0f, 0.2f),        // Mountain 1: Black/Deep Indigo (Fountain of Life)
        new Color(0.58f, 0f, 0.83f),    // Mountain 2: Deep Violet
        new Color(1f, 0f, 0f),          // Mountain 3: Red (Red Intellect)
        new Color(1f, 0.65f, 0f),       // Mountain 4: Orange
        new Color(1f, 1f, 0f),          // Mountain 5: Yellow
        new Color(0.68f, 1f, 0.18f),    // Mountain 6: Yellow-Green
        new Color(0f, 1f, 0f),          // Mountain 7: Green
        new Color(0f, 0.81f, 0.82f),    // Mountain 8: Blue-Green
        new Color(0f, 0f, 1f),          // Mountain 9: Blue
        new Color(0.29f, 0f, 0.51f),    // Mountain 10: Indigo
        new Color(0.54f, 0.17f, 0.89f)  // Mountain 11: Violet (Mystical Knowledge)
    };

    // Radii scaled for VR
    private readonly float[] mountainRadii = new float[]
    {
        6.0f, 9.0f, 12.0f, 15.0f, 18.0f, 21.0f, 24.0f, 27.0f, 30.0f, 33.0f, 36.0f
    };

    // State variables
    private int currentJewelPhase = 0;
    private float jewelRotationTime = 0f;
    private Vector3 centerPosition;
    private bool isInitialized = false;
    private List<Light> activeLights = new List<Light>();
    private bool isMobileVR = false;

    private void Start()
    {
        // Determine if we're on a mobile VR platform
        isMobileVR = Application.isMobilePlatform || SystemInfo.deviceType == DeviceType.Handheld;

        // Choose appropriate shader for the platform
        SetupShader();

        // Delay initialization to ensure VR system is ready
        StartCoroutine(DelayedInitialization());
    }

    private void SetupShader()
    {
        // Choose VR-compatible shader based on platform
        if (isMobileVR)
        {
            // Try to find mobile-friendly shaders in order of preference
            vrShader = Shader.Find("Mobile/Diffuse");
            if (vrShader == null)
                vrShader = Shader.Find("Mobile/VertexLit");
            if (vrShader == null)
                vrShader = Shader.Find("Unlit/Color");

            // Limit emission on mobile
            emissionMultiplier = Mathf.Min(emissionMultiplier, 1.5f);
            maxLightsActive = Mathf.Min(maxLightsActive, 4);
        }
        else
        {
            // For desktop VR, try these options
            vrShader = Shader.Find("Unlit/Color");
            if (vrShader == null)
                vrShader = Shader.Find("Unlit/Texture");
            if (vrShader == null)
                vrShader = Shader.Find("Mobile/Diffuse");
        }

        // Last resort fallback
        if (vrShader == null)
        {
            Debug.LogWarning("Could not find VR-compatible shader. Using fallback.");
            vrShader = Shader.Find("Diffuse");
        }

        Debug.Log($"Using shader: {vrShader.name} for VR rendering");
    }

    private IEnumerator DelayedInitialization()
    {
        Log("Starting initialization with 1 second delay for VR system...");

        // Wait for VR system to initialize
        yield return new WaitForSeconds(1f);

        Log("Finding VR camera...");

        // Find the VR camera if needed
        if (findVRCameraAutomatically || cameraTransform == null)
        {
            FindVRCamera();
        }

        if (cameraTransform == null)
        {
            Debug.LogError("No camera found! Cannot continue initialization.");
            yield break;
        }

        Log($"Using camera at position {cameraTransform.position}, rotation {cameraTransform.rotation.eulerAngles}");

        // Position in front of the camera
        PositionRelativeToCamera();

        // Create debug sphere first to help locate in VR
        if (showDebugSphere)
        {
            CreateDebugSphere();
        }

        // Create direct VR camera attachment if enabled
        if (attachVisualizersToVRCamera)
        {
            CreateVRCameraAttachment();
        }

        // Create materials and main visualization
        CreateMaterials();
        GenerateCosmicStructure();

        // Mark as initialized
        isInitialized = true;

        Log($"Cosmos generated at position {transform.position} with scale {transform.localScale}");
    }

    private void FindVRCamera()
    {
        // Try finding by common VR camera names first
        string[] possibleVRCameraNames = new string[] {
            "CenterEyeAnchor",   // Oculus
            "Camera (eye)",      // SteamVR
            "XRRig/Camera",      // XR Toolkit
            "XR Origin/Camera Offset/Main Camera", // Universal XR
            "VRCamera",          // Generic VR
            "Main Camera",       // Fallback
            "Camera"             // Generic fallback
        };

        foreach (string name in possibleVRCameraNames)
        {
            GameObject cam = GameObject.Find(name);
            if (cam != null)
            {
                cameraTransform = cam.transform;
                Log($"Found VR camera: {name}");
                return;
            }
        }

        // Try active camera in scene
        Camera[] allCameras = FindObjectsOfType<Camera>();
        foreach (Camera cam in allCameras)
        {
            if (cam.isActiveAndEnabled)
            {
                cameraTransform = cam.transform;
                Log($"Using active camera: {cam.name}");
                return;
            }
        }

        // Last resort - main camera
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
            Log("Using Camera.main as fallback");
        }
        else
        {
            Debug.LogError("No cameras found in the scene!");
        }
    }

    private void CreateVRCameraAttachment()
    {
        // This is our guaranteed visibility approach - attach directly to camera
        vrAttachedVisualizer = new GameObject("VR_Attached_Visualizer");
        vrAttachedVisualizer.transform.SetParent(cameraTransform);
        vrAttachedVisualizer.transform.localPosition = new Vector3(0, 0, directAttachmentDistance);
        vrAttachedVisualizer.transform.localRotation = Quaternion.identity;

        // Create a miniature version of key elements

        // 1. Create a beacon sphere
        GameObject beacon = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        beacon.name = "VR_Beacon";
        beacon.transform.SetParent(vrAttachedVisualizer.transform);
        beacon.transform.localPosition = Vector3.zero;
        beacon.transform.localScale = Vector3.one * 0.05f;

        // Use VR-compatible material
        Material beaconMaterial = new Material(vrShader);
        beaconMaterial.color = debugSphereColor;
        beacon.GetComponent<Renderer>().material = beaconMaterial;

        // Add light to the beacon (if not on mobile VR)
        if (!isMobileVR)
        {
            GameObject beaconLight = new GameObject("BeaconLight");
            beaconLight.transform.SetParent(beacon.transform);
            beaconLight.transform.localPosition = Vector3.zero;

            Light bLight = beaconLight.AddComponent<Light>();
            bLight.type = LightType.Point;
            bLight.color = debugSphereColor;
            bLight.intensity = 1.5f;
            bLight.range = 5f;
        }

        // 2. Create a miniature tree for reference
        GameObject miniTree = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        miniTree.name = "MiniTree";
        miniTree.transform.SetParent(vrAttachedVisualizer.transform);
        miniTree.transform.localPosition = new Vector3(0, -0.05f, 0);
        miniTree.transform.localScale = new Vector3(0.01f, 0.1f, 0.01f);

        Material miniTreeMaterial = new Material(vrShader);
        miniTreeMaterial.color = new Color(0f, 0.8f, 0f);
        miniTree.GetComponent<Renderer>().material = miniTreeMaterial;

        // 3. Create a miniature jewel
        GameObject miniJewel = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        miniJewel.name = "MiniJewel";
        miniJewel.transform.SetParent(vrAttachedVisualizer.transform);
        miniJewel.transform.localPosition = new Vector3(0.03f, 0, 0);
        miniJewel.transform.localScale = Vector3.one * 0.02f;

        Material miniJewelMaterial = new Material(vrShader);
        miniJewelMaterial.color = new Color(1f, 0.8f, 0f);
        miniJewel.GetComponent<Renderer>().material = miniJewelMaterial;

        // Add text to help user locate main visualization
        GameObject textObj = new GameObject("LookHereText");
        textObj.transform.SetParent(vrAttachedVisualizer.transform);
        textObj.transform.localPosition = new Vector3(0, 0.05f, 0);
        textObj.transform.localRotation = Quaternion.identity;

        TextMesh textMesh = textObj.AddComponent<TextMesh>();
        textMesh.text = "LOOK HERE → Main visualization is nearby";
        textMesh.fontSize = 24;
        textMesh.characterSize = 0.005f;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = Color.yellow;

        Log("Created direct VR camera attachment for guaranteed visibility");
    }

    private void PositionRelativeToCamera()
    {
        if (cameraTransform == null)
        {
            Debug.LogError("No camera reference for positioning!");
            return;
        }

        // Get forward direction on horizontal plane
        Vector3 forward = cameraTransform.forward;
        forward.y = 0;

        // If forward is too close to zero after flattening, use world forward
        if (forward.magnitude < 0.1f)
        {
            forward = Vector3.forward;
        }
        else
        {
            forward.Normalize();
        }

        // Position in front of camera
        centerPosition = cameraTransform.position + forward * spawnDistance;
        centerPosition.y = cameraTransform.position.y + spawnHeightOffset;

        transform.position = centerPosition;

        Log($"Positioned at {transform.position}, forward={forward}");
    }

    private void CreateDebugSphere()
    {
        // Create a bright, visible sphere to help locate the structure in VR
        debugSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        debugSphere.name = "DebugSphere";
        debugSphere.transform.SetParent(transform, false);
        debugSphere.transform.localPosition = Vector3.zero;
        debugSphere.transform.localScale = Vector3.one * debugSphereSize;

        // Create VR-compatible bright material
        Material debugMaterial = new Material(vrShader);
        debugMaterial.color = debugSphereColor;
        debugSphere.GetComponent<Renderer>().material = debugMaterial;

        // Add a light only if not on mobile VR
        if (!isMobileVR)
        {
            GameObject debugLight = new GameObject("DebugLight");
            debugLight.transform.SetParent(debugSphere.transform, false);

            Light light = debugLight.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = debugSphereColor;
            light.intensity = 3f;
            light.range = 20f;
        }

        Log("Debug sphere created at cosmos center");
    }

    private void CreateMaterials()
    {
        for (int i = 0; i < 11; i++)
        {
            mountainMaterials[i] = new Material(vrShader);
            Color mountainColor = mountainColors[i];

            // Always use fully opaque for VR
            mountainColor.a = 1.0f;
            mountainMaterials[i].color = mountainColor;
        }

        // Tuba Tree material (green)
        treeMaterial = new Material(vrShader);
        treeMaterial.color = new Color(0f, 0.8f, 0f);

        // Night-Illuminating Jewel material (yellow)
        jewelMaterial = new Material(vrShader);
        jewelMaterial.color = new Color(1f, 0.8f, 0f);

        // Fountain of Life material
        fountainMaterial = new Material(vrShader);
        fountainMaterial.color = new Color(0f, 0.2f, 0.5f);

        // Water material - no transparency in VR
        waterMaterial = new Material(vrShader);
        waterMaterial.color = new Color(0f, 0.8f, 1f, 1.0f);

        // David's Armor material
        armorMaterial = new Material(vrShader);
        armorMaterial.color = Color.white;

        // Workshop materials
        for (int i = 0; i < 12; i++)
        {
            workshopMaterials[i] = new Material(vrShader);
            Color workshopColor = Color.Lerp(new Color(1f, 0f, 1f), new Color(0.5f, 0.5f, 1f), i / 11f);
            workshopMaterials[i].color = workshopColor;
        }
    }

    private void GenerateCosmicStructure()
    {
        // Adjust global scale
        transform.localScale = Vector3.one * worldScale;

        // Create parent container
        mountQafContainer = new GameObject("Mount Qaf");
        mountQafContainer.transform.position = Vector3.zero;
        mountQafContainer.transform.SetParent(transform, false);

        // Generate the 11 mountains
        GenerateMountQaf();

        // Generate Fountain of Life
        GenerateFountainOfLife();

        // Generate Tuba Tree
        GenerateTubaTree();

        // Generate Night-Illuminating Jewel
        GenerateNightIlluminatingJewel();

        // Generate Twelve Workshops
        GenerateTwelveWorkshops();

        // Generate David's Armor
        GenerateDavidsArmor();

        // Start animation coroutines
        StartCoroutine(AnimateJewel());
    }

    private void GenerateMountQaf()
    {
        for (int i = 0; i < 11; i++)
        {
            // Always use wireframe-style mountains for better VR performance
            if (i < 10)
            {
                mountainLayers[i] = new GameObject($"Mountain_{i + 1}");
                mountainLayers[i].transform.SetParent(mountQafContainer.transform, false);
                mountainLayers[i].transform.localPosition = Vector3.zero;

                CreateWireframeSphere(mountainLayers[i], mountainRadii[i], mountainMaterials[i]);
            }
            else
            {
                // Create sphere for the outermost mountain layer
                mountainLayers[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                mountainLayers[i].name = $"Mountain_{i + 1}";
                mountainLayers[i].transform.SetParent(mountQafContainer.transform, false);
                mountainLayers[i].transform.localPosition = Vector3.zero;
                mountainLayers[i].transform.localScale = Vector3.one * mountainRadii[i] * 2;

                // Apply material
                mountainLayers[i].GetComponent<Renderer>().material = mountainMaterials[i];

                // Reverse normals for inside visibility
                ReverseNormals(mountainLayers[i]);
            }

            // Add collider for interaction
            SphereCollider collider = mountainLayers[i].AddComponent<SphereCollider>();
            if (i < 10)
            {
                collider.radius = mountainRadii[i] / 2;
            }
            else
            {
                collider.radius = mountainRadii[i];
            }
            collider.isTrigger = true;

            // Add interaction component
            mountainLayers[i].AddComponent<InteractableElement>().elementType = ElementType.Mountain;
        }
    }

    private void CreateWireframeSphere(GameObject parent, float radius, Material material)
    {
        // Create a series of rings around the sphere for better VR visibility
        int ringsCount = isMobileVR ? 6 : 8; // Reduce complexity for mobile
        int segmentsPerRing = isMobileVR ? 16 : 24;

        for (int ring = 0; ring < ringsCount; ring++)
        {
            float ringAngle = ring * Mathf.PI / (ringsCount - 1);
            float ringRadius = radius * Mathf.Sin(ringAngle);
            float ringHeight = radius * Mathf.Cos(ringAngle);

            GameObject ringObj = new GameObject($"Ring_{ring}");
            ringObj.transform.SetParent(parent.transform, false);

            LineRenderer ringRenderer = ringObj.AddComponent<LineRenderer>();
            ringRenderer.material = material;
            ringRenderer.startWidth = 0.2f;
            ringRenderer.endWidth = 0.2f;
            ringRenderer.positionCount = segmentsPerRing + 1;
            ringRenderer.useWorldSpace = false;

            for (int i = 0; i <= segmentsPerRing; i++)
            {
                float segmentAngle = i * 2 * Mathf.PI / segmentsPerRing;
                float x = ringRadius * Mathf.Cos(segmentAngle);
                float z = ringRadius * Mathf.Sin(segmentAngle);
                ringRenderer.SetPosition(i, new Vector3(x, ringHeight, z));
            }
        }

        // Add vertical lines for better structure visibility
        int verticalLinesCount = isMobileVR ? 6 : 12;

        for (int v = 0; v < verticalLinesCount; v++)
        {
            float angle = v * 2 * Mathf.PI / verticalLinesCount;
            float x = Mathf.Cos(angle);
            float z = Mathf.Sin(angle);

            GameObject lineObj = new GameObject($"VerticalLine_{v}");
            lineObj.transform.SetParent(parent.transform, false);

            LineRenderer lineRenderer = lineObj.AddComponent<LineRenderer>();
            lineRenderer.material = material;
            lineRenderer.startWidth = 0.2f;
            lineRenderer.endWidth = 0.2f;
            lineRenderer.positionCount = ringsCount;
            lineRenderer.useWorldSpace = false;

            for (int ring = 0; ring < ringsCount; ring++)
            {
                float ringAngle = ring * Mathf.PI / (ringsCount - 1);
                float ringRadius = radius * Mathf.Sin(ringAngle);
                float ringHeight = radius * Mathf.Cos(ringAngle);

                lineRenderer.SetPosition(ring, new Vector3(x * ringRadius, ringHeight, z * ringRadius));
            }
        }
    }

    private void GenerateFountainOfLife()
    {
        // Create fountain container
        fountainOfLife = new GameObject("Fountain of Life");
        fountainOfLife.transform.SetParent(transform, false);
        fountainOfLife.transform.localPosition = Vector3.zero;

        // Create basin
        GameObject basin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        basin.name = "Basin";
        basin.transform.SetParent(fountainOfLife.transform, false);
        basin.transform.localPosition = new Vector3(0f, -2.5f, 0f);
        basin.transform.localScale = new Vector3(5f, 0.5f, 5f);
        basin.GetComponent<Renderer>().material = fountainMaterial;

        // Create water surface
        GameObject water = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        water.name = "Water";
        water.transform.SetParent(fountainOfLife.transform, false);
        water.transform.localPosition = new Vector3(0f, -2.2f, 0f);
        water.transform.localScale = new Vector3(4.8f, 0.1f, 4.8f);
        water.GetComponent<Renderer>().material = waterMaterial;

        // Add light if not on mobile VR or if we have space for lights
        if (!isMobileVR || activeLights.Count < maxLightsActive)
        {
            GameObject light = new GameObject("FountainLight");
            light.transform.SetParent(fountainOfLife.transform, false);
            light.transform.localPosition = new Vector3(0f, -2f, 0f);

            Light fountainLight = light.AddComponent<Light>();
            fountainLight.type = LightType.Point;
            fountainLight.color = new Color(0f, 0.8f, 1f);
            fountainLight.intensity = 3f;
            fountainLight.range = 15f;

            // Register light
            RegisterLight(fountainLight);
        }

        // Add interaction component
        fountainOfLife.AddComponent<InteractableElement>().elementType = ElementType.Fountain;

        // Add sphere collider for better interaction
        SphereCollider collider = fountainOfLife.AddComponent<SphereCollider>();
        collider.radius = 5f;
        collider.isTrigger = true;
    }

    private void GenerateTubaTree()
    {
        // Create tree container
        tubaTree = new GameObject("Tuba Tree");
        tubaTree.transform.SetParent(transform, false);
        tubaTree.transform.localPosition = Vector3.zero;

        // Create trunk
        GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.name = "Trunk";
        trunk.transform.SetParent(tubaTree.transform, false);
        trunk.transform.localPosition = new Vector3(0f, 0f, 0f);
        trunk.transform.localScale = new Vector3(1.2f, 20f, 1.2f);
        trunk.GetComponent<Renderer>().material = treeMaterial;

        // Create branches - fewer for mobile VR
        int branchCount = isMobileVR ? 4 : 8;
        for (int i = 0; i < branchCount; i++)
        {
            float y = -8f + i * (16f / branchCount);
            float angle = i * (360f / branchCount);

            GameObject branch = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            branch.name = $"Branch_{i}";
            branch.transform.SetParent(tubaTree.transform, false);
            branch.transform.localPosition = new Vector3(0f, y, 0f);
            branch.transform.localScale = new Vector3(0.5f, 3f, 0.5f);
            branch.transform.rotation = Quaternion.Euler(0f, angle, 90f);
            branch.GetComponent<Renderer>().material = treeMaterial;
        }

        // Create foliage
        for (int i = 0; i < 3; i++)
        {
            float y = 4f + i * 3f;

            GameObject foliage = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            foliage.name = $"Foliage_{i}";
            foliage.transform.SetParent(tubaTree.transform, false);
            foliage.transform.localPosition = new Vector3(0f, y, 0f);
            foliage.transform.localScale = new Vector3(5f - i * 0.6f, 4f - i * 0.4f, 5f - i * 0.6f);
            foliage.GetComponent<Renderer>().material = treeMaterial;
        }

        // Add light if not on mobile VR or if we have space for lights
        if (!isMobileVR || activeLights.Count < maxLightsActive)
        {
            GameObject treeLight = new GameObject("TreeLight");
            treeLight.transform.SetParent(tubaTree.transform, false);
            treeLight.transform.localPosition = new Vector3(0f, 6f, 0f);

            Light light = treeLight.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0f, 1f, 0.2f);
            light.intensity = 3f;
            light.range = 25f;

            // Register light
            RegisterLight(light);
        }

        // Add interaction component
        tubaTree.AddComponent<InteractableElement>().elementType = ElementType.Tree;

        // Add cylinder collider for better interaction
        CapsuleCollider treeCollider = tubaTree.AddComponent<CapsuleCollider>();
        treeCollider.height = 20f;
        treeCollider.radius = 1.5f;
        treeCollider.isTrigger = true;
    }

    private void GenerateNightIlluminatingJewel()
    {
        // Create main jewel container
        nightIlluminatingJewel = new GameObject("Night-Illuminating Jewel");
        nightIlluminatingJewel.transform.SetParent(transform, false);
        nightIlluminatingJewel.transform.localPosition = new Vector3(0f, 0f, 0f);

        // Create the 4 phases as simplified spheres for better VR rendering

        // Simplified for VR - just use different colors instead of multiple materials
        for (int i = 0; i < 4; i++)
        {
            jewelPhases[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            jewelPhases[i].name = $"Jewel_Phase_{i}";
            jewelPhases[i].transform.SetParent(nightIlluminatingJewel.transform, false);
            jewelPhases[i].transform.localPosition = Vector3.zero;
            jewelPhases[i].transform.localScale = Vector3.one * 2.5f;

            Material phaseMaterial = new Material(vrShader);

            // Different colors based on phase
            Color phaseColor;
            switch (i)
            {
                case 0: phaseColor = new Color(1f, 0.8f, 0f); break; // Full Light
                case 1: phaseColor = new Color(0.8f, 0.6f, 0f); break; // Half Dark
                case 2: phaseColor = new Color(0.6f, 0.4f, 0f); break; // Mostly Dark
                case 3: phaseColor = new Color(0.3f, 0.3f, 0.4f); break; // Full Dark
                default: phaseColor = Color.white; break;
            }

            phaseMaterial.color = phaseColor;
            jewelPhases[i].GetComponent<Renderer>().material = phaseMaterial;
            jewelPhases[i].SetActive(i == 0); // Initially only first phase visible
        }

        // Add light if not on mobile VR or if we have space for lights
        if (!isMobileVR || activeLights.Count < maxLightsActive)
        {
            GameObject jewelLight = new GameObject("JewelLight");
            jewelLight.transform.SetParent(nightIlluminatingJewel.transform, false);

            Light light = jewelLight.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.8f, 0f);
            light.intensity = 3f;
            light.range = 15f;

            // Register light
            RegisterLight(light);
        }

        // Add interaction component
        nightIlluminatingJewel.AddComponent<InteractableElement>().elementType = ElementType.Jewel;

        // Add sphere collider for better interaction
        SphereCollider jewelCollider = nightIlluminatingJewel.AddComponent<SphereCollider>();
        jewelCollider.radius = 2.5f;
        jewelCollider.isTrigger = true;
    }

    private void GenerateTwelveWorkshops()
    {
        // Generate 12 workshops in a circular arrangement
        float radius = 20f;

        // For mobile VR, create fewer workshops for performance
        int workshopCount = isMobileVR ? 6 : 12;

        for (int i = 0; i < workshopCount; i++)
        {
            float angle = i * Mathf.PI * 2f / workshopCount;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            float y = (i % 2 == 0) ? 4f : -4f;

            GameObject workshop = new GameObject($"Workshop_{i + 1}");
            workshop.transform.SetParent(transform, false);
            workshop.transform.localPosition = new Vector3(x, y, z);

            // Visual representation
            GameObject workshopVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            workshopVisual.name = "WorkshopVisual";
            workshopVisual.transform.SetParent(workshop.transform, false);
            workshopVisual.transform.localPosition = Vector3.zero;
            workshopVisual.transform.localScale = Vector3.one * 2.5f;
            workshopVisual.GetComponent<Renderer>().material = workshopMaterials[i % 12];

            // Add light only for a few workshops
            if (i < maxLightsActive && !isMobileVR)
            {
                GameObject workshopLight = new GameObject("WorkshopLight");
                workshopLight.transform.SetParent(workshop.transform, false);

                Light light = workshopLight.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = workshopMaterials[i % 12].color;
                light.intensity = 1.5f;
                light.range = 8f;

                // Register light
                RegisterLight(light);
            }

            // Add to array if within bounds
            if (i < 12)
            {
                twelveWorkshops[i] = workshop;
            }

            // Add interaction component
            workshop.AddComponent<InteractableElement>().elementType = ElementType.Workshop;

            // Add box collider for better interaction
            BoxCollider workshopCollider = workshop.AddComponent<BoxCollider>();
            workshopCollider.size = Vector3.one * 2.5f;
            workshopCollider.isTrigger = true;
        }

        // For mobile VR, disable the empty workshop slots
        if (isMobileVR)
        {
            for (int i = workshopCount; i < 12; i++)
            {
                twelveWorkshops[i] = null;
            }
        }
    }

    private void GenerateDavidsArmor()
    {
        // Create David's Armor (network of lines)
        davidsArmor = new GameObject("David's Armor");
        davidsArmor.transform.SetParent(transform, false);
        davidsArmor.transform.localPosition = Vector3.zero;

        // Create lattice pattern using multiple line renderers
        int lineCount = isMobileVR ? 12 : 24;

        // Vertical lines
        GameObject verticalLines = new GameObject("VerticalLines");
        verticalLines.transform.SetParent(davidsArmor.transform, false);

        LineRenderer verticalRenderer = verticalLines.AddComponent<LineRenderer>();
        verticalRenderer.material = armorMaterial;
        verticalRenderer.startWidth = 0.2f;
        verticalRenderer.endWidth = 0.2f;
        verticalRenderer.positionCount = lineCount * 2;

        float radius = 10f;
        float height = 20f;

        for (int i = 0; i < lineCount; i++)
        {
            float angle = i * Mathf.PI * 2f / lineCount;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;

            verticalRenderer.SetPosition(i * 2, new Vector3(x, height / 2, z));
            verticalRenderer.SetPosition(i * 2 + 1, new Vector3(x, -height / 2, z));
        }

        // Horizontal rings - fewer for mobile VR
        int ringCount = isMobileVR ? 3 : 5;
        for (int ring = 0; ring < ringCount; ring++)
        {
            GameObject horizontalRing = new GameObject($"HorizontalRing_{ring}");
            horizontalRing.transform.SetParent(davidsArmor.transform, false);

            LineRenderer ringRenderer = horizontalRing.AddComponent<LineRenderer>();
            ringRenderer.material = armorMaterial;
            ringRenderer.startWidth = 0.2f;
            ringRenderer.endWidth = 0.2f;
            ringRenderer.positionCount = lineCount + 1;

            float ringY = -height / 2 + height * ring / (ringCount - 1);

            for (int i = 0; i <= lineCount; i++)
            {
                float angle = i * Mathf.PI * 2f / lineCount;
                float x = Mathf.Cos(angle) * radius;
                float z = Mathf.Sin(angle) * radius;

                ringRenderer.SetPosition(i, new Vector3(x, ringY, z));
            }
        }

        // Add interaction component
        davidsArmor.AddComponent<InteractableElement>().elementType = ElementType.DavidsArmor;
    }

    // Animation coroutines
    private IEnumerator AnimateJewel()
    {
        while (true)
        {
            // Orbit around the tree
            jewelRotationTime += Time.deltaTime * 0.2f;
            float radius = 10f;
            float x = Mathf.Cos(jewelRotationTime) * radius;
            float z = Mathf.Sin(jewelRotationTime) * radius;

            // Position the jewel
            if (nightIlluminatingJewel != null)
            {
                nightIlluminatingJewel.transform.localPosition = new Vector3(x, 5f, z);

                // Update the jewel phase based on position
                UpdateJewelPhase();
            }

            yield return null;
        }
    }

    private void UpdateJewelPhase()
    {
        if (!isInitialized || nightIlluminatingJewel == null) return;

        // Calculate angle between jewel, origin and positive x-axis
        float angle = Mathf.Atan2(nightIlluminatingJewel.transform.localPosition.z, nightIlluminatingJewel.transform.localPosition.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;

        // Determine phase based on angle (0-3, dividing the circle into quarters)
        int newPhase = Mathf.FloorToInt(angle / 90f) % 4;

        // Update active phase if needed
        if (newPhase != currentJewelPhase)
        {
            // Deactivate current phase
            if (jewelPhases[currentJewelPhase] != null)
                jewelPhases[currentJewelPhase].SetActive(false);

            // Activate new phase
            currentJewelPhase = newPhase;
            if (jewelPhases[currentJewelPhase] != null)
                jewelPhases[currentJewelPhase].SetActive(true);

            // Update light intensity based on phase
            Light light = nightIlluminatingJewel.GetComponentInChildren<Light>();
            if (light != null)
            {
                // Phase 0 - brightest, Phase 3 - darkest
                float intensity = 3f;
                switch (currentJewelPhase)
                {
                    case 0: intensity = 3f; break;
                    case 1: intensity = 2f; break;
                    case 2: intensity = 1f; break;
                    case 3: intensity = 0.5f; break;
                }
                light.intensity = intensity;
            }
        }

        // Orient the jewel to face the tree
        nightIlluminatingJewel.transform.LookAt(transform.position);
    }

    private void RegisterLight(Light light)
    {
        // Keep track of lights for potential performance management
        activeLights.Add(light);

        // If we have too many lights, dim or disable the oldest ones
        if (maxLightsActive > 0 && activeLights.Count > maxLightsActive)
        {
            for (int i = 0; i < activeLights.Count - maxLightsActive; i++)
            {
                if (activeLights[i] != null)
                {
                    // For VR, just deactivate excess lights instead of dimming
                    activeLights[i].gameObject.SetActive(false);
                }
            }
        }
    }

    private void ReverseNormals(GameObject obj)
    {
        // Reverse normals to make the sphere visible from inside
        MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
        if (meshFilter == null || meshFilter.mesh == null) return;

        Mesh mesh = meshFilter.mesh;
        Vector3[] normals = mesh.normals;
        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = -normals[i];
        }
        mesh.normals = normals;

        // Reverse triangle order
        int[] triangles = mesh.triangles;
        for (int i = 0; i < triangles.Length; i += 3)
        {
            int temp = triangles[i];
            triangles[i] = triangles[i + 2];
            triangles[i + 2] = temp;
        }
        mesh.triangles = triangles;
    }

    private void Update()
    {
        // Reposition if tracking camera is enabled
        if (trackCameraPosition && cameraTransform != null && isInitialized)
        {
            // Only update position occasionally to avoid constant movement
            if (Time.frameCount % 60 == 0)
            {
                PositionRelativeToCamera();
            }
        }

        // Position text panel to face camera
        if (textPanel != null && textPanel.activeSelf && cameraTransform != null)
        {
            // Position in front of camera
            Vector3 targetPosition = cameraTransform.position + cameraTransform.forward * 2f;
            targetPosition.y = cameraTransform.position.y;

            // Set position and rotation
            textPanel.transform.position = targetPosition;
            textPanel.transform.rotation = Quaternion.LookRotation(textPanel.transform.position - cameraTransform.position);
        }

        // Handle interaction based on mouse clicks (also works in VR with gaze)
        ProcessInteraction();
    }

    private void ProcessInteraction()
    {
        // Process input - this works with both mouse in editor and VR raycasting
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f))
            {
                // Look for interactable in hit object or its parents
                InteractableElement element = hit.collider.GetComponentInParent<InteractableElement>();
                if (element != null)
                {
                    HandleInteraction(element);
                }
            }
        }
    }

    private void HandleInteraction(InteractableElement element)
    {
        // Show text panel with information
        if (textPanel != null && interactionText != null)
        {
            textPanel.SetActive(true);

            switch (element.elementType)
            {
                case ElementType.Mountain:
                    int mountainIndex = 0;
                    for (int i = 0; i < mountainLayers.Length; i++)
                    {
                        if (mountainLayers[i] == element.gameObject)
                        {
                            mountainIndex = i;
                            break;
                        }
                    }

                    interactionText.text = $"Mountain {mountainIndex + 1} - {GetMountainName(mountainIndex)}";
                    break;

                case ElementType.Fountain:
                    interactionText.text = "The Fountain of Life (چشمه‌ی زندگانی) - hidden in darkness, it grants the ability to traverse Mount Qaf";
                    break;

                case ElementType.Tree:
                    interactionText.text = "The Tuba Tree (درخت طوبی) - the cosmic axis from which all fruits and plants emanate";
                    break;

                case ElementType.Jewel:
                    interactionText.text = "The Night-Illuminating Jewel (گوهر شب افروز) - its light comes from the Tuba Tree";
                    break;

                case ElementType.Workshop:
                    interactionText.text = "One of the Twelve Workshops (دوازده کارگاه) - where cosmic realities are woven";
                    break;

                case ElementType.DavidsArmor:
                    interactionText.text = "David's Armor (زره داوودی) - the bonds that constrain souls in the material world";
                    break;
            }

            // Auto-hide after delay
            StartCoroutine(HideTextAfterDelay(5f));
        }
    }

    private string GetMountainName(int index)
    {
        switch (index)
        {
            case 0: return "Black/Deep Indigo - Fountain of Life";
            case 1: return "Deep Violet";
            case 2: return "Red - Domain of Red Intellect";
            case 3: return "Orange";
            case 4: return "Yellow";
            case 5: return "Yellow-Green";
            case 6: return "Green";
            case 7: return "Blue-Green";
            case 8: return "Blue";
            case 9: return "Indigo";
            case 10: return "Violet - Mystical Knowledge";
            default: return "";
        }
    }

    private IEnumerator HideTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (textPanel != null)
        {
            textPanel.SetActive(false);
        }
    }

    private void Log(string message)
    {
        if (logPositioningInfo)
        {
            Debug.Log($"SuhrawardiVRCosmology: {message}");
        }
    }
}

// Element types based on the cosmic structure
public enum ElementType
{
    Mountain,
    Fountain,
    Tree,
    Jewel,
    Workshop,
    DavidsArmor
}

// Component to make objects interactable
public class InteractableElement : MonoBehaviour
{
    public ElementType elementType;
}