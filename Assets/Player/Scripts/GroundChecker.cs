using UnityEngine;

public class GroundChecker : MonoBehaviour
{
    [Header("Ground Check")]
    public CharacterController characterController;
    public Transform checkOrigin;
    public LayerMask groundMask = ~0;
    public float checkRadius = 0.24f;
    public float checkOffset = 0.08f;

    public bool IsGrounded { get; private set; }

    private void Reset()
    {
        characterController = GetComponent<CharacterController>();
    }

    public void Tick()
    {
        bool controllerGrounded = characterController != null && characterController.isGrounded;
        Vector3 origin = checkOrigin != null ? checkOrigin.position : transform.position;

        if (characterController != null && checkOrigin == null)
        {
            origin += characterController.center;
            origin.y -= (characterController.height * 0.5f) - characterController.radius + checkOffset;
        }

        bool sphereGrounded = Physics.CheckSphere(origin, checkRadius, groundMask, QueryTriggerInteraction.Ignore);
        IsGrounded = controllerGrounded || sphereGrounded;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 origin = checkOrigin != null ? checkOrigin.position : transform.position;

        if (characterController != null && checkOrigin == null)
        {
            origin += characterController.center;
            origin.y -= (characterController.height * 0.5f) - characterController.radius + checkOffset;
        }

        Gizmos.color = IsGrounded ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(origin, checkRadius);
    }
}
