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
        string passTag; // 名字，方便从frameDebug里看到


        private RenderTargetIdentifier passSource { get; set; } // 源图像

        public void setup(RenderTargetIdentifier sour) // 把相机的图像copy过来，具体哪张取决于renderEvent
        {
            this.passSource = sour;
        }
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            // 类似shaderGUI里的传递，这里就不是setfloat了，就是拿shaderProperty的ID了,因为下面要用id创建临时图像

            int ScreenID = Shader.PropertyToID("_DepthFogScreenTex");

            CommandBuffer cmd = CommandBufferPool.Get(passTag); // 类似于cbuffer，把整个渲染命令圈起来
            RenderTextureDescriptor getCameraData = renderingData.cameraData.cameraTargetDescriptor;   // 拿到相机数据，方便创建共享屬性的rt
            int width = getCameraData.width;// passdownsample; //downSample
            int height = getCameraData.height;// / passdownsample;

            // 获取临时渲染纹理 注意hdr的支持
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

            // 拷贝
            cmd.Blit(passSource, Screen);
            cmd.Blit(Screen, passSource, m_Material);

            cmd.SetGlobalTexture("_DepthFogScreenTex", Screen);

            // 释放
            cmd.ReleaseTemporaryRT(ScreenID);   // 释放 RT 

            context.ExecuteCommandBuffer(cmd);  //执行命令缓冲区的该命令
            CommandBufferPool.Release(cmd);     //释放该命令
        }
    }
    CustomRenderPass m_ScriptablePass;
    public override void Create()
    {
        m_ScriptablePass = new CustomRenderPass();
        m_ScriptablePass.renderPassEvent = setting.passEvent;   // 渲染位置

        m_ScriptablePass.passName = setting.passRenderName;     // 渲染名称
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
        m_ScriptablePass.setup(renderer.cameraColorTarget);     // 通过setup函数设置不同的渲染阶段的渲染结果进 passSource 里面
        renderer.EnqueuePass(m_ScriptablePass);                 // 执行
    }
}