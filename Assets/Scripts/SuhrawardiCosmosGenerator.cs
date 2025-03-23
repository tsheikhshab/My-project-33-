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

    [Header("Illuminationist Correspondences")]
    [SerializeField] private bool showCorrespondences = true;
    [SerializeField] private float correspondenceIntensity = 1.0f;
    [SerializeField] private Color correspondenceColor = new Color(1f, 0.9f, 0.5f);
    [SerializeField] private float pulseSpeed = 0.5f;
    [SerializeField] private bool useSharedEssenceMaterial = true;

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

    // Shared materials for showing essential unity
    private Material sharedEssenceMaterial;
    private List<LineRenderer> correspondenceLines = new List<LineRenderer>();
    private GameObject correspondencesContainer;

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

    // Add new class members to SuhrawardiVRCosmology class after the existing state variables
    private List<GameObject> allCosmicElements = new List<GameObject>();
    private List<GameObject> emanatingElements = new List<GameObject>();
    private float maxRadius = 40f; // Maximum radius of cosmic structure
    private List<ElementWaveConnection> allWaveConnections = new List<ElementWaveConnection>();
    private Shader illuminationShader;
    private Shader unifiedFieldShader;
    private Shader threadShader;
    private Shader instantPresenceShader;

    // Add new field to the SuhrawardiVRCosmology class after other private variables
    private List<Transform> allBranches = new List<Transform>();
    private Vector3 cosmicEastDirection = Vector3.right; // Default east direction

    // Inside SuhrawardiVRCosmology class, add this field near other component references
    private CosmicRelationshipManager relationshipManager;

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

        // Add cosmic relationship manager to manage containment and connections
        if (relationshipManager == null)
        {
            GameObject managerObj = new GameObject("CosmicRelationshipManager");
            managerObj.transform.SetParent(transform);
            relationshipManager = managerObj.AddComponent<CosmicRelationshipManager>();
        }

        // Ensure the manager has a reference to this cosmology
        relationshipManager.cosmologyInstance = this;

        // After generating the cosmic structure, apply the constraints
        yield return new WaitForSeconds(0.5f);
        relationshipManager.ApplyCosmicConstraints();

        // Create debug visualization if needed
        if (Debug.isDebugBuild || Application.isEditor)
        {
            CreateRootBoundaryDebugger();
            Debug.Log("Debug build detected: Created root boundary debugger");
        }
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

        // Create unified essence material
        CreateUnifiedEssenceMaterial();
    }

    private void CreateUnifiedEssenceMaterial()
    {
        if (!useSharedEssenceMaterial) return;

        // Create a shared material representing the unified essence of light
        // This is the "nūr" (نور) that Suhrawardi describes as fundamental reality
        sharedEssenceMaterial = new Material(vrShader);
        sharedEssenceMaterial.color = new Color(1f, 0.95f, 0.8f, 0.9f);

        // In a non-VR environment, we could use more advanced shader effects
        // But for VR compatibility, we'll keep it simple
        if (!isMobileVR && !forceOpaqueRendering)
        {
            sharedEssenceMaterial.EnableKeyword("_EMISSION");
            sharedEssenceMaterial.SetColor("_EmissionColor", new Color(1f, 0.95f, 0.8f) * 0.8f);
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

        // Generate Tuba Tree (now includes David's Armor as part of unified structure)
        GenerateTubaTree();

        // Generate Night-Illuminating Jewel
        GenerateNightIlluminatingJewel();

        // Generate Twelve Workshops
        GenerateTwelveWorkshops();

        // Generate Seven Lower Workshops
        GenerateSevenLowerWorkshops();

        // Note: David's Armor is now generated as part of the tree, so we don't call it separately

        // Add the illuminationist correspondences
        CreateCorrespondences();

        // Add wave physics visualizations
        ImplementTreeFountainWaveDynamics();
        ImplementInterferencePatterns();
        ImplementStandingWaves();

        // Collect all cosmic elements for unified effects
        CollectCosmicElements();

        // Create unified illuminationist effects
        CreateUnifiedLightGradient();
        CreateIlluminationRays();
        CreateUnifiedField();
        CreateWaveDynamicsBetweenElements();
        CreateInstantaneousPresenceEffects();

        // Start animation coroutines
        StartCoroutine(AnimateJewel());
        StartCoroutine(AnimateIshraqiElements());

        // Apply illuminationist positioning principles
        ApplyIlluminationistPrinciples();

        // Apply cosmic containment and relationship constraints after all elements are created
        if (relationshipManager != null)
        {
            StartCoroutine(ApplyCosmicRelationshipsAfterDelay(1.0f));
        }
    }

    private void ImplementInterferencePatterns()
    {
        // Create a plane between Tree and Fountain where interference occurs
        GameObject interferenceField = GameObject.CreatePrimitive(PrimitiveType.Plane);
        interferenceField.name = "InterferenceField";
        interferenceField.transform.SetParent(transform, false);

        // Position in the intermediate space
        Vector3 midpoint = (tubaTree.transform.position + fountainOfLife.transform.position) / 2f;
        interferenceField.transform.position = midpoint;
        interferenceField.transform.localScale = new Vector3(5f, 1f, 5f);

        // Create special material for interference visualization
        Shader interferenceShader = Shader.Find("Unlit/Texture"); // Simple for VR
        if (interferenceShader == null)
            interferenceShader = vrShader; // Fall back to our VR-compatible shader

        Material interferenceMaterial = new Material(interferenceShader);

        // Create texture that will be updated in real-time
        Texture2D interferenceTexture = new Texture2D(128, 128, TextureFormat.RGBA32, false);
        interferenceMaterial.mainTexture = interferenceTexture;

        // Apply material
        interferenceField.GetComponent<Renderer>().material = interferenceMaterial;

        // Add component to update texture based on wave interactions
        interferenceField.AddComponent<InterferencePatternGenerator>().Initialize(
            tubaTree, fountainOfLife, interferenceTexture);

        // Add interaction component
        var interaction = interferenceField.AddComponent<InteractableElement>();
        interaction.elementType = ElementType.Field; // Reuse Field type
    }

    private void ImplementStandingWaves()
    {
        // Create visualizations of nodes and antinodes between tree and fountain
        GameObject standingWaveSystem = new GameObject("StandingWaveSystem");
        standingWaveSystem.transform.SetParent(transform, false);

        // Position between tree and fountain
        Vector3 direction = fountainOfLife.transform.position - tubaTree.transform.position;
        float distance = direction.magnitude;
        Vector3 center = tubaTree.transform.position + direction * 0.5f;

        standingWaveSystem.transform.position = center;

        // Add component to generate standing waves
        StandingWaveGenerator waveGen = standingWaveSystem.AddComponent<StandingWaveGenerator>();
        waveGen.Initialize(tubaTree, fountainOfLife, 7); // 7 nodes, a sacred number in Suhrawardi's system
    }

    private void GenerateMountQaf()
    {
        mountQafContainer = new GameObject("MountQaf");
        mountQafContainer.transform.SetParent(transform);

        for (int i = 0; i < 11; i++)
        {
            GameObject mountain = new GameObject($"Mountain_{i}");
            mountain.transform.SetParent(mountQafContainer.transform);
            
            // Create wireframe structure instead of solid sphere
            CreateWireframeSphere(mountain, mountainRadii[i], mountainMaterials[i]);
        }
    }

    private void CreateWireframeSphere(GameObject parent, float radius, Material material)
    {
        // Create latitude/longitude lines
        int rings = 12;
        int segments = 24;
        
        // Create horizontal rings
        for (int r = 0; r < rings; r++) {
            float y = radius * Mathf.Cos(r * Mathf.PI / (rings-1));
            float ringRadius = radius * Mathf.Sin(r * Mathf.PI / (rings-1));
            
            GameObject ring = new GameObject($"Ring_{r}");
            ring.transform.SetParent(parent.transform);
            
            LineRenderer lineRenderer = ring.AddComponent<LineRenderer>();
            lineRenderer.material = material;
            lineRenderer.startWidth = 0.2f;  // Thicker for VR visibility
            lineRenderer.endWidth = 0.2f;
            lineRenderer.positionCount = segments + 1;
            lineRenderer.useWorldSpace = false;
            
            for (int s = 0; s <= segments; s++) {
                float angle = s * Mathf.PI * 2 / segments;
                float x = ringRadius * Mathf.Cos(angle);
                float z = ringRadius * Mathf.Sin(angle);
                lineRenderer.SetPosition(s, new Vector3(x, y, z));
            }
        }
        
        // Create vertical lines
        for (int s = 0; s < segments; s++) {
            float angle = s * Mathf.PI * 2 / segments;
            
            GameObject line = new GameObject($"Line_{s}");
            line.transform.SetParent(parent.transform);
            
            LineRenderer lineRenderer = line.AddComponent<LineRenderer>();
            lineRenderer.material = material;
            lineRenderer.startWidth = 0.2f;
            lineRenderer.endWidth = 0.2f;
            lineRenderer.positionCount = rings;
            lineRenderer.useWorldSpace = false;
            
            for (int r = 0; r < rings; r++) {
                float y = radius * Mathf.Cos(r * Mathf.PI / (rings-1));
                float ringRadius = radius * Mathf.Sin(r * Mathf.PI / (rings-1));
                float x = ringRadius * Mathf.Cos(angle);
                float z = ringRadius * Mathf.Sin(angle);
                lineRenderer.SetPosition(r, new Vector3(x, y, z));
            }
        }
    }

    private void GenerateFountainOfLife()
    {
        fountainOfLife = new GameObject("FountainOfLife");
        fountainOfLife.transform.SetParent(transform);
        fountainOfLife.transform.localPosition = new Vector3(0, -2, 5);

        // Create basin structure with line renderers
        GameObject basin = new GameObject("Basin");
        basin.transform.SetParent(fountainOfLife.transform);

        // Create concentric rings
        int ringCount = 5;
        for (int i = 0; i < ringCount; i++)
        {
            GameObject ring = new GameObject($"Ring_{i}");
            ring.transform.SetParent(basin.transform);

            float radius = 3f - (i * 0.3f);
            float height = -0.5f + (i * 0.15f);

            LineRenderer lineRenderer = ring.AddComponent<LineRenderer>();
            lineRenderer.material = fountainMaterial;
            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.1f;

            int segments = 30;
            lineRenderer.positionCount = segments + 1;
            lineRenderer.useWorldSpace = false;

            for (int j = 0; j <= segments; j++)
            {
                float angle = j * Mathf.PI * 2 / segments;
                float x = radius * Mathf.Cos(angle);
                float z = radius * Mathf.Sin(angle);
                lineRenderer.SetPosition(j, new Vector3(x, height, z));
            }
        }
    }

    private void GenerateTubaTree()
    {
        tubaTree = new GameObject("TubaTree");
        tubaTree.transform.SetParent(transform);

        // Create trunk
        GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.transform.SetParent(tubaTree.transform);
        trunk.transform.localPosition = Vector3.zero;
        trunk.transform.localScale = new Vector3(1f, 15f, 1f);
        trunk.GetComponent<Renderer>().material = treeMaterial;

        // Create branches using line renderers
        CreateBranches(tubaTree.transform, 10);

        // Create foliage using wireframe
        GameObject foliage = new GameObject("Foliage");
        foliage.transform.SetParent(tubaTree.transform);
        foliage.transform.localPosition = new Vector3(0, 8, 0);
        
        CreateWireframeSphere(foliage, 4, treeMaterial);
    }

    private void CreateBranches(Transform treeTransform, int branchCount)
    {
        GameObject branches = new GameObject("Branches");
        branches.transform.SetParent(treeTransform);

        for (int i = 0; i < branchCount; i++)
        {
            GameObject branch = new GameObject($"Branch_{i}");
            branch.transform.SetParent(branches.transform);

            LineRenderer lineRenderer = branch.AddComponent<LineRenderer>();
            lineRenderer.material = treeMaterial;
            lineRenderer.startWidth = 0.2f;
            lineRenderer.endWidth = 0.05f;

            // Create branch path
            int points = 10;
            lineRenderer.positionCount = points;

            float angle = i * Mathf.PI * 2 / branchCount;
            float height = 3 + i % 5;

            for (int p = 0; p < points; p++)
            {
                float t = p / (float)(points - 1);
                float length = t * 6f;
                float curve = Mathf.Sin(t * Mathf.PI) * 2f;

                Vector3 pos = new Vector3(
                    Mathf.Cos(angle) * length,
                    height + curve,
                    Mathf.Sin(angle) * length
                );

                lineRenderer.SetPosition(p, pos);
            }
        }
    }

    private void GenerateUnifiedTreeArmorStructure()
    {
        // Create David's Armor as an extension of the tree
        davidsArmor = new GameObject("UnifiedTreeArmor");
        davidsArmor.transform.SetParent(tubaTree.transform, false);
        davidsArmor.transform.localPosition = Vector3.zero;

        // Create materials for gradient effect from tree to armor
        Material[] gradientMaterials = new Material[10];
        for (int i = 0; i < 10; i++)
        {
            gradientMaterials[i] = new Material(vrShader);
            // Gradient from green (tree) to white (armor)
            float t = i / 9f;
            Color gradientColor = Color.Lerp(treeMaterial.color, armorMaterial.color, t);
            gradientMaterials[i].color = gradientColor;
        }

        // Create variably dense radial branches that transform into the armor lattice
        int radialBranchCount = isMobileVR ? 8 : 16;
        float outerRadius = 10f;
        float height = 20f;

        // Create container for branches
        GameObject branchesContainer = new GameObject("RadialBranches");
        branchesContainer.transform.SetParent(davidsArmor.transform, false);

        // Create the variable-density pattern - some areas more dense than others
        float[] densityFactors = new float[radialBranchCount];
        for (int i = 0; i < radialBranchCount; i++)
        {
            // Create a pattern of varying density
            float angle = i * Mathf.PI * 2f / radialBranchCount;
            densityFactors[i] = 0.5f + 0.5f * Mathf.Sin(angle * 3f); // Varies from 0-1
        }

        // Create the radial branches that transform into lattice structure
        for (int i = 0; i < radialBranchCount; i++)
        {
            float angle = i * Mathf.PI * 2f / radialBranchCount;
            float x = Mathf.Cos(angle);
            float z = Mathf.Sin(angle);

            // Create a branching structure that extends outward
            GameObject branch = new GameObject($"Branch_{i}");
            branch.transform.SetParent(branchesContainer.transform, false);

            // Create the main branch as a line renderer
            LineRenderer mainBranch = branch.AddComponent<LineRenderer>();
            mainBranch.material = gradientMaterials[0]; // Start with tree material

            // Branch width varies with density factor - denser in some directions
            float width = 0.3f * (0.5f + densityFactors[i]);
            mainBranch.startWidth = width;
            mainBranch.endWidth = width * 0.5f; // Thinner at the ends

            // Set number of segments based on density - more segments for denser areas
            int segmentCount = Mathf.Max(6, Mathf.FloorToInt(10 * densityFactors[i]));
            mainBranch.positionCount = segmentCount;

            // Create curved path from tree trunk outward
            for (int j = 0; j < segmentCount; j++)
            {
                float t = j / (float)(segmentCount - 1);

                // Position - start at trunk and curve outward
                float curveFactor = t * t; // Quadratic curve
                float radius = outerRadius * curveFactor;
                float branchX = x * radius;
                float branchZ = z * radius;

                // Height varies - start at different heights on trunk, end at different heights
                float startY = -5f + i % 5 * 3f; // Vary starting height
                float endY = -6f + Mathf.Sin(angle * 4f) * 12f; // Vary ending height
                float branchY = Mathf.Lerp(startY, endY, t);

                // Add some organic variation
                float waveFactor = Mathf.Sin(t * 5f + i) * densityFactors[i] * 2f;
                branchX += waveFactor * 0.5f;
                branchZ += waveFactor * 0.5f;

                // Set position
                mainBranch.SetPosition(j, new Vector3(branchX, branchY, branchZ));

                // Set material gradient
                if (j > 0 && j < segmentCount - 1)
                {
                    // Create progressive gradient along the branch
                    int materialIndex = Mathf.Min(9, Mathf.FloorToInt(t * 10));
                    mainBranch.materials = new Material[] { gradientMaterials[materialIndex] };
                }
            }

            // Add sub-branches for more complex structure in denser areas
            if (densityFactors[i] > 0.6f && !isMobileVR)
            {
                int subBranchCount = Mathf.FloorToInt(densityFactors[i] * 6);

                for (int s = 0; s < subBranchCount; s++)
                {
                    // Create sub-branch
                    GameObject subBranch = new GameObject($"SubBranch_{i}_{s}");
                    subBranch.transform.SetParent(branch.transform, false);

                    LineRenderer subBranchRenderer = subBranch.AddComponent<LineRenderer>();
                    int subMaterialIndex = Mathf.Min(9, 3 + s);
                    subBranchRenderer.material = gradientMaterials[subMaterialIndex];
                    subBranchRenderer.startWidth = width * 0.6f;
                    subBranchRenderer.endWidth = width * 0.3f;

                    // Create shorter sub-branches
                    int subSegments = 4;
                    subBranchRenderer.positionCount = subSegments;

                    // Choose a point along the main branch to start from
                    float startT = 0.3f + s * 0.15f;
                    int startSegmentIndex = Mathf.FloorToInt(startT * segmentCount);
                    if (startSegmentIndex >= segmentCount) startSegmentIndex = segmentCount - 1;

                    Vector3 startPoint = mainBranch.GetPosition(startSegmentIndex);

                    // Create path for sub-branch
                    for (int p = 0; p < subSegments; p++)
                    {
                        float subT = p / (float)(subSegments - 1);

                        // Direction varies based on position
                        float subAngle = angle + Mathf.PI * 0.25f * (s % 3 - 1) + subT * 0.5f;
                        float subX = Mathf.Cos(subAngle) * 2f * subT;
                        float subZ = Mathf.Sin(subAngle) * 2f * subT;
                        float subY = (s % 2 == 0) ? 1f : -1f;

                        // Position relative to start point
                        Vector3 offset = new Vector3(subX, subY * subT, subZ);
                        subBranchRenderer.SetPosition(p, startPoint + offset);
                    }
                }
            }
        }

        // Create connecting horizontal rings to complete the armor structure
        int ringCount = isMobileVR ? 4 : 7;
        float ringSpacing = height / (ringCount + 1);

        GameObject ringsContainer = new GameObject("HorizontalRings");
        ringsContainer.transform.SetParent(davidsArmor.transform, false);

        for (int r = 0; r < ringCount; r++)
        {
            float ringY = -height / 2 + (r + 1) * ringSpacing;

            GameObject ring = new GameObject($"Ring_{r}");
            ring.transform.SetParent(ringsContainer.transform, false);

            LineRenderer ringRenderer = ring.AddComponent<LineRenderer>();

            // Use gradient materials - more tree-like near center, more armor-like at edges
            float heightFactor = Mathf.Abs(ringY) / (height / 2); // 0 at center, 1 at edges
            int materialIndex = Mathf.Min(9, Mathf.FloorToInt(heightFactor * 10));
            ringRenderer.material = gradientMaterials[materialIndex];

            // Width varies by position and density
            float ringWidth = 0.2f * (1f - heightFactor * 0.5f);
            ringRenderer.startWidth = ringWidth;
            ringRenderer.endWidth = ringWidth;

            // More points for complexity
            int ringSegments = isMobileVR ? 24 : 36;
            ringRenderer.positionCount = ringSegments + 1;

            // Create ring with variable radius - not perfectly circular
            for (int p = 0; p <= ringSegments; p++)
            {
                float circleAngle = p * Mathf.PI * 2f / ringSegments;

                // Variable radius based on angle and position
                float variableFactor = 1f + 0.2f * Mathf.Sin(circleAngle * 3f + r * 1.5f);
                float ringRadius = outerRadius * variableFactor * (0.7f + heightFactor * 0.3f);

                float ringX = Mathf.Cos(circleAngle) * ringRadius;
                float ringZ = Mathf.Sin(circleAngle) * ringRadius;

                // Add some vertical variation
                float ringYOffset = Mathf.Sin(circleAngle * 5f) * (1f - heightFactor) * 1.5f;

                ringRenderer.SetPosition(p, new Vector3(ringX, ringY + ringYOffset, ringZ));
            }
        }

        // Add interaction component
        davidsArmor.AddComponent<InteractableElement>().elementType = ElementType.DavidsArmor;

        // Start the pulsating animation
        StartCoroutine(AnimateTreeArmorPulsation(branchesContainer, ringsContainer, gradientMaterials));

        // Generate the root workshop system
        GenerateRootWorkshopSystem();
    }

    private void GenerateRootWorkshopSystem()
    {
        // Create container for root-workshops
        GameObject rootWorkshopsContainer = new GameObject("RootWorkshops");
        rootWorkshopsContainer.transform.SetParent(tubaTree.transform, false);
        rootWorkshopsContainer.transform.localPosition = new Vector3(0, -10f, 0);

        // The seven root-workshops extend from the tree's base
        float baseRadius = 3f; // Starting radius at tree trunk
        float maxDepth = 20f;  // Maximum depth of roots

        // Create primary root structures
        for (int i = 0; i < 7; i++)
        {
            // Calculate angle based on position of corresponding master
            float angleOffset = (i == 3 || i == 6) ? Mathf.PI / 7 : 0; // Special positions for 4th and 7th
            float angle = (i * Mathf.PI * 2f / 7) + angleOffset;

            GameObject rootWorkshop = new GameObject($"RootWorkshop_{i + 1}");
            rootWorkshop.transform.SetParent(rootWorkshopsContainer.transform, false);

            // Create the actual root structure
            GenerateRootStructure(rootWorkshop.transform, angle, i, baseRadius, maxDepth);

            // Connect this root to corresponding armor rings
            ConnectRootToArmorRings(rootWorkshop.transform, i);
        }

        // Create the field (مزرعه) at the convergence point of roots
        CreateWorkshopField(rootWorkshopsContainer.transform, -maxDepth);
    }

    private void GenerateRootStructure(Transform parent, float angle, int rootIndex, float baseRadius, float maxDepth)
    {
        // Force a much smaller radius constraint - 60% of max Qaf radius instead of 90%
        float maxQafRadius = mountainRadii[mountainRadii.Length - 1] * 0.6f;

        // Create a lattice container for this root system
        GameObject rootLattice = new GameObject($"RootLattice_{rootIndex}");
        rootLattice.transform.SetParent(parent, false);

        // Create primary vertical elements (similar to trunk)
        LineRenderer mainRoot = parent.gameObject.AddComponent<LineRenderer>();

        // Material varies by position in workshop hierarchy
        Material rootMaterial = new Material(vrShader);

        // Colors follow Suhrawardi's description of masters
        if (rootIndex == 3) // 4th master with finest robe
        {
            rootMaterial.color = new Color(0.6f, 0.7f, 1f); // Luminous blue
        }
        else if (rootIndex == 6) // 7th master with no robe but field authority
        {
            rootMaterial.color = new Color(0.4f, 0.6f, 0.2f); // Fertile green
        }
        else // Regular masters
        {
            float hue = 0.7f + (rootIndex / 20f); // Purplish variations
            rootMaterial.color = Color.HSVToRGB(hue, 0.6f, 0.7f);
        }

        mainRoot.material = rootMaterial;
        mainRoot.startWidth = 0.5f;
        mainRoot.endWidth = 0.2f;

        // Create curved path following illuminationist principles
        int segments = isMobileVR ? 15 : 25;
        mainRoot.positionCount = segments;

        // Plot main root path
        for (int i = 0; i < segments; i++)
        {
            float t = i / (float)(segments - 1);

            // Root follows logarithmic spiral inward and downward - MUCH tighter curve
            float depth = t * maxDepth;

            // Calculate depth ratio to constrain radius as we go deeper
            float depthRatio = depth / maxDepth;

            // CRITICAL CHANGE: Much more aggressive inward curve
            // Maximum allowed radius decreases dramatically with depth
            float depthConstrainedRadius = maxQafRadius * (1.0f - 0.6f * Mathf.Pow(depthRatio, 0.8f));

            // Apply logarithmic spiral with tighter constraints
            float spiralFactor = Mathf.Pow(t, 0.5f) * baseRadius * 2f; // Less expanding
            spiralFactor = Mathf.Min(spiralFactor, depthConstrainedRadius * 0.8f); // Stricter constraint

            // Directional spread with tighter curve
            float curveModifier = 1.5f; // More aggressive inward curve
            float x = Mathf.Cos(angle + t * curveModifier) * spiralFactor;
            float z = Mathf.Sin(angle + t * curveModifier) * spiralFactor;

            // Add minimal variation - with strict constraint check
            float noiseX = Mathf.PerlinNoise(t * 3f, rootIndex) * t * 1f; // Less noise
            float noiseZ = Mathf.PerlinNoise(t * 3f, rootIndex + 10) * t * 1f; // Less noise

            // Calculate base position without noise
            Vector3 basePos = new Vector3(x, -depth, z);
            Vector3 noiseVector = new Vector3(noiseX, 0, noiseZ);

            // Scale noise down dramatically as we get deeper
            float noiseScale = 1.0f - Mathf.Pow(depthRatio, 0.5f) * 0.9f;
            noiseVector *= noiseScale;

            // Position with minimal variation
            Vector3 pos = basePos + noiseVector;

            // Final safety check - forcibly constrain to max radius if still outside
            float horizontalDistance = new Vector3(pos.x, 0, pos.z).magnitude;
            if (horizontalDistance > depthConstrainedRadius * 0.95f) // Extra 5% buffer
            {
                float scaleFactor = (depthConstrainedRadius * 0.95f) / horizontalDistance;
                pos.x *= scaleFactor;
                pos.z *= scaleFactor;
            }

            mainRoot.SetPosition(i, pos);

            // Create workshop nodes at key points only - fewer nodes
            if (i > 0 && i % 6 == 0 && i < segments - 2)
            {
                CreateWorkshopNode(parent, pos, rootMaterial.color, rootIndex, i / 6);
            }
        }

        // Create minimal horizontal rings - dramatically reduced in size
        int ringCount = 3; // Fewer rings
        float ringSpacing = maxDepth / (ringCount + 1);

        for (int r = 0; r < ringCount; r++)
        {
            float depth = (r + 1) * ringSpacing;
            // Create smaller rings that stay well within bounds
            CreateRootLatticeRing(rootLattice.transform, rootIndex, depth, baseRadius * (0.4f + r * 0.2f));
        }

        // Create minimal diagonal connectors
        CreateRootLatticeConnectors(rootLattice.transform, rootIndex, ringCount, ringSpacing, baseRadius);

        // Add fewer sub-roots that stay closer to main root
        int subRootCount = rootIndex == 6 ? 3 : 2; // Fewer sub-roots
        for (int s = 0; s < subRootCount; s++)
        {
            // Start from middle points along main root
            int startSegment = 4 + s * 6;
            if (startSegment < segments - 5)
            {
                Vector3 startPos = mainRoot.GetPosition(startSegment);
                CreateSubRoot(parent, startPos, rootMaterial.color, angle, rootIndex, s);
            }
        }
    }

    private void CreateRootLatticeRing(Transform parent, int rootIndex, float depth, float radius)
    {
        // Use a much smaller radius constraint
        float maxQafRadius = mountainRadii[mountainRadii.Length - 1] * 0.55f;

        // Calculate depth-based constraint (very aggressive inward curve)
        float maxDepth = 20f;
        float depthRatio = depth / maxDepth;
        float maxAllowedRadius = maxQafRadius * (1.0f - 0.65f * Mathf.Pow(depthRatio, 0.7f));

        // Constrain radius much more aggressively
        radius = Mathf.Min(radius, maxAllowedRadius * 0.8f);

        GameObject ring = new GameObject($"RootRing_{rootIndex}_{depth}");
        ring.transform.SetParent(parent, false);

        LineRenderer ringRenderer = ring.AddComponent<LineRenderer>();

        // Material based on rootIndex
        Material ringMaterial = new Material(vrShader);

        // Colors follow Suhrawardi's description of masters
        if (rootIndex == 3)
            ringMaterial.color = new Color(0.5f, 0.6f, 0.9f);
        else if (rootIndex == 6)
            ringMaterial.color = new Color(0.3f, 0.5f, 0.15f);
        else
        {
            float hue = 0.7f + (rootIndex / 20f);
            ringMaterial.color = Color.HSVToRGB(hue, 0.5f, 0.6f);
        }

        ringRenderer.material = ringMaterial;
        ringRenderer.startWidth = 0.2f;
        ringRenderer.endWidth = 0.2f;

        // Create simpler ring with fewer segments
        int segments = 12;
        ringRenderer.positionCount = segments + 1;

        // Create ring with minimal variations that stays within constraints
        for (int i = 0; i <= segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;

            // Very minimal radius variation
            float variableFactor = 1f + 0.1f * Mathf.Sin(angle * 3f + depth * 0.5f);
            float ringRadius = radius * variableFactor;

            // Strict constraint
            if (ringRadius > maxAllowedRadius * 0.9f)
            {
                ringRadius = maxAllowedRadius * 0.9f;
            }

            // Position with depth
            float x = Mathf.Cos(angle) * ringRadius;
            float z = Mathf.Sin(angle) * ringRadius;

            ringRenderer.SetPosition(i, new Vector3(x, -depth, z));
        }
    }

    private void CreateRootLatticeConnectors(Transform parent, int rootIndex, int ringCount, float ringSpacing, float baseRadius)
    {
        // Use the outermost mountain radius as the constraint
        float maxQafRadius = mountainRadii[mountainRadii.Length - 1] * 0.9f; // 90% of max radius for safety

        // Add aggressive boundary check for all connectors
        maxQafRadius *= 0.85f; // Further reduce max radius for connectors

        // Create diagonal connectors between rings
        int connectorCount = 12; // Fewer than above, representing condensation

        for (int c = 0; c < connectorCount; c++)
        {
            float angle = c * Mathf.PI * 2f / connectorCount;

            GameObject connector = new GameObject($"RootConnector_{rootIndex}_{c}");
            connector.transform.SetParent(parent, false);

            LineRenderer connectorRenderer = connector.AddComponent<LineRenderer>();

            // Material based on rootIndex
            Material connectorMaterial = new Material(vrShader);

            // Colors follow Suhrawardi's description of masters
            if (rootIndex == 3) // 4th master with finest robe
            {
                connectorMaterial.color = new Color(0.7f, 0.75f, 0.95f); // Lighter blue for connectors
            }
            else if (rootIndex == 6) // 7th master with no robe but field authority
            {
                connectorMaterial.color = new Color(0.35f, 0.55f, 0.25f); // Mixed green for connectors
            }
            else // Regular masters
            {
                float hue = 0.7f + (rootIndex / 20f); // Purplish variations
                connectorMaterial.color = Color.HSVToRGB(hue, 0.4f, 0.8f); // Lighter than main root
            }

            connectorRenderer.material = connectorMaterial;
            connectorRenderer.startWidth = 0.15f;
            connectorRenderer.endWidth = 0.1f;

            // Connect between rings in a diagonal pattern
            connectorRenderer.positionCount = ringCount + 1;

            for (int r = 0; r <= ringCount; r++)
            {
                float depth = r * ringSpacing;
                float connectorAngle = angle + (r * Mathf.PI / (3f * connectorCount)); // Spiral effect

                // Increasing radius as we go deeper, but constrained
                float depthRatio = depth / (ringCount * ringSpacing);
                float maxAllowedRadius = maxQafRadius * (1.0f - 0.25f * depthRatio);
                float ringRadius = Mathf.Min(baseRadius * (0.5f + r * 0.3f), maxAllowedRadius);

                // Position with twist
                float x = Mathf.Cos(connectorAngle) * ringRadius;
                float z = Mathf.Sin(connectorAngle) * ringRadius;

                connectorRenderer.SetPosition(r, new Vector3(x, -depth, z));
            }
        }
    }

    private void CreateSubRoot(Transform parent, Vector3 startPos, Color baseColor, float mainAngle, int rootIndex, int subIndex)
    {
        // Use a much smaller radius constraint - 50% instead of 85%
        float maxQafRadius = mountainRadii[mountainRadii.Length - 1] * 0.5f;

        GameObject subRoot = new GameObject($"SubRoot_{rootIndex}_{subIndex}");
        subRoot.transform.SetParent(parent, false);

        LineRenderer subRenderer = subRoot.AddComponent<LineRenderer>();

        // Slightly different color shade
        Material subMaterial = new Material(vrShader);
        subMaterial.color = new Color(
            baseColor.r * 0.9f,
            baseColor.g * 0.9f,
            baseColor.b * 0.9f
        );

        subRenderer.material = subMaterial;
        subRenderer.startWidth = 0.25f;
        subRenderer.endWidth = 0.1f;

        // Create path for sub-root - shorter path
        int subSegments = 8; // Fewer segments
        subRenderer.positionCount = subSegments;

        // Angle divergence from main root - less divergence
        float angleOffset = Mathf.PI / 6f * (subIndex % 2 == 0 ? 1 : -1); // 30 degrees instead of 45
        float subAngle = mainAngle + angleOffset;

        for (int i = 0; i < subSegments; i++)
        {
            float t = (float)i / (subSegments - 1);

            // Diverging path - shorter
            float depth = t * 5f; // 5 instead of 8

            // Calculate depthRatio to constrain radius - very aggressive constraint
            float depthRatio = depth / 5f;
            float maxAllowedRadius = maxQafRadius * (1.0f - 0.7f * Mathf.Pow(depthRatio, 0.7f));

            // Much smaller spread radius
            float spreadRadius = Mathf.Min(t * 2f, maxAllowedRadius * 0.7f);

            // Calculate position
            float x = Mathf.Cos(subAngle) * spreadRadius;
            float z = Mathf.Sin(subAngle) * spreadRadius;

            // Almost no variations
            float noiseX = Mathf.PerlinNoise(t * 2f, subIndex) * t * 0.5f;
            float noiseZ = Mathf.PerlinNoise(t * 2f, subIndex + 5) * t * 0.5f;

            // Scale noise down dramatically as we get deeper
            float noiseScale = 1.0f - depthRatio * 0.8f;
            noiseX *= noiseScale;
            noiseZ *= noiseScale;

            // Calculate base position
            Vector3 basePos = new Vector3(startPos.x + x, startPos.y - depth, startPos.z + z);
            Vector3 pos = basePos + new Vector3(noiseX, 0, noiseZ);

            // Final safety check - directly constrain to max radius
            float horizontalDistance = new Vector3(pos.x, 0, pos.z).magnitude;
            if (horizontalDistance > maxAllowedRadius * 0.9f)
            {
                float scaleFactor = (maxAllowedRadius * 0.9f) / horizontalDistance;
                pos.x *= scaleFactor;
                pos.z *= scaleFactor;
            }

            subRenderer.SetPosition(i, pos);

            // Only create a workshop node at the very end
            if (i == subSegments - 1)
            {
                CreateWorkshopNode(parent, pos, subMaterial.color, rootIndex, 10 + subIndex);
            }
        }
    }

    private void CreateWorkshopNode(Transform parent, Vector3 position, Color color, int rootIndex, int nodeIndex)
    {
        // Create a visual representation of a workshop node in the root system
        GameObject node = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        node.name = $"WorkshopNode_{rootIndex}_{nodeIndex}";
        node.transform.SetParent(parent, false);
        node.transform.position = position;
        node.transform.localScale = Vector3.one * (nodeIndex == 0 ? 1.2f : 0.8f);

        // Assign color - slightly brighter than the root
        Material nodeMaterial = new Material(vrShader);
        nodeMaterial.color = new Color(
            Mathf.Min(1f, color.r * 1.2f),
            Mathf.Min(1f, color.g * 1.2f),
            Mathf.Min(1f, color.b * 1.2f)
        );

        node.GetComponent<Renderer>().material = nodeMaterial;

        // Subtle light for workshop nodes
        if (!isMobileVR && rootIndex % 2 == 0 && nodeIndex == 0)
        {
            GameObject nodeLight = new GameObject($"NodeLight_{rootIndex}");
            nodeLight.transform.SetParent(node.transform, false);

            Light light = nodeLight.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = nodeMaterial.color;
            light.intensity = 1f;
            light.range = 5f;

            RegisterLight(light);
        }
    }

    private void ConnectRootToArmorRings(Transform root, int rootIndex)
    {
        // The connection between roots (lower workshops) and armor (upper workshops)
        // This implements Suhrawardi's description of how rings are processed
        // Each root connects to specific armor rings based on workshop authority

        LineRenderer[] armorRings = davidsArmor.GetComponentsInChildren<LineRenderer>();
        if (armorRings == null || armorRings.Length == 0) return;

        // Define connection pattern based on master's workshop authority
        int connectionCount;
        int[] ringIndices;

        switch (rootIndex)
        {
            case 3: // 4th master (special authority)
                connectionCount = 1;
                ringIndices = new int[] { 3 };
                break;
            case 6: // 7th master (field authority)
                connectionCount = 3;
                ringIndices = new int[] { 6, 7, 8 };
                break;
            default: // Regular masters
                connectionCount = 2;
                ringIndices = new int[] { rootIndex * 2, rootIndex * 2 + 1 };
                break;
        }

        // Create the connections
        for (int i = 0; i < connectionCount; i++)
        {
            int ringIndex = ringIndices[i];
            if (ringIndex < armorRings.Length)
            {
                // Find a good connection point on the armor ring
                Vector3 ringPoint = Vector3.zero;
                LineRenderer ring = armorRings[ringIndex];

                if (ring.positionCount > 0)
                {
                    ringPoint = ring.GetPosition(ring.positionCount / 2);
                }

                if (ringPoint != Vector3.zero)
                {
                    CreateRingRootConnection(root, ringPoint, rootIndex);
                }
            }
        }
    }

    private void CreateRingRootConnection(Transform root, Vector3 ringPoint, int rootIndex)
    {
        // Create fiber-like connection between root system and armor rings
        // This visually represents how the lower workshops process armor rings

        GameObject connectionObj = new GameObject($"RingRootConnection_{rootIndex}");
        connectionObj.transform.SetParent(root, false);

        LineRenderer connection = connectionObj.AddComponent<LineRenderer>();

        // Material represents "subtle matter" in illuminationist cosmology
        Material connectionMaterial = new Material(vrShader);
        connectionMaterial.color = new Color(0.8f, 0.8f, 1f, 0.8f);

        connection.material = connectionMaterial;
        connection.startWidth = 0.1f;
        connection.endWidth = 0.2f;

        // Create energy flow path
        int segments = 20;
        connection.positionCount = segments;

        Vector3 rootPoint = root.position;

        for (int i = 0; i < segments; i++)
        {
            float t = i / (float)(segments - 1);

            // Bezier curve with multiple control points
            Vector3 midPoint = Vector3.Lerp(rootPoint, ringPoint, 0.5f);
            midPoint.y += 5f; // Curve upward

            // Add subtle winding pattern
            float windFactor = Mathf.Sin(t * Mathf.PI * 4f) * (1f - t) * 2f;
            Vector3 windOffset = new Vector3(
                Mathf.Sin(t * Mathf.PI * 2f) * windFactor,
                0,
                Mathf.Cos(t * Mathf.PI * 2f) * windFactor
            );

            // Calculate position with Bezier curve
            Vector3 p1 = Vector3.Lerp(rootPoint, midPoint, t);
            Vector3 p2 = Vector3.Lerp(midPoint, ringPoint, t);
            Vector3 pos = Vector3.Lerp(p1, p2, t) + windOffset;

            connection.SetPosition(i, pos);
        }
    }

    private void CreateWorkshopField(Transform parent, float depth)
    {
        // Get the outermost mountain radius to constrain the field
        float maxQafRadius = mountainRadii[mountainRadii.Length - 1] * 0.9f; // Use 90% of the radius

        // The field (مزرعه) mentioned by Suhrawardi
        // This is the domain of the 7th master

        GameObject field = new GameObject("WorkshopField");
        field.transform.SetParent(parent, false);
        field.transform.localPosition = new Vector3(0, depth, 0);

        // Create field as a radial disc with organic patterns - constrained to Qaf
        GameObject fieldDisc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        fieldDisc.name = "FieldDisc";
        fieldDisc.transform.SetParent(field.transform, false);
        fieldDisc.transform.localPosition = Vector3.zero;
        // Constrain field size to stay within Qaf boundaries
        float fieldRadius = Mathf.Min(15f, maxQafRadius * 0.85f);
        fieldDisc.transform.localScale = new Vector3(fieldRadius, 0.2f, fieldRadius);
        fieldDisc.transform.localRotation = Quaternion.identity;

        // Material representing fertile ground
        Material fieldMaterial = new Material(vrShader);
        fieldMaterial.color = new Color(0.3f, 0.5f, 0.2f);
        fieldDisc.GetComponent<Renderer>().material = fieldMaterial;

        // Add organic pattern on surface - representing growing armor rings
        int ringCount = isMobileVR ? 4 : 7;
        for (int r = 0; r < ringCount; r++)
        {
            GameObject ringObj = new GameObject($"ArmorRing_{r}");
            ringObj.transform.SetParent(field.transform, false);

            // Position rings at various points on the field - but constrain to available space
            float angle = r * Mathf.PI * 2f / ringCount;
            // Ensure rings stay well within the field
            float maxRingDistance = fieldRadius * 0.8f;
            float distance = Mathf.Min(3f + r * 1.5f, maxRingDistance);

            Vector3 pos = new Vector3(
                Mathf.Cos(angle) * distance,
                0.15f,
                Mathf.Sin(angle) * distance
            );

            ringObj.transform.position = field.transform.position + pos;
            ringObj.transform.localScale = Vector3.one * 0.5f;

            // Create a ring visualization
            LineRenderer ringRenderer = ringObj.AddComponent<LineRenderer>();
            ringRenderer.material = new Material(vrShader);
            ringRenderer.material.color = new Color(0.7f, 0.7f, 0.7f);
            ringRenderer.startWidth = 0.1f;
            ringRenderer.endWidth = 0.1f;

            // Create ring segments
            int ringSegments = 32;
            ringRenderer.positionCount = ringSegments;

            for (int i = 0; i < ringSegments; i++)
            {
                float segmentAngle = i * Mathf.PI * 2f / ringSegments;
                float ringX = Mathf.Cos(segmentAngle) * 0.5f;
                float ringZ = Mathf.Sin(segmentAngle) * 0.5f;
                ringRenderer.SetPosition(i, new Vector3(ringX, 0, ringZ));
            }
        }

        // Add subtle glow effect
        if (!isMobileVR)
        {
            GameObject fieldLight = new GameObject("FieldLight");
            fieldLight.transform.SetParent(field.transform, false);

            Light light = fieldLight.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.3f, 0.6f, 0.3f);
            light.intensity = 1.5f;
            light.range = 20f;

            RegisterLight(light);
        }

        // Add interaction component
        field.AddComponent<InteractableElement>().elementType = ElementType.Field;
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
                    interactionText.text = "The Tuba Tree (درخت طوبی) - the cosmic axis from which all fruits and plants emanate. Its branches extend to form David's Armor, demonstrating their shared metaphysical essence.";
                    break;

                case ElementType.Jewel:
                    interactionText.text = "The Night-Illuminating Jewel (گوهر شب افروز) - its light comes from the Tuba Tree";
                    break;

                case ElementType.Workshop:
                    interactionText.text = "One of the Twelve Workshops (دوازده کارگاه) - where cosmic realities are woven";
                    break;

                case ElementType.DavidsArmor:
                    interactionText.text = "David's Armor (زره داوودی) - the constraining lattice that emerges from the Tuba Tree's branches. In Suhrawardi's cosmology, constraint and liberation share the same substance.";
                    break;

                case ElementType.Field:
                    interactionText.text = "The Cosmic Field (مزرعه) - the lowest level of manifestation where the physical world takes form. In Suhrawardi's cosmology, this represents the material realm where illuminationist principles manifest in physical form.";
                    break;
            }

            // Add additional text explaining illuminationist correspondences
            if (element.elementType == ElementType.Tree ||
                element.elementType == ElementType.DavidsArmor ||
                element.elementType == ElementType.Fountain)
            {
                interactionText.text += "\n\nIn illuminationist wisdom, this element represents one manifestation of the same metaphysical reality that appears as " +
                    (element.elementType == ElementType.Tree ? "David's Armor and the Fountain of Life. The pulsing light (nūr qāhir) demonstrates the unified essence." :
                     element.elementType == ElementType.DavidsArmor ? "the branches of Tuba Tree. The gradient materials show how the same essence transforms as it extends outward." :
                     "the roots of Tuba and the light of consciousness.");
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

    private void GenerateSevenLowerWorkshops()
    {
        // Create container for the lower workshops
        GameObject lowerWorkshopsContainer = new GameObject("LowerWorkshops");
        lowerWorkshopsContainer.transform.SetParent(transform, false);

        // Position beneath the twelve workshops
        float lowerRadius = 15f; // Smaller radius than upper workshops
        float lowerY = -12f;    // Positioned lower in Y axis

        // Create the seven workshops in heptagonal arrangement
        for (int i = 0; i < 7; i++)
        {
            float angle = i * Mathf.PI * 2f / 7;

            // Apply vortex mathematics - logarithmic spiral
            float vortexFactor = 1f - (i / 7f) * 0.3f; // Decreasing radius
            float x = Mathf.Cos(angle) * lowerRadius * vortexFactor;
            float z = Mathf.Sin(angle) * lowerRadius * vortexFactor;

            // Descending Y position - following illuminationist hierarchy
            float individualY = lowerY - (i % 3) * 2f;

            GameObject lowerWorkshop = new GameObject($"LowerWorkshop_{i + 1}");
            lowerWorkshop.transform.SetParent(lowerWorkshopsContainer.transform, false);
            lowerWorkshop.transform.localPosition = new Vector3(x, individualY, z);

            // Visual representation - smaller than upper workshops
            GameObject workshopVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            workshopVisual.name = "WorkshopVisual";
            workshopVisual.transform.SetParent(lowerWorkshop.transform, false);
            workshopVisual.transform.localPosition = Vector3.zero;
            workshopVisual.transform.localScale = Vector3.one * 2f;

            // Materials show hierarchy - gradient based on position
            Material lowerWorkshopMaterial = new Material(vrShader);

            // Colors based on workshop master position
            Color workshopColor;
            if (i == 3) // 4th master with finest robe
            {
                workshopColor = new Color(0.8f, 0.8f, 1f); // Luminous blue-white
            }
            else if (i == 6) // 7th master with no robe
            {
                workshopColor = new Color(0.4f, 0.4f, 0.4f); // Darker, earthly tone
            }
            else // Regular masters
            {
                workshopColor = Color.Lerp(
                    new Color(0.7f, 0.3f, 0.7f), // Upper workshop color
                    new Color(0.3f, 0.3f, 0.5f), // Lower workshop color
                    i / 6f
                );
            }

            lowerWorkshopMaterial.color = workshopColor;
            workshopVisual.GetComponent<Renderer>().material = lowerWorkshopMaterial;

            // Add interaction component
            lowerWorkshop.AddComponent<InteractableElement>().elementType = ElementType.Workshop;
        }
    }

    private void CreateCorrespondences()
    {
        if (!showCorrespondences) return;

        // Create container for all correspondence elements
        correspondencesContainer = new GameObject("IlluminationistCorrespondences");
        correspondencesContainer.transform.SetParent(transform, false);

        // Only proceed if we have the key elements
        if (davidsArmor == null || tubaTree == null || fountainOfLife == null) return;

        // 1. Connect David's Armor and Tuba Tree
        CreateCorrespondenceLine(
            davidsArmor.transform.position,
            tubaTree.transform.position,
            "Armor_Tree_Correspondence"
        );

        // 2. Connect Tuba Tree and Fountain of Life
        CreateCorrespondenceLine(
            tubaTree.transform.position,
            fountainOfLife.transform.position,
            "Tree_Fountain_Correspondence"
        );

        // 3. Connect Tuba Tree and Jewel
        if (nightIlluminatingJewel != null)
        {
            CreateCorrespondenceLine(
                tubaTree.transform.position,
                nightIlluminatingJewel.transform.position,
                "Tree_Jewel_Correspondence"
            );
        }
    }

    private void CreateCorrespondenceLine(Vector3 start, Vector3 end, string name)
    {
        GameObject lineObj = new GameObject(name);
        lineObj.transform.SetParent(correspondencesContainer.transform, false);

        LineRenderer line = lineObj.AddComponent<LineRenderer>();
        line.material = sharedEssenceMaterial != null ? sharedEssenceMaterial : new Material(vrShader);
        line.material.color = correspondenceColor;
        line.startWidth = 0.2f;
        line.endWidth = 0.2f;
        line.positionCount = 2;
        line.SetPosition(0, start);
        line.SetPosition(1, end);

        correspondenceLines.Add(line);
    }

    private IEnumerator AnimateJewel()
    {
        while (true)
        {
            jewelRotationTime += Time.deltaTime * 0.2f;
            
            if (nightIlluminatingJewel != null)
            {
                float radius = 8f;
                float x = Mathf.Cos(jewelRotationTime) * radius;
                float z = Mathf.Sin(jewelRotationTime) * radius;
                float y = 5f + Mathf.Sin(jewelRotationTime * 0.5f) * 1f;
                
                nightIlluminatingJewel.transform.localPosition = new Vector3(x, y, z);
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

        // Orient the jewel to face the tree - symbolic of its ontological dependence
        nightIlluminatingJewel.transform.LookAt(transform.position);
    }

    private IEnumerator AnimateIshraqiElements()
    {
        float time = 0;

        while (true)
        {
            time += Time.deltaTime;

            // Animate correspondence lines to pulse with divine illumination
            if (showCorrespondences && correspondenceLines.Count > 0)
            {
                float pulse = (Mathf.Sin(time * pulseSpeed * Mathf.PI) + 1f) / 2f;
                Color pulsedColor = new Color(
                    correspondenceColor.r,
                    correspondenceColor.g,
                    correspondenceColor.b,
                    correspondenceColor.a * pulse * correspondenceIntensity
                );

                foreach (LineRenderer line in correspondenceLines)
                {
                    if (line != null)
                    {
                        // Apply pulsed color
                        line.material.color = pulsedColor;

                        // Vary width slightly based on pulse
                        float width = 0.1f + 0.1f * pulse;
                        line.startWidth = width + 0.1f;
                        line.endWidth = width;
                    }
                }
            }

            yield return null;
        }
    }

    private IEnumerator AnimateTreeArmorPulsation(GameObject branches, GameObject rings, Material[] gradientMaterials)
    {
        float time = 0;

        while (true)
        {
            time += Time.deltaTime;

            // Calculate pulsation effect - the 'dominating light' (nūr qāhir)
            float pulseFactor = (Mathf.Sin(time * 0.8f) + 1f) * 0.5f; // 0-1 range

            // Get all line renderers in the branches and rings
            LineRenderer[] branchRenderers = branches.GetComponentsInChildren<LineRenderer>();
            LineRenderer[] ringRenderers = rings.GetComponentsInChildren<LineRenderer>();

            // Apply pulsation to branches
            foreach (LineRenderer branch in branchRenderers)
            {
                if (branch != null)
                {
                    // Get current material and position in the hierarchy to determine intensity
                    Material mat = branch.material;
                    Transform branchTransform = branch.transform;

                    // Calculate distance from tree center (intensity fades with distance)
                    float distanceFromCenter = Vector3.Distance(branchTransform.position, Vector3.zero);
                    float normalizedDistance = Mathf.Clamp01(distanceFromCenter / 10f);

                    // Light flows from center outward - delay based on distance
                    float delayedPulse = Mathf.Sin((time - normalizedDistance * 0.5f) * 0.8f) * 0.5f + 0.5f;

                    // Combine base color with pulse effect
                    Color baseColor = mat.color;
                    Color pulseColor = Color.Lerp(baseColor, Color.white, delayedPulse * 0.4f);

                    // Apply color
                    branch.material.color = pulseColor;

                    // Vary width slightly with pulse
                    float baseWidth = branch.startWidth;
                    branch.startWidth = baseWidth * (1f + delayedPulse * 0.2f);
                    branch.endWidth = branch.endWidth * (1f + delayedPulse * 0.15f);
                }
            }

            // Apply pulsation to rings
            foreach (LineRenderer ring in ringRenderers)
            {
                if (ring != null)
                {
                    // Get position to determine delay
                    float heightPosition = ring.transform.position.y;
                    float normalizedHeight = Mathf.Abs(heightPosition) / 10f; // 0 at center, 1 at edges

                    // Light flows outward from center
                    float delayedPulse = Mathf.Sin((time - normalizedHeight * 0.6f) * 0.8f) * 0.5f + 0.5f;

                    // Apply color pulsation
                    Color baseColor = ring.material.color;
                    Color pulseColor = Color.Lerp(baseColor, Color.white, delayedPulse * 0.3f);
                    ring.material.color = pulseColor;

                    // Vary width with pulse
                    float baseWidth = ring.startWidth;
                    ring.startWidth = baseWidth * (1f + delayedPulse * 0.15f);
                    ring.endWidth = ring.startWidth;
                }
            }

            yield return null;
        }
    }

    private void ImplementTreeFountainWaveDynamics()
    {
        // Create container for wave dynamics
        GameObject waveDynamicsContainer = new GameObject("TreeFountainWaveDynamics");
        waveDynamicsContainer.transform.SetParent(transform, false);

        // Position between tree and fountain
        Vector3 midpoint = (tubaTree.transform.position + fountainOfLife.transform.position) / 2f;
        waveDynamicsContainer.transform.position = midpoint;

        // Add controller component
        TreeFountainWaveController wavesController = waveDynamicsContainer.AddComponent<TreeFountainWaveController>();

        // Initialize with references to sources and shader
        wavesController.Initialize(tubaTree, fountainOfLife, vrShader);
    }

    // Add after existing methods
    private void CollectCosmicElements()
    {
        // Clear existing lists
        allCosmicElements.Clear();
        emanatingElements.Clear();

        // Add main cosmic elements
        if (tubaTree != null) allCosmicElements.Add(tubaTree);
        if (fountainOfLife != null) allCosmicElements.Add(fountainOfLife);
        if (nightIlluminatingJewel != null) allCosmicElements.Add(nightIlluminatingJewel);
        if (davidsArmor != null) allCosmicElements.Add(davidsArmor);

        // Add mountains
        foreach (GameObject mountain in mountainLayers)
        {
            if (mountain != null) allCosmicElements.Add(mountain);
        }

        // Add workshops
        foreach (GameObject workshop in twelveWorkshops)
        {
            if (workshop != null) allCosmicElements.Add(workshop);
        }

        // Set emanating elements (those that emit light)
        emanatingElements.Add(tubaTree);
        emanatingElements.Add(fountainOfLife);
        emanatingElements.Add(nightIlluminatingJewel);

        // Set up shaders
        SetupUnifiedShaders();
    }

    private void SetupUnifiedShaders()
    {
        // Use best available shaders for the platform
        illuminationShader = vrShader;
        unifiedFieldShader = vrShader;
        threadShader = vrShader;
        instantPresenceShader = vrShader;
    }

    // Create a unified light gradient that spans all elements
    private void CreateUnifiedLightGradient()
    {
        // Define the "Light of Lights" base color
        Color nurAlAnwar = new Color(1.0f, 0.95f, 0.8f);

        // For each element, calculate its "light distance" from the source
        foreach (GameObject element in allCosmicElements)
        {
            if (element == null || element.GetComponent<Renderer>() == null) continue;

            float distanceFromCenter = Vector3.Distance(element.transform.position, Vector3.zero);
            float normalizedDistance = Mathf.Clamp01(distanceFromCenter / maxRadius);

            // Calculate luminosity based on ontological position
            float luminosityFactor = Mathf.Pow(1 - normalizedDistance, 2.0f);

            // Create a material that represents this element's position in light hierarchy
            Material gradientMaterial = new Material(illuminationShader);
            gradientMaterial.color = Color.Lerp(nurAlAnwar, element.GetComponent<Renderer>().material.color, normalizedDistance);
            gradientMaterial.SetFloat("_Glossiness", 0.8f - normalizedDistance * 0.5f);

            // For VR compatibility, we're simplifying and just setting the material color
            // but keeping the original element materials
        }
    }

    private void CreateIlluminationRays()
    {
        // Create a central light source (nūr al-anwār)
        GameObject centralLight = new GameObject("NurAlAnwar");
        centralLight.transform.SetParent(transform, false);
        centralLight.transform.position = Vector3.zero;

        // For each element that appears disconnected
        foreach (GameObject emanatingElement in emanatingElements)
        {
            if (emanatingElement == null) continue;

            // Create ray connection
            GameObject rayConnection = new GameObject("IshraqiRay");
            rayConnection.transform.SetParent(centralLight.transform, false);
            LineRenderer ray = rayConnection.AddComponent<LineRenderer>();

            // Set ray properties
            ray.startWidth = 0.05f;
            ray.endWidth = 0.02f;
            ray.material = new Material(vrShader);
            ray.material.color = new Color(1.0f, 0.95f, 0.8f, 0.5f);

            // Create connection points
            int raySegments = 10;
            ray.positionCount = raySegments;

            // Calculate path using illuminationist mathematics
            for (int i = 0; i < raySegments; i++)
            {
                float t = (float)i / (raySegments - 1);

                // Apply ishrāqī wave function - light travels in subtle waves, not straight lines
                float waveAmplitude = 0.2f * (1f - t); // Diminishes as approaches source
                float waveOffset = Mathf.Sin(t * 6f) * waveAmplitude;

                // Calculate position along path with wave displacement
                Vector3 directPath = Vector3.Lerp(emanatingElement.transform.position, centralLight.transform.position, t);
                Vector3 perpendicular = Vector3.Cross(directPath.normalized, Vector3.up).normalized;
                Vector3 wavePosition = directPath + perpendicular * waveOffset;

                ray.SetPosition(i, wavePosition);
            }

            // Add pulsation effect representing "continuous illumination" (ishrāq mustamirr)
            StartCoroutine(PulsateRay(ray));
        }
    }

    private IEnumerator PulsateRay(LineRenderer ray)
    {
        if (ray == null) yield break;

        float time = 0f;
        while (ray != null)
        {
            // Calculate pulsation based on illuminationist principles
            time += Time.deltaTime;
            float pulse = Mathf.Sin(time * 2f) * 0.5f + 0.5f; // 0-1 range

            // Update alpha based on pulse
            Color rayColor = ray.material.color;
            rayColor.a = 0.2f + pulse * 0.4f;
            ray.material.color = rayColor;

            // Vary width slightly based on pulse
            float width = 0.03f + pulse * 0.04f;
            ray.startWidth = width + 0.02f;
            ray.endWidth = width;

            yield return null;
        }
    }

    private void CreateUnifiedField()
    {
        // Create the field container
        GameObject unifiedField = new GameObject("HadrahJamiah");
        unifiedField.transform.SetParent(transform, false);

        // Create visual representation of the unified field
        // This should be subtle but perceptible
        Material fieldMaterial = new Material(unifiedFieldShader);
        fieldMaterial.color = new Color(0.9f, 0.9f, 1.0f, 0.1f);

        // Create spherical field with specialized shader
        GameObject fieldSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        fieldSphere.name = "UnifiedField";
        fieldSphere.transform.SetParent(unifiedField.transform, false);
        fieldSphere.transform.localScale = Vector3.one * (maxRadius * 1.2f); // Slightly larger than cosmos
        fieldSphere.GetComponent<Renderer>().material = fieldMaterial;

        // For VR compatibility, reverse normals to make it visible from inside
        ReverseNormals(fieldSphere);

        // Add subtle pulsation that encompasses all elements
        StartCoroutine(PulsateUnifiedField(fieldSphere));

        // Create energy threads connecting seemingly disconnected elements
        CreateUnityThreads();
    }

    private IEnumerator PulsateUnifiedField(GameObject fieldSphere)
    {
        if (fieldSphere == null) yield break;

        Renderer renderer = fieldSphere.GetComponent<Renderer>();
        if (renderer == null) yield break;

        float time = 0f;
        while (fieldSphere != null && renderer != null)
        {
            time += Time.deltaTime;

            // Subtle pulsation
            float pulse = (Mathf.Sin(time * 0.2f) + 1f) * 0.5f; // 0-1 range, very slow

            // Update color alpha
            Color fieldColor = renderer.material.color;
            fieldColor.a = 0.05f + pulse * 0.05f; // Keep very subtle
            renderer.material.color = fieldColor;

            yield return null;
        }
    }

    private void CreateUnityThreads()
    {
        // Get potentially disconnected pairs
        List<(GameObject, GameObject)> disconnectedPairs = GetDisconnectedPairs();

        // For each pair of apparently disconnected elements
        foreach (var elementPair in disconnectedPairs)
        {
            GameObject threadObj = new GameObject("UnityThread");
            threadObj.transform.SetParent(transform, false);

            // Create specialized line renderer
            LineRenderer thread = threadObj.AddComponent<LineRenderer>();
            thread.material = new Material(threadShader);
            thread.startWidth = 0.03f;
            thread.endWidth = 0.01f;
            thread.material.color = new Color(0.9f, 0.9f, 1.0f, 0.15f); // Very subtle

            // Create curved path between elements with multiple control points
            int segments = 10;
            thread.positionCount = segments;

            Vector3 start = elementPair.Item1.transform.position;
            Vector3 end = elementPair.Item2.transform.position;

            // Create path with multiple control points
            for (int i = 0; i < segments; i++)
            {
                float t = (float)i / (segments - 1);

                // First compute direct path
                Vector3 directPath = Vector3.Lerp(start, end, t);

                // Add curve toward center - the "gravitational pull" of the Light of Lights
                Vector3 centerPull = Vector3.Lerp(directPath, Vector3.zero, 0.2f * Mathf.Sin(t * Mathf.PI));

                thread.SetPosition(i, centerPull);
            }
        }
    }

    private List<(GameObject, GameObject)> GetDisconnectedPairs()
    {
        List<(GameObject, GameObject)> pairs = new List<(GameObject, GameObject)>();

        // For a simplified version, connect some key elements
        if (tubaTree != null && nightIlluminatingJewel != null)
            pairs.Add((tubaTree, nightIlluminatingJewel));

        if (fountainOfLife != null && tubaTree != null)
            pairs.Add((fountainOfLife, tubaTree));

        // Add some random mountain connections if we have at least 3 mountains
        if (mountainLayers.Length >= 3)
        {
            pairs.Add((mountainLayers[0], mountainLayers[mountainLayers.Length - 1]));
            pairs.Add((mountainLayers[1], mountainLayers[mountainLayers.Length - 2]));
        }

        return pairs;
    }

    private (GameObject, GameObject) GetRandomDisconnectedPair()
    {
        List<(GameObject, GameObject)> pairs = GetDisconnectedPairs();
        if (pairs.Count == 0)
            return (null, null);

        int randomIndex = Random.Range(0, pairs.Count);
        return pairs[randomIndex];
    }

    private void CreateWaveDynamicsBetweenElements()
    {
        // Create controller for wave dynamics
        GameObject waveController = new GameObject("IshraqiWaveDynamics");
        waveController.transform.SetParent(transform, false);

        // Connect all elements with subtle wave patterns
        ConnectAllElementsWithWaves(waveController);
    }

    private void ConnectAllElementsWithWaves(GameObject controller)
    {
        // Get all major elements
        GameObject[] majorElements = GetMajorElements();

        // For each element, create subtle wave connection to 2-3 nearest elements
        foreach (GameObject element in majorElements)
        {
            if (element == null) continue;

            // Find nearest elements
            var nearestElements = FindNearestElements(element, majorElements, 3);

            foreach (var nearElement in nearestElements)
            {
                // Create wave connection
                GameObject waveConnection = new GameObject($"Wave_{element.name}_to_{nearElement.name}");
                waveConnection.transform.SetParent(controller.transform, false);

                // Add specialized component that creates wave effect
                ElementWaveConnection waveComp = waveConnection.AddComponent<ElementWaveConnection>();
                waveComp.Initialize(element, nearElement, vrShader);

                // Store reference for unified animation
                allWaveConnections.Add(waveComp);
            }
        }

        // Start unified animation coroutine
        StartCoroutine(AnimateUnifiedWaveSystem(allWaveConnections));
    }

    private GameObject[] GetMajorElements()
    {
        List<GameObject> elements = new List<GameObject>();

        // Add main cosmic elements
        if (tubaTree != null) elements.Add(tubaTree);
        if (fountainOfLife != null) elements.Add(fountainOfLife);
        if (nightIlluminatingJewel != null) elements.Add(nightIlluminatingJewel);

        // Add some key mountains (just a few for performance)
        for (int i = 0; i < mountainLayers.Length; i += 3)
        {
            if (i < mountainLayers.Length && mountainLayers[i] != null)
                elements.Add(mountainLayers[i]);
        }

        // Add some key workshops (just a few for performance)
        for (int i = 0; i < twelveWorkshops.Length; i += 3)
        {
            if (i < twelveWorkshops.Length && twelveWorkshops[i] != null)
                elements.Add(twelveWorkshops[i]);
        }

        return elements.ToArray();
    }

    private List<GameObject> FindNearestElements(GameObject sourceElement, GameObject[] allElements, int count)
    {
        // Find nearest elements
        List<(GameObject, float)> distances = new List<(GameObject, float)>();

        foreach (GameObject element in allElements)
        {
            if (element == null || element == sourceElement) continue;

            float distance = Vector3.Distance(sourceElement.transform.position, element.transform.position);
            distances.Add((element, distance));
        }

        // Sort by distance
        distances.Sort((a, b) => a.Item2.CompareTo(b.Item2));

        // Return nearest elements
        List<GameObject> nearest = new List<GameObject>();
        int actualCount = Mathf.Min(count, distances.Count);

        for (int i = 0; i < actualCount; i++)
        {
            nearest.Add(distances[i].Item1);
        }

        return nearest;
    }

    private IEnumerator AnimateUnifiedWaveSystem(List<ElementWaveConnection> connections)
    {
        float time = 0f;

        while (true)
        {
            time += Time.deltaTime;

            // Calculate global wave state for synchronization
            float globalWave = Mathf.Sin(time * 0.5f) * 0.5f + 0.5f;

            // Update all connections
            foreach (var connection in connections)
            {
                if (connection != null)
                {
                    connection.UpdateWaveState(time, globalWave);
                }
            }

            yield return null;
        }
    }

    private void CreateInstantaneousPresenceEffects()
    {
        // Create controller for instantaneous effects
        GameObject presenceController = new GameObject("InstantaneousPresence");
        presenceController.transform.SetParent(transform, false);

        // Add controller component
        InstantaneousPresenceController controller = presenceController.AddComponent<InstantaneousPresenceController>();
        controller.Initialize(allCosmicElements.ToArray(), this);

        // Start coroutine that creates "flashes" of connection
        StartCoroutine(CreateConnectionFlashes(controller));
    }

    private IEnumerator CreateConnectionFlashes(InstantaneousPresenceController controller)
    {
        if (controller == null) yield break;

        while (controller != null)
        {
            // Wait random interval
            yield return new WaitForSeconds(Random.Range(6f, 16f)); // Less frequent for subtlety

            // Select random pair of elements that appear disconnected
            var elementPair = GetRandomDisconnectedPair();

            if (elementPair.Item1 != null && elementPair.Item2 != null)
            {
                // Create flash effect
                controller.CreateConnectionFlash(elementPair.Item1, elementPair.Item2);

                // Create momentary light path
                StartCoroutine(CreateMomentaryLightPath(elementPair.Item1, elementPair.Item2));
            }
        }
    }

    private IEnumerator CreateMomentaryLightPath(GameObject elementA, GameObject elementB)
    {
        if (elementA == null || elementB == null) yield break;

        // Create light path object
        GameObject lightPath = new GameObject("MomentaryLightPath");
        lightPath.transform.SetParent(transform, false);

        // Add line renderer
        LineRenderer path = lightPath.AddComponent<LineRenderer>();
        path.material = new Material(instantPresenceShader);
        path.startWidth = 0.1f;
        path.endWidth = 0.1f;

        // Set color
        path.startColor = new Color(1f, 0.9f, 0.7f, 0.8f);
        path.endColor = new Color(1f, 0.9f, 0.7f, 0.0f);

        // Create path with multiple segments
        int segments = 20;
        path.positionCount = segments;

        Vector3 startPos = elementA.transform.position;
        Vector3 endPos = elementB.transform.position;

        // Generate path points
        for (int i = 0; i < segments; i++)
        {
            float t = (float)i / (segments - 1);

            // Create curved path through central point
            Vector3 midPoint = Vector3.Lerp(startPos, endPos, 0.5f);
            midPoint += Vector3.up * 2f; // Elevate midpoint

            Vector3 p1 = Vector3.Lerp(startPos, midPoint, t);
            Vector3 p2 = Vector3.Lerp(midPoint, endPos, t);
            Vector3 finalPos = Vector3.Lerp(p1, p2, t);

            path.SetPosition(i, finalPos);
        }

        // Animate flash effect
        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / duration;

            // Fade out effect
            Color startColor = path.startColor;
            startColor.a = Mathf.Lerp(0.8f, 0f, normalizedTime);
            path.startColor = startColor;

            yield return null;
        }

        // Remove path
        Destroy(lightPath);
    }

    // Add the RecenterBranchesAroundLuminousAxis method
    private void RecenterBranchesAroundLuminousAxis()
    {
        // Identify the central luminous axis (qutb)
        Transform luminousAxis = tubaTree.transform;

        // Gather all branches if not already done
        if (allBranches.Count == 0)
        {
            GatherAllBranches(davidsArmor.transform, allBranches);
        }

        // Calculate current perceived center
        Vector3 perceivedCenter = CalculatePerceivedCenter(allBranches);

        // Calculate offset from true center
        Vector3 offset = luminousAxis.position - perceivedCenter;

        // Apply logarithmic redistribution based on luminous intensity
        foreach (Transform branch in allBranches)
        {
            // Calculate branch's position in luminous hierarchy
            float luminousRank = CalculateLuminousRank(branch);

            // Higher-ranked branches should be closer to true center
            float repositioningFactor = Mathf.Exp(-luminousRank);

            // Apply repositioning with ishrāqī mathematical principles
            Vector3 newPosition = branch.position + offset * repositioningFactor;

            // Gradually transition to new position
            StartCoroutine(TransitionToPosition(branch, newPosition));
        }
    }

    private void ReorientObserverPosition()
    {
        // Find the optimal viewing position described in illuminationist texts
        Vector3 illuminationistViewpoint = CalculateIdealViewpoint();

        // Adjust camera position gradually
        if (cameraTransform != null)
        {
            StartCoroutine(TransitionCameraToPosition(cameraTransform, illuminationistViewpoint));
        }

        // Apply subtle visual cues to indicate proper orientation
        ApplyVisualOrientationCues();
    }

    private Vector3 CalculateIdealViewpoint()
    {
        // In illuminationist cosmology, the ideal viewpoint is 45° above the eastern horizon
        // This is the "ishrāqī angle" mentioned in Suhrawardi's works

        // First find the cosmic East direction (direction of illumination)
        Vector3 eastDirection = GetCosmicEastDirection();

        // Calculate position 45° elevated from center along eastern axis
        float distanceFromCenter = 20f; // Adjust based on scale
        Vector3 eastwardOffset = eastDirection * distanceFromCenter;

        // Apply 45° elevation (the angle of illumination in ishrāqī cosmology)
        Vector3 elevatedPosition = eastwardOffset + Vector3.up * distanceFromCenter * Mathf.Sin(45f * Mathf.Deg2Rad);

        return transform.position + elevatedPosition;
    }

    private void RedistributeBranchesAccordingToLuminousProportions()
    {
        // Center of redistribution is the Light of Lights (nūr al-anwār)
        Vector3 center = Vector3.zero;

        // Gather all branches if not already done
        if (allBranches.Count == 0)
        {
            GatherAllBranches(davidsArmor.transform, allBranches);
        }

        // Calculate current luminous intensity for each branch
        Dictionary<Transform, float> luminousIntensities = new Dictionary<Transform, float>();
        foreach (Transform branch in allBranches)
        {
            float intensity = CalculateLuminousIntensity(branch);
            luminousIntensities[branch] = intensity;
        }

        // Sort branches by luminous intensity
        allBranches.Sort((a, b) => luminousIntensities[b].CompareTo(luminousIntensities[a]));

        // Apply the golden spiral distribution (based on Suhrawardi's cosmic architecture)
        for (int i = 0; i < allBranches.Count; i++)
        {
            Transform branch = allBranches[i];

            // Calculate position along golden spiral
            float theta = i * Mathf.PI * (3f - Mathf.Sqrt(5f)); // Golden angle
            float radius = 2f * Mathf.Sqrt(i); // Expanding spiral

            // Calculate position in spherical coordinates
            float x = radius * Mathf.Cos(theta);
            float z = radius * Mathf.Sin(theta);
            float y = branch.position.y; // Maintain vertical position

            // Create new position
            Vector3 newPosition = center + new Vector3(x, y, z);

            // Constrain new position to stay within Mount Qaf
            // Calculate depth ratio based on vertical position (higher = less constraint)
            float heightFromCenter = Mathf.Abs(y);
            float verticalRange = 20f; // Approximate max height/depth
            float depthRatio = heightFromCenter / verticalRange;

            // Higher elements get more space, lower elements more constrained
            float safetyFactor = 0.95f - depthRatio * 0.15f; // 0.8 to 0.95

            // Apply constraint
            newPosition = CalculateConstrainedPositionWithinQaf(newPosition, depthRatio, safetyFactor);

            // Transition branch to new position
            StartCoroutine(TransitionToPosition(branch, newPosition));
        }
    }

    // Add supporting methods

    private Vector3 CalculatePerceivedCenter(List<Transform> branches)
    {
        if (branches == null || branches.Count == 0)
            return Vector3.zero;

        Vector3 sum = Vector3.zero;
        foreach (Transform branch in branches)
        {
            if (branch != null)
                sum += branch.position;
        }

        return sum / branches.Count;
    }

    private float CalculateLuminousRank(Transform branch)
    {
        if (branch == null)
            return 0f;

        // Calculate rank based on distance from center and material properties
        float distanceFromCenter = Vector3.Distance(branch.position, Vector3.zero);
        float normalizedDistance = Mathf.Clamp01(distanceFromCenter / maxRadius);

        // Check if the branch has a line renderer (most branches do in this visualization)
        LineRenderer lineRenderer = branch.GetComponent<LineRenderer>();
        float materialFactor = 0f;

        if (lineRenderer != null && lineRenderer.material != null)
        {
            // Estimate luminous rank from material brightness
            Color color = lineRenderer.material.color;
            materialFactor = (color.r + color.g + color.b) / 3f; // Average RGB as brightness
        }

        // Combine factors - closer branches with brighter materials have lower rank (higher importance)
        return normalizedDistance * (1f - materialFactor * 0.5f);
    }

    private IEnumerator TransitionToPosition(Transform target, Vector3 newPosition)
    {
        if (target == null)
            yield break;

        Vector3 startPosition = target.position;
        float duration = 2.0f; // Duration of transition in seconds
        float elapsedTime = 0f;

        // Apply ishrāqī easing function - accelerates then decelerates
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / duration;

            // Ishrāqī easing function - represents principle of gradual illumination
            float t = 1f - Mathf.Pow(1f - normalizedTime, 3f);

            // Update position
            target.position = Vector3.Lerp(startPosition, newPosition, t);

            yield return null;
        }

        // Ensure final position is exact
        target.position = newPosition;
    }

    private IEnumerator TransitionCameraToPosition(Transform camera, Vector3 newPosition)
    {
        if (camera == null)
            yield break;

        Vector3 startPosition = camera.position;
        float duration = 3.0f; // Camera moves more slowly for comfort in VR
        float elapsedTime = 0f;

        // Store initial forward direction
        Vector3 initialForward = camera.forward;

        // Calculate target forward direction (looking at center)
        Vector3 targetForward = (transform.position - newPosition).normalized;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / duration;

            // Smooth easing function for camera movement (VR comfort)
            float t = normalizedTime * normalizedTime * (3f - 2f * normalizedTime);

            // Update position
            camera.position = Vector3.Lerp(startPosition, newPosition, t);

            // Gradually rotate to face center
            Vector3 currentForward = Vector3.Slerp(initialForward, targetForward, t);
            camera.rotation = Quaternion.LookRotation(currentForward);

            yield return null;
        }

        // Ensure final position is exact
        camera.position = newPosition;
        camera.rotation = Quaternion.LookRotation(targetForward);
    }

    private void ApplyVisualOrientationCues()
    {
        // Create subtle visual cues to help orient the viewer according to ishrāqī principles

        // 1. Create a light beam indicating the eastern direction (source of illumination)
        GameObject orientationBeam = new GameObject("OrientationBeam");
        orientationBeam.transform.SetParent(transform, false);
        orientationBeam.transform.position = Vector3.zero;

        LineRenderer beam = orientationBeam.AddComponent<LineRenderer>();
        beam.material = new Material(vrShader);
        beam.material.color = new Color(1.0f, 0.9f, 0.7f, 0.3f); // Subtle golden light
        beam.startWidth = 0.2f;
        beam.endWidth = 0.1f;
        beam.positionCount = 2;

        // The beam extends eastward
        beam.SetPosition(0, Vector3.zero);
        beam.SetPosition(1, GetCosmicEastDirection() * maxRadius * 0.5f);

        // Add subtle pulsation
        StartCoroutine(PulsateOrientationCue(beam));

        // 2. Create subtle particle effect at ideal viewpoint
        if (!isMobileVR) // Skip on mobile VR for performance
        {
            GameObject viewpointMarker = new GameObject("ViewpointMarker");
            viewpointMarker.transform.SetParent(transform, false);
            viewpointMarker.transform.position = CalculateIdealViewpoint();

            ParticleSystem particles = viewpointMarker.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.startSize = 0.1f;
            main.startLifetime = 2.0f;
            main.startSpeed = 0.2f;
            main.maxParticles = 50;

            var emission = particles.emission;
            emission.rateOverTime = 10;

            var colorOverLifetime = particles.colorOverLifetime;
            colorOverLifetime.enabled = true;

            // Create gradient for particles
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(new Color(1.0f, 0.9f, 0.7f), 0.0f),
                    new GradientColorKey(new Color(0.7f, 0.8f, 1.0f), 1.0f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(0.0f, 0.0f),
                    new GradientAlphaKey(0.5f, 0.2f),
                    new GradientAlphaKey(0.5f, 0.8f),
                    new GradientAlphaKey(0.0f, 1.0f)
                }
            );

            colorOverLifetime.color = gradient;
        }
    }

    private Vector3 GetCosmicEastDirection()
    {
        // In illuminationist cosmology, "East" is not literal geographical east
        // but the direction from which divine light emanates

        // If we have both a tree and fountain, the east direction is perpendicular to their connection
        if (tubaTree != null && fountainOfLife != null)
        {
            Vector3 treeToFountain = fountainOfLife.transform.position - tubaTree.transform.position;
            Vector3 up = Vector3.up;
            cosmicEastDirection = Vector3.Cross(treeToFountain, up).normalized;
        }

        return cosmicEastDirection;
    }

    private void GatherAllBranches(Transform parent, List<Transform> branches)
    {
        if (parent == null)
            return;

        // Add LineRenderer components as branches (most of the structure uses LineRenderers)
        LineRenderer lineRenderer = parent.GetComponent<LineRenderer>();
        if (lineRenderer != null)
        {
            branches.Add(parent);
        }

        // Recursively search children
        foreach (Transform child in parent)
        {
            GatherAllBranches(child, branches);
        }
    }

    private float CalculateLuminousIntensity(Transform branch)
    {
        if (branch == null)
            return 0f;

        // Base intensity on distance from center and material properties
        float distanceFromCenter = Vector3.Distance(branch.position, Vector3.zero);
        float normalizedDistance = Mathf.Clamp01(distanceFromCenter / maxRadius);

        // Distance component: closer elements have higher intensity
        float distanceComponent = 1f - normalizedDistance;

        // Material component: based on color brightness
        float materialComponent = 0.5f; // Default midpoint

        LineRenderer lineRenderer = branch.GetComponent<LineRenderer>();
        if (lineRenderer != null && lineRenderer.material != null)
        {
            Color color = lineRenderer.material.color;
            materialComponent = (color.r + color.g + color.b) / 3f; // Average RGB as brightness
        }

        // Combine components with Suhrawardi's illuminationist formula
        // Intensity = base illumination * material receptivity / (distance factor)²
        float luminousIntensity = materialComponent * distanceComponent * distanceComponent;

        return luminousIntensity;
    }

    // Add a method to call these functions after the cosmic structure is fully generated
    private void ApplyIlluminationistPrinciples()
    {
        // Give time for all elements to initialize
        StartCoroutine(DelayedIlluminationistAdjustments());
    }

    private IEnumerator DelayedIlluminationistAdjustments()
    {
        // Wait for initialization to complete
        yield return new WaitForSeconds(3f);

        // Apply illuminationist reorganization
        RecenterBranchesAroundLuminousAxis();
        yield return new WaitForSeconds(1f);

        RedistributeBranchesAccordingToLuminousProportions();
        yield return new WaitForSeconds(2f);

        // Finally, adjust viewer position
        ReorientObserverPosition();
    }

    private IEnumerator PulsateOrientationCue(LineRenderer cue)
    {
        if (cue == null) yield break;

        float time = 0f;
        while (cue != null)
        {
            time += Time.deltaTime;

            // Calculate subtle pulsation
            float pulse = (Mathf.Sin(time * 0.5f) + 1f) * 0.5f; // 0-1 range, slow

            // Update color alpha
            Color color = cue.material.color;
            color.a = 0.1f + pulse * 0.2f; // Very subtle
            cue.material.color = color;

            // Vary width slightly
            float width = 0.1f + pulse * 0.1f;
            cue.startWidth = width + 0.1f;
            cue.endWidth = width;

            yield return null;
        }
    }

    // Add this helper method after the PulsateOrientationCue method and before the TreeFountainWaveController class

    /// <summary>
    /// Helper method to ensure any position stays within the boundaries of Mount Qaf
    /// Uses illuminationist principles of containment within cosmic structure
    /// </summary>
    private Vector3 CalculateConstrainedPositionWithinQaf(Vector3 originalPosition, float depthRatio = 0f, float safetyFactor = 0.9f)
    {
        // If mountainRadii is not initialized or empty, return original position
        if (mountainRadii == null || mountainRadii.Length == 0)
            return originalPosition;

        // Get the maximum radius of Mount Qaf (outermost mountain)
        float maxQafRadius = mountainRadii[mountainRadii.Length - 1] * safetyFactor;

        // Calculate allowed radius based on depth (deeper elements curve inward more aggressively)
        float maxAllowedRadius = maxQafRadius;
        if (depthRatio > 0)
        {
            // More aggressive inward curve: as depth increases, allowed radius decreases faster
            // Original formula was (1.0f - 0.2f * depthRatio) which was too generous
            maxAllowedRadius *= (1.0f - 0.4f * Mathf.Pow(depthRatio, 1.2f));
        }

        // Extract horizontal components (x and z)
        Vector3 horizontalPosition = new Vector3(originalPosition.x, 0, originalPosition.z);
        float currentRadius = horizontalPosition.magnitude;

        // If already within constraints, return original position
        if (currentRadius <= maxAllowedRadius)
            return originalPosition;

        // Otherwise, scale the horizontal components to fit within constraints
        float scaleFactor = maxAllowedRadius / currentRadius;

        // Return constrained position with original y-value preserved
        return new Vector3(
            originalPosition.x * scaleFactor,
            originalPosition.y,
            originalPosition.z * scaleFactor
        );
    }

    // Add this new method to SuhrawardiVRCosmology class:
    private IEnumerator ApplyCosmicRelationshipsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        relationshipManager.RecalculateAllConstraints();
        Log("Applied cosmic relationship constraints based on illuminationist principles");
    }

    // Also expose the maxQafRadius field to the public for the RelationshipManager to access
    public float GetOutermostQafRadius()
    {
        if (mountainRadii != null && mountainRadii.Length > 0)
        {
            return mountainRadii[mountainRadii.Length - 1];
        }
        return 40f; // Default fallback
    }

    // In the SuhrawardiVRCosmology class, add this method after the ApplyCosmicRelationshipsAfterDelay method
    private void CreateRootBoundaryDebugger()
    {
        GameObject debuggerObj = new GameObject("RootBoundaryDebugger");
        debuggerObj.transform.SetParent(transform);
        RootBoundaryDebugger debugger = debuggerObj.AddComponent<RootBoundaryDebugger>();

        // Set reference to outermost mountain for boundary checking
        if (mountQafContainer != null && mountainLayers.Length > 0)
        {
            // Find outermost mountain layer
            Transform outerMountain = null;
            for (int i = mountainLayers.Length - 1; i >= 0; i--)
            {
                if (mountainLayers[i] != null)
                {
                    outerMountain = mountainLayers[i].transform;
                    break;
                }
            }

            if (outerMountain != null)
            {
                debugger.qafBoundary = outerMountain;
                Debug.Log("Root boundary debugger initialized with outermost mountain reference.");
            }
        }
    }

    private void GenerateJewel()
    {
        nightIlluminatingJewel = new GameObject("NightIlluminatingJewel");
        nightIlluminatingJewel.transform.SetParent(transform);
        nightIlluminatingJewel.transform.localPosition = new Vector3(5, 5, 0);
        
        // Create faceted structure using line renderers
        CreateSacredGeometryForm(nightIlluminatingJewel.transform, 2.0f, 20, jewelMaterial);
        
        // Start animation
        StartCoroutine(AnimateJewelObject());
    }
    
    private void CreateSacredGeometryForm(Transform parent, float radius, int faceCount, Material material)
    {
        // Create edges based on platonic solid structure
        int edgeCount = faceCount * 3 / 2;
        
        for (int i = 0; i < edgeCount; i++)
        {
            GameObject edge = new GameObject($"Edge_{i}");
            edge.transform.SetParent(parent);
            
            LineRenderer edgeRenderer = edge.AddComponent<LineRenderer>();
            edgeRenderer.material = material;
            edgeRenderer.startWidth = 0.05f;
            edgeRenderer.endWidth = 0.05f;
            edgeRenderer.positionCount = 2;
            
            // Calculate vertices using golden ratio
            float phi = (1f + Mathf.Sqrt(5f)) / 2f;
            float angle1 = i * Mathf.PI * 2f / edgeCount;
            float angle2 = angle1 + Mathf.PI * 2f / (faceCount / 2);
            
            float x1 = Mathf.Cos(angle1) * radius;
            float y1 = Mathf.Sin(angle1 * phi) * radius * 0.8f;
            float z1 = Mathf.Sin(angle1) * radius;
            
            float x2 = Mathf.Cos(angle2) * radius;
            float y2 = Mathf.Sin(angle2 * phi) * radius * 0.8f;
            float z2 = Mathf.Sin(angle2) * radius;
            
            edgeRenderer.SetPosition(0, new Vector3(x1, y1, z1));
            edgeRenderer.SetPosition(1, new Vector3(x2, y2, z2));
        }
    }
    
    private void GenerateWorkshops()
    {
        GameObject workshops = new GameObject("Workshops");
        workshops.transform.SetParent(transform);
        
        // Create 12 workshops in circular pattern
        int count = 12;
        float radius = 20f;
        
        for (int i = 0; i < count; i++)
        {
            float angle = i * Mathf.PI * 2 / count;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            float y = (i % 2 == 0) ? 4 : -4;
            
            GameObject workshop = new GameObject($"Workshop_{i}");
            workshop.transform.SetParent(workshops.transform);
            workshop.transform.localPosition = new Vector3(x, y, z);
            
            // Create wireframe cube
            CreateWireframeCube(workshop.transform, 2f, new Material(vrShader) { 
                color = new Color(0.7f, 0.5f, 1f) 
            });
        }
    }
    
    private void CreateWireframeCube(Transform parent, float size, Material material)
    {
        Vector3[] vertices = new Vector3[] {
            new Vector3(-1, -1, -1), new Vector3(1, -1, -1),
            new Vector3(1, -1, 1), new Vector3(-1, -1, 1),
            new Vector3(-1, 1, -1), new Vector3(1, 1, -1),
            new Vector3(1, 1, 1), new Vector3(-1, 1, 1)
        };
        
        int[][] edges = new int[][] {
            new int[] {0, 1}, new int[] {1, 2}, new int[] {2, 3}, new int[] {3, 0},
            new int[] {4, 5}, new int[] {5, 6}, new int[] {6, 7}, new int[] {7, 4},
            new int[] {0, 4}, new int[] {1, 5}, new int[] {2, 6}, new int[] {3, 7}
        };
        
        for (int i = 0; i < edges.Length; i++)
        {
            GameObject edge = new GameObject($"Edge_{i}");
            edge.transform.SetParent(parent);
            
            LineRenderer lineRenderer = edge.AddComponent<LineRenderer>();
            lineRenderer.material = material;
            lineRenderer.startWidth = 0.05f;
            lineRenderer.endWidth = 0.05f;
            lineRenderer.positionCount = 2;
            
            lineRenderer.SetPosition(0, vertices[edges[i][0]] * size/2);
            lineRenderer.SetPosition(1, vertices[edges[i][1]] * size/2);
        }
    }
    
    private IEnumerator AnimateJewelObject()
    {
        float time = 0;
        
        while (true)
        {
            time += Time.deltaTime * 0.2f;
            
            if (nightIlluminatingJewel != null)
            {
                float radius = 8f;
                float x = Mathf.Cos(time) * radius;
                float z = Mathf.Sin(time) * radius;
                float y = 5f + Mathf.Sin(time * 0.5f) * 1f;
                
                nightIlluminatingJewel.transform.localPosition = new Vector3(x, y, z);
            }
            
            yield return null;
        }
    }
} // End of SuhrawardiVRCosmology class

