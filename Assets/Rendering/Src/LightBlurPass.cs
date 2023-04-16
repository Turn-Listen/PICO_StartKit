namespace UnityEngine.Rendering.Universal
{
    class CustomRenderPass : ScriptableRenderPass
    {
        public Material passMat = null;
        public Material passMat2 = null;
        public Material passMat3 = null;
        public string passName;
        public int passdownsample = 1;
        public int passloop = 2;
        public float passblur = 4;
        public float _BlurLightIntensity = 1.0f;
        string passTag; // ���֣������frameDebug�￴��
        private RenderTargetIdentifier passSource { get; set; } // Դͼ��

        public CustomRenderPass(RenderPassEvent renderPassEvent, Material passMat, Material passMat2, Material passMat3, string passName, int passdownsample, int passloop, float passblur, float _BlurLightIntensity, string passTag)
        {
            this.renderPassEvent = renderPassEvent;
            this.passMat = passMat;
            this.passMat2 = passMat2;
            this.passMat3 = passMat3;
            this.passName = passName;
            this.passdownsample = passdownsample;
            this.passloop = passloop;
            this.passblur = passblur;
            this._BlurLightIntensity = _BlurLightIntensity;
            this.passTag = passTag;
        }

        public void setup(RenderTargetIdentifier sour) // �������ͼ��copy��������������ȡ����renderEvent
        {
            this.passSource = sour;
        }

        private void Render(CommandBuffer cmd, RenderTargetIdentifier target, Material material)
        {
            cmd.SetRenderTarget(target);
            cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, material, 0);
        }


        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            // ����shaderGUI��Ĵ��ݣ�����Ͳ���setfloat�ˣ�������shaderProperty��ID��,��Ϊ����Ҫ��id������ʱͼ��
            int TempID1 = Shader.PropertyToID("Temp1");         // ��ʱͼ��Ҳ������ Handle��ID���ҾͿ���
            int TempID2 = Shader.PropertyToID("Temp2");
            int BlurLightIntensityID = Shader.PropertyToID("_BlurLightIntensity");
            int ScreenID = Shader.PropertyToID("_ScreenTex");

            CommandBuffer cmd = CommandBufferPool.Get(passTag); // ������cbuffer����������Ⱦ����Ȧ����
            RenderTextureDescriptor getCameraData = renderingData.cameraData.cameraTargetDescriptor;   // �õ�������ݣ����㴴��������Ե�rt
            RenderTextureDescriptor getCameraDataNoDS = renderingData.cameraData.cameraTargetDescriptor;   // �õ�������ݣ����㴴��������Ե�rt
            int width = getCameraData.width / passdownsample; //downSample
            int height = getCameraData.height / passdownsample;

            // ��ȡ��ʱ��Ⱦ���� ע��hdr��֧��
            cmd.GetTemporaryRT(TempID1, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.DefaultHDR); //����һ����ʱͼ�񣬲��������rt�Ĳ�����ȥ
            cmd.GetTemporaryRT(TempID2, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.DefaultHDR);
            cmd.GetTemporaryRT(ScreenID, getCameraDataNoDS);

            RenderTargetIdentifier Temp1 = TempID1;
            RenderTargetIdentifier Temp2 = TempID2;
            RenderTargetIdentifier Screen = ScreenID;

            // ���ò���
            cmd.SetGlobalFloat("_Blur", 1f);
            cmd.SetGlobalFloat(BlurLightIntensityID, _BlurLightIntensity);

            // ����
            cmd.Blit(passSource, Screen);

            cmd.Blit(passSource, Temp1, passMat);   // ��Դ��ͼ���뵽���ʶ�Ӧ��pass�ﴦ�����Ѵ�������ͼ��洢����ʱͼ��
            for (int t = 1; t < passloop; t++)      // ÿ��ѭ���൱�ڰ��Ѿ�ģ����ͼƬ�Ž�������ģ������
            {
                cmd.SetGlobalFloat("_Blur", t * passblur); // 1.5
                cmd.Blit(Temp1, Temp2, passMat);
                var temRT = Temp1;
                Temp1 = Temp2;
                Temp2 = temRT;
            }

            cmd.SetGlobalTexture("_ScreenTex", Screen);
            cmd.Blit(Temp1, passSource, passMat2);
            Render(cmd, passSource, passMat3);
            

            // �ͷ�
            cmd.ReleaseTemporaryRT(ScreenID);   // �ͷ� RT 
            cmd.ReleaseTemporaryRT(TempID1);    // ����������������ͷŵĻ��ͻ�һֱ���У���Ӱ�� passSource �� Tex ͼ��
            cmd.ReleaseTemporaryRT(TempID2);
            context.ExecuteCommandBuffer(cmd);  //ִ����������ĸ�����
            CommandBufferPool.Release(cmd);     //�ͷŸ�����
        }

        //public override void FrameCleanup(CommandBuffer cmd)
        //{
        //    cmd.ReleaseTemporaryRT(g);
        //}
    }
}