namespace UnityEngine.Rendering.Universal
{
    public struct BatchObjectData
    {
        public int MeshKey { get;  set; }
        public int[] MaterialKey { get;  set; }
        public byte ObjType { get;  set; }
    }
    
    public struct BatchGroupData
    {
        public Mesh Mesh;
        public Material[] Materials;
        public int MatNum;
        public PassID[] PassIds;
        // private ImpostorSnapshotAtlas.Snapshot m_snapshotRT;
        public void SetInstanceRenderInfo(Mesh mesh, Material[] materials)
        {
            this.Mesh = mesh;
            this.Materials = materials;
            int num = materials.Length;
            this.PassIds = new PassID[num];
            for (int i = 0; i < this.PassIds.Length; i++)
            {
                this.PassIds[i].ReadPassFromMaterial(materials[i], false);
            }
            MatNum = num;
        }
        
        public void SetImpostorInstanceRenderInfo(Mesh mesh, Material[] materials,ImpostorSnapshotAtlas.Snapshot snapshotRT)
        {
            this.Mesh = mesh;
            int num  = materials.Length;
            this.Materials = new Material[num];
            for (int i = 0; i < num; ++i)
            {
                this.Materials[i] = new Material(materials[i]);
                var material = this.Materials[i];
                material.enableInstancing = true;
                material.SetTexture(ShaderHelper.GetPropertyID(ImpostorConst.PROPERTY_IMPOSTOR_TEX), snapshotRT.SnapshotAtlas.ColorRT);
                material.SetTexture(ShaderHelper.GetPropertyID(ImpostorConst.PROPERTY_IMPOSTOR_DEPTH_TEX), snapshotRT.SnapshotAtlas.DepthRT);
            }
            MatNum = num;
            this.PassIds = new PassID[1];
            this.PassIds[0].ReadPassFromMaterial(materials[0], true);
        }

        public void ClearRef()
        {
            Mesh = null;
            Materials = null;
            PassIds = null;
        }
    }
}