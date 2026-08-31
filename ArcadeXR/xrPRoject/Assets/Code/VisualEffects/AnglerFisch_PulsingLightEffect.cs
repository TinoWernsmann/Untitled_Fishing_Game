using UnityEngine;

public class AnglerFishGlowPulse : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Renderer sphere_glowRenderer;

    [Header("Glow Settings")]
    [SerializeField] private Color emissionColor = Color.cyan;

    [Tooltip("Die minimale Emissionsintensität.")]
    [SerializeField] private float minimumEmissionIntensity = 1.5f;

    [Tooltip("Wie stark die Emission um den Minimalwert schwankt.")]
    [SerializeField] private float emissionPulseAmplitude = 2.5f;

    [Tooltip("Wie schnell der Glow pulsiert.")]
    [SerializeField] private float pulseSpeed = 2f;

    private Material emissionMaterial;

    private void Awake()
    {
        if (sphere_glowRenderer == null)
        {
            sphere_glowRenderer = GetComponent<Renderer>();
        }

        emissionMaterial = sphere_glowRenderer.material;
        emissionMaterial.EnableKeyword("_EMISSION");
    }

    private void Update()
    {
        float currentEmissionIntensity =
            minimumEmissionIntensity +
            Mathf.Sin(Time.time * pulseSpeed) * emissionPulseAmplitude;

        currentEmissionIntensity = Mathf.Max(0f, currentEmissionIntensity);

        emissionMaterial.SetColor(
            "_EmissionColor",
            emissionColor * currentEmissionIntensity);
    }
}