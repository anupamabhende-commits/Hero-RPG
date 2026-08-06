using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Vector3 currentVelocity;

    public Vector3 CurrentVelocity => currentVelocity;

    public Vector3 Tick(Vector2 moveInput, Transform cameraTransform, bool sprinting, bool canMove, PlayerStats stats)
    {
        if (stats == null || stats.useRootMotion)
        {
            currentVelocity = Vector3.zero;
            return currentVelocity;
        }

        Vector3 desiredDirection = GetCameraRelativeDirection(moveInput, cameraTransform);
        float targetSpeed = sprinting ? stats.sprintSpeed : stats.walkSpeed;
        Vector3 targetVelocity = canMove ? desiredDirection * targetSpeed : Vector3.zero;
        float response = targetVelocity.sqrMagnitude > currentVelocity.sqrMagnitude ? stats.acceleration : stats.deceleration;

        currentVelocity = Vector3.MoveTowards(
            currentVelocity,
            targetVelocity,
            response * Time.deltaTime);

        return currentVelocity;
    }

    public Vector3 GetCameraRelativeDirection(Vector2 moveInput, Transform cameraTransform)
    {
        if (moveInput.sqrMagnitude < 0.0001f)
        {
            return Vector3.zero;
        }

        Vector3 forward = cameraTransform != null ? cameraTransform.forward : transform.forward;
        Vector3 right = cameraTransform != null ? cameraTransform.right : transform.right;

        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        return (forward * moveInput.y + right * moveInput.x).normalized;
    }
}