// Component that manages wave dynamics between Tree and Fountain
public class TreeFountainWaveController : MonoBehaviour
{
    private GameObject treeSource; // Tuba Tree
    private GameObject fountainSource; // Fountain of Life
    private Shader waveShader;

    // Wave visualization elements
    private LineRenderer[] waveLines;
    private ParticleSystem waveParticleSystem;

    // Wave physics parameters
    private float waveFrequency = 1.2f;
    private float waveAmplitude = 1.0f;
    private float waveSpeed = 0.8f;
    private float waveDensity = 8f; // Number of wave lines
    private int pointsPerWave = 50;

    // State tracking
    private float elapsedTime = 0f;
    private bool isMobileVR = false;

    public void Initialize(GameObject tree, GameObject fountain, Shader shader)
    {
        this.treeSource = tree;
        this.fountainSource = fountain;
        this.waveShader = shader;

        // Check if we're on mobile VR
        isMobileVR = Application.isMobilePlatform || SystemInfo.deviceType == DeviceType.Handheld;

        // Create visualization elements
        CreateWaveVisualization();
        CreateParticleSystem();

        // Add interaction component
        gameObject.AddComponent<InteractableElement>().elementType = ElementType.Field;
    }

    private void CreateWaveVisualization()
    {
        // Create wave lines with different phases
        int lineCount = isMobileVR ? 4 : 8;
        waveLines = new LineRenderer[lineCount];

        for (int i = 0; i < lineCount; i++)
        {
            GameObject waveLine = new GameObject($"WaveLine_{i}");
            waveLine.transform.SetParent(transform, false);

            LineRenderer line = waveLine.AddComponent<LineRenderer>();

            // Create material for this line
            Material waveMaterial = new Material(waveShader);

            // Determine color based on source - gradient from tree to fountain
            Color treeColor = new Color(0f, 0.8f, 0.2f); // Green for Tree
            Color fountainColor = new Color(0f, 0.7f, 1.0f); // Blue for Fountain

            // Create gradient
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(treeColor, 0.0f),
                    new GradientColorKey(fountainColor, 1.0f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(0.8f, 0.0f),
                    new GradientAlphaKey(0.4f, 0.5f),
                    new GradientAlphaKey(0.8f, 1.0f)
                }
            );

            line.colorGradient = gradient;
            line.material = waveMaterial;

            // Set line properties
            line.startWidth = 0.15f;
            line.endWidth = 0.15f;
            line.positionCount = pointsPerWave;

            // Store reference
            waveLines[i] = line;
        }
    }

    private void CreateParticleSystem()
    {
        // Create particle system for quantum representation
        GameObject particleObj = new GameObject("WaveParticles");
        particleObj.transform.SetParent(transform, false);

        waveParticleSystem = particleObj.AddComponent<ParticleSystem>();

        // Skip on mobile for performance
        if (isMobileVR)
        {
            waveParticleSystem.gameObject.SetActive(false);
            return;
        }

        // Configure particle system
        var main = waveParticleSystem.main;
        main.startSize = 0.1f;
        main.startSpeed = 2.0f;
        main.startLifetime = 2.0f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        // Emission module
        var emission = waveParticleSystem.emission;
        emission.rateOverTime = 20;

        // Color module
        var colorOverLifetime = waveParticleSystem.colorOverLifetime;
        colorOverLifetime.enabled = true;

        // Create gradient
        Gradient waveGradient = new Gradient();
        waveGradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(0f, 0.8f, 0.2f), 0.0f), // Tree color
                new GradientColorKey(new Color(0f, 0.7f, 1.0f), 1.0f)  // Fountain color
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0.0f, 0.0f),
                new GradientAlphaKey(0.7f, 0.2f),
                new GradientAlphaKey(0.7f, 0.8f),
                new GradientAlphaKey(0.0f, 1.0f)
            }
        );

        colorOverLifetime.color = waveGradient;
    }

    private void Update()
    {
        if (treeSource == null || fountainSource == null) return;

        elapsedTime += Time.deltaTime;

        // Update wave visualization
        UpdateWaves();

        // Update particle system
        if (!isMobileVR && waveParticleSystem != null)
        {
            UpdateParticles();
        }
    }

    private void UpdateWaves()
    {
        // Get positions of tree and fountain
        Vector3 treePos = treeSource.transform.position;
        Vector3 fountainPos = fountainSource.transform.position;

        // Calculate direction and distance
        Vector3 direction = fountainPos - treePos;
        float distance = direction.magnitude;
        Vector3 dirNormalized = direction.normalized;

        // Calculate perpendicular vectors for wave displacement
        Vector3 up = Vector3.up;
        Vector3 right = Vector3.Cross(dirNormalized, up).normalized;

        // Update each wave line with different phase offset
        for (int i = 0; i < waveLines.Length; i++)
        {
            LineRenderer line = waveLines[i];
            if (line == null) continue;

            // Phase offset for this line
            float phaseOffset = (float)i / waveLines.Length * Mathf.PI * 2;

            // Update points along wave
            for (int p = 0; p < pointsPerWave; p++)
            {
                float t = (float)p / (pointsPerWave - 1);

                // Base position along line from tree to fountain
                Vector3 basePos = Vector3.Lerp(treePos, fountainPos, t);

                // Calculate wave displacement
                // Combination of different frequencies creates complex interfering patterns
                float frequency1 = waveFrequency;
                float frequency2 = waveFrequency * 1.5f;

                float phase1 = elapsedTime * waveSpeed + phaseOffset;
                float phase2 = elapsedTime * waveSpeed * 0.7f + phaseOffset;

                // Primary displacement
                float displacement1 = Mathf.Sin(t * waveDensity + phase1) * waveAmplitude;
                float displacement2 = Mathf.Cos(t * waveDensity * 0.5f + phase2) * waveAmplitude * 0.5f;

                // Calculate perpendicular displacement - create 3D wave pattern
                Vector3 waveOffset = right * displacement1 + up * displacement2;

                // Wave amplitude decreases in middle to represent "standing wave" effect
                float amplitudeModulator = 0.5f + 0.5f * Mathf.Sin(t * Mathf.PI); // Peaks at middle

                // Final position with modulated amplitude
                Vector3 finalPos = basePos + waveOffset * amplitudeModulator;

                // Set point position
                line.SetPosition(p, finalPos);
            }
        }
    }

    private void UpdateParticles()
    {
        // Get positions
        Vector3 treePos = treeSource.transform.position;
        Vector3 fountainPos = fountainSource.transform.position;

        // Update particle system shape
        ParticleSystem.ShapeModule shape = waveParticleSystem.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Rectangle;

        // Update position - midpoint between tree and fountain
        Vector3 midPoint = (treePos + fountainPos) / 2f;
        waveParticleSystem.transform.position = midPoint;

        // Calculate orientation
        Vector3 direction = fountainPos - treePos;
        waveParticleSystem.transform.rotation = Quaternion.LookRotation(direction);

        // Scale shape based on distance
        float distance = direction.magnitude;
        shape.scale = new Vector3(distance * 0.8f, 3f, 3f);

        // Update emission rate based on "energy" in the connection
        ParticleSystem.EmissionModule emission = waveParticleSystem.emission;
        float energyFactor = (Mathf.Sin(elapsedTime * 0.5f) + 1f) * 0.5f; // Pulsing energy
        emission.rateOverTime = 10f + energyFactor * 30f;
    }
}

