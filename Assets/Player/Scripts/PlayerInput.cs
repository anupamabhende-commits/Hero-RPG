using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    [Header("Keys")]
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode dashKey = KeyCode.Q;
    public KeyCode interactKey = KeyCode.E;
    public KeyCode portalKey = KeyCode.F;
    public KeyCode lockOnKey = KeyCode.Tab;

    [Header("Mouse")]
    public int attackMouseButton = 0;
    public int magicMouseButton = 1;

    public Vector2 Move { get; private set; }
    public Vector2 Look { get; private set; }
    public bool SprintHeld { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool DashPressed { get; private set; }
    public bool AttackPressed { get; private set; }
    public bool MagicPressed { get; private set; }
    public bool InteractPressed { get; private set; }
    public bool PortalPressed { get; private set; }
    public bool LockOnPressed { get; private set; }

    public void Capture()
    {
        Move = new Vector2(UnityEngine.Input.GetAxisRaw("Horizontal"), UnityEngine.Input.GetAxisRaw("Vertical"));
        Move = Vector2.ClampMagnitude(Move, 1f);
        Look = new Vector2(UnityEngine.Input.GetAxis("Mouse X"), UnityEngine.Input.GetAxis("Mouse Y"));

        SprintHeld = Input.GetKey(sprintKey);
        JumpPressed = UnityEngine.Input.GetKeyDown(jumpKey);
        DashPressed = UnityEngine.Input.GetKeyDown(dashKey);
        AttackPressed = UnityEngine.Input.GetMouseButtonDown(attackMouseButton);
        MagicPressed = UnityEngine.Input.GetMouseButtonDown(magicMouseButton);
        InteractPressed = UnityEngine.Input.GetKeyDown(interactKey);
        PortalPressed = UnityEngine.Input.GetKeyDown(portalKey);
        LockOnPressed = UnityEngine.Input.GetKeyDown(lockOnKey);
    }
}
