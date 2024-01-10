using System.Collections.Generic;

namespace UnityEngine.Rendering.Universal.Internal
{
    public class ImpostorSnapshotPass: ScriptableRenderPass
    {


        private Matrix4x4 m_worldToCameraMatrix;
        private Matrix4x4 m_projectionMatrix;
        private Vector3 m_verticeScale;
        
        
        private  RenderTexture m_tempColor;
        private RenderTexture m_tempDepth;
        
        private bool m_pause = false;
        private Vector3 m_lastEulerAngles = Vector3.zero;
        private static GeometryInstancingManager s_Manager;
        
        public ImpostorSnapshotPass(RenderPassEvent evt)
        {
            renderPassEvent = evt;
            
            
            m_worldToCameraMatrix = Matrix4x4.identity;
            m_projectionMatrix = Matrix4x4.Ortho(-1, 1, -1, 1, ImpostorConst.IMPOSTOR_PROJECT_NEAR, ImpostorConst.IMPOSTOR_PROJECT_FAR);

            m_tempColor = new RenderTexture(ImpostorConst.SnapRTSize, ImpostorConst.SnapRTSize, 16, ImpostorConst.AtlasColorFormat, RenderTextureReadWrite.Linear);
            m_tempColor.name = "SnapshotTempColor";
            m_tempColor.useMipMap = false;
            m_tempColor.filterMode = FilterMode.Bilinear;
            m_tempColor.autoGenerateMips = false;

            m_tempDepth = new RenderTexture(ImpostorConst.SnapRTSize, ImpostorConst.SnapRTSize, 16, ImpostorConst.AtlasColorFormat, RenderTextureReadWrite.Linear);
            m_tempDepth.name = "SnapshotTempDepth";
            m_tempDepth.useMipMap = false;
            m_tempDepth.filterMode = FilterMode.Point;
            s_Manager = GeometryInstancingManager.Instance;
        }
        
        
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            Camera cam = renderingData.cameraData.camera;
            if (cam == null)
                return;

            if (!UniversalRenderPipeline.IsGameCamera(cam))
            {
                // 只有GameCamera才刷新
                return;
            }
            int shadowLightIndex = renderingData.lightData.mainLightIndex;
            if (shadowLightIndex == -1)
            {
                return;
            }

            Vector3 eulerAngles = cam.transform.eulerAngles;
            if (IsNeedUpdate(eulerAngles))
            {
                Update();
            }
            if (!m_pause && s_Manager.IsTaskNotEmpty())
            {
                //Vector3 vEye = -cam.transform.forward * 15;
                //_worldToCameraMatrix = Matrix4x4.LookAt(vEye, Vector3.zero, cam.transform.up);

                //Note that camera space matches OpenGL convention: camera's forward is the negative Z axis. This is different from Unity's convention, where forward is the positive Z axis.
                //注意相机空间和OpenGl的约定相匹配，相机的前面为Z轴负方向，这不同于Unity的约定，向前为Z轴正向。
                Vector3 vEye = Vector3.zero;
                Matrix4x4 worldToLocalMatrix = Matrix4x4.Rotate(cam.transform.rotation);
                worldToLocalMatrix.m03 = vEye.x;
                worldToLocalMatrix.m13 = vEye.y;
                worldToLocalMatrix.m23 = vEye.z;
                worldToLocalMatrix = worldToLocalMatrix.inverse;
                m_worldToCameraMatrix = Matrix4x4.Scale(new Vector3(1, 1, -1)) * worldToLocalMatrix;

                CommandBuffer cmd = CommandBufferPool.Get(ImpostorConst.IMPOSTOR_SNAPSHOT_PASS);
                using (ProfilingScope scope = new ProfilingScope(cmd, new ProfilingSampler(ImpostorConst.IMPOSTOR_SNAPSHOT_PASS)))
                {
                    cmd.Clear();
                    cmd.SetViewProjectionMatrices(m_worldToCameraMatrix,m_projectionMatrix);
                    cmd.SetGlobalVector(ImpostorConst.ImpostorZBufferParam,ComputeZBufferParam());
                    Queue<SnapshotTask> queue = s_Manager.SwitchTask();
                    while (queue.Count > 0)
                    {
                        SnapshotTask task = queue.Dequeue();
                        if (task.Mesh!=null)
                        {
                            Bounds aabb = task.Mesh.bounds;
                            float scale = 1 / Mathf.Max(Mathf.Max(aabb.extents.x, aabb.extents.y), aabb.extents.z);
                            //float scale = 1 / Mathf.Max(aabb.extents.x, aabb.extents.y);
                            m_verticeScale = new Vector3(scale, scale, scale);
                            Matrix4x4 modelMatrix = Matrix4x4.Scale(m_verticeScale) * Matrix4x4.Translate(-aabb.center);
                            // 零时处理，对应fbx导入时轴对齐，x轴-90
                            //Matrix4x4 modelMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(-90, 0, 0), _verticeScale) * Matrix4x4.Translate(-aabb.center);
                            cmd.SetRenderTarget(m_tempColor, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
                            cmd.ClearRenderTarget(true, true, ImpostorConst.ImpostorBackGroundColor);
                            for (int i = 0; i < task.Materials.Length; ++i)
                            {
                                cmd.DrawMesh(task.Mesh, modelMatrix, task.Materials[i], i, task.ShaderPass[i]);
                            }
                            if (task.Atlas != null)
                            {
                                task.Atlas.Blit(cmd, m_tempColor, ImpostorSnapshotAtlas.EBlitMode.BLIT_COLOR);
                            }
                            cmd.SetRenderTarget(m_tempDepth, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
                            cmd.ClearRenderTarget(true, true, new Color(0, 0, 0, 0));
                            for (int i = 0; i < task.Materials.Length; ++i)
                            {
                                cmd.DrawMesh(task.Mesh, modelMatrix, task.Materials[i], i, task.DepthPass[i]);
                            }
                            if (task.Atlas != null)
                            {
                                task.Atlas.Blit(cmd, m_tempDepth, ImpostorSnapshotAtlas.EBlitMode.BLIT_DEPTH);
                            }
                        }
                    }
                    cmd.SetViewProjectionMatrices(cam.worldToCameraMatrix, cam.projectionMatrix);
                    context.ExecuteCommandBuffer(cmd);
                    cmd.Clear();
                }
                CommandBufferPool.Release(cmd);
            }
        }
        private void Update()
        {
            GeometryInstancingManager.Instance.RefreshImpostor();
        }
        
        private Vector4 ComputeZBufferParam()
        {
            float n = ImpostorConst.IMPOSTOR_PROJECT_NEAR;
            float f = ImpostorConst.IMPOSTOR_PROJECT_FAR;
            float v = n > 0 ? (f - n) / n * f : 0;
            return new Vector4(n, f - n, v, 1 / f);
        }
        private bool IsNeedUpdate(Vector3 eulerAngles)
        {
            // 如果光方向也会变，则这里也得加上判断
            if (m_pause)
                return false;

            bool isNeedUpdate = false;

            if (Vector3.Distance(m_lastEulerAngles, eulerAngles) > 5)
            {
                m_lastEulerAngles = eulerAngles;
                isNeedUpdate = true;
            }

            return isNeedUpdate;
        }
    }
}