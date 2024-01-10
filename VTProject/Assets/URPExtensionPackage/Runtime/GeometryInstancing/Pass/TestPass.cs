// namespace UnityEngine.Rendering.Universal
// {
//     public class TestPass : ScriptableRenderPass
//     {
//         public static RenderTexture S_RenderTexture;
//
//         public static Mesh RenderMesh;
//         public static Material RenderMaterial;
//
//         public void Init()
//         {
//             S_RenderTexture = Resources.Load<RenderTexture>("RenderTexture");
//             Debug.Log(S_RenderTexture.name);
//             // renderPassEvent = RenderPassEvent.AfterRenderingShadows;
//         }
//
//         public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
//         {
//             Camera cam = renderingData.cameraData.camera;
//             if (cam == null)
//                 return;
//
//             if (!UniversalRenderPipeline.IsGameCamera(cam))
//             {
//                 // 只有GameCamera才刷新
//                 return;
//             }
//
//             if (RenderMesh == null || RenderMaterial == null)
//             {
//                 return;
//             }
//             CommandBuffer cmd = CommandBufferPool.Get(InstanceConst.IMPOSTOR_SNAPSHOT_PASS);
//             cmd.Clear();
//             cmd.SetViewProjectionMatrices(cam.worldToCameraMatrix, cam.projectionMatrix);
//             // cmd.SetRenderTarget(S_RenderTexture);
//             cmd.SetRenderTarget(S_RenderTexture, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
//             cmd.ClearRenderTarget(true, true, new Color(0, 0, 0, 0));
//             cmd.DrawMesh(RenderMesh, Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one), RenderMaterial, 0);
//             cmd.SetRenderTarget(S_RenderTexture, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
//             cmd.ClearRenderTarget(true, true, new Color(0, 0, 0, 0));
//             context.ExecuteCommandBuffer(cmd);
//             CommandBufferPool.Release(cmd);
//         }
//     }
// }