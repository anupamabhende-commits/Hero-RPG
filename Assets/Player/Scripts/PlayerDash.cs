using UnityEngine;

public class PlayerDash : MonoBehaviour
{
    private float dashTimer;
    private float cooldownTimer;
    private Vector3 dashDirection;
    public bool IsDashing => dashTimer > 0f;
    public Vector3 Tick(bool dashPressed, Vector3 desiredDirection, bool canDash, PlayerStats stats)
    {
        if (stats == null)
        {
            return Vector3.zero;
        }
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }
        if (dashTimer > 0f)
        {
            dashTimer -= Time.deltaTime;
            float speed = stats.dashDistance / Mathf.Max(0.01f, stats.dashDuration);
            return dashDirection * speed;
        }
        if (!dashPressed || !canDash || cooldownTimer > 0f)
        {
            return Vector3.zero;
        }
        dashDirection = desiredDirection.sqrMagnitude > 0.0001f ? desiredDirection.normalized : transform.forward;
        dashTimer = stats.dashDuration;
        cooldownTimer = stats.dashCooldown;
        return dashDirection * (stats.dashDistance / Mathf.Max(0.01f, stats.dashDuration));
    }
}
