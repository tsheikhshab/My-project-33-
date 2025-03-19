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

        // Start animation coroutines
        StartCoroutine(AnimateJewel());
        StartCoroutine(AnimateIshraqiElements());
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
            
            // Colors based on workshop master (different for 4th and 7th)
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
            
            // Add connection lines to appropriate upper workshops
            ConnectToUpperWorkshops(lowerWorkshop, i);
        }
        
        // Create the "fields" (مزرعه) beneath the seventh workshop
        CreateCosmicField(lowerWorkshopsContainer.transform, lowerY - 8f);
    }

    private void ConnectToUpperWorkshops(GameObject lowerWorkshop, int workshopIndex)
    {
        // Matrix of connections based on Suhrawardi's description
        bool[,] connectionMatrix = new bool[7, 12] {
            {true, true, false, false, false, false, false, false, false, false, false, false}, // 1st master: workshops 1-2
            {false, false, true, true, false, false, false, false, false, false, false, false}, // 2nd master: workshops 3-4
            {false, false, false, false, true, true, false, false, false, false, false, false}, // 3rd master: workshops 5-6
            {false, false, false, false, false, false, true, false, false, false, false, false}, // 4th master: workshop 7
            {false, false, false, false, false, false, false, true, true, false, false, false}, // 5th master: workshops 8-9
            {false, false, false, false, false, false, false, false, false, true, true, false}, // 6th master: workshops 10-11
            {false, false, false, false, false, false, false, false, false, false, false, true}  // 7th master: workshop 12
        };
        
        // Create connections based on the matrix
        for (int i = 0; i < 12; i++)
        {
            if (connectionMatrix[workshopIndex, i] && twelveWorkshops[i] != null)
            {
                CreateVortexConnection(lowerWorkshop.transform, twelveWorkshops[i].transform, workshopIndex);
            }
        }
    }

    private void CreateVortexConnection(Transform lowerWorkshop, Transform upperWorkshop, int masterIndex)
    {
        GameObject connectionObj = new GameObject($"VortexConnection_{masterIndex}_{upperWorkshop.name}");
        connectionObj.transform.SetParent(lowerWorkshop, false);
        
        // Create vortex-like connection
        LineRenderer connection = connectionObj.AddComponent<LineRenderer>();
        
        // Set material based on master - 4th has special material, 7th has earthly material
        Material connectionMaterial = new Material(vrShader);
        if (masterIndex == 3) // 4th master
        {
            connectionMaterial.color = new Color(0.7f, 0.7f, 1f, 0.8f);
        }
        else if (masterIndex == 6) // 7th master
        {
            connectionMaterial.color = new Color(0.3f, 0.5f, 0.3f, 0.8f);
        }
        else
        {
            connectionMaterial.color = new Color(0.6f, 0.3f, 0.6f, 0.7f);
        }
        
        connection.material = connectionMaterial;
        connection.startWidth = 0.2f;
        connection.endWidth = 0.1f;
        
        // Create spiral path - this is key to illuminationist vortex theory
        int segments = 16;
        connection.positionCount = segments;
        
        for (int i = 0; i < segments; i++)
        {
            float t = i / (float)(segments - 1);
            
            // Calculate spiral path
            Vector3 start = lowerWorkshop.position;
            Vector3 end = upperWorkshop.position;
            Vector3 dir = end - start;
            float distance = dir.magnitude;
            
            // Apply logarithmic spiral formula
            float spiralR = t * distance;
            float spiralAngle = t * 4f * Mathf.PI * (masterIndex % 3 + 1); // Varies by master
            
            // Calculate position on spiral
            Vector3 dirNorm = dir.normalized;
            Vector3 perpendicular = Vector3.Cross(dirNorm, Vector3.up).normalized;
            if (perpendicular.magnitude < 0.1f)
                perpendicular = Vector3.Cross(dirNorm, Vector3.forward).normalized;
            
            Vector3 spiral = start + dirNorm * spiralR + 
                             (perpendicular * Mathf.Sin(spiralAngle) + 
                              Vector3.Cross(perpendicular, dirNorm) * Mathf.Cos(spiralAngle)) * 
                             spiralR * 0.2f * (1 - t); // Spiral diminishes as it approaches target
            
            connection.SetPosition(i, spiral);
        }
    }

    private void CreateCosmicField(Transform parent, float yPosition)
    {
        // The "field" (مزرعه) mentioned by Suhrawardi
        // This is the domain of the 7th master
        
        GameObject field = new GameObject("WorkshopField");
        field.transform.SetParent(parent, false);
        field.transform.localPosition = new Vector3(0, yPosition, 0);
        
        // Create field as a radial disc with organic patterns
        GameObject fieldDisc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        fieldDisc.name = "FieldDisc";
        fieldDisc.transform.SetParent(field.transform, false);
        fieldDisc.transform.localPosition = Vector3.zero;
        fieldDisc.transform.localScale = new Vector3(15f, 0.2f, 15f);
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
            
            // Position rings at various points on the field
            float angle = r * Mathf.PI * 2f / ringCount;
            float distance = 3f + r * 1.5f;
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

    private void CreateCorrespondences()
    {
        if (!showCorrespondences) return;
        
        // Create container for all correspondence elements
        correspondencesContainer = new GameObject("IlluminationistCorrespondences");
        correspondencesContainer.transform.SetParent(transform, false);
        
        // Only proceed if we have the key elements
        if (davidsArmor == null || tubaTree == null || fountainOfLife == null) return;
        
        // 1. Connect David's Armor and Tuba Tree (showing they are structurally equivalent)
        CreateCorrespondenceLine(
            davidsArmor.transform.position, 
            tubaTree.transform.position,
            "Armor_Tree_Correspondence"
        );
        
        // 2. Connect Tuba Tree and Fountain of Life (showing channels of spiritual essence)
        CreateCorrespondenceLine(
            tubaTree.transform.position, 
            fountainOfLife.transform.position,
            "Tree_Fountain_Correspondence"
        );
        
        // 3. Connect Tuba Tree and Jewel (illumination source)
        if (nightIlluminatingJewel != null)
        {
            CreateCorrespondenceLine(
                tubaTree.transform.position, 
                nightIlluminatingJewel.transform.position,
                "Tree_Jewel_Correspondence"
            );
        }
        
        // 4. Create root system connecting Tuba Tree to inner mountains
        if (mountainLayers.Length > 3 && mountainLayers[2] != null)
        {
            CreateRootSystem(tubaTree.transform.position, mountainLayers[2].transform);
        }
        
        // 5. Create correspondence node at the spiritual center
        CreateCorrespondenceNode();
        
        // Start animation for correspondence lines
        StartCoroutine(AnimateCorrespondences());
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
    
    private void CreateRootSystem(Vector3 treeBase, Transform innerMountain)
    {
        // Create a root system connecting tree to inner mountains (metaphysically unified)
        int rootCount = isMobileVR ? 8 : 16;
        
        for (int i = 0; i < rootCount; i++)
        {
            float angle = i * 360f / rootCount;
            
            GameObject rootObj = new GameObject($"Root_{i}");
            rootObj.transform.SetParent(correspondencesContainer.transform, false);
            
            LineRenderer root = rootObj.AddComponent<LineRenderer>();
            root.material = sharedEssenceMaterial != null ? sharedEssenceMaterial : new Material(vrShader);
            root.material.color = correspondenceColor;
            root.startWidth = 0.15f;
            root.endWidth = 0.05f;
            
            // Create a curved path for the root
            int segments = 10;
            root.positionCount = segments;
            
            for (int j = 0; j < segments; j++)
            {
                float t = j / (float)(segments - 1);
                float curveAngle = angle + Mathf.Sin(t * Mathf.PI) * 20f;
                float curveRadius = Mathf.Lerp(1f, mountainRadii[2], t);
                
                float x = Mathf.Cos(curveAngle * Mathf.Deg2Rad) * curveRadius;
                float z = Mathf.Sin(curveAngle * Mathf.Deg2Rad) * curveRadius;
                float y = Mathf.Lerp(treeBase.y - 5f, -5f, t);
                
                root.SetPosition(j, new Vector3(x, y, z));
            }
            
            correspondenceLines.Add(root);
        }
    }
    
    private void CreateCorrespondenceNode()
    {
        // Create a central node representing the unified essence
        GameObject nodeObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        nodeObj.name = "UnifiedEssenceNode";
        nodeObj.transform.SetParent(correspondencesContainer.transform, false);
        nodeObj.transform.localPosition = new Vector3(0, 0, 0);
        nodeObj.transform.localScale = Vector3.one * 1.5f;
        
        // Apply shared essence material
        nodeObj.GetComponent<Renderer>().material = sharedEssenceMaterial != null ? 
            sharedEssenceMaterial : new Material(vrShader);
        nodeObj.GetComponent<Renderer>().material.color = correspondenceColor;
        
        // Add light effect for the node
        if (!isMobileVR || activeLights.Count < maxLightsActive)
        {
            GameObject nodeLight = new GameObject("NodeLight");
            nodeLight.transform.SetParent(nodeObj.transform, false);
            
            Light light = nodeLight.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = correspondenceColor;
            light.intensity = 2f;
            light.range = 10f;
            
            RegisterLight(light);
        }
    }
    
    private IEnumerator AnimateCorrespondences()
    {
        float time = 0;
        
        while (true)
        {
            time += Time.deltaTime;
            
            // Pulse effect for correspondence lines
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
                    line.material.color = pulsedColor;
                    
                    // Vary width slightly based on pulse
                    float width = 0.1f + 0.1f * pulse;
                    line.startWidth = width + 0.1f;
                    line.endWidth = width;
                }
            }
            
            // Connect waterflow from Fountain to Tree
            if (fountainOfLife != null && tubaTree != null)
            {
                LineRenderer waterflowLine = correspondenceLines.Find(l => l.gameObject.name == "Tree_Fountain_Correspondence");
                if (waterflowLine != null)
                {
                    // Make this line flow from fountain to tree
                    int segments = waterflowLine.positionCount;
                    Vector3 start = fountainOfLife.transform.position;
                    Vector3 end = tubaTree.transform.position + Vector3.down * 10f;
                    
                    for (int i = 0; i < segments; i++)
                    {
                        float t = (float)i / (segments - 1);
                        Vector3 pos = Vector3.Lerp(start, end, t);
                        
                        // Add flowing water effect
                        pos.y += Mathf.Sin((t + time) * 5f) * 0.2f;
                        waterflowLine.SetPosition(i, pos);
                    }
                }
            }
            
            yield return null;
        }
    }
    
    private IEnumerator AnimateIshraqiElements()
    {
        float time = 0;
        
        while (true)
        {
            time += Time.deltaTime;
            
            // Tuba Tree and David's Armor interaction
            if (tubaTree != null && davidsArmor != null)
            {
                // Make David's Armor subtly respond to the tree
                float pulse = (Mathf.Sin(time * 0.3f) + 1) / 2;
                
                // Find all line renderers in David's Armor
                LineRenderer[] armorLines = davidsArmor.GetComponentsInChildren<LineRenderer>();
                foreach (LineRenderer line in armorLines)
                {
                    if (line != null)
                    {
                        // Subtle color shift based on proximity to tree
                        Color baseColor = armorMaterial.color;
                        Color treeInfluenceColor = treeMaterial.color;
                        line.material.color = Color.Lerp(baseColor, treeInfluenceColor, pulse * 0.2f);
                    }
                }
            }
            
            // Fountain and Jewel interaction
            if (fountainOfLife != null && nightIlluminatingJewel != null)
            {
                // The jewel's light affects the water's appearance
                Transform waterTransform = fountainOfLife.transform.Find("Water");
                if (waterTransform != null)
                {
                    Renderer waterRenderer = waterTransform.GetComponent<Renderer>();
                    if (waterRenderer != null)
                    {
                        // Calculate distance between jewel and fountain
                        float distance = Vector3.Distance(
                            nightIlluminatingJewel.transform.position, 
                            fountainOfLife.transform.position
                        );
                        
                        // Water color influenced by jewel's light
                        float influence = Mathf.Clamp01(5f / distance);
                        Color waterColor = Color.Lerp(
                            waterMaterial.color,
                            jewelPhases[currentJewelPhase].GetComponent<Renderer>().material.color,
                            influence * 0.3f
                        );
                        
                        waterRenderer.material.color = waterColor;
                    }
                }
            }
            
            yield return null;
        }
    }

    private IEnumerator AnimateJewel()
    {
        while (true)
        {
            // Orbit around the tree - representing the jewel's dependence on Tuba Tree's light
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

        // Orient the jewel to face the tree - symbolic of its ontological dependence
        nightIlluminatingJewel.transform.LookAt(transform.position);
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

        // Create the unified David's Armor structure (branches transforming into lattice)
        GenerateUnifiedTreeArmorStructure();

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
            float ringY = -height/2 + (r + 1) * ringSpacing;
            
            GameObject ring = new GameObject($"Ring_{r}");
            ring.transform.SetParent(ringsContainer.transform, false);
            
            LineRenderer ringRenderer = ring.AddComponent<LineRenderer>();
            
            // Use gradient materials - more tree-like near center, more armor-like at edges
            float heightFactor = Mathf.Abs(ringY) / (height/2); // 0 at center, 1 at edges
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
            
            // Root follows logarithmic spiral downward
            float depth = t * maxDepth;
            float spiralFactor = Mathf.Pow(t, 0.7f) * baseRadius * 3f; // Expanding
            
            // Directional spread
            float x = Mathf.Cos(angle + t * 0.8f) * spiralFactor;
            float z = Mathf.Sin(angle + t * 0.8f) * spiralFactor;
            
            // Add naturalistic variation
            float noiseX = Mathf.PerlinNoise(t * 5f, rootIndex) * t * 2f;
            float noiseZ = Mathf.PerlinNoise(t * 5f, rootIndex + 10) * t * 2f;
            
            // Position with slight variation
            Vector3 pos = new Vector3(x + noiseX, -depth, z + noiseZ);
            mainRoot.SetPosition(i, pos);
            
            // Create workshop nodes at interval points
            if (i > 0 && i % 4 == 0 && i < segments - 2)
            {
                CreateWorkshopNode(parent, pos, rootMaterial.color, rootIndex, i / 4);
            }
        }
        
        // NEW: Create horizontal connector rings at various depths
        int ringCount = 4; // Fewer than above to represent condensation of reality
        float ringSpacing = maxDepth / (ringCount + 1);
        
        for (int r = 0; r < ringCount; r++)
        {
            float depth = (r + 1) * ringSpacing;
            CreateRootLatticeRing(rootLattice.transform, rootIndex, depth, baseRadius * (0.5f + r * 0.3f));
        }
        
        // NEW: Create diagonal connectors between rings to form lattice
        CreateRootLatticeConnectors(rootLattice.transform, rootIndex, ringCount, ringSpacing, baseRadius);
        
        // Add sub-roots for complexity
        int subRootCount = rootIndex == 6 ? 5 : 3; // 7th master has more sub-roots
        for (int s = 0; s < subRootCount; s++)
        {
            // Start from different points along main root
            int startSegment = 4 + s * 4;
            if (startSegment < segments - 5)
            {
                Vector3 startPos = mainRoot.GetPosition(startSegment);
                CreateSubRoot(parent, startPos, rootMaterial.color, angle, rootIndex, s);
            }
        }
    }

    private void CreateRootLatticeRing(Transform parent, int rootIndex, float depth, float radius)
    {
        GameObject ring = new GameObject($"RootRing_{rootIndex}_{depth}");
        ring.transform.SetParent(parent, false);
        
        LineRenderer ringRenderer = ring.AddComponent<LineRenderer>();
        
        // Material based on rootIndex
        Material ringMaterial = new Material(vrShader);
        
        // Colors follow Suhrawardi's description of masters
        if (rootIndex == 3) // 4th master with finest robe
        {
            ringMaterial.color = new Color(0.5f, 0.6f, 0.9f); // Slightly darker blue than main root
        }
        else if (rootIndex == 6) // 7th master with no robe but field authority
        {
            ringMaterial.color = new Color(0.3f, 0.5f, 0.15f); // Slightly darker green than main root
        }
        else // Regular masters
        {
            float hue = 0.7f + (rootIndex / 20f); // Purplish variations
            ringMaterial.color = Color.HSVToRGB(hue, 0.5f, 0.6f); // Slightly darker than main root
        }
        
        ringRenderer.material = ringMaterial;
        ringRenderer.startWidth = 0.2f;
        ringRenderer.endWidth = 0.2f;
        
        // Create smaller ring with fewer segments
        int segments = 16; // Fewer than above, representing condensation
        ringRenderer.positionCount = segments + 1;
        
        // Create ring with variations
        for (int i = 0; i <= segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            
            // Variable radius to create organic look
            float variableFactor = 1f + 0.2f * Mathf.Sin(angle * 3f + depth * 0.5f);
            float ringRadius = radius * variableFactor;
            
            // Position with depth
            float x = Mathf.Cos(angle) * ringRadius;
            float z = Mathf.Sin(angle) * ringRadius;
            
            ringRenderer.SetPosition(i, new Vector3(x, -depth, z));
        }
    }

    private void CreateRootLatticeConnectors(Transform parent, int rootIndex, int ringCount, float ringSpacing, float baseRadius)
    {
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
                
                // Increasing radius as we go deeper
                float ringRadius = baseRadius * (0.5f + r * 0.3f);
                
                // Position with twist
                float x = Mathf.Cos(connectorAngle) * ringRadius;
                float z = Mathf.Sin(connectorAngle) * ringRadius;
                
                connectorRenderer.SetPosition(r, new Vector3(x, -depth, z));
            }
        }
    }

    private void CreateSubRoot(Transform parent, Vector3 startPos, Color baseColor, float mainAngle, int rootIndex, int subIndex)
    {
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
        
        // Create path for sub-root
        int subSegments = 10;
        subRenderer.positionCount = subSegments;
        
        // Angle divergence from main root
        float angleOffset = Mathf.PI / 4f * (subIndex % 2 == 0 ? 1 : -1);
        float subAngle = mainAngle + angleOffset;
        
        for (int i = 0; i < subSegments; i++)
        {
            float t = i / (float)(subSegments - 1);
            
            // Diverging path
            float depth = t * 8f;
            float spreadRadius = t * 4f;
            
            // Calculate position
            float x = Mathf.Cos(subAngle) * spreadRadius;
            float z = Mathf.Sin(subAngle) * spreadRadius;
            
            // Add small variations
            float noiseX = Mathf.PerlinNoise(t * 3f, subIndex) * t;
            float noiseZ = Mathf.PerlinNoise(t * 3f, subIndex + 5) * t;
            
            // Final position
            Vector3 pos = startPos + new Vector3(x + noiseX, -depth, z + noiseZ);
            subRenderer.SetPosition(i, pos);
            
            // Create a smaller workshop node near the end
            if (i == subSegments - 2)
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
        // The field (مزرعه) mentioned by Suhrawardi
        // This is the domain of the 7th master
        
        GameObject field = new GameObject("WorkshopField");
        field.transform.SetParent(parent, false);
        field.transform.localPosition = new Vector3(0, depth, 0);
        
        // Create field as a radial disc with organic patterns
        GameObject fieldDisc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        fieldDisc.name = "FieldDisc";
        fieldDisc.transform.SetParent(field.transform, false);
        fieldDisc.transform.localPosition = Vector3.zero;
        fieldDisc.transform.localScale = new Vector3(15f, 0.2f, 15f);
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
            
            // Position rings at various points on the field
            float angle = r * Mathf.PI * 2f / ringCount;
            float distance = 3f + r * 1.5f;
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