// Component that generates actual interference patterns
public class InterferencePatternGenerator : MonoBehaviour
{
    private GameObject sourceA; // Tree
    private GameObject sourceB; // Fountain
    private Texture2D outputTexture;

    private float wavelengthA = 0.5f; // Tree's emission wavelength
    private float wavelengthB = 0.7f; // Fountain's emission wavelength
    private float speedA = 1.2f;      // Tree's emission speed
    private float speedB = 0.8f;      // Fountain's emission speed

    private float elapsedTime = 0f;
    private bool isMobileVR = false;
    private int updateInterval = 0;
    private const int MOBILE_UPDATE_INTERVAL = 5; // Update texture less frequently on mobile

    public void Initialize(GameObject sourceA, GameObject sourceB, Texture2D texture)
    {
        this.sourceA = sourceA;
        this.sourceB = sourceB;
        this.outputTexture = texture;

        // Check if we're on mobile VR
        isMobileVR = Application.isMobilePlatform || SystemInfo.deviceType == DeviceType.Handheld;

        // Initial texture update
        UpdateInterferencePattern();
    }

    private void Update()
    {
        if (sourceA == null || sourceB == null || outputTexture == null)
            return;

        elapsedTime += Time.deltaTime;

        // On mobile, update texture less frequently to save performance
        if (isMobileVR)
        {
            updateInterval++;
            if (updateInterval >= MOBILE_UPDATE_INTERVAL)
            {
                UpdateInterferencePattern();
                updateInterval = 0;
            }
        }
        else
        {
            // On desktop, update every frame
            UpdateInterferencePattern();
        }
    }

