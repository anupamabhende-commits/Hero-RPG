using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 4.2f;
    public float sprintSpeed = 7.2f;
    public float acceleration = 18f;
    public float deceleration = 22f;
    public float rotationSmoothTime = 0.08f;

    [Header("Jump & Gravity")]
    public float jumpHeight = 1.45f;
    public float gravity = -24f;
    public float groundedStickForce = -2f;

    [Header("Dash")]
    public float dashDistance = 5f;
    public float dashDuration = 0.24f;
    public float dashCooldown = 0.45f;

    [Header("Combat")]
    public float attackLockTime = 0.55f;
    public float magicLockTime = 0.85f;
    public float comboBufferTime = 0.35f;

    [Header("Root Motion")]
    public bool useRootMotion = false;
    public float rootMotionScale = 1f;
}
