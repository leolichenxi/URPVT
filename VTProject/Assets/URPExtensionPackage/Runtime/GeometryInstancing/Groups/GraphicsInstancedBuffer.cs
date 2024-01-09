namespace UnityEngine.Rendering.Universal
{
    public class GraphicsInstancedBuffer: IBatchGroupBuffer
    {
        public EInstanceRenderMode InstanceRenderMode { get; } = EInstanceRenderMode.GraphicsInstanced;
        public int Layer { get; set; }

        
        public void Init(int meshKey, int[] materialKey, byte objType, DrawPrefabSetting setting)
        {
            throw new System.NotImplementedException();
        }

        public void SetRenderInfo(Mesh mesh, Material[] materials)
        {
            throw new System.NotImplementedException();
        }

        public void DrawBatch(CommandBuffer cmd,InstancePassInfo passInfo)
        {
            throw new System.NotImplementedException();
        }

        public void AddInstanceObject(int uid, Matrix4x4 matrix)
        {
            throw new System.NotImplementedException();
        }

        public bool RemoveInstanceObject(int uid)
        {
            throw new System.NotImplementedException();
        }

        public void Clear()
        {
            throw new System.NotImplementedException();
        }
    }
}