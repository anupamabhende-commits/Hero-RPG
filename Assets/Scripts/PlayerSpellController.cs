using System.Collections;
using UnityEngine;

public class PlayerSpellController : MonoBehaviour
{
    private enum SpellMode
    {
        None,
        Fireball,
        Portal
    }

    [Header("References")]
    public Animator animator;
    public GameObject fireballPrefab;
    public Transform firePoint;
    public Transform portalDestination;

    [Header("Settings")]
    public float animationLockTime = 6.2f;
    public Vector3 fireballLocalOffset = new Vector3(-0.025f, -0.01f, 0.113f);
    public Vector3 fireballLocalEulerAngles = Vector3.zero;
    public float fireballScale = 0.075f;
    public bool launchAtEndOfAnimation = true;
    public float endLaunchFallbackTime = 7.85f;

    [Header("Portal")]
    public KeyCode portalKey = KeyCode.F;
    public float portalSpawnDelay = 1.1f;
    public float portalForwardOffset = 3f;
    public float portalVerticalOffset = 1.2f;
    public float portalRadius = 1.35f;
    public float portalOpenDuration = 0.8f;
    public float portalCloseDuration = 0.45f;
    public float portalLifetime = 20f;
    public float portalFallbackDestinationDistance = 12f;
    public bool portalUseScreenFade = true;

    [Header("Debug")]
    public bool debugLogs = true;
    public bool debugDrawLaunchDirection = true;

