namespace UnityEngine.Rendering.Universal
{
    public class ImpostorInstancingBuffer : IBatchGroupBuffer
    {
        public EInstanceRenderMode InstanceRenderMode { get; } = EInstanceRenderMode.Impostor;
        private ImpostorSnapshotAtlas.Snapshot m_snapshotRT;
        public Mesh ImpostorMesh;
        private InstancingMaterialProperty m_instancingMaterialProperty = new InstancingMaterialProperty();
        public int Layer { get; set; }
        private Material[] SnapshotMaterails;
        public BatchInstancedGroup BatchInstancedGroup { get; private set; }  = new BatchInstancedGroup();
        private MaterialPropertyBlock m_propertyBlock = new MaterialPropertyBlock();
        internal float[][] m_PropertyFloatsBuffer;
        internal Vector4[][] m_PropertyVectorsBuffer;
        public Material[] ImpostorMaterials => BatchInstancedGroup.BatchGroupData.Materials;

        public ImpostorInstancingBuffer()
        {
        }

        public void Init(int meshKey, int[] materialKey, byte objType, DrawPrefabSetting setting)
        {
            BatchInstancedGroup.Init(meshKey, materialKey, objType, setting);
            m_instancingMaterialProperty.RegisterFloatProperty("_Size");
            m_snapshotRT = GeometryInstancingManager.Instance.CreateSnapshot();
            m_isRenderInfoSet = false;

            int floatProCount = m_instancingMaterialProperty.FloatPropertyName.Count;
            int vecProCount = m_instancingMaterialProperty.VectorPropertyName.Count;
            m_PropertyFloatsBuffer = new float[floatProCount][];
            for (int i = 0; i < floatProCount; i++)
            {
                float[] buffer = new float[InstanceConst.MAX_GEOMETRY_INSTANCE_DRAW_COUNT];
                m_PropertyFloatsBuffer[i] = buffer;
            }

            m_PropertyVectorsBuffer = new Vector4[vecProCount][];
            for (int i = 0; i < vecProCount; i++)
            {
                Vector4[] buffer = new Vector4[InstanceConst.MAX_GEOMETRY_INSTANCE_DRAW_COUNT];
                m_PropertyVectorsBuffer[i] = buffer;
            }
        }

        private bool m_isRenderInfoSet;

        public void SetRenderInfo(Mesh mesh, Material[] materials)
        {
            if (m_isRenderInfoSet)
            {
                return;
            }

            m_isRenderInfoSet = false;
            if (!ImpostorMesh)
            {
                ImpostorMesh = ImpostorUtility.CreateImpostorMesh(mesh.bounds, m_snapshotRT);
            }
            SnapshotMaterails = materials;
            BatchInstancedGroup.SetImpostorInstanceRenderInfo(mesh, materials,m_snapshotRT);
            GeometryInstancingManager.Instance.AddSnapshotTask(this.m_snapshotRT, ImpostorMesh, SnapshotMaterails);
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

        public void DrawBatch(CommandBuffer cmd,InstancePassInfo passInfo)
        {
            for (int i = 0; i < BatchInstancedGroup.BatchGroups.Count; i++)
            {
                DrawBatchGroup(cmd, BatchInstancedGroup.BatchGroups[i]);
            }
        }


        public void DrawShadow(CommandBuffer cmd)
        {
            if (!BatchInstancedGroup.ShadowCaster)
                return;
            for (int i = 0; i < BatchInstancedGroup.BatchGroups.Count; i++)
            {
                DrawBatchShadow(cmd, BatchInstancedGroup.BatchGroups[i]);
            }
        }

        public void DrawPreZ(CommandBuffer cmd)
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

                int floatPropertyCount = m_instancingMaterialProperty.FloatPropertyName.Count;
                int vectorPropertyCount = m_instancingMaterialProperty.VectorPropertyName.Count;
                for (int i = 0; i < floatPropertyCount; i++)
                {
                    m_propertyBlock.SetFloatArray(m_instancingMaterialProperty.FloatPropertyName[i], m_PropertyFloatsBuffer[i]);
                }

                for (int i = 0; i < vectorPropertyCount; i++)
                {
                    m_propertyBlock.SetVectorArray(m_instancingMaterialProperty.VectorPropertyName[i], m_PropertyVectorsBuffer[i]);
                }

                for (int i = 0; i < BatchInstancedGroup.MatNum; ++i)
                {
                    var passId = batchGroupData.PassIds[i];
                    if (passId.HasPass)
                    {
                        cmd.DrawMeshInstanced(ImpostorMesh, i, ImpostorMaterials[i], passId.Pass, batchGroup.MatrixBuffer, batchGroup.ValidLength, m_propertyBlock);
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
                        cmd.DrawMeshInstanced(batchGroupData.Mesh, i, batchGroupData.Materials[i], batchGroupData.PassIds[i].PreZPass, batchGroup.MatrixBuffer, batchGroup.ValidLength);
                    }
                }
            }
        }
    }
}