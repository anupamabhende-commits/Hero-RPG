using System.Collections;
using UnityEngine;

public class PortalTeleportController : MonoBehaviour
{
    private const int RingSegments = 28;

    private Transform player;
    private Transform destination;
    private GameObject visualRoot;
    private GameObject interior;
    private ParticleSystem sparks;
    private Material ringMaterial;
    private Material interiorMaterial;
    private float radius;
    private float openDuration;
    private float closeDuration;
    private float lifetime;
    private float destinationDistance;
    private bool useScreenFade;
    private bool debugLogs;
    private bool isOpen;
    private bool isClosing;
    private bool teleported;
    private float previousPlaneDistance;

    public void Initialize(
        Transform playerTransform,
        Transform destinationTransform,
        float portalRadius,
        float portalOpenDuration,
        float portalCloseDuration,
        float portalLifetime,
        float fallbackDestinationDistance,
        bool enableScreenFade,
        bool enableDebugLogs)
    {
        player = playerTransform;
        destination = destinationTransform;
        radius = portalRadius;
        openDuration = Mathf.Max(0.01f, portalOpenDuration);
        closeDuration = Mathf.Max(0.01f, portalCloseDuration);
        lifetime = portalLifetime;
        destinationDistance = fallbackDestinationDistance;
        useScreenFade = enableScreenFade;
        debugLogs = enableDebugLogs;

        BuildPortalVisuals();
        visualRoot.transform.localScale = Vector3.zero;
        previousPlaneDistance = GetPlayerPlaneDistance();

        StartCoroutine(OpenPortal());
    }

    private void Update()
    {
        if (!isOpen || isClosing || teleported || player == null)
        {
            return;
        }

        float planeDistance = GetPlayerPlaneDistance();
        Vector3 localPoint = transform.InverseTransformPoint(player.position);
        float radialDistance = new Vector2(localPoint.x, localPoint.y).magnitude;
        bool crossedPortalPlane = previousPlaneDistance < 0f && planeDistance >= 0f;
        bool insideRing = radialDistance <= radius * 0.85f;

        if (crossedPortalPlane && insideRing)
        {
            StartCoroutine(TeleportAndClose());
        }

        previousPlaneDistance = planeDistance;
    }

    private void BuildPortalVisuals()
    {
        visualRoot = new GameObject("Portal Visuals");
        visualRoot.transform.SetParent(transform, false);

        ringMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        ringMaterial.name = "Runtime Portal Ring";
        ringMaterial.color = new Color(1f, 0.36f, 0.05f, 1f);
        ringMaterial.EnableKeyword("_EMISSION");
        ringMaterial.SetColor("_EmissionColor", new Color(1f, 0.25f, 0.02f, 1f) * 2.5f);

        interiorMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        interiorMaterial.name = "Runtime Portal Interior";
        interiorMaterial.color = new Color(0.1f, 0.45f, 1f, 0.65f);
        interiorMaterial.EnableKeyword("_EMISSION");
        interiorMaterial.SetColor("_EmissionColor", new Color(0.05f, 0.35f, 1f, 1f) * 1.8f);

        CreateInterior();
        CreateRingSegments();
        CreateSparks();
    }