    private void UpdateInterferencePattern()
    {
        // Get positions in local space
        Vector3 localPosA = transform.InverseTransformPoint(sourceA.transform.position);
        Vector3 localPosB = transform.InverseTransformPoint(sourceB.transform.position);

        // Get texture dimensions
        int width = outputTexture.width;
        int height = outputTexture.height;

        // Calculate actual physical interference for each pixel
        Color[] pixels = new Color[width * height];

        // Scale for VR performance
        int step = isMobileVR ? 2 : 1; // Process fewer pixels on mobile

        for (int y = 0; y < height; y += step)
        {
            for (int x = 0; x < width; x += step)
            {
                // Convert to -1 to 1 space
                float nx = (float)x / width * 2 - 1;
                float ny = (float)y / height * 2 - 1;

                // Point position in local space
                Vector3 pointPos = new Vector3(nx * 5f, 0, ny * 5f);

                // Calculate distances from sources
                float distA = Vector3.Distance(pointPos, localPosA);
                float distB = Vector3.Distance(pointPos, localPosB);

                // Calculate wave phases at this point (using actual wave equations)
                float phaseA = (distA / wavelengthA + elapsedTime * speedA) % 1f;
                float phaseB = (distB / wavelengthB + elapsedTime * speedB) % 1f;

                // Calculate amplitudes from sources (diminishing with distance)
                float ampA = 0.5f + 0.5f * Mathf.Sin(phaseA * Mathf.PI * 2);
                float ampB = 0.5f + 0.5f * Mathf.Sin(phaseB * Mathf.PI * 2);

                // Attenuate based on distance
                ampA *= Mathf.Clamp01(1.0f / (distA + 0.1f));
                ampB *= Mathf.Clamp01(1.0f / (distB + 0.1f));

                // Calculate actual physical interference
                float interference = ampA + ampB; // Constructive at 2, destructive at 0

                // Create colors based on sources
                Color colorA = new Color(0, 0.8f, 0.2f); // Tree color
                Color colorB = new Color(0, 0.6f, 1.0f);  // Fountain color

                // Final color based on interference pattern
                Color finalColor;
                if (interference > 1.5f) // Strong constructive interference
                    finalColor = Color.white * (interference - 1.0f);
                else if (interference < 0.5f) // Destructive interference
                    finalColor = Color.black;
                else // Normal mixing
                    finalColor = Color.Lerp(colorA, colorB, (interference - 0.5f) * 2);

                // Set alpha based on intensity
                finalColor.a = Mathf.Clamp01(interference);

                // Apply to texture
                int pixelIndex = y * width + x;
                pixels[pixelIndex] = finalColor;

                // Fill in adjacent pixels on mobile (since we're skipping some)
                if (step > 1 && x < width - step && y < height - step)
                {
                    for (int fy = 0; fy < step; fy++)
                    {
                        for (int fx = 0; fx < step; fx++)
                        {
                            int fillIndex = (y + fy) * width + (x + fx);
                            if (fillIndex < pixels.Length)
                                pixels[fillIndex] = finalColor;
                        }
                    }
                }
            }
        }

        // Apply pixels to texture
        outputTexture.SetPixels(pixels);
        outputTexture.Apply();
    }
}

// Component that creates and animates standing waves
public class StandingWaveGenerator : MonoBehaviour
{
    private GameObject sourceA;
    private GameObject sourceB;
    private int nodeCount;

    private GameObject[] nodes;
    private GameObject[] antinodes;
    private float elapsedTime = 0f;
    private bool isMobileVR = false;
    private Shader vfxShader;

    public void Initialize(GameObject sourceA, GameObject sourceB, int nodeCount)
    {
        this.sourceA = sourceA;
        this.sourceB = sourceB;
        this.nodeCount = nodeCount;

        // Check if we're on mobile VR
        isMobileVR = Application.isMobilePlatform || SystemInfo.deviceType == DeviceType.Handheld;

        // Find appropriate shader
        vfxShader = Shader.Find("Unlit/Color");
        if (vfxShader == null)
            vfxShader = Shader.Find("Mobile/Diffuse");
        if (vfxShader == null)
            vfxShader = Shader.Find("Diffuse");

        CreateStandingWaveVisuals();
    }

    private void CreateStandingWaveVisuals()
    {
        // Calculate direction and distance
        Vector3 direction = sourceB.transform.position - sourceA.transform.position;
        float distance = direction.magnitude;
        Vector3 normalizedDir = direction.normalized;

        // Create arrays for nodes (fixed points) and antinodes (maximum amplitude points)
        nodes = new GameObject[nodeCount];
        antinodes = new GameObject[nodeCount - 1];

        // Create nodes - places where wave amplitude is always zero
        for (int i = 0; i < nodeCount; i++)
        {
            float t = (float)(i + 1) / (nodeCount + 1);
            Vector3 nodePosition = sourceA.transform.position + normalizedDir * distance * t;

            GameObject node = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            node.name = $"Node_{i}";
            node.transform.SetParent(transform, false);
            node.transform.position = nodePosition;
            node.transform.localScale = Vector3.one * 0.3f;

            // Nodes are dark - they represent points of stillness
            Material nodeMaterial = new Material(vfxShader);
            nodeMaterial.color = new Color(0.1f, 0.1f, 0.1f);
            node.GetComponent<Renderer>().material = nodeMaterial;

            nodes[i] = node;
        }

        // Create antinodes - places where wave amplitude is maximum
        for (int i = 0; i < nodeCount - 1; i++)
        {
            // Antinodes are positioned between nodes
            Vector3 antinodePosition = (nodes[i].transform.position + nodes[i + 1].transform.position) / 2f;

            GameObject antinode = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            antinode.name = $"Antinode_{i}";
            antinode.transform.SetParent(transform, false);
            antinode.transform.position = antinodePosition;
            antinode.transform.localScale = Vector3.one * 0.5f;

            // Antinodes are bright - they represent points of maximum intensity
            Material antinodeMaterial = new Material(vfxShader);
            antinodeMaterial.color = new Color(0.9f, 0.9f, 1.0f);
            antinode.GetComponent<Renderer>().material = antinodeMaterial;

            antinodes[i] = antinode;
        }

        // Add interaction component
        gameObject.AddComponent<InteractableElement>().elementType = ElementType.Field;
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;

        // Animate antinodes - they pulsate to show standing wave behavior
        for (int i = 0; i < antinodes.Length; i++)
        {
            if (antinodes[i] != null)
            {
                // Different phase for each antinode
                float phase = elapsedTime * 2f + i * 0.5f;

                // Pulsating scale based on standing wave equation
                float pulsation = 0.5f + 0.3f * Mathf.Sin(phase);
                antinodes[i].transform.localScale = Vector3.one * pulsation;

                // Also adjust brightness
                Renderer renderer = antinodes[i].GetComponent<Renderer>();
                if (renderer != null && renderer.material != null)
                {
                    float brightness = 0.7f + 0.3f * Mathf.Sin(phase);
                    renderer.material.color = new Color(brightness, brightness, brightness * 1.2f);
                }
            }
        }

        // Nodes remain fixed - the defining characteristic of standing waves
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
    DavidsArmor,
    Field
}

// Component to make objects interactable
public class InteractableElement : MonoBehaviour
{
    public ElementType elementType;
}

// Element Wave Connection component
public class ElementWaveConnection : MonoBehaviour
{
    private GameObject sourceA;
    private GameObject sourceB;
    private LineRenderer waveLine;
    private int pointCount = 20;
    private float waveAmplitude = 0.5f;
    private float waveFrequency = 4f;

