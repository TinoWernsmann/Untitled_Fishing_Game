using UnityEngine;

namespace Horror.Changer
{
    public class HorrorFogController : ContinuousHorrorReactor
    {
        [SerializeField] private ParticleSystem _fogParticles;
        [SerializeField] private Material _fogMaterial;

        [Header("Fog Density")]
        [SerializeField] private float _calmFogDensity = 0.0001f;
        [SerializeField] private float _horrorFogDensity = 0.05f;

        [Header("Fog Color")]
        [SerializeField] private Color _calmFogColor = new Color(0.867f, 0.886f, 0.882f); // DDE2E1
        [SerializeField] private Color _horrorFogColor = new Color(0.176f, 0.263f, 0.200f); // 2D4333

        [Header("Fog Start/End (Shader)")]
        [SerializeField] private float _calmFogStart = 25f;
        [SerializeField] private float _horrorFogStart = 6f;
        [SerializeField] private float _calmFogEnd = 60f;
        [SerializeField] private float _horrorFogEnd = 60f;

        [Header("Particle Emission")]
        [SerializeField] private float _calmEmissionRate = 20f;
        [SerializeField] private float _horrorEmissionRate = 50f;

        protected override void Apply(float t)
        {
            RenderSettings.fogDensity = Mathf.Lerp(_calmFogDensity, _horrorFogDensity, t);
            RenderSettings.fogColor = Color.Lerp(_calmFogColor, _horrorFogColor, t);

            if (_fogMaterial != null)
            {
                _fogMaterial.SetColor("_Fog_Color", Color.Lerp(_calmFogColor, _horrorFogColor, t));
                _fogMaterial.SetFloat("_Fog_Start", Mathf.Lerp(_calmFogStart, _horrorFogStart, t));
                _fogMaterial.SetFloat("_Fog_End", Mathf.Lerp(_calmFogEnd, _horrorFogEnd, t));
            }

            if (_fogParticles != null)
            {
                var emission = _fogParticles.emission;
                emission.rateOverTime = Mathf.Lerp(_calmEmissionRate, _horrorEmissionRate, t);
            }
        }
    }
}
