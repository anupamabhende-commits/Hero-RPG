using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ActionRPGCamera : MonoBehaviour
{
    [Header("Targets")]
    public Transform target;
    public PlayerController player;
    public Transform lockOnTarget;

    [Header("Orbit")]
    public float distance = 5.4f;
    public float height = 1.15f;
    public float mouseSensitivity = 2.2f;
    public float minPitch = -25f;
    public float maxPitch = 62f;
    public float autoCenterDelay = 1.4f;
    public float autoCenterSpeed = 85f;

    [Header("Feel")]
    public float positionLag = 14f;
    public float rotationLag = 18f;
    public float sprintFov = 68f;
    public float normalFov = 60f;
    public float fovSpeed = 8f;

    [Header("Collision")]
    public LayerMask collisionMask = ~0;
    public float collisionRadius = 0.28f;
    public float collisionPadding = 0.18f;

    private const int MaxCollisionHits = 8;

    private readonly RaycastHit[] collisionHits = new RaycastHit[MaxCollisionHits];
    private Camera controlledCamera;
    private float yaw;
    private float pitch = 18f;
    private float lastManualLookTime;
    private float shakeTimer;
    private float shakeIntensity;

    private void Awake()
    {
        controlledCamera = GetComponent<Camera>();
        Vector3 euler = transform.eulerAngles;
        yaw = euler.y;
        pitch = euler.x;

        if (controlledCamera != null)
        {
            controlledCamera.fieldOfView = normalFov;
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        CaptureLookInput();
        AutoCenterBehindPlayer();
        FollowTarget();
        UpdateFov();
    }

    public void Shake(float intensity, float duration)
    {
        shakeIntensity = Mathf.Max(shakeIntensity, intensity);
        shakeTimer = Mathf.Max(shakeTimer, duration);
    }

    private void CaptureLookInput()
    {
        float lookX = Input.GetAxis("Mouse X");
        float lookY = Input.GetAxis("Mouse Y");

        if (Mathf.Abs(lookX) > 0.01f || Mathf.Abs(lookY) > 0.01f)
        {
            yaw += lookX * mouseSensitivity;
            pitch -= lookY * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            lastManualLookTime = Time.time;
        }
    }

    private void AutoCenterBehindPlayer()
    {
        if (player == null || player.LockOn || Time.time - lastManualLookTime < autoCenterDelay)
        {
            return;
        }

        Vector3 forward = player.transform.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude < 0.001f || player.PlanarVelocity.sqrMagnitude < 0.25f)
        {
            return;
        }

        float targetYaw = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
        yaw = Mathf.MoveTowardsAngle(yaw, targetYaw, autoCenterSpeed * Time.deltaTime);
    }

    private void FollowTarget()
    {
        if (player != null && player.LockOn && lockOnTarget != null)
        {
            Vector3 targetDirection = lockOnTarget.position - target.position;
            targetDirection.y = 0f;

            if (targetDirection.sqrMagnitude > 0.001f)
            {
                yaw = Mathf.Atan2(targetDirection.x, targetDirection.z) * Mathf.Rad2Deg;
                pitch = Mathf.Lerp(pitch, 16f, 8f * Time.deltaTime);
            }
        }

        Quaternion orbitRotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 focusPoint = target.position + Vector3.up * height;
        Vector3 desiredPosition = focusPoint - orbitRotation * Vector3.forward * distance;
        desiredPosition = ResolveCollision(focusPoint, desiredPosition);
        desiredPosition += GetShakeOffset();

        transform.position = Vector3.Lerp(transform.position, desiredPosition, 1f - Mathf.Exp(-positionLag * Time.deltaTime));

        Quaternion desiredRotation = Quaternion.LookRotation(focusPoint - transform.position, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, 1f - Mathf.Exp(-rotationLag * Time.deltaTime));
    }

    private Vector3 ResolveCollision(Vector3 focusPoint, Vector3 desiredPosition)
    {
        Vector3 direction = desiredPosition - focusPoint;
        float desiredDistance = direction.magnitude;

        if (desiredDistance <= 0.001f)
        {
            return desiredPosition;
        }

        direction /= desiredDistance;
        int hitCount = Physics.SphereCastNonAlloc(
            focusPoint,
            collisionRadius,
            direction,
            collisionHits,
            desiredDistance,
            collisionMask,
            QueryTriggerInteraction.Ignore);

        float closestDistance = desiredDistance;
        Transform ignoredRoot = target.root;

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit hit = collisionHits[i];

            if (hit.transform == null || hit.transform.root == ignoredRoot)
            {
                continue;
            }

            closestDistance = Mathf.Min(closestDistance, Mathf.Max(0.2f, hit.distance - collisionPadding));
        }

        return focusPoint + direction * closestDistance;
    }

    private Vector3 GetShakeOffset()
    {
        if (shakeTimer <= 0f)
        {
            return Vector3.zero;
        }

        shakeTimer -= Time.deltaTime;
        float strength = shakeIntensity * Mathf.Clamp01(shakeTimer);

        if (shakeTimer <= 0f)
        {
            shakeIntensity = 0f;
        }

        return transform.right * Random.Range(-strength, strength) + transform.up * Random.Range(-strength, strength);
    }

    private void UpdateFov()
    {
        if (controlledCamera == null)
        {
            return;
        }

        bool sprinting = player != null && player.IsSprinting;
        float targetFov = sprinting ? sprintFov : normalFov;
        controlledCamera.fieldOfView = Mathf.Lerp(controlledCamera.fieldOfView, targetFov, 1f - Mathf.Exp(-fovSpeed * Time.deltaTime));
    }
}