    public void Initialize(GameObject sourceA, GameObject sourceB, Shader shader)
    {
        this.sourceA = sourceA;
        this.sourceB = sourceB;

        // Create wave line
        waveLine = gameObject.AddComponent<LineRenderer>();
        waveLine.material = new Material(shader);
        waveLine.material.color = new Color(0.7f, 0.7f, 1f, 0.3f);
        waveLine.startWidth = 0.05f;
        waveLine.endWidth = 0.05f;
        waveLine.positionCount = pointCount;

        // Initial update
        UpdateWaveLine(0f, 0.5f);
    }

    public void UpdateWaveState(float time, float globalWave)
    {
        UpdateWaveLine(time, globalWave);
    }

    private void UpdateWaveLine(float time, float globalWave)
    {
        if (sourceA == null || sourceB == null || waveLine == null) return;

        // Update line positions
        for (int i = 0; i < pointCount; i++)
        {
            float t = (float)i / (pointCount - 1);

            // Base position between elements
            Vector3 basePosition = Vector3.Lerp(sourceA.transform.position, sourceB.transform.position, t);

            // Calculate perpendicular vectors for displacement
            Vector3 direction = (sourceB.transform.position - sourceA.transform.position).normalized;
            Vector3 up = Vector3.up;
            Vector3 perpendicular = Vector3.Cross(direction, up).normalized;

            // Calculate wave displacement
            float wave = Mathf.Sin(t * waveFrequency + time * 2f) * waveAmplitude;

            // Modulate wave amplitude by global wave state
            wave *= 0.5f + globalWave * 0.5f;

            // Apply displacement
            Vector3 finalPosition = basePosition + perpendicular * wave;

            waveLine.SetPosition(i, finalPosition);
        }
    }
}

// Instantaneous Presence Controller
public class InstantaneousPresenceController : MonoBehaviour
{
    private GameObject[] cosmicElements;
    private SuhrawardiVRCosmology cosmosGenerator;

