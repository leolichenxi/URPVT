using System;

namespace UnityEngine.Rendering.Universal
{
    public class CommandInstancingBuffer : IBatchGroupBuffer
    {
        public EInstanceRenderMode InstanceRenderMode { get; }
        public int Layer { get; set; }

        public BatchInstancedGroup BatchInstancedGroup { get; private set; } = new BatchInstancedGroup();

        public void Init(int meshKey, int[] materialKey, byte objType, DrawPrefabSetting setting)
        {
            BatchInstancedGroup.Init(meshKey, materialKey, objType, setting);
        }

        public void SetRenderInfo(Mesh mesh, Material[] materials)
        {
            BatchInstancedGroup.SetSetInstanceRenderInfo(mesh, materials);
        }

        public void AddInstanceObject(int uid, Matrix4x4 matrix)
        {
            BatchInstancedGroup.AddInstanceObject(uid, matrix);
        }

        public bool RemoveInstanceObject(int uid)
        {
            return BatchInstancedGroup.RemoveInstanceObject(uid);
        }

        public void Clear()
        {
            BatchInstancedGroup.Clear();
        }

        public void DrawBatch(CommandBuffer cmd, InstancePassInfo passInfo)
        {
            switch (passInfo.passType)
            {
                case EInstancePassType.ShadowCaster:
                {
                    DrawShadow(cmd);
                }
                    break;
                case EInstancePassType.RenderingOpaque:
                {
                    if (!BatchInstancedGroup.Setting.IsTransparent)
                    {
                        for (int i = 0; i < BatchInstancedGroup.BatchGroups.Count; i++)
                        {
                            DrawBatchGroup(cmd, BatchInstancedGroup.BatchGroups[i]);
                        }
                    }
                }
                    break;
                case EInstancePassType.RenderingTransparent:
                    if (BatchInstancedGroup.Setting.IsTransparent)
                    {
                        for (int i = 0; i < BatchInstancedGroup.BatchGroups.Count; i++)
                        {
                            DrawBatchGroup(cmd, BatchInstancedGroup.BatchGroups[i]);
                        }
                    }
                    break;
                case EInstancePassType.PreZ:
                    if (BatchInstancedGroup.Setting.HasPreZ)
                    {
                        DrawPreZ(cmd);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void DrawShadow(CommandBuffer cmd)
        {
            if (!BatchInstancedGroup.ShadowCaster)
                return;
            for (int i = 0; i < BatchInstancedGroup.BatchGroups.Count; i++)
            {
                DrawBatchShadow(cmd, BatchInstancedGroup.BatchGroups[i]);
            }
        }

        private void DrawPreZ(CommandBuffer cmd)
        {
            if (!BatchInstancedGroup.Setting.HasPreZ)
                return;
            for (int i = 0; i < BatchInstancedGroup.BatchGroups.Count; i++)
            {
                DrawBatchPreZ(cmd, BatchInstancedGroup.BatchGroups[i]);
            }
        }

        private void DrawBatchGroup(CommandBuffer cmd, BatchGroup batchGroup)
        {
            if (batchGroup.HasElement)
            {
                var batchGroupData = BatchInstancedGroup.BatchGroupData;
                for (int i = 0; i < BatchInstancedGroup.MatNum; i++)
                {
                    var passId = batchGroupData.PassIds[i];
                    if (BatchInstancedGroup.Setting.HasPreZ)
                    {
                        if (passId.HasAfterPreZPass)
                        {
                            cmd.DrawMeshInstanced(batchGroupData.Mesh, i, batchGroupData.Materials[i], batchGroupData.PassIds[i].AfterPreZPass, batchGroup.MatrixBuffer,
                                batchGroup.ValidLength,
                                batchGroup.PropertyBlocks);
                            continue;
                        }
                    }

                    if (passId.HasPass)
                    {
                        cmd.DrawMeshInstanced(batchGroupData.Mesh, i, batchGroupData.Materials[i], batchGroupData.PassIds[i].Pass, batchGroup.MatrixBuffer, batchGroup.ValidLength,
                            batchGroup.PropertyBlocks);
                    }
                }
            }
        }

        private void DrawBatchShadow(CommandBuffer cmd, BatchGroup batchGroup)
        {
            if (batchGroup.HasElement)
            {
                var batchGroupData = BatchInstancedGroup.BatchGroupData;
                for (int i = 0; i < batchGroupData.MatNum; i++)
                {
                    var passId = batchGroupData.PassIds[i];
                    if (passId.HasShadowCasterPass)
                    {
                        cmd.DrawMeshInstanced(batchGroupData.Mesh, i, batchGroupData.Materials[i], batchGroupData.PassIds[i].ShadowCasterPass, batchGroup.MatrixBuffer, batchGroup.ValidLength);
                    }
                }
            }
        }

        private void DrawBatchPreZ(CommandBuffer cmd, BatchGroup batchGroup)
        {
            if (batchGroup.HasElement)
            {
                var batchGroupData = BatchInstancedGroup.BatchGroupData;
                for (int i = 0; i < batchGroupData.MatNum; i++)
                {
                    var passId = batchGroupData.PassIds[i];
                    if (passId.HasPreZPass)
                    {
                        cmd.DrawMeshInstanced(batchGroupData.Mesh, i, batchGroupData.Materials[i], batchGroupData.PassIds[i].PreZPass, batchGroup.MatrixBuffer,
                            batchGroup.ValidLength);
                    }
                }
            }
        }
    }
}