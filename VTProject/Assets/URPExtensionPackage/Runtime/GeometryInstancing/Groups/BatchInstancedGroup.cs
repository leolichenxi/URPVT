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

        /// <summary>
        /// 物件的uid 对应的绘制位置
        /// </summary>
        private Dictionary<int, DrawHandler> m_uidToBatchIndex = new Dictionary<int, DrawHandler>();

        /// <summary>
        /// 所有的绘制列表
        /// </summary>
        private List<BatchGroup> m_drawGroups = new List<BatchGroup>();

        public int MeshKey { get; private set; }
        public int[] MaterialKey { get; private set; }

        public int PreferSize = 512;
        public byte ObjType { get; private set; }

        public IReadOnlyList<BatchGroup> DrawGroupDatas => m_drawGroups;

        public static ShadowCastingMode CastShadowMode = ShadowCastingMode.On;
        public static bool ReceiveShadows = true;

        private Mesh m_Mesh;
        private Material[] m_Materials;
        private int m_MatNum;
        private PassID[] m_passIds;
        private bool m_impostor;

        public bool ShadowCaster
        {
            get { return m_setting.HasShadow; }
        }

        private DrawPrefabSetting m_setting;

        // 数据初始化
        public void Init(int meshKey, int[] materialKey, byte objType, DrawPrefabSetting setting)
        {
            MeshKey = meshKey;
            MaterialKey = materialKey;
            ObjType = objType;
            m_setting = setting;
        }
        
        // Render信息初始化
        public void InitRenderInfo(Mesh mesh, Material[] materials)
        {
            m_Mesh = mesh;
            m_Materials = materials;
            int num = materials.Length;
            m_passIds = new PassID[num];
            for (int i = 0; i < m_passIds.Length; i++)
            {
                m_passIds[i].ReadPassFromMaterial(materials[i], m_impostor);
            }
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
            m_Mesh = null;
            m_Materials = null;
            m_passIds = null;
        }

        public bool AddInstanceRenderInfo(int uid, Matrix4x4 matrix)
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
                    var groupData = new BatchGroup(PreferSize);
                    batchIndex = m_drawGroups.Count;
                    m_drawGroups.Add(groupData);
                    dataIndex = groupData.AddMatrix(uid, matrix);
                }

                m_uidToBatchIndex.Add(uid, new DrawHandler() { BatchIndex = batchIndex, DataIndex = dataIndex });
            }

            return true;
        }

        public bool RemoveInstance(int uid)
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
                for (int i = 0; i < m_MatNum; i++)
                {
                    var passId = m_passIds[i];
                    if (m_setting.HasPreZ)
                    {
                        if (passId.HasAfterPreZPass)
                        {
                            cmd.DrawMeshInstanced(m_Mesh, i, m_Materials[i], m_passIds[i].AfterPreZPass, batchGroup.MatrixBuffer, batchGroup.ValidLength,
                                batchGroup.PropertyBlocks);
                            continue;
                        }
                    }

                    if (passId.HasPass)
                    {
                        cmd.DrawMeshInstanced(m_Mesh, i, m_Materials[i], m_passIds[i].Pass, batchGroup.MatrixBuffer, batchGroup.ValidLength, batchGroup.PropertyBlocks);
                    }
                }
            }
        }

        private void DrawBatchShadow(CommandBuffer cmd, BatchGroup batchGroup)
        {
            if (batchGroup.HasElement)
            {
                for (int i = 0; i < m_MatNum; i++)
                {
                    var passId = m_passIds[i];
                    if (passId.HasShadowCasterPass)
                    {
                        cmd.DrawMeshInstanced(m_Mesh, i, m_Materials[i], m_passIds[i].ShadowCasterPass, batchGroup.MatrixBuffer, batchGroup.ValidLength);
                    }
                }
            }
        }

        private void DrawBatchPreZ(CommandBuffer cmd, BatchGroup batchGroup)
        {
            if (batchGroup.HasElement)
            {
                for (int i = 0; i < m_MatNum; i++)
                {
                    var passId = m_passIds[i];
                    if (passId.HasPreZPass)
                    {
                        cmd.DrawMeshInstanced(m_Mesh, i, m_Materials[i], m_passIds[i].PreZPass, batchGroup.MatrixBuffer, batchGroup.ValidLength);
                    }
                }
            }
        }
    }
}