    public void Initialize(GameObject[] elements, SuhrawardiVRCosmology generator)
    {
        this.cosmicElements = elements;
        this.cosmosGenerator = generator;
    }

    public void CreateConnectionFlash(GameObject elementA, GameObject elementB)
    {
        if (elementA == null || elementB == null) return;

        // Create flash objects at both elements
        CreateFlash(elementA.transform.position);
        CreateFlash(elementB.transform.position);
    }

    private void CreateFlash(Vector3 position)
    {
        // Create flash object
        GameObject flash = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        flash.name = "ConnectionFlash";
        flash.transform.SetParent(transform, false);
        flash.transform.position = position;
        flash.transform.localScale = Vector3.one * 0.5f;

        // Create material
        Material flashMaterial = new Material(Shader.Find("Unlit/Color"));
        if (flashMaterial == null)
            flashMaterial = new Material(Shader.Find("Mobile/Diffuse"));

        flashMaterial.color = new Color(1f, 0.9f, 0.7f, 0.8f);
        flash.GetComponent<Renderer>().material = flashMaterial;

        // Add light if not on mobile VR
        bool isMobileVR = Application.isMobilePlatform || SystemInfo.deviceType == DeviceType.Handheld;
        if (!isMobileVR)
        {
            Light flashLight = flash.AddComponent<Light>();
            flashLight.type = LightType.Point;
            flashLight.color = new Color(1f, 0.9f, 0.7f);
            flashLight.intensity = 2f;
            flashLight.range = 5f;
        }

        // Start animation and auto-destruction
        StartCoroutine(AnimateAndDestroyFlash(flash));
    }

    private IEnumerator AnimateAndDestroyFlash(GameObject flash)
    {
        if (flash == null) yield break;

        Renderer renderer = flash.GetComponent<Renderer>();
        Light light = flash.GetComponent<Light>();
        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration && flash != null)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / duration;
            float size = Mathf.Lerp(0.5f, 2f, normalizedTime);
            float alpha = Mathf.Lerp(0.8f, 0f, normalizedTime);

            // Scale up and fade out
            flash.transform.localScale = Vector3.one * size;

            if (renderer != null)
            {
                Color color = renderer.material.color;
                color.a = alpha;
                renderer.material.color = color;
            }

            // Fade light intensity
            if (light != null)
            {
                light.intensity = Mathf.Lerp(2f, 0f, normalizedTime);
            }

            yield return null;
        }

        if (flash != null)
            Destroy(flash);
    }
}

// Create a new system controller that manages all cosmic relationships
public class CosmicRelationshipManager : MonoBehaviour
{
    public SuhrawardiVRCosmology cosmologyInstance;

    // Threshold defining Mount Qaf boundary as percentage of max radius
    [Range(0.5f, 0.95f)]
    [SerializeField] private float qafContainmentThreshold = 0.8f;

    // Constraint intensification factor - higher values create more aggressive inward curves
    [Range(1.0f, 4.0f)]
    [SerializeField] private float depthConstraintPower = 2.2f;

    // Reference points for key cosmic elements
    private Dictionary<string, List<Transform>> cosmicElements = new Dictionary<string, List<Transform>>();

    // Cosmic boundaries
    private float maxQafRadius;
    private float maxCosmicDepth = 20f;

    // Cache of materials for connection lines
    private Material connectionMaterial;

    // Container for all relationship lines
    private GameObject relationshipLinesContainer;

    private void Awake()
    {
        if (cosmologyInstance == null)
        {
            cosmologyInstance = FindObjectOfType<SuhrawardiVRCosmology>();
            if (cosmologyInstance == null)
            {
                Debug.LogError("CosmicRelationshipManager requires a reference to SuhrawardiVRCosmology");
                enabled = false;
                return;
            }
        }

        relationshipLinesContainer = new GameObject("RelationshipLines");
        relationshipLinesContainer.transform.SetParent(transform);
    }

    private void Start()
    {
        // Initialize materials
        InitializeMaterials();

        // Initial application of constraints
        ApplyCosmicConstraints();
    }

    private void InitializeMaterials()
    {
        Shader lineShader = Shader.Find("Unlit/Color");
        if (lineShader == null)
        {
            lineShader = Shader.Find("Universal Render Pipeline/Unlit");
        }

        connectionMaterial = new Material(lineShader);
        connectionMaterial.color = new Color(0.7f, 0.9f, 1.0f, 0.5f);
    }

    // Apply system-wide rules that maintain proper cosmic relationships
    public void ApplyCosmicConstraints()
    {
        // 1. Register all elements in the cosmos
        GatherCosmicElements();

        // 2. Calculate cosmic center and boundaries
        CalculateCosmicBoundaries();

        // 3. Apply inward spiral constraint (uses illuminationist logarithmic ratios)
        ApplyInwardSpiralConstraint();

        // 4. Verify cross-element relationships are maintained
        VerifyCosmicRelationships();

        // 5. Apply final adjustments to ensure all elements remain within Qaf
        ApplyFinalQafConstraints();
    }

    // Collect all cosmic elements from the SuhrawardiVRCosmology instance
    private void GatherCosmicElements()
    {
        cosmicElements.Clear();

        // Create categories for different element types
        cosmicElements["Mountains"] = new List<Transform>();
        cosmicElements["Roots"] = new List<Transform>();
        cosmicElements["Workshops"] = new List<Transform>();
        cosmicElements["Trees"] = new List<Transform>();
        cosmicElements["Fountains"] = new List<Transform>();
        cosmicElements["Jewels"] = new List<Transform>();
        cosmicElements["ArmorRings"] = new List<Transform>();
        cosmicElements["Fields"] = new List<Transform>();

        // Find elements by tag or through the cosmology instance
        if (cosmologyInstance != null)
        {
            // Find all root elements
            Transform rootsParent = cosmologyInstance.transform.Find("RootSystem");
            if (rootsParent != null)
            {
                // Find all SubRoot objects
                foreach (Transform child in rootsParent)
                {
                    if (child.name.StartsWith("RootLattice_") || child.name.StartsWith("SubRoot_"))
                    {
                        cosmicElements["Roots"].Add(child);
                    }
                }
            }

            // Get workshops
            Transform workshopsParent = cosmologyInstance.transform.Find("Workshops");
            if (workshopsParent != null)
            {
                foreach (Transform workshop in workshopsParent)
                {
                    cosmicElements["Workshops"].Add(workshop);
                }
            }

            // Get Mount Qaf mountains
            Transform qafParent = cosmologyInstance.transform.Find("MountQaf");
            if (qafParent != null)
            {
                foreach (Transform mountain in qafParent)
                {
                    if (mountain.name.StartsWith("Mountain_"))
                    {
                        cosmicElements["Mountains"].Add(mountain);
                    }
                }
            }

            // Get Tree
            Transform tree = cosmologyInstance.transform.Find("TubaTree");
            if (tree != null)
            {
                cosmicElements["Trees"].Add(tree);
            }

            // Get Fountain
            Transform fountain = cosmologyInstance.transform.Find("FountainOfLife");
            if (fountain != null)
            {
                cosmicElements["Fountains"].Add(fountain);
            }

            // Get Jewel
            Transform jewel = cosmologyInstance.transform.Find("NightIlluminatingJewel");
            if (jewel != null)
            {
                cosmicElements["Jewels"].Add(jewel);
            }

            // Get Armor Rings
            Transform armor = cosmologyInstance.transform.Find("DavidsArmor");
            if (armor != null)
            {
                foreach (Transform ring in armor)
                {
                    if (ring.name.StartsWith("ArmorRing_"))
                    {
                        cosmicElements["ArmorRings"].Add(ring);
                    }
                }
            }

            // Get Energy Fields
            Transform fields = cosmologyInstance.transform.Find("Fields");
            if (fields != null)
            {
                foreach (Transform field in fields)
                {
                    cosmicElements["Fields"].Add(field);
                }
            }
        }

        // Log element counts for debugging
        foreach (var category in cosmicElements)
        {
            Debug.Log($"Found {category.Value.Count} {category.Key}");
        }
    }

