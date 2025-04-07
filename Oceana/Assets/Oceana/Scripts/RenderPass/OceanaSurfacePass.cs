using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

namespace Oceana {
    public class ScreenCopyPassData : ContextItem {
        public TextureHandle sceneColor;
        public TextureHandle sceneDepth;

        public override void Reset() {
            sceneColor = TextureHandle.nullHandle;
            sceneDepth = TextureHandle.nullHandle;
        }
    }

    public class OceanaSurfacePass : ScriptableRenderPass {
        private Material m_Material;
        private Mesh m_Mesh;
        private Vector4 m_ScrollST = new Vector4(1, 1, 0, 0);
        private float m_SeaLevel = 0;
        private float m_DisplaceHeight = 1;

        private OceanaSettings m_Settings;

        public OceanaSurfacePass(RenderPassEvent injection) {
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

        public void FetchProperties() {
            m_Material = m_Settings.SurfaceMaterial;
            m_Mesh = m_Settings.SurfaceMesh;
            m_ScrollST = m_Settings.ScrollST;
            m_SeaLevel = m_Settings.SeaLevel;
            m_DisplaceHeight = m_Settings.DisplaceHeight;
        }

        private class SurfacePassData {
            public TextureHandle scrollMap;

            public TextureHandle sourceColor;
            public TextureHandle sourceDepth;

            public TextureHandle cameraColor;
            public TextureHandle cameraDepth;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData) {
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            ScreenCopyPassData copyData = frameData.GetOrCreate<ScreenCopyPassData>();

            TextureDesc sceneColorDesc = renderGraph.GetTextureDesc(resourceData.activeColorTexture);
            sceneColorDesc.bindTextureMS = false;
            sceneColorDesc.msaaSamples = MSAASamples.None;
            TextureDesc sceneDepthDesc = renderGraph.GetTextureDesc(resourceData.activeDepthTexture);
            sceneDepthDesc.bindTextureMS = false;
            sceneDepthDesc.msaaSamples = MSAASamples.None;
            sceneDepthDesc.colorFormat = GraphicsFormat.R32_SFloat;
            sceneDepthDesc.format = GraphicsFormat.R32_SFloat;

            copyData = frameData.GetOrCreate<ScreenCopyPassData>();
            copyData.sceneColor = renderGraph.CreateTexture(sceneColorDesc);
            copyData.sceneDepth = renderGraph.CreateTexture(sceneDepthDesc);

            renderGraph.AddBlitPass(resourceData.activeColorTexture, copyData.sceneColor,
                Vector2.one, Vector2.zero, filterMode: RenderGraphUtils.BlitFilterMode.ClampNearest);
            renderGraph.AddBlitPass(resourceData.activeDepthTexture, copyData.sceneDepth, 
                Vector2.one, Vector2.zero, filterMode: RenderGraphUtils.BlitFilterMode.ClampNearest);

            using (IRasterRenderGraphBuilder builder = renderGraph.AddRasterRenderPass(passName, out SurfacePassData passData)) {
                UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
                OceanaScrollPass.ScrollGlobalData mapData = frameData.Get<OceanaScrollPass.ScrollGlobalData>();
                ScreenCopyPassData screenData = frameData.Get<ScreenCopyPassData>();

                passData.scrollMap = mapData.scrollMap;
                passData.cameraColor = resourceData.activeColorTexture;
                passData.cameraDepth = resourceData.activeDepthTexture;
                passData.sourceColor = screenData.sceneColor;
                passData.sourceDepth = screenData.sceneDepth;

                builder.UseTexture(passData.scrollMap, AccessFlags.Read);
                builder.UseTexture(passData.sourceColor, AccessFlags.Read);
                builder.UseTexture(passData.sourceDepth, AccessFlags.Read);

                builder.SetRenderAttachment(resourceData.activeColorTexture, 0, AccessFlags.ReadWrite);
                builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture, AccessFlags.ReadWrite);

                builder.SetRenderFunc((SurfacePassData data, RasterGraphContext context) => Execute(data, context));
            }
        }

        private void Execute(SurfacePassData data, RasterGraphContext context) {
            m_Material.SetFloat("_SeaLevel", m_SeaLevel);
            m_Material.SetFloat("_DisplaceHeight", m_DisplaceHeight);
            m_Material.SetVector("_ScrollMap_ST", m_ScrollST);

            m_Material.SetTexture("_ScrollMap", data.scrollMap);
            m_Material.SetTexture("_SourceColor", data.sourceColor);
            m_Material.SetTexture("_SourceDepth", data.sourceDepth);

            context.cmd.DrawMesh(m_Mesh, Matrix4x4.identity, m_Material);
        }

        public void Dispose() {
            if (m_Settings != null) m_Settings.OnUpdate -= FetchProperties;
        }
    }
}