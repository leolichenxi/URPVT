namespace UnityEngine.Rendering.Universal
{
    public struct SnapshotTask
    {
        public ImpostorSnapshotAtlas.Snapshot Atlas;
        public Mesh Mesh;
        public Material[] Materials;
        public int[] ShaderPass;
        public int[] DepthPass;

        public SnapshotTask(ImpostorSnapshotAtlas.Snapshot obj, Mesh mesh, Material[] materials)
        {
            Atlas = obj;
            Materials = materials;
            Mesh = mesh;
            ShaderPass = new int[materials.Length];
            DepthPass = new int[materials.Length];
            for (int i = 0; i < materials.Length; ++i)
            {
                ShaderPass[i] = materials[i].FindPass(InstanceConst.PASS_NAME_IMPOSTOR_SNAPSHOT);
                DepthPass[i] = materials[i].FindPass(InstanceConst.PASS_NAME_IMPOSTOR_SNAPSHOT_SHADOW);
                ShaderPass[i] = ShaderPass[i] < 0 ? 0 : ShaderPass[i];
                DepthPass[i] = DepthPass[i] < 0 ? 0 : DepthPass[i];
            }
        }
    }
}