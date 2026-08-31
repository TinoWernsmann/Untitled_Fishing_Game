using UnityEngine;

namespace Horror.Changer
{
    public class HorrorWaterController : ContinuousHorrorReactor
    {
        [SerializeField] private Renderer _waterRenderer;

        [SerializeField] private Color _calmWaterColor = new Color(0.169f, 0.290f, 0.647f); // 2B4AA5
        [SerializeField] private Color _horrorWaterColor = new Color(0.153f, 0.537f, 0.325f); // 278953

        [SerializeField] private Color _calmFoamColor = new Color(0.161f, 0.604f, 0.659f); // 299AA8
        [SerializeField] private Color _horrorFoamColor = new Color(0.239f, 0.698f, 0.325f); // 3DB253

        [SerializeField] private float _calmWaveScale = 0f;
        [SerializeField] private float _horrorWaveScale = 2.5f;

        private void Reset()
        {
            _waterRenderer = GetComponent<Renderer>();
        }

        protected override void Apply(float t)
        {
            var mat = _waterRenderer.material;

            mat.SetColor("_Water_Color", Color.Lerp(_calmWaterColor, _horrorWaterColor, t));
            mat.SetColor("_Foam_Color", Color.Lerp(_calmFoamColor, _horrorFoamColor, t));
            mat.SetFloat("_Wave_Scale", Mathf.Lerp(_calmWaveScale, _horrorWaveScale, t));
        }
    }
}
