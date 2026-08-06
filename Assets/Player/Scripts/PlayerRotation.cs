using UnityEngine;

public class PlayerRotation : MonoBehaviour
{
    private float turnVelocity;

    public void Tick(Vector3 movementVelocity, Transform lockOnTarget, bool lockOn, bool canRotate, PlayerStats stats)
    {
        if (!canRotate || stats == null)
        {
            return;
        }
        Vector3 lookDirection = Vector3.zero;
        if (lockOn && lockOnTarget != null)
        {
          lookDirection = lockOnTarget.position - transform.position;
        }
        else if (movementVelocity.sqrMagnitude > 0.01f)
        {
            lookDirection = movementVelocity;
        }
        lookDirection.y = 0f;
        if (lookDirection.sqrMagnitude < 0.0001f)
        {
            return;
        }
        float targetAngle = Mathf.Atan2(lookDirection.x, lookDirection.z) * Mathf.Rad2Deg;
        float smoothedAngle = Mathf.SmoothDampAngle(
            transform.eulerAngles.y,
            targetAngle,
            ref turnVelocity,
            stats.rotationSmoothTime);
        transform.rotation = Quaternion.Euler(0f, smoothedAngle, 0f);
    }
}
