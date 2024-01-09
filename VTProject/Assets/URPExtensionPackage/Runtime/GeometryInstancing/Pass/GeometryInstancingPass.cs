using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Rendering.Universal.Internal
{
    public class GeometryInstancingPass : ScriptableRenderPass
    {
        public EInstancePassType InstancePassType { get; private set; }
        
        private static GeometryInstancingManager s_Manager;
        public GeometryInstancingPass(RenderPassEvent renderPassEvent, EInstancePassType type)
        {
            this.renderPassEvent = renderPassEvent;
            this.InstancePassType = type;
            s_Manager = GeometryInstancingManager.Instance;
        }
            
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (!Application.isPlaying)
            {
                return;
            }
            
            Camera cam = renderingData.cameraData.camera;
            InstancePassInfo passInfo = s_Manager.SafeGetPassInfo(cam, InstancePassType);
            passInfo.UpdateCameraPlanes(cam);
            CommandBuffer cmd = CommandBufferPool.Get(InstanceConst.GEOMETRY_INSTANCING_PASS_TAG);
            using (new ProfilingScope(cmd, new ProfilingSampler(InstanceConst.GEOMETRY_INSTANCING_PASS_TAG)))
            {
                cmd.Clear();
                var buffers = s_Manager.BatchGroupBuffers;
                for (int i = 0; i < buffers.Count; i++)
                {
                     buffers[i].DrawBatch(cmd,passInfo);
                }
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
            }
            CommandBufferPool.Release(cmd);
        }
    }
}
