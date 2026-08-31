Shader "Skybox/CubemapBlend"
{
    Properties
    {
        _Cube1 ("Friendly Cubemap", Cube) = "white" {}
        _Cube2 ("Evil Cubemap", Cube) = "white" {}
        _Blend ("Blend", Range(0, 1)) = 0
        _Exposure ("Exposure", Range(0, 8)) = 1.0
    }

    SubShader
    {
        Tags { "RenderType"="Background" "Queue"="Background" }

        Pass
        {
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURECUBE(_Cube1);
            SAMPLER(sampler_Cube1);
            TEXTURECUBE(_Cube2);
            SAMPLER(sampler_Cube2);

            float _Blend;
            float _Exposure;

            struct Attributes
            {
                float3 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionOS : TEXCOORD0;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS);
                output.positionOS = input.positionOS;
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float3 dir = normalize(input.positionOS);

                float4 col1 = SAMPLE_TEXTURECUBE(_Cube1, sampler_Cube1, dir);
                float4 col2 = SAMPLE_TEXTURECUBE(_Cube2, sampler_Cube2, dir);

                float4 result = lerp(col1, col2, _Blend);
                return float4(result.rgb * _Exposure, result.a);
            }
            ENDHLSL
        }
    }
}