    // Calculate the boundaries of the cosmic structure
    private void CalculateCosmicBoundaries()
    {
        // Get the largest mountain radius as the Qaf boundary
        if (cosmicElements.ContainsKey("Mountains") && cosmicElements["Mountains"].Count > 0)
        {
            // Get the outermost mountain radius from the SuhrawardiVRCosmology
            if (cosmologyInstance != null)
            {
                // Access the mountainRadii array via reflection if needed
                System.Reflection.FieldInfo mountainRadiiField = typeof(SuhrawardiVRCosmology).GetField("mountainRadii",
                                                                  System.Reflection.BindingFlags.NonPublic |
                                                                  System.Reflection.BindingFlags.Instance);

                if (mountainRadiiField != null)
                {
                    float[] mountainRadii = (float[])mountainRadiiField.GetValue(cosmologyInstance);
                    if (mountainRadii != null && mountainRadii.Length > 0)
                    {
                        maxQafRadius = mountainRadii[mountainRadii.Length - 1];
                    }
                }
                else
                {
                    // Fallback: estimate from mountain positions
                    float maxRadius = 0f;
                    foreach (Transform mountain in cosmicElements["Mountains"])
                    {
                        float radius = new Vector3(mountain.position.x, 0, mountain.position.z).magnitude;
                        if (radius > maxRadius)
                        {
                            maxRadius = radius;
                        }
                    }
                    maxQafRadius = maxRadius;
                }
            }

            // Find the maximum depth of any root element
            if (cosmicElements.ContainsKey("Roots") && cosmicElements["Roots"].Count > 0)
            {
                float maxDepth = 0f;
                foreach (Transform root in cosmicElements["Roots"])
                {
                    if (-root.position.y > maxDepth)
                    {
                        maxDepth = -root.position.y;
                    }
                }
                if (maxDepth > 0)
                {
                    maxCosmicDepth = maxDepth;
                }
            }
        }

        Debug.Log($"Cosmic Boundaries: Max Qaf Radius = {maxQafRadius}, Max Depth = {maxCosmicDepth}");
    }

    // This function implements the crucial logarithmic spiral that all roots must follow
    // According to illuminationist principles, deeper elements curve more sharply inward
    private void ApplyInwardSpiralConstraint()
    {
        if (!cosmicElements.ContainsKey("Roots")) return;

        foreach (Transform rootElement in cosmicElements["Roots"])
        {
            // Process all LineRenderer components which define root paths
            LineRenderer[] lineRenderers = rootElement.GetComponentsInChildren<LineRenderer>();
            foreach (LineRenderer line in lineRenderers)
            {
                // Skip if no line renderer found
                if (line == null) continue;

                // For each point in the line
                Vector3[] positions = new Vector3[line.positionCount];
                line.GetPositions(positions);

                for (int i = 0; i < positions.Length; i++)
                {
                    // Get depth ratio (how far down the root extends)
                    float depthRatio = Mathf.Abs(positions[i].y) / maxCosmicDepth;

                    // Apply illuminationist curve formula - deeper elements curve inward logarithmically
                    // The formula represents the principle that light diminishes with ontological distance
                    float horizontalRadius = CalculateAllowedRadiusAtDepth(depthRatio);

                    // Apply constraint while preserving vertical position
                    Vector3 horizontalPos = new Vector3(positions[i].x, 0, positions[i].z);
                    float currentRadius = horizontalPos.magnitude;

                    if (currentRadius > horizontalRadius)
                    {
                        // Scale position to fit within allowed radius
                        float scaleFactor = horizontalRadius / currentRadius;

                        // Apply transformation
                        positions[i] = new Vector3(
                            positions[i].x * scaleFactor,
                            positions[i].y,
                            positions[i].z * scaleFactor
                        );
                    }
                }

                // Update line renderer with new positions
                line.SetPositions(positions);
            }

            // Also process any child objects like workshop nodes
            foreach (Transform child in rootElement)
            {
                if (child.name.Contains("WorkshopNode"))
                {
                    // Get depth ratio (how far down the root extends)
                    float depthRatio = Mathf.Abs(child.position.y) / maxCosmicDepth;

                    // Apply illuminationist curve formula
                    float horizontalRadius = CalculateAllowedRadiusAtDepth(depthRatio);

                    // Apply constraint while preserving vertical position
                    Vector3 horizontalPos = new Vector3(child.position.x, 0, child.position.z);
                    float currentRadius = horizontalPos.magnitude;

                    if (currentRadius > horizontalRadius)
                    {
                        // Scale position to fit within allowed radius
                        float scaleFactor = horizontalRadius / currentRadius;

                        // Apply transformation
                        Vector3 newPosition = new Vector3(
                            child.position.x * scaleFactor,
                            child.position.y,
                            child.position.z * scaleFactor
                        );

                        // Move element to new position
                        child.position = newPosition;
                    }
                }
            }
        }
    }

    // Uses the illuminationist logarithmic spiral formula to calculate allowed radius at each depth
    private float CalculateAllowedRadiusAtDepth(float depthRatio)
    {
        // Higher depthConstraintPower = more aggressive inward curve
        float constraintFactor = Mathf.Pow(depthRatio, depthConstraintPower);

        // Illuminationist formula: radius decreases with depth according to light propagation laws
        return maxQafRadius * qafContainmentThreshold * (1.0f - 0.7f * constraintFactor);
    }

    // This ensures all cross-element relationships are maintained after constraints are applied
    private void VerifyCosmicRelationships()
    {
        // Connect workshop nodes to roots using revised positions
        ConnectWorkshopsToRoots();

        // Connect roots to armor rings while maintaining symbolic meanings
        ConnectRootsToArmorRings();

        // Update energy field connections to reflect new cosmic geometry
        UpdateEnergyFields();
    }

    // Connect workshop nodes to their associated roots
    private void ConnectWorkshopsToRoots()
    {
        if (!cosmicElements.ContainsKey("Workshops") || !cosmicElements.ContainsKey("Roots")) return;

        // Clear previous connection lines
        ClearConnectionLines("WorkshopConnector");

        foreach (Transform workshop in cosmicElements["Workshops"])
        {
            // Find nearest root node
            Transform nearestRoot = FindNearestRoot(workshop.position);
            if (nearestRoot != null)
            {
                CreateConnectionLine(workshop.position, nearestRoot.position, "WorkshopConnector");
            }
        }
    }

    // Find the nearest root element to a given position
    private Transform FindNearestRoot(Vector3 position)
    {
        Transform nearest = null;
        float minDistance = float.MaxValue;

        foreach (Transform root in cosmicElements["Roots"])
        {
            float distance = Vector3.Distance(position, root.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = root;
            }

            // Also check children of the root (like workshop nodes)
            foreach (Transform child in root)
            {
                distance = Vector3.Distance(position, child.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = child;
                }
            }
        }

        return nearest;
    }

    // Connect the roots to the Armor Rings of David (symbolic connection)
    private void ConnectRootsToArmorRings()
    {
        if (!cosmicElements.ContainsKey("Roots") || !cosmicElements.ContainsKey("ArmorRings")) return;

        // Clear previous connection lines
        ClearConnectionLines("ArmorConnector");

        // Connect only the primary roots to armor rings
        List<Transform> primaryRoots = cosmicElements["Roots"].FindAll(r => r.name.StartsWith("RootLattice_"));
        List<Transform> rings = cosmicElements["ArmorRings"];

        // Make connections between the most symbolically aligned elements
        for (int i = 0; i < Mathf.Min(primaryRoots.Count, rings.Count); i++)
        {
            // Find a suitable point on the root (e.g., its midpoint or endpoint)
            Vector3 rootPoint = primaryRoots[i].position;
            Vector3 ringPoint = rings[i].position;

            CreateConnectionLine(rootPoint, ringPoint, "ArmorConnector");
        }
    }

    // Update energy field positions and connections
    private void UpdateEnergyFields()
    {
        if (!cosmicElements.ContainsKey("Fields")) return;

        foreach (Transform field in cosmicElements["Fields"])
        {
            // Calculate depth ratio for the field
            float depthRatio = Mathf.Abs(field.position.y) / maxCosmicDepth;

            // Apply the same constraints to fields as to roots
            float horizontalRadius = CalculateAllowedRadiusAtDepth(depthRatio);

            Vector3 horizontalPos = new Vector3(field.position.x, 0, field.position.z);
            float currentRadius = horizontalPos.magnitude;

            if (currentRadius > horizontalRadius)
            {
                // Scale position to fit within allowed radius
                float scaleFactor = horizontalRadius / currentRadius;

                // Apply transformation
                Vector3 newPosition = new Vector3(
                    field.position.x * scaleFactor,
                    field.position.y,
                    field.position.z * scaleFactor
                );

                // Move field to new position
                field.position = newPosition;

                // Also scale the field's size according to depth
                float depthScale = 1.0f - 0.3f * depthRatio;
                field.localScale = new Vector3(depthScale, depthScale, depthScale);
            }
        }
    }

    // Apply final safety checks to ensure all elements remain inside Mount Qaf
    private void ApplyFinalQafConstraints()
    {
        // Check all categories except Mountains (which define the boundary)
        foreach (var category in cosmicElements)
        {
            if (category.Key == "Mountains") continue;

            foreach (Transform element in category.Value)
            {
                // Get the horizontal distance from center
                Vector3 horizontalPos = new Vector3(element.position.x, 0, element.position.z);
                float radius = horizontalPos.magnitude;

                // If outside Mount Qaf, constrain it
                if (radius > maxQafRadius * qafContainmentThreshold)
                {
                    float scaleFactor = (maxQafRadius * qafContainmentThreshold) / radius;
                    Vector3 newPosition = new Vector3(
                        element.position.x * scaleFactor,
                        element.position.y,
                        element.position.z * scaleFactor
                    );
                    element.position = newPosition;
                }

                // Also check all children of this element
                foreach (Transform child in element)
                {
                    horizontalPos = new Vector3(child.position.x, 0, child.position.z);
                    radius = horizontalPos.magnitude;

                    if (radius > maxQafRadius * qafContainmentThreshold)
                    {
                        float scaleFactor = (maxQafRadius * qafContainmentThreshold) / radius;
                        Vector3 newPosition = new Vector3(
                            child.position.x * scaleFactor,
                            child.position.y,
                            child.position.z * scaleFactor
                        );
                        child.position = newPosition;
                    }
                }
            }
        }
    }

    // Create a visual connection line between two points
    private void CreateConnectionLine(Vector3 start, Vector3 end, string name)
    {
        GameObject lineObj = new GameObject(name);
        lineObj.transform.SetParent(relationshipLinesContainer.transform);

        LineRenderer line = lineObj.AddComponent<LineRenderer>();
        line.material = connectionMaterial;
        line.startWidth = 0.05f;
        line.endWidth = 0.05f;
        line.positionCount = 2;
        line.SetPosition(0, start);
        line.SetPosition(1, end);
    }

    // Clear all connection lines of a certain type
    private void ClearConnectionLines(string nameContains)
    {
        // Find and destroy all children with the specified name pattern
        if (relationshipLinesContainer != null)
        {
            List<Transform> linesToRemove = new List<Transform>();

            foreach (Transform child in relationshipLinesContainer.transform)
            {
                if (child.name.Contains(nameContains))
                {
                    linesToRemove.Add(child);
                }
            }

            foreach (Transform line in linesToRemove)
            {
                DestroyImmediate(line.gameObject);
            }
        }
    }

    // Public method to recalculate all constraints on demand
    public void RecalculateAllConstraints()
    {
        ApplyCosmicConstraints();
    }

    // Used for visual debugging
    private void OnDrawGizmos()
    {
        if (enabled && Application.isPlaying)
        {
            // Draw the Qaf boundary
            Gizmos.color = new Color(1, 0.5f, 0, 0.3f);
            Gizmos.DrawWireSphere(Vector3.zero, maxQafRadius);

            // Draw the constrainted boundary
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawWireSphere(Vector3.zero, maxQafRadius * qafContainmentThreshold);
        }
    }
}

// Add this debug component to your project
public class RootBoundaryDebugger : MonoBehaviour
{
    [Header("References")]
    public Transform qafBoundary; // Reference to outermost mountain
    public Color validColor = Color.green;
    public Color invalidColor = Color.red;
    public bool visualizeConstraints = true;

    [Header("Live Debugging")]
    [SerializeField] private int outOfBoundaryElements = 0;
    [SerializeField] private float maxAllowedRadius = 0;
    [SerializeField] private float biggestViolationDistance = 0;

    private void Start()
    {
        // Defer boundary check to after everything is generated
        StartCoroutine(DelayedBoundaryCheck());
    }

    private IEnumerator DelayedBoundaryCheck()
    {
        // Wait for full initialization
        yield return new WaitForSeconds(2f);

        // Get max radius from Qaf boundary (outermost mountain)
        Renderer qafRenderer = qafBoundary.GetComponent<Renderer>();
        if (qafRenderer != null)
        {
            maxAllowedRadius = qafRenderer.bounds.extents.x * 0.85f; // 85% of boundary
            Debug.Log($"Max allowed radius: {maxAllowedRadius}");
        }

        // Find all root elements - cast wide net to catch everything
        LineRenderer[] allLines = FindObjectsOfType<LineRenderer>();
        Transform[] rootObjects = GameObject.Find("RootWorkshops")?.GetComponentsInChildren<Transform>();

        List<Transform> problematicElements = new List<Transform>();

        // Check line renderers
        foreach (LineRenderer line in allLines)
        {
            for (int i = 0; i < line.positionCount; i++)
            {
                Vector3 worldPos = line.transform.TransformPoint(line.GetPosition(i));
                Vector3 horizontalPos = new Vector3(worldPos.x, 0, worldPos.z);
                float distance = horizontalPos.magnitude;

                if (distance > maxAllowedRadius)
                {
                    biggestViolationDistance = Mathf.Max(biggestViolationDistance, distance);
                    outOfBoundaryElements++;
                    problematicElements.Add(line.transform);

                    if (visualizeConstraints)
                    {
                        VisualizeViolation(worldPos);
                    }

                    // Force correct position immediately - strict enforcement
                    Vector3 correctedPosition = line.GetPosition(i);
                    float correctionFactor = maxAllowedRadius / distance;
                    correctedPosition.x *= correctionFactor;
                    correctedPosition.z *= correctionFactor;
                    line.SetPosition(i, correctedPosition);
                }
            }
        }

        // Check root objects
        if (rootObjects != null)
        {
            foreach (Transform root in rootObjects)
            {
                if (root == null) continue;

                Vector3 horizontalPos = new Vector3(root.position.x, 0, root.position.z);
                float distance = horizontalPos.magnitude;

                if (distance > maxAllowedRadius)
                {
                    biggestViolationDistance = Mathf.Max(biggestViolationDistance, distance);
                    outOfBoundaryElements++;
                    problematicElements.Add(root);

                    if (visualizeConstraints)
                    {
                        VisualizeViolation(root.position);
                    }

                    // Force correct position immediately - strict enforcement
                    Vector3 newPos = root.position;
                    float correctionFactor = maxAllowedRadius / distance;
                    newPos.x *= correctionFactor;
                    newPos.z *= correctionFactor;
                    root.position = newPos;
                }
            }
        }

        Debug.Log($"Found {outOfBoundaryElements} elements violating boundary. " +
                  $"Maximum violation: {biggestViolationDistance} units " +
                  $"(allowed: {maxAllowedRadius})");

        // Process second-pass to ensure no cascading issues
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(ForceBoundaryCompliance());
    }

    private void VisualizeViolation(Vector3 position)
    {
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        marker.transform.position = position;
        marker.transform.localScale = Vector3.one * 0.2f;

        Renderer renderer = marker.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = invalidColor;
        }

        Destroy(marker, 10f); // Clean up after debugging
    }

    private IEnumerator ForceBoundaryCompliance()
    {
        // Final sweep for problematic elements
        var allTransforms = FindObjectsOfType<Transform>();
        foreach (var t in allTransforms)
        {
            if (t.name.Contains("Root") || t.name.Contains("Workshop"))
            {
                Vector3 horizontalPos = new Vector3(t.position.x, 0, t.position.z);
                float distance = horizontalPos.magnitude;

                if (distance > maxAllowedRadius)
                {
                    // Force correct position
                    Vector3 newPos = t.position;
                    float correctionFactor = maxAllowedRadius / distance;
                    newPos.x *= correctionFactor;
                    newPos.z *= correctionFactor;
                    t.position = newPos;
                }
            }
        }

        // Create visualization of valid boundary
        if (visualizeConstraints)
        {
            CreateBoundaryVisualization();
        }

        yield return null;
    }

    private void CreateBoundaryVisualization()
    {
        GameObject boundaryViz = new GameObject("AllowedBoundary");
        boundaryViz.transform.SetParent(transform);

        LineRenderer boundaryLine = boundaryViz.AddComponent<LineRenderer>();
        boundaryLine.startWidth = 0.1f;
        boundaryLine.endWidth = 0.1f;
        boundaryLine.material = new Material(Shader.Find("Unlit/Color"));
        boundaryLine.material.color = validColor;

        int segments = 32;
        boundaryLine.positionCount = segments + 1;

        for (int i = 0; i <= segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            float x = Mathf.Cos(angle) * maxAllowedRadius;
            float z = Mathf.Sin(angle) * maxAllowedRadius;

            boundaryLine.SetPosition(i, new Vector3(x, 0, z));
        }
    }
}