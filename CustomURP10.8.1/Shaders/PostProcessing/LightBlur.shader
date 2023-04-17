Shader "Hidden/Universal Render Pipeline/LightBlur"
{
    HLSLINCLUDE
        #pragma exclude_renderers gles
		#pragma multi_compile_local _ _USE_RGBM
        #pragma multi_compile _ _USE_DRAW_PROCEDURAL

        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Filtering.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"

        TEXTURE2D_X(_SourceTex);
        float4 _SourceTex_TexelSize;
		float _Blur;

		half4 FragPrefilter(Varyings input) : SV_Target
        {
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
            float2 uv = UnityStereoTransformScreenSpaceTex(input.uv);


            half4 color = SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv);

            return color;
        }



        half4 FragBlurH(Varyings input) : SV_Target
        {
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

            float2 uv = UnityStereoTransformScreenSpaceTex(input.uv);

			half4 tex = SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp,  uv);

			half4 tex1 = SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv + float2(-1,-1) * _SourceTex_TexelSize.xy * _Blur);  
            tex +=  tex1 - min(min(tex1.r,tex1.g),tex1.b);
				
            half4 tex2 = SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv + float2(1,-1) * _SourceTex_TexelSize.xy * _Blur);
			tex +=  tex2 - min(min(tex2.r,tex2.g),tex2.b);
			
            half4 tex3 = SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv + float2(-1,1) * _SourceTex_TexelSize.xy * _Blur);
			tex +=  tex3 - min(min(tex3.r,tex3.g),tex3.b);
			
            half4 tex4 = SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv + float2(1,1) * _SourceTex_TexelSize.xy * _Blur);
			tex +=  tex4 - min(min(tex4.r,tex4.g),tex4.b);

            tex /= 5;

            return tex;
        }

    ENDHLSL

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}

        LOD 100

        ZTest Always ZWrite Off Cull Off

		Pass
        {
            Name "Light Blur Copy"

            HLSLPROGRAM
                #pragma vertex FullscreenVert
                #pragma fragment FragPrefilter
            ENDHLSL
        }

        Pass
        {
            Name "Light Blur"

            HLSLPROGRAM
                #pragma vertex FullscreenVert
                #pragma fragment FragBlurH
            ENDHLSL
        }
    }
}
