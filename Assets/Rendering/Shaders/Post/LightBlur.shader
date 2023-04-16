Shader "WX/URP/Post/Kawaseblur"
{
    Properties
    {
      _MainTex("MainTex",2D) = "white"{}
    }
        SubShader
    {
        Tags{"RenderPipeline" = "UniversalRenderPipeline"}

        Cull Off ZWrite Off ZTest Always
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
        float _Blur,_BlurLightIntensity;
        float4 _MainTex_TexelSize;
        CBUFFER_END

        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);

         struct a2v
         {
             float4 positionOS:POSITION;
             float2 texcoord:TEXCOORD;
         };

         struct v2f
         {
             float4 positionCS:SV_POSITION;
             float2 texcoord:TEXCOORD;
         };
         v2f VERT(a2v i)
            {
                v2f o;
                o.positionCS = TransformObjectToHClip(i.positionOS.xyz);
                o.texcoord = i.texcoord;
                return o;
            }
         half4 FRAG(v2f i) :SV_TARGET
            {
                float4 col;
                half4 tex = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.texcoord);

				/*
				half4 tex1 = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.texcoord + float2(-1,-1) * _MainTex_TexelSize.xy * _Blur);  
                tex +=  tex1;
				
                half4 tex2 = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.texcoord + float2(1,-1) * _MainTex_TexelSize.xy * _Blur);
				tex +=  tex2;
				
                half4 tex3 = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.texcoord + float2(-1,1) * _MainTex_TexelSize.xy * _Blur);
				tex +=  tex3;
				
                half4 tex4 = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.texcoord + float2(1,1) * _MainTex_TexelSize.xy * _Blur);
				tex +=  tex4;
				*/

				half4 tex1 = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.texcoord + float2(-1,-1) * _MainTex_TexelSize.xy * _Blur);  
                tex +=  tex1 - min(min(tex1.r,tex1.g),tex1.b);
				
                half4 tex2 = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.texcoord + float2(1,-1) * _MainTex_TexelSize.xy * _Blur);
				tex +=  tex2 - min(min(tex2.r,tex2.g),tex2.b);
				
                half4 tex3 = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.texcoord + float2(-1,1) * _MainTex_TexelSize.xy * _Blur);
				tex +=  tex3 - min(min(tex3.r,tex3.g),tex3.b);
				
                half4 tex4 = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.texcoord + float2(1,1) * _MainTex_TexelSize.xy * _Blur);
				tex +=  tex4 - min(min(tex4.r,tex4.g),tex4.b);

                tex /= 5;

                return tex * _BlurLightIntensity;
            }
        ENDHLSL

        pass
        {
            HLSLPROGRAM
            #pragma vertex VERT
            #pragma fragment FRAG
            ENDHLSL
        }
    }
}