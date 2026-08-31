using UnityEngine;

namespace Horror.Changer
{
    public class HorrorLightController : ContinuousHorrorReactor
    {
        [SerializeField] private Light _light;

        // colors of the light
        [SerializeField] private Color _calmColor = new Color(1.0f, 0.957f, 0.839f); // FFF4D6
        [SerializeField] private Color _horrorColor = new Color(0.337f, 0.471f, 0.435f); // 56786F

        // brightness of the light
        [SerializeField] private float _calmIntensity = 1f;
        [SerializeField] private float _horrorIntensity = 0.2f;

        private void Reset()
        {
            _light = GetComponent<Light>();
        }

        protected override void Apply(float t)
        {
            _light.color = Color.Lerp(_calmColor, _horrorColor, t);
            _light.intensity = Mathf.Lerp(_calmIntensity, _horrorIntensity, t);
        }
    }
}
