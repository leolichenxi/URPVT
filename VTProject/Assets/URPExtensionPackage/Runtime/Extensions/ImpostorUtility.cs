using Unity.Mathematics;

namespace UnityEngine.Rendering.Universal
{
    public static class ImpostorUtility
    {
        public static void RenderShadowSlice(CommandBuffer cmd, ref ScriptableRenderContext context, ref RenderingData renderingData, int cascadeIndex,
            ref ShadowSliceData shadowSliceData, ref ShadowDrawingSettings settings,
            Matrix4x4 proj, Matrix4x4 view, Plane[] cullingPlanes = null)
        {
            Camera cam = renderingData.cameraData.camera;
#if UNITY_EDITOR
            if (!UniversalRenderPipeline.IsGameCamera(cam))
            {
                return;
            }
#endif
            GeometryInstancingManager manager = GeometryInstancingManager.Instance;
            InstancePassInfo passInfo = manager.SafeGetPassInfo(cam, EInstancePassType.ShadowCaster, cascadeIndex);
            
            // TODO 
            // if (!manager.CheckVisible(passInfo))
            // {
            //     return;
            // }
            passInfo.UpdateCameraPlanes(view, proj);
            if (cullingPlanes != null)
            {
                passInfo.CameraPlanes = cullingPlanes;
            }

            // CameraPos主要用于Impostor切换
            Matrix4x4 matCamera = view.inverse;
            passInfo.CameraPos.x = matCamera.m03;
            passInfo.CameraPos.y = matCamera.m13;
            passInfo.CameraPos.z = matCamera.m23;

            cmd.SetViewport(new Rect(shadowSliceData.offsetX, shadowSliceData.offsetY, shadowSliceData.resolution, shadowSliceData.resolution));
            cmd.EnableScissorRect(new Rect(shadowSliceData.offsetX + 4, shadowSliceData.offsetY + 4, shadowSliceData.resolution - 8, shadowSliceData.resolution - 8));

            cmd.SetViewProjectionMatrices(view, proj);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            for (int i = 0; i < manager.BatchGroupBuffers.Count; i++)
            {
                manager.BatchGroupBuffers[i].DrawBatch(cmd,passInfo);   
            }
            cmd.DisableScissorRect();
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            
        }

        public static bool DoCopyColor(CommandBuffer cmd, RenderTargetIdentifier src)
        {
            cmd.Blit(src, BuiltinRenderTextureType.CurrentActive);
            return true;
        }

        public static RenderTexture CreateSnapColorRT(string rtName)
        {
            RenderTextureFormat renderTextureFormat = InstanceConst.AtlasColorFormat;
            RenderTextureDescriptor descriptor = new RenderTextureDescriptor(InstanceConst.AtlasRTSize, InstanceConst.AtlasRTSize);
            descriptor.depthBufferBits = 0;
            descriptor.colorFormat = renderTextureFormat;
            descriptor.useMipMap = true;
            descriptor.autoGenerateMips = true;
            descriptor.mipCount = 4;
            descriptor.msaaSamples = 1;
            var rt = new RenderTexture(descriptor);

            rt.name = rtName;
            rt.filterMode = FilterMode.Bilinear;

            return new RenderTexture(descriptor);
        }

        public static RenderTexture CreateSnapShadowRT(string rtName)
        {
            RenderTextureFormat renderTextureFormat = InstanceConst.AtlasShadowFormat;
            RenderTextureDescriptor descriptor = new RenderTextureDescriptor(InstanceConst.AtlasRTSize, InstanceConst.AtlasRTSize);
            descriptor.depthBufferBits = 16;
            descriptor.colorFormat = renderTextureFormat;
            descriptor.useMipMap = false;
            descriptor.autoGenerateMips = false;
            descriptor.mipCount = 4;
            descriptor.sRGB = false;

            var rt = new RenderTexture(descriptor);

            rt.name = rtName;
            rt.filterMode = FilterMode.Point;

            return rt;
        }


        public static Mesh CreateImpostorMesh(Bounds bounds, ImpostorSnapshotAtlas.Snapshot snapshotRT)
        {
            Vector3 extents = bounds.extents;

            // 顶点
            Vector3[] vertices = new Vector3[4];
            // 计算长宽比
            float x = 1.0f;
            float y = 1.0f;
            float scale = 1.0f;
            if (extents.x < extents.y)
            {
                // 宽高比，为了减少面板面积
                x = extents.x / extents.y;
            }

            if (extents.x > extents.y)
            {
                // 宽高比，为了减少面板面积
                //y = extents.y / extents.x;
                // 由于正交投影和从斜上方投影，造成矮宽的物体Impostor大一点，这里特殊修正
                y = extents.y * 1.414f / extents.x;
                if (y > 1) y = 1;
                scale = y;
            }

            scale *= 0.5f; // 因为属性"_Size"默认是1，所以这里乘0.5

            vertices[0] = new Vector3(-x * scale, -y * scale, 0);
            vertices[1] = new Vector3(x * scale, -y * scale, 0);
            vertices[2] = new Vector3(x * scale, +y * scale, 0);
            vertices[3] = new Vector3(-x * scale, +y * scale, 0);

            // UV
            Vector2[] uvs = new Vector2[4];
            uvs[0] = new Vector2(0, 0);
            uvs[1] = new Vector2(1, 0);
            uvs[2] = new Vector2(1, 1);
            uvs[3] = new Vector2(0, 1);

            if (extents.x < extents.y)
            {
                uvs[0] = new Vector2(0.5f - x * 0.5f, 0);
                uvs[1] = new Vector2(0.5f + x * 0.5f, 0);
                uvs[2] = new Vector2(0.5f + x * 0.5f, 1);
                uvs[3] = new Vector2(0.5f - x * 0.5f, 1);
            }

            if (extents.x > extents.y)
            {
                uvs[0] = new Vector2(0, 0.5f - y * 0.5f);
                uvs[1] = new Vector2(1, 0.5f - y * 0.5f);
                uvs[2] = new Vector2(1, 0.5f + y * 0.5f);
                uvs[3] = new Vector2(0, 0.5f + y * 0.5f);
            }

            // uv在图集中的位置
            for (int i = 0; i < 4; i++)
            {
                uvs[i].x = snapshotRT.u + uvs[i].x * snapshotRT.size;
                uvs[i].y = snapshotRT.v + uvs[i].y * snapshotRT.size;
            }


            // 顶点索引
            int[] indices = new int[6];
            indices[0] = 0;
            indices[1] = 2;
            indices[2] = 1;
            indices[3] = 0;
            indices[4] = 3;
            indices[5] = 2;

            Mesh mesh = new Mesh();
            mesh.name = "ImpostorMesh";
            mesh.vertices = vertices;
            mesh.uv = uvs;
            // 某些老机型使用uv2有问题，比如红米Note3的uv2无效
            //mesh.uv2 = uvs2;
            mesh.SetIndices(indices, MeshTopology.Triangles, 0, false);

            return mesh;
        }
    }
}