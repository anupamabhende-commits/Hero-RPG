using UnityEngine;

public class PlayerJump : MonoBehaviour
{
    private float verticalVelocity;

    public float VerticalVelocity => verticalVelocity;

    public float Tick(bool grounded, bool jumpPressed, bool canJump, PlayerStats stats)
    {
        if (stats == null)
        {
            return 0f;
        }

        if (grounded && verticalVelocity < 0f)
        {
            verticalVelocity = stats.groundedStickForce;
        }

        if (grounded && canJump && jumpPressed)
        {
            verticalVelocity = Mathf.Sqrt(stats.jumpHeight * -2f * stats.gravity);
        }

        verticalVelocity += stats.gravity * Time.deltaTime;
        return verticalVelocity;
    }
}
