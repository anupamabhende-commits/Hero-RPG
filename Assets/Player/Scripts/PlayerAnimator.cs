using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    [Header("Parameters")]
    public string speedParameter = "Speed";
    public string groundedParameter = "Grounded";
    public string jumpTrigger = "Jump";
    public string rollTrigger = "Roll";
    public string sprintParameter = "Sprint";
    public string lockOnParameter = "LockOn";
    private bool wasGrounded = true;
    private bool wasDashing;
    private void Reset()
    {
        animator = GetComponentInChildren<Animator>();
    }
    public void Tick(
        Vector3 planarVelocity,
        bool grounded,
        bool sprinting,
        bool lockOn,
        bool dashing,
        float maxSpeed)
    {
        if (animator == null)
        {
            return;
        }

        float normalizedSpeed = maxSpeed > 0f ? Mathf.Clamp01(planarVelocity.magnitude / maxSpeed) : 0f;
        SetFloat(speedParameter, normalizedSpeed, 0.12f);
        SetBool(groundedParameter, grounded);
        SetBool(sprintParameter, sprinting);
        SetBool(lockOnParameter, lockOn);

        if (grounded == false && wasGrounded)
        {
            SetTrigger(jumpTrigger);
        }

        if (dashing && !wasDashing)
        {
            SetTrigger(rollTrigger);
        }

        wasGrounded = grounded;
        wasDashing = dashing;
    }
    private void SetFloat(string parameterName, float value, float dampTime)
    {
        if (HasParameter(parameterName, AnimatorControllerParameterType.Float))
        {
            animator.SetFloat(parameterName, value, dampTime, Time.deltaTime);
        }
    }
    private void SetBool(string parameterName, bool value)
    {
        if (HasParameter(parameterName, AnimatorControllerParameterType.Bool))
        {
            animator.SetBool(parameterName, value);
        }
    }
    private void SetTrigger(string parameterName)
    {
        if (HasParameter(parameterName, AnimatorControllerParameterType.Trigger))
        {
            animator.SetTrigger(parameterName);
        }
    }
    private bool HasParameter(string parameterName, AnimatorControllerParameterType parameterType)
    {
        if (string.IsNullOrEmpty(parameterName))
        {
            return false;
        }

        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.name == parameterName && parameter.type == parameterType)
            {
                return true;
            }
        }

        return false;
    }
}
