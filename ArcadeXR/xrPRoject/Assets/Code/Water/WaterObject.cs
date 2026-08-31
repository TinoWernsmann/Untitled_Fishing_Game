using UnityEngine;

public class WaterObject : MonoBehaviour
{
    [Header("Water Contact")]
    // Small offset before exact water contact
    public float contactTolerance = 0.02f;

    [Header("Particle Effect")]
    // Particle system used for the water splash
    public ParticleSystem splashParticleSystem;

    // Stop particles when the object leaves the water
    public bool stopWhenLeavingWater = true;

    private Collider[] colliders;
    private bool wasTouchingWater;

    private void Awake()
    {
        // Cache all child colliders
        colliders = GetComponentsInChildren<Collider>();

        // Find particle system automatically if not assigned
        if (splashParticleSystem == null)
            splashParticleSystem = GetComponentInChildren<ParticleSystem>(true);

        if (splashParticleSystem != null)
            splashParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private void Update()
    {
        if (WaterInteractionManager.Instance == null)
            return;

        // Get water height from the manager
        float waterHeight = WaterInteractionManager.Instance.WaterHeight;

        // Get lowest point of this object
        float objectBottomY = GetObjectBottomY();

        // Check if the object touches the water
        bool isTouchingWater = objectBottomY <= waterHeight + contactTolerance;

        // Play splash on contact (entering or exiting water)
        if (!wasTouchingWater && isTouchingWater)
            PlaySplash();
        if (wasTouchingWater && !isTouchingWater)
            PlaySplash();

        // Stop splash when leaving water
        if (wasTouchingWater && !isTouchingWater && stopWhenLeavingWater)
            StopSplash();

        // Save current contact state
        wasTouchingWater = isTouchingWater;
    }

    private float GetObjectBottomY()
    {
        float minY = float.MaxValue;
        bool foundCollider = false;

        foreach (Collider col in colliders)
        {
            // Ignore missing or trigger colliders
            if (col == null || col.isTrigger)
                continue;

            // Store the lowest collider point
            minY = Mathf.Min(minY, col.bounds.min.y);
            foundCollider = true;
        }

        // Use collider bottom, otherwise fallback to object position
        return foundCollider ? minY : transform.position.y;
    }

    private void PlaySplash()
    {
        if (splashParticleSystem == null)
            return;

        // Move splash to the water surface
        Vector3 surfacePosition =
            WaterInteractionManager.Instance.GetSurfacePosition(transform.position);

        splashParticleSystem.transform.position = surfacePosition;

        // Restart particle effect
        splashParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        splashParticleSystem.Play(true);
    }

    private void StopSplash()
    {
        if (splashParticleSystem == null)
            return;

        // Stop creating new particles
        splashParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }
}