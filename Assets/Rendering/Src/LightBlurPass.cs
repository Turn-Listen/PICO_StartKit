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
        string passTag; // 名字，方便从frameDebug里看到
        private RenderTargetIdentifier passSource { get; set; } // 源图像

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

        public void setup(RenderTargetIdentifier sour) // 把相机的图像copy过来，具体哪张取决于renderEvent
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
            // 类似shaderGUI里的传递，这里就不是setfloat了，就是拿shaderProperty的ID了,因为下面要用id创建临时图像
            int TempID1 = Shader.PropertyToID("Temp1");         // 临时图像，也可以用 Handle，ID不乱就可以
            int TempID2 = Shader.PropertyToID("Temp2");
            int BlurLightIntensityID = Shader.PropertyToID("_BlurLightIntensity");
            int ScreenID = Shader.PropertyToID("_ScreenTex");

            CommandBuffer cmd = CommandBufferPool.Get(passTag); // 类似于cbuffer，把整个渲染命令圈起来
            RenderTextureDescriptor getCameraData = renderingData.cameraData.cameraTargetDescriptor;   // 拿到相机数据，方便创建共享屬性的rt
            RenderTextureDescriptor getCameraDataNoDS = renderingData.cameraData.cameraTargetDescriptor;   // 拿到相机数据，方便创建共享屬性的rt
            int width = getCameraData.width / passdownsample; //downSample
            int height = getCameraData.height / passdownsample;

            // 获取临时渲染纹理 注意hdr的支持
            cmd.GetTemporaryRT(TempID1, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.DefaultHDR); //申请一个临时图像，并设置相机rt的参数进去
            cmd.GetTemporaryRT(TempID2, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.DefaultHDR);
            cmd.GetTemporaryRT(ScreenID, getCameraDataNoDS);

            RenderTargetIdentifier Temp1 = TempID1;
            RenderTargetIdentifier Temp2 = TempID2;
            RenderTargetIdentifier Screen = ScreenID;

            // 设置参数
            cmd.SetGlobalFloat("_Blur", 1f);
            cmd.SetGlobalFloat(BlurLightIntensityID, _BlurLightIntensity);

            // 拷贝
            cmd.Blit(passSource, Screen);

            cmd.Blit(passSource, Temp1, passMat);   // 把源贴图输入到材质对应的pass里处理，并把处理结果的图像存储到临时图像；
            for (int t = 1; t < passloop; t++)      // 每次循环相当于把已经模糊的图片放进来进行模糊运算
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
            

            // 释放
            cmd.ReleaseTemporaryRT(ScreenID);   // 释放 RT 
            cmd.ReleaseTemporaryRT(TempID1);    // 看起来好像如果不释放的话就会一直运行，会影响 passSource 的 Tex 图像
            cmd.ReleaseTemporaryRT(TempID2);
            context.ExecuteCommandBuffer(cmd);  //执行命令缓冲区的该命令
            CommandBufferPool.Release(cmd);     //释放该命令
        }

        //public override void FrameCleanup(CommandBuffer cmd)
        //{
        //    cmd.ReleaseTemporaryRT(g);
        //}
    }
}