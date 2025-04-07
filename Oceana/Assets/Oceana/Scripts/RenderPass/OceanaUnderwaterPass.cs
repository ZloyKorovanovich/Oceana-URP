using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule.Util;

namespace Oceana {
    public class OceanaUnderwaterPass : ScriptableRenderPass {
        private Material m_Material;
        private float m_SeaLevel;
        private float m_DisplaceHeight;
        private Vector2 m_ScrollST;

        private OceanaSettings m_Settings;

        public OceanaUnderwaterPass(RenderPassEvent injection) {
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
            m_Material = m_Settings.UnderwaterMaterial;
            m_SeaLevel = m_Settings.SeaLevel;
            m_DisplaceHeight = m_Settings.DisplaceHeight;
            m_ScrollST = m_Settings.ScrollST;
        }

        private class UnderwaterPassData {
            public TextureHandle scrollMap;

            public TextureHandle sourceColor;
            public TextureHandle sourceDepth;

            public TextureHandle cameraColor;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData) {
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            ScreenCopyPassData copyData = frameData.Get<ScreenCopyPassData>();

            renderGraph.AddBlitPass(resourceData.activeColorTexture, copyData.sceneColor,
                Vector2.one, Vector2.zero, filterMode: RenderGraphUtils.BlitFilterMode.ClampNearest);
            renderGraph.AddBlitPass(resourceData.activeDepthTexture, copyData.sceneDepth,
                Vector2.one, Vector2.zero, filterMode: RenderGraphUtils.BlitFilterMode.ClampNearest);

            using (IRasterRenderGraphBuilder builder = renderGraph.AddRasterRenderPass(passName, out UnderwaterPassData passData)) {
                UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
                OceanaScrollPass.ScrollGlobalData mapData = frameData.Get<OceanaScrollPass.ScrollGlobalData>();
                ScreenCopyPassData screenData = frameData.Get<ScreenCopyPassData>();

                passData.scrollMap = mapData.scrollMap;
                passData.cameraColor = resourceData.activeColorTexture;
                passData.sourceColor = screenData.sceneColor;
                passData.sourceDepth = screenData.sceneDepth;

                builder.UseTexture(passData.scrollMap, AccessFlags.Read);
                builder.UseTexture(passData.sourceColor, AccessFlags.Read);
                builder.UseTexture(passData.sourceDepth, AccessFlags.Read);
                builder.SetRenderAttachment(resourceData.activeColorTexture, 0, AccessFlags.ReadWrite);

                builder.SetRenderFunc((UnderwaterPassData data, RasterGraphContext context) => Execute(data, context));
            }
        }

        private void Execute(UnderwaterPassData data, RasterGraphContext context) {
            m_Material.SetFloat("_SeaLevel", m_SeaLevel);
            m_Material.SetFloat("_DisplaceHeight", m_DisplaceHeight);
            m_Material.SetVector("_ScrollMap_ST", m_ScrollST);

            m_Material.SetTexture("_ScrollMap", data.scrollMap);
            m_Material.SetTexture("_SourceColor", data.sourceColor);
            m_Material.SetTexture("_SourceDepth", data.sourceDepth);
            context.cmd.DrawProcedural(Matrix4x4.identity, m_Material, 0, MeshTopology.Quads, 6);
        }

        public void Dispose() {
            if (m_Settings != null) m_Settings.OnUpdate -= FetchProperties;
        }
    }
}