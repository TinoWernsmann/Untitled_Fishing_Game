using UnityEngine;
using Horror.Changer;

public class HorrorSkyboxBlendController : ContinuousHorrorReactor
{
    [SerializeField] private Material _skyboxMaterial;

    private const float BLEND_START = 0.55f;
    private const float BLEND_END = 0.70f;

    protected override void Apply(float t)
    {
        float blendAmount;

        if (t < BLEND_START)
        {
            blendAmount = 0f;
        }
        else if (t > BLEND_END)
        {
            blendAmount = 1f;
        }
        else
        {
            blendAmount = (t - BLEND_START) / (BLEND_END - BLEND_START);
        }

        _skyboxMaterial.SetFloat("_Blend", blendAmount);
        RenderSettings.skybox = _skyboxMaterial;
    }
}
