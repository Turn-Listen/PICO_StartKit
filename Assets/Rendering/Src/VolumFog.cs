using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[ExecuteInEditMode]
public class VolumFog : ScriptableRendererFeature
{
    [System.Serializable]
    public class featureSetting
    {
        public Shader m_shader;
        public Color m_FogColor;
        public float m_FogStart;
        public float m_FogEnd;
        public float m_NoiseCellSize;
        public float m_NoiseRoughness;
        public float m_NoisePersistance;
        public Vector3 m_NoiseSpeed;
        public float m_NoiseScale;
        public Material m_Material;

        public int passdownsample = 1;
        public string passRenderName = "VolumFog";
        public RenderPassEvent passEvent = RenderPassEvent.AfterRenderingTransparents;
    }

    public featureSetting setting = new featureSetting();


    class CustomRenderPass : ScriptableRenderPass
    {

        public Shader m_shader;
        public Color m_FogColor;
        public float m_FogStart;
        public float m_FogEnd;
        public float m_NoiseCellSize;
        public float m_NoiseRoughness;
        public float m_NoisePersistance;
        public Vector3 m_NoiseSpeed;
        public float m_NoiseScale;
        public Material m_Material;

        public int passdownsample = 1;
        public string passName;
        string passTag; // ���֣������frameDebug�￴��


        private RenderTargetIdentifier passSource { get; set; } // Դͼ��

        public void setup(RenderTargetIdentifier sour) // �������ͼ��copy��������������ȡ����renderEvent
        {
            this.passSource = sour;
        }
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            // ����shaderGUI��Ĵ��ݣ�����Ͳ���setfloat�ˣ�������shaderProperty��ID��,��Ϊ����Ҫ��id������ʱͼ��

            int ScreenID = Shader.PropertyToID("_DepthFogScreenTex");

            CommandBuffer cmd = CommandBufferPool.Get(passTag); // ������cbuffer����������Ⱦ����Ȧ����
            RenderTextureDescriptor getCameraData = renderingData.cameraData.cameraTargetDescriptor;   // �õ�������ݣ����㴴��������Ե�rt
            int width = getCameraData.width;// passdownsample; //downSample
            int height = getCameraData.height;// / passdownsample;

            // ��ȡ��ʱ��Ⱦ���� ע��hdr��֧��
            cmd.GetTemporaryRT(ScreenID, getCameraData);



            var cameraData = renderingData.cameraData;
            if (cameraData.camera.cameraType != CameraType.Game)
                return;

            if (m_Material == null)
                return;



            m_Material.SetColor("_FogColor", m_FogColor);
            m_Material.SetFloat("_FogStart", m_FogStart);
            m_Material.SetFloat("_FogEnd", m_FogEnd);
            m_Material.SetFloat("_NoiseCellSize", m_NoiseCellSize);
            m_Material.SetFloat("_NoiseRoughness", m_NoiseRoughness);
            m_Material.SetFloat("_NoisePersistance", m_NoisePersistance);
            m_Material.SetVector("_NoiseSpeed", m_NoiseSpeed);
            m_Material.SetFloat("_NoiseScale", m_NoiseScale);

            //m_Material.SetMatrix("_InverseView", renderingData.cameraData.camera.cameraToWorldMatrix);


            RenderTargetIdentifier Screen = ScreenID;

            // ����
            cmd.Blit(passSource, Screen);
            cmd.Blit(Screen, passSource, m_Material);

            cmd.SetGlobalTexture("_DepthFogScreenTex", Screen);

            // �ͷ�
            cmd.ReleaseTemporaryRT(ScreenID);   // �ͷ� RT 

            context.ExecuteCommandBuffer(cmd);  //ִ����������ĸ�����
            CommandBufferPool.Release(cmd);     //�ͷŸ�����
        }
    }
    CustomRenderPass m_ScriptablePass;
    public override void Create()
    {
        m_ScriptablePass = new CustomRenderPass();
        m_ScriptablePass.renderPassEvent = setting.passEvent;   // ��Ⱦλ��

        m_ScriptablePass.passName = setting.passRenderName;     // ��Ⱦ����
        m_ScriptablePass.passdownsample = setting.passdownsample;

        m_ScriptablePass.m_shader = setting.m_shader;
        m_ScriptablePass.m_FogColor = setting.m_FogColor;
        m_ScriptablePass.m_FogStart = setting.m_FogStart;
        m_ScriptablePass.m_FogEnd = setting.m_FogEnd;
        m_ScriptablePass.m_NoiseCellSize = setting.m_NoiseCellSize;
        m_ScriptablePass.m_NoiseRoughness = setting.m_NoiseRoughness;
        m_ScriptablePass.m_NoisePersistance = setting.m_NoisePersistance;
        m_ScriptablePass.m_NoiseSpeed = setting.m_NoiseSpeed;
        m_ScriptablePass.m_NoiseScale = setting.m_NoiseScale;
        m_ScriptablePass.m_Material = setting.m_Material;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        m_ScriptablePass.setup(renderer.cameraColorTarget);     // ͨ��setup�������ò�ͬ����Ⱦ�׶ε���Ⱦ����� passSource ����
        renderer.EnqueuePass(m_ScriptablePass);                 // ִ��
    }
}