﻿using System.Collections.Generic;

namespace UnityEngine.Rendering.Universal
{
    /// <summary>
    /// 同一个 Mesh Key, Material Key， SubMesh, Matrix,
    /// </summary>
    public class BatchInstancedGroup
    {
        internal struct DrawHandler
        {
            public int BatchIndex;
            public int DataIndex;
        }

        public EInstanceRenderMode InstanceRenderMode { get; } = EInstanceRenderMode.CommandInstancing;
        public int Layer { get; set; }

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

        public BatchObjectData BatchObjectData;
        public DrawPrefabSetting Setting;
        public BatchGroupData BatchGroupData;

        public int MatNum => BatchGroupData.MatNum;
        public bool ShadowCaster => Setting.HasShadow;

        public void Init(int meshKey, int[] materialKey, byte objType, DrawPrefabSetting setting)
        {
            BatchObjectData.MeshKey = meshKey;
            BatchObjectData.MaterialKey = materialKey;
            BatchObjectData. ObjType = objType;
            Setting = setting;
        }
        
        // Render信息初始化
        public void SetSetInstanceRenderInfo(Mesh mesh, Material[] materials)
        {
            BatchGroupData.SetInstanceRenderInfo(mesh,materials);
        }
        public void SetImpostorInstanceRenderInfo(Mesh mesh, Material[] materials,ImpostorSnapshotAtlas.Snapshot snapshot)
        {
            BatchGroupData.SetImpostorInstanceRenderInfo(mesh,materials, snapshot);
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
            BatchGroupData.ClearRef();
        }

        public void AddInstanceObject(int uid, Matrix4x4 matrix)
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
                    var groupData = new BatchGroup(ImpostorConst.MAX_BATCH_DRAW_COUNT);
                    batchIndex = m_drawGroups.Count;
                    m_drawGroups.Add(groupData);
                    dataIndex = groupData.AddMatrix(uid, matrix);
                }

                m_uidToBatchIndex.Add(uid, new DrawHandler() { BatchIndex = batchIndex, DataIndex = dataIndex });
            }
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


    }
}