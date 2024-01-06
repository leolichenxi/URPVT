namespace UnityEngine.Rendering.Universal
{
    public enum EInstanceRenderMode
    {
        CommandInstancing = 0,
        Impostor = 1,
    }

    public interface IBatchGroupBuffer 
    {
        EInstanceRenderMode InstanceRenderMode { get; }
        void Init(int meshKey, int[] materialKey, byte objType, DrawPrefabSetting setting);
        void SetRenderInfo(Mesh mesh, Material[] materials);
        void DrawBatch(CommandBuffer cmd);
        public bool AddInstanceObject(int uid, Matrix4x4 matrix);
        public bool RemoveInstanceObject(int uid);
        void Clear();
    }

    public interface IBatchGroupDrawer
    {
    }
}