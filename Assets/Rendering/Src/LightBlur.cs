namespace UnityEngine.Rendering.Universal
{
    [ExecuteInEditMode]
    public class LightBlur : ScriptableRendererFeature
    {
        public static LightBlur Instance { get; set; }

        [System.Serializable]
        public class featureSetting
        {
            public RenderPassEvent passEvent = RenderPassEvent.AfterRenderingTransparents;
            public string passRenderName = "LightBlur";
            public Material passMat;
            public Material passMat2;
            public Material passMat3;
            [Range(1, 10)] public int downsample = 1;
            [Range(1, 10)] public int loop = 2;
            [Range(0.5f, 5)] public float blur = 0.5f;
            public float _BlurLightIntensity = 1.0f;
        }

        public featureSetting setting = new featureSetting();

        CustomRenderPass m_ScriptablePass;

        public override void Create()
        {
            m_ScriptablePass = new CustomRenderPass(setting.passEvent, setting.passMat, setting.passMat2, setting.passMat3,
                setting.passRenderName, setting.downsample, setting.loop, setting.blur, setting._BlurLightIntensity, this.name);

        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (Instance == null)
            {
                Instance = this;
            }
            m_ScriptablePass.setup(renderer.cameraColorTarget);     // 通过setup函数设置不同的渲染阶段的渲染结果进 passSource 里面
            renderer.EnqueuePass(m_ScriptablePass);                 // 执行
        }
    }
}

