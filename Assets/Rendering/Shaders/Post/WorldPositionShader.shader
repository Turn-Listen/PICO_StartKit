Shader "Example/URPReconstructWorldPos"
{
    Properties
    { }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
		LOD 100
        ZWrite Off Cull Off

        Pass
        {
			Name "FogBlitPass"

            HLSLPROGRAM
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            #pragma vertex vert
            #pragma fragment frag

			//TEXTURE2D_X(_CameraOpaqueTexture);      SAMPLER(sampler_CameraOpaqueTexture);
			SAMPLER(_CameraOpaqueTexture);

            #define OCTAVES 4 



            struct Attributes
            {
                float4 positionOS   : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
				float4 texcoord		: TEXCOORD;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 UV = IN.positionHCS.xy / _ScaledScreenParams.xy;

                #if UNITY_REVERSED_Z
                    real depth = SampleSceneDepth(UV);
                #else
                    real depth = lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(UV));
                #endif

                float3 worldPos = ComputeWorldSpacePosition(UV, depth, UNITY_MATRIX_I_VP);

				//float4 color = SAMPLE_TEXTURE2D_X(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, IN.texcoord);
				float4 color = tex2D(_CameraOpaqueTexture,IN.texcoord);

                //#if UNITY_REVERSED_Z
                //    if(depth < 0.0001)
                //        return half4(0,0,0,1);
                //#else
                //    if(depth > 0.9999)
                //        return half4(0,0,0,1);
                //#endif

                return depth*1000;
            }
            ENDHLSL
        }
    }
}