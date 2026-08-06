using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public enum ActionKind
    {
        None,
        Attack,
        Cast
    }
    [Header("References")]
    public PlayerSpellController spellController;
    [Header("Animator Triggers")]
    public string attackTrigger = "Attack";
    public string castTrigger = "Cast";
    private float lockTimer;
    private float comboTimer;
    private bool comboQueued;
    private ActionKind currentAction;
    public bool IsActionLocked => lockTimer > 0f || (spellController != null && spellController.IsCasting);
    public bool ComboQueued => comboQueued;
    public ActionKind CurrentAction => currentAction;
    private void Reset()
    {
        spellController = GetComponent<PlayerSpellController>();
    }
    public void Tick(PlayerInput input, PlayerStats stats, Animator animator)
    {
        if (lockTimer > 0f)
        {
            lockTimer -= Time.deltaTime;
        }
        else if (currentAction != ActionKind.None && (spellController == null || !spellController.IsCasting))
        {
            currentAction = ActionKind.None;
        }

        if (comboTimer > 0f)
        {
            comboTimer -= Time.deltaTime;
        }

        if (input == null || stats == null)
        {
            return;
        }

        if (input.AttackPressed)
        {
            TryAttack(stats, animator);
        }

        if (input.MagicPressed)
        {
            TryCastMagic(stats, animator);
        }

        if (input.PortalPressed)
        {
            TryCastPortal(stats, animator);
        }

        if (comboQueued && comboTimer > 0f && !IsActionLocked)
        {
            TryAttack(stats, animator);
        }
        else if (comboTimer <= 0f)
        {
            comboQueued = false;
        }
    }
    public bool TryAttack(PlayerStats stats, Animator animator)
    {
        if (IsActionLocked)
        {
            comboQueued = true;
            comboTimer = stats.comboBufferTime;
            return false;
        }

        comboQueued = false;
        lockTimer = stats.attackLockTime;
        currentAction = ActionKind.Attack;
        SetTrigger(animator, attackTrigger);
        return true;
    }
    public bool TryCastMagic(PlayerStats stats, Animator animator)
    {
        if (IsActionLocked)
        {
            return false;
        }

        lockTimer = stats.magicLockTime;
        currentAction = ActionKind.Cast;
        SetTrigger(animator, castTrigger);

        if (spellController != null)
        {
            spellController.CastFireball();
        }

        return true;
    }
    public bool TryCastPortal(PlayerStats stats, Animator animator)
    {
        if (IsActionLocked)
        {
            return false;
        }

        lockTimer = stats.magicLockTime;
        currentAction = ActionKind.Cast;
        SetTrigger(animator, castTrigger);

        if (spellController != null)
        {
            spellController.CastPortal();
        }

        return true;
    }
    private static void SetTrigger(Animator animator, string triggerName)
    {
        if (animator != null && HasParameter(animator, triggerName, AnimatorControllerParameterType.Trigger))
        {
            animator.SetTrigger(triggerName);
        }
    }
    private static bool HasParameter(Animator animator, string parameterName, AnimatorControllerParameterType parameterType)
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
