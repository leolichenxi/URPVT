﻿
using UnityEngine.Rendering.Universal.Internal;

namespace UnityEngine.Rendering.Universal
{
    [DisallowMultipleRendererFeature]
    [Tooltip("Impostor Feature")]
    public class ImpostorFeature : ScriptableRendererFeature
    {
        private GeometryInstancingPass m_geometryInstancingOpaquePass;
        private GeometryInstancingPass m_geometryInstancingTransparentsPass;
        public override void Create()
        {
            m_geometryInstancingOpaquePass = new GeometryInstancingPass(RenderPassEvent.BeforeRenderingOpaques + 1,EInstancePassType.RenderingOpaque);
            m_geometryInstancingTransparentsPass = new GeometryInstancingPass(RenderPassEvent.BeforeRenderingTransparents,EInstancePassType.RenderingTransparent);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.renderType == CameraRenderType.Overlay)
            {
                return;
            }
            renderer.EnqueuePass(m_geometryInstancingOpaquePass);
            renderer.EnqueuePass(m_geometryInstancingTransparentsPass);
        }
    }
}