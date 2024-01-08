namespace UnityEngine.Rendering.Universal
{
    public class ImpostorInstancingGroup : IBatchGroupBuffer
    {
        public EInstanceRenderMode InstanceRenderMode { get; } = EInstanceRenderMode.Impostor;
        private BatchObjectData m_batchObjectData;
        private DrawPrefabSetting m_setting;
        private BatchGroupData m_batchGroupData;
        private ImpostorSnapshotAtlas.Snapshot m_snapshotRT;
        public Mesh ImpostorMesh;
        
        public void Init(int meshKey, int[] materialKey, byte objType, DrawPrefabSetting setting)
        {
            m_batchObjectData.MeshKey = meshKey;
            m_batchObjectData.MaterialKey = materialKey;
            m_batchObjectData. ObjType = objType;
            m_setting = setting;
         
        }

        public void SetRenderInfo(Mesh mesh, Material[] materials)
        {
            m_batchGroupData.SetImpostorInstanceRenderInfo(mesh,materials);
            ImpostorMesh = DrawBatchUtility.CreateImpostorMesh(mesh.bounds,m_snapshotRT);
        }

        public void DrawBatch(CommandBuffer cmd)
        {
           
        }

        public bool AddInstanceObject(int uid, Matrix4x4 matrix)
        {
            return true;
        }

        public bool RemoveInstanceObject(int uid)
        {
            return true;
        }

        public void Clear()
        {
           
        }
    }
}