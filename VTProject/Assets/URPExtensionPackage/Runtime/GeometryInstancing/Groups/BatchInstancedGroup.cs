using System.Collections.Generic;

namespace UnityEngine.Rendering.Universal
{
    /// <summary>
    /// 同一个 Mesh Key, Material Key， SubMesh, Matrix,
    /// </summary>
    public class BatchInstancedGroupGroup : IBatchGroupBuffer
    {
        internal struct DrawHandler
        {
            public int BatchIndex;
            public int DataIndex;
        }

        public EInstanceRenderMode InstanceRenderMode { get; } = EInstanceRenderMode.CommandInstancing;
        /// <summary>
        /// 物件的uid 对应的绘制位置
        /// </summary>
        private Dictionary<int, DrawHandler> m_uidToBatchIndex = new Dictionary<int, DrawHandler>();

        private InstancingMaterialProperty m_instancingMaterialProperty = new InstancingMaterialProperty();
        
        /// <summary>
        /// 所有的绘制列表
        /// </summary>
        private List<BatchGroup> m_drawGroups = new List<BatchGroup>();

        public IReadOnlyList<BatchGroup> BatchGroups => m_drawGroups;

        public static ShadowCastingMode CastShadowMode = ShadowCastingMode.On;
        public static bool ReceiveShadows = true;

        private BatchObjectData m_batchObjectData;
        private DrawPrefabSetting m_setting;
        private BatchGroupData m_batchGroupData;
        
        public bool ShadowCaster => m_setting.HasShadow;


        // 数据初始化
        public void Init(int meshKey, int[] materialKey, byte objType, DrawPrefabSetting setting)
        {
            m_batchObjectData.MeshKey = meshKey;
            m_batchObjectData.MaterialKey = materialKey;
            m_batchObjectData. ObjType = objType;
            m_setting = setting;
        }
        
        // Render信息初始化
        public void SetRenderInfo(Mesh mesh, Material[] materials)
        {
            m_batchGroupData.SetInstanceRenderInfo(mesh,materials);
        }

        public bool IsEmpty()
        {
#if UNITY_EDITOR
            if (m_uidToBatchIndex.Count == 0)
            {
                int c = 0;
                for (int i = 0; i < m_drawGroups.Count; i++)
                {
                    c += m_drawGroups[i].ValidLength;
                }

                if (c != 0)
                {
                    Debug.LogError("data error");
                }
            }
#endif
            return m_uidToBatchIndex.Count == 0;
        }

        public void Clear()
        {
            for (int i = 0; i < m_drawGroups.Count; i++)
            {
                m_drawGroups[i].Release();
            }

            m_uidToBatchIndex.Clear();
            m_drawGroups.Clear();
            m_batchGroupData.ClearRef();
        }

        public bool AddInstanceObject(int uid, Matrix4x4 matrix)
        {
            if (m_uidToBatchIndex.TryGetValue(uid, out var drawHandler))
            {
                m_drawGroups[drawHandler.BatchIndex].SetMatrixAt(drawHandler.DataIndex, matrix);
            }
            else
            {
                int batchIndex = -1;
                int dataIndex = -1;
                for (int i = 0; i < m_drawGroups.Count; i++)
                {
                    if (m_drawGroups[i].TryAddMatrix(uid, matrix, ref dataIndex))
                    {
                        batchIndex = i;
                        break;
                    }
                }

                if (dataIndex < 0)
                {
                    var groupData = new BatchGroup(InstanceConst.MAX_BATCH_DRAW_COUNT);
                    batchIndex = m_drawGroups.Count;
                    m_drawGroups.Add(groupData);
                    dataIndex = groupData.AddMatrix(uid, matrix);
                }

                m_uidToBatchIndex.Add(uid, new DrawHandler() { BatchIndex = batchIndex, DataIndex = dataIndex });
            }

            return true;
        }

        public bool RemoveInstanceObject(int uid)
        {
            if (m_uidToBatchIndex.TryGetValue(uid, out var drawHandler))
            {
                var group = m_drawGroups[drawHandler.BatchIndex];
                if (group.RemoveAt(drawHandler.DataIndex, out var changedObjUid, out var changedToIndex))
                {
                    var data = m_uidToBatchIndex[changedObjUid];
                    data.DataIndex = changedToIndex;
                    m_uidToBatchIndex[changedToIndex] = data;
                }
                return m_uidToBatchIndex.Remove(uid);
            }
            return false;
        }


        public void DrawBatch(CommandBuffer cmd)
        {
            for (int i = 0; i < m_drawGroups.Count; i++)
            {
                DrawBatchGroup(cmd, m_drawGroups[i]);
            }
        }

        public void DrawShadow(CommandBuffer cmd)
        {
            if (!ShadowCaster)
                return;
            for (int i = 0; i < m_drawGroups.Count; i++)
            {
                DrawBatchShadow(cmd, m_drawGroups[i]);
            }
        }

        public void DrawPreZ(CommandBuffer cmd)
        {
            if (!m_setting.HasPreZ)
                return;
            for (int i = 0; i < m_drawGroups.Count; i++)
            {
                DrawBatchPreZ(cmd, m_drawGroups[i]);
            }
        }

        private void DrawBatchGroup(CommandBuffer cmd, BatchGroup batchGroup)
        {
            if (batchGroup.HasElement)
            {
                for (int i = 0; i < m_batchGroupData.MatNum; i++)
                {
                    var passId = m_batchGroupData.PassIds[i];
                    if (m_setting.HasPreZ)
                    {
                        if (passId.HasAfterPreZPass)
                        {
                            cmd.DrawMeshInstanced( m_batchGroupData.Mesh, i,  m_batchGroupData.Materials[i],  m_batchGroupData.PassIds[i].AfterPreZPass, batchGroup.MatrixBuffer, batchGroup.ValidLength,
                                batchGroup.PropertyBlocks);
                            continue;
                        }
                    }

                    if (passId.HasPass)
                    {
                        cmd.DrawMeshInstanced( m_batchGroupData.Mesh, i, m_batchGroupData. Materials[i],  m_batchGroupData.PassIds[i].Pass, batchGroup.MatrixBuffer, batchGroup.ValidLength, batchGroup.PropertyBlocks);
                    }
                }
            }
        }

        private void DrawBatchShadow(CommandBuffer cmd, BatchGroup batchGroup)
        {
            if (batchGroup.HasElement)
            {
                for (int i = 0; i <  m_batchGroupData.MatNum; i++)
                {
                    var passId =  m_batchGroupData.PassIds[i];
                    if (passId.HasShadowCasterPass)
                    {
                        cmd.DrawMeshInstanced( m_batchGroupData.Mesh, i,  m_batchGroupData.Materials[i],  m_batchGroupData.PassIds[i].ShadowCasterPass, batchGroup.MatrixBuffer, batchGroup.ValidLength);
                    }
                }
            }
        }

        private void DrawBatchPreZ(CommandBuffer cmd, BatchGroup batchGroup)
        {
            if (batchGroup.HasElement)
            {
                for (int i = 0; i <  m_batchGroupData.MatNum; i++)
                {
                    var passId =  m_batchGroupData.PassIds[i];
                    if (passId.HasPreZPass)
                    {
                        cmd.DrawMeshInstanced( m_batchGroupData.Mesh, i,  m_batchGroupData.Materials[i],  m_batchGroupData.PassIds[i].PreZPass, batchGroup.MatrixBuffer, batchGroup.ValidLength);
                    }
                }
            }
        }
    }
}