    private bool isCasting = false;
    private bool fireballLaunchedThisCast = false;
    private SpellMode currentSpell = SpellMode.None;
    private GameObject currentFireball;
    private Coroutine endLaunchFallbackCoroutine;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            CastFireball();
        }

        if (Input.GetKeyDown(portalKey))
        {
            CastPortal();
        }
    }

    public bool IsCasting => isCasting;

    public void CastFireball()
    {
        if (isCasting)
        {
            Log("CastFireball ignored because a cast is already running.");
            return;
        }

        isCasting = true;
        fireballLaunchedThisCast = false;
        currentSpell = SpellMode.Fireball;
        Log("CastFireball started.");

        if (animator == null)
        {
            animator = GetComponent<Animator>();
            Log($"Animator auto-found: {animator != null}.");
        }

        if (animator != null)
        {
            Log("Animator trigger sent: Fireball.");
            animator.SetTrigger("Fireball");

            if (launchAtEndOfAnimation)
            {
                ScheduleEndLaunchFallback();
            }
        }
        else
        {
            Debug.LogWarning("PlayerSpellController needs an Animator to play the fireball cast animation.", this);
            SpawnFireball();
            ShootCurrentFireball();
        }

        StartCoroutine(UnlockAfterAnimation());
    }

    IEnumerator UnlockAfterAnimation()
    {
        yield return new WaitForSeconds(animationLockTime);
        isCasting = false;
        currentSpell = SpellMode.None;
        Log("Cast lock released.");
    }

    public void CastPortal()
    {
        if (isCasting)
        {
            Log("CastPortal ignored because a cast is already running.");
            return;
        }

        isCasting = true;
        currentSpell = SpellMode.Portal;
        fireballLaunchedThisCast = false;
        Log("CastPortal started.");

        if (animator == null)
        {
            animator = GetComponent<Animator>();
            Log($"Animator auto-found: {animator != null}.");
        }

        if (animator != null)
        {
            Log("Animator trigger sent for portal cast: Fireball.");
            animator.SetTrigger("Fireball");
        }

        StartCoroutine(SpawnPortalAfterCastDelay());
        StartCoroutine(UnlockAfterAnimation());
    }

    private IEnumerator SpawnPortalAfterCastDelay()
    {
        yield return new WaitForSeconds(portalSpawnDelay);

        if (currentSpell != SpellMode.Portal)
        {
            yield break;
        }

        SpawnPortal();
    }

    private void SpawnPortal()
    {
        Vector3 launchDirection = GetLaunchDirection();
        Vector3 portalPosition = transform.position + launchDirection * portalForwardOffset + Vector3.up * portalVerticalOffset;
        Quaternion portalRotation = Quaternion.LookRotation(launchDirection, Vector3.up);
        GameObject portalObject = new GameObject("Runtime Portal");
        portalObject.transform.SetPositionAndRotation(portalPosition, portalRotation);

        PortalTeleportController portal = portalObject.AddComponent<PortalTeleportController>();
        portal.Initialize(
            transform,
            portalDestination,
            portalRadius,
            portalOpenDuration,
            portalCloseDuration,
            portalLifetime,
            portalFallbackDestinationDistance,
            portalUseScreenFade,
            debugLogs);

        Log($"Portal spawned at {portalPosition} facing {launchDirection}.");
    }

    //===========================
    // ANIMATION EVENTS
    //===========================

    // Spawn Fireball
    public void SpawnFireball()
    {
        if (currentFireball != null)
        {
            Log($"SpawnFireball skipped; current fireball already exists: {currentFireball.name}.");
            return;
        }

        if (fireballPrefab == null)
        {
            Debug.LogWarning("PlayerSpellController has no fireball prefab assigned.", this);
            return;
        }

        if (firePoint == null)
        {
            firePoint = transform;
            Log("Fire point was missing, using player transform.");
        }

        currentFireball = Instantiate(fireballPrefab, firePoint);
        currentFireball.transform.localPosition = fireballLocalOffset;
        currentFireball.transform.localRotation = Quaternion.Euler(fireballLocalEulerAngles);
        currentFireball.transform.localScale = Vector3.one * fireballScale;
        currentFireball.SetActive(true);

        Log(
            $"SpawnFireball created {currentFireball.name}. " +
            $"worldPos={currentFireball.transform.position}, " +
            $"worldRot={currentFireball.transform.rotation.eulerAngles}, " +
            $"forward={currentFireball.transform.forward}, " +
            $"parent={currentFireball.transform.parent?.name}."
        );
    }

    // Enable Charge VFX
    public void EnableFireballVFX()
    {
        Log("Animation event received: EnableFireballVFX.");

        if (currentSpell != SpellMode.Fireball)
        {
            Log($"EnableFireballVFX ignored because current spell is {currentSpell}.");
            return;
        }

        SpawnFireball();

        Fireball fireball = GetCurrentFireball();
        if (fireball != null)
        {
            fireball.EnableVFX();
        }
        else
        {
            Log("EnableFireballVFX could not find a Fireball component.");
        }
    }

    // Disable Charge VFX & Shoot
    public void DisableFireballVFX()
    {
        Log("Animation event received: DisableFireballVFX.");

        if (currentSpell != SpellMode.Fireball)
        {
            Log($"DisableFireballVFX ignored because current spell is {currentSpell}.");
            return;
        }

        if (fireballLaunchedThisCast)
        {
            Log("DisableFireballVFX ignored because this cast already launched a fireball.");
            return;
        }

        CancelEndLaunchFallback();
        SpawnFireball();
        ShootCurrentFireball();
    }

    private void ScheduleEndLaunchFallback()
    {
        CancelEndLaunchFallback();
        endLaunchFallbackCoroutine = StartCoroutine(LaunchAtEndFallback());
        Log($"End launch fallback scheduled in {endLaunchFallbackTime:0.00}s.");
    }

    private IEnumerator LaunchAtEndFallback()
    {
        yield return new WaitForSeconds(endLaunchFallbackTime);

        if (isCasting && !fireballLaunchedThisCast)
        {
            Log("End launch fallback fired.");
            SpawnFireball();
            ShootCurrentFireball();
        }
        else
        {
            Log("End launch fallback skipped because the cast already ended or already launched.");
        }

        endLaunchFallbackCoroutine = null;
    }

    private void CancelEndLaunchFallback()
    {
        if (endLaunchFallbackCoroutine == null)
        {
            return;
        }

        StopCoroutine(endLaunchFallbackCoroutine);
        endLaunchFallbackCoroutine = null;
        Log("End launch fallback cancelled.");
    }

    private Fireball GetCurrentFireball()
    {
        if (currentFireball == null)
        {
            Log("GetCurrentFireball returned null; no current fireball object.");
            return null;
        }

        Fireball fireball = currentFireball.GetComponent<Fireball>();

        if (fireball == null)
        {
            Debug.LogWarning("The assigned fireball prefab needs a Fireball component on its root object.", currentFireball);
        }

        return fireball;
    }

    private void ShootCurrentFireball()
    {
        if (fireballLaunchedThisCast)
        {
            Log("ShootCurrentFireball ignored because this cast already launched a fireball.");
            return;
        }

        Fireball fireball = GetCurrentFireball();

        if (fireball != null)
        {
            fireballLaunchedThisCast = true;
            CancelEndLaunchFallback();
            Log(
                $"ShootCurrentFireball launching {currentFireball.name}. " +
                $"beforeDetachPos={currentFireball.transform.position}, " +
                $"beforeDetachForward={currentFireball.transform.forward}, " +
                $"parent={currentFireball.transform.parent?.name}."
            );

            currentFireball.transform.SetParent(null, true);

            if (debugDrawLaunchDirection)
            {
                Debug.DrawRay(currentFireball.transform.position, GetLaunchDirection() * 3f, Color.red, 3f);
            }

            fireball.DisableVFX();
            fireball.Launch(GetLaunchDirection());
            Log(
                $"ShootCurrentFireball done. " +
                $"afterDetachPos={currentFireball.transform.position}, " +
                $"launchDirection={GetLaunchDirection()}, " +
                $"parent={currentFireball.transform.parent?.name ?? "None"}."
            );
            currentFireball = null;
        }
        else
        {
            Log("ShootCurrentFireball failed because no Fireball component/object was available.");
        }
    }

    private Vector3 GetLaunchDirection()
    {
        Vector3 direction = transform.forward;

        if (animator != null)
        {
            direction = animator.transform.forward;
        }

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f && firePoint != null)
        {
            direction = firePoint.forward;
            direction.y = 0f;
        }

        if (direction.sqrMagnitude < 0.0001f)
        {
            direction = transform.forward;
        }

        return direction.normalized;
    }

    private void Log(string message)
    {
        if (!debugLogs)
        {
            return;
        }

        Debug.Log($"[PlayerSpellController] t={Time.time:0.00} frame={Time.frameCount} {message}", this);
    }
}