    private void CreateInterior()
    {
        interior = GameObject.CreatePrimitive(PrimitiveType.Quad);
        interior.name = "Destination Interior";
        interior.transform.SetParent(visualRoot.transform, false);
        interior.transform.localPosition = Vector3.forward * 0.01f;
        interior.transform.localScale = Vector3.one * radius * 1.55f;

        MeshRenderer renderer = interior.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = interiorMaterial;

        Collider collider = interior.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }
    }

    private void CreateRingSegments()
    {
        for (int i = 0; i < RingSegments; i++)
        {
            float angle = i * Mathf.PI * 2f / RingSegments;
            Vector3 position = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;

            GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Cube);
            segment.name = "Orange Ring Spark";
            segment.transform.SetParent(visualRoot.transform, false);
            segment.transform.localPosition = position;
            segment.transform.localRotation = Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg);
            segment.transform.localScale = new Vector3(radius * 0.16f, radius * 0.05f, radius * 0.08f);

            MeshRenderer renderer = segment.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = ringMaterial;

            Collider collider = segment.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }
        }
    }

    private void CreateSparks()
    {
        GameObject sparkObject = new GameObject("Orange Portal Sparks");
        sparkObject.transform.SetParent(visualRoot.transform, false);
        sparks = sparkObject.AddComponent<ParticleSystem>();

        ParticleSystem.MainModule main = sparks.main;
        main.loop = true;
        main.startLifetime = 0.65f;
        main.startSpeed = 1.8f;
        main.startSize = 0.05f;
        main.startColor = new Color(1f, 0.45f, 0.05f, 1f);
        main.simulationSpace = ParticleSystemSimulationSpace.Local;

        ParticleSystem.EmissionModule emission = sparks.emission;
        emission.rateOverTime = 80f;

        ParticleSystem.ShapeModule shape = sparks.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = radius;
        shape.radiusThickness = 0.12f;

        ParticleSystem.VelocityOverLifetimeModule velocity = sparks.velocityOverLifetime;
        velocity.enabled = true;
        velocity.orbitalZ = 1.4f;
        velocity.radial = 0.35f;

        ParticleSystemRenderer renderer = sparks.GetComponent<ParticleSystemRenderer>();
        renderer.material = ringMaterial;
    }

    private IEnumerator OpenPortal()
    {
        Log("Opening portal.");
        float elapsed = 0f;

        while (elapsed < openDuration)
        {
            float t = Mathf.SmoothStep(0f, 1f, elapsed / openDuration);
            visualRoot.transform.localScale = Vector3.one * t;
            elapsed += Time.deltaTime;
            yield return null;
        }

        visualRoot.transform.localScale = Vector3.one;
        isOpen = true;
        previousPlaneDistance = GetPlayerPlaneDistance();
        Log("Portal fully open.");

        if (lifetime > 0f)
        {
            yield return new WaitForSeconds(lifetime);

            if (!teleported)
            {
                StartCoroutine(ClosePortal());
            }
        }
    }

    private IEnumerator TeleportAndClose()
    {
        teleported = true;
        Log("Player entered portal.");

        if (useScreenFade && player != null)
        {
            PortalScreenFade fade = PortalScreenFade.GetOrCreate(player.gameObject);
            yield return fade.FadeOutIn(() => TeleportPlayer());
        }
        else
        {
            TeleportPlayer();
        }

        StartCoroutine(ClosePortal());
    }

    private void TeleportPlayer()
    {
        Transform target = destination != null ? destination : CreateFallbackDestination();
        CharacterController characterController = player.GetComponent<CharacterController>();
        Rigidbody body = player.GetComponent<Rigidbody>();

        if (characterController != null)
        {
            characterController.enabled = false;
        }

        player.SetPositionAndRotation(target.position, target.rotation);

        if (body != null)
        {
            body.position = target.position;
            body.rotation = target.rotation;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }

        if (characterController != null)
        {
            characterController.enabled = true;
        }

        Log($"Teleported player to {target.position}.");
    }

    private Transform CreateFallbackDestination()
    {
        GameObject fallback = new GameObject("Runtime Portal Destination");
        Vector3 direction = transform.forward;
        fallback.transform.position = transform.position + direction * destinationDistance;
        fallback.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        destination = fallback.transform;
        return destination;
    }

    private IEnumerator ClosePortal()
    {
        if (isClosing)
        {
            yield break;
        }

        isClosing = true;
        isOpen = false;
        Log("Closing portal.");
        float elapsed = 0f;
        Vector3 startScale = visualRoot.transform.localScale;

        while (elapsed < closeDuration)
        {
            float t = 1f - Mathf.SmoothStep(0f, 1f, elapsed / closeDuration);
            visualRoot.transform.localScale = startScale * t;
            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

    private float GetPlayerPlaneDistance()
    {
        if (player == null)
        {
            return 0f;
        }

        return Vector3.Dot(player.position - transform.position, transform.forward);
    }

    private void Log(string message)
    {
        if (!debugLogs)
        {
            return;
        }

        Debug.Log($"[PortalTeleport] t={Time.time:0.00} frame={Time.frameCount} {message}", this);
    }
}

public class PortalScreenFade : MonoBehaviour
{
    private float alpha;

    public static PortalScreenFade GetOrCreate(GameObject host)
    {
        PortalScreenFade fade = host.GetComponent<PortalScreenFade>();

        if (fade == null)
        {
            fade = host.AddComponent<PortalScreenFade>();
        }

        return fade;
    }

    public IEnumerator FadeOutIn(System.Action middleAction)
    {
        yield return FadeTo(1f, 0.25f);
        middleAction?.Invoke();
        yield return FadeTo(0f, 0.25f);
    }

    private IEnumerator FadeTo(float target, float duration)
    {
        float start = alpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            alpha = Mathf.Lerp(start, target, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        alpha = target;
    }

    private void OnGUI()
    {
        if (alpha <= 0f)
        {
            return;
        }

        Color previousColor = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, alpha);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = previousColor;
    }
}
