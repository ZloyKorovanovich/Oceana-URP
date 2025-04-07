using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Oceana {
    public class OceanaScrollPass : ScriptableRenderPass {
        private const int k_KernelID = 0;
        private const int k_GroupX = 32;
        private const int k_GroupY = 32;

        private ComputeShader m_Shader;
        private Texture2DArray m_ScrollArray;
        private GraphicsFormat m_ScrollFormat;
        private Vector4[] m_ScrollArrayST;
        private int m_ScrollCount;
        private int m_Resolution;
        private int m_MipLevel;

        private OceanaSettings m_Settings;

        public OceanaScrollPass(RenderPassEvent injection) {
            renderPassEvent = injection;
        }

        public void FetchSettings(OceanaSettings settings) {
            if (m_Settings != null) m_Settings.OnUpdate -= FetchProperties;

            if (settings != null) {
                m_Settings = settings;
                m_Settings.OnUpdate += FetchProperties;
                FetchProperties();
            }
            else {
                m_Settings = null;
            }
        }

        private void FetchProperties() {
            m_Shader = m_Settings.ScrollShader;
            m_ScrollArray = m_Settings.ScrollArray;
            m_ScrollFormat = m_Settings.ScrollFormat;
            m_ScrollArrayST = m_Settings.ScrollArrayST;

            m_Resolution = (int)m_Settings.ScrollResolution;
            m_ScrollCount = m_ScrollArray.depth;
            m_MipLevel = Mathf.Clamp(m_ScrollArray.width / m_Resolution, 1, m_ScrollArray.mipmapCount) - 1;
        }

        public class ScrollGlobalData : ContextItem {
            public TextureHandle scrollMap;

            public override void Reset() {
                scrollMap = TextureHandle.nullHandle;
            }
        }

        private class ScrollPassData {
            internal RTHandle scrollRT;

            internal TextureHandle scrollFetch;
            internal TextureHandle scrollOutput;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData) {
            using (IComputeRenderGraphBuilder builder = renderGraph.AddComputePass(passName, out ScrollPassData passData)) {
                RTHandle scrollArrayRT = RTHandles.Alloc(m_ScrollArray);
                passData.scrollFetch = renderGraph.ImportTexture(scrollArrayRT);

                RenderTextureDescriptor scrollMapDesc = new RenderTextureDescriptor(m_Resolution, m_Resolution) {
                    useMipMap = true,
                    autoGenerateMips = false,
                    graphicsFormat = m_ScrollFormat,
                    sRGB = false,
                    enableRandomWrite = true
                };
                RenderingUtils.ReAllocateHandleIfNeeded(ref passData.scrollRT, in scrollMapDesc, FilterMode.Bilinear, TextureWrapMode.Repeat);
                passData.scrollOutput = renderGraph.ImportTexture(passData.scrollRT);

                ScrollGlobalData mapData = frameData.GetOrCreate<ScrollGlobalData>();
                mapData.scrollMap = passData.scrollOutput;

                builder.UseTexture(passData.scrollFetch, AccessFlags.Read);
                builder.UseTexture(passData.scrollOutput, AccessFlags.Write);

                builder.SetRenderFunc((ScrollPassData data, ComputeGraphContext context) => Execute(data, context));
            }
        }

        private void Execute(ScrollPassData data, ComputeGraphContext context) {
            context.cmd.SetComputeVectorArrayParam(m_Shader, "_ScrollArray_ST", m_ScrollArrayST);
            context.cmd.SetComputeIntParam(m_Shader, "_ScrollCount", m_ScrollCount);
            context.cmd.SetComputeFloatParam(m_Shader, "_Time", Time.time);

            context.cmd.SetComputeTextureParam(m_Shader, k_KernelID, "_ScrollArray", data.scrollFetch);

            context.cmd.SetComputeIntParam(m_Shader, "_ScrollResolution", m_Resolution);
            context.cmd.SetComputeIntParam(m_Shader, "_MipLevel", m_MipLevel);

            context.cmd.SetComputeTextureParam(m_Shader, k_KernelID, "_ScrollMap", data.scrollOutput);
            context.cmd.DispatchCompute(m_Shader, k_KernelID, m_Resolution / k_GroupX, m_Resolution / k_GroupY, 1);

            data.scrollRT.rt.GenerateMips();
        }

        public void Dispose() {
            if (m_Settings != null) m_Settings.OnUpdate -= FetchProperties;
        }
    }
}