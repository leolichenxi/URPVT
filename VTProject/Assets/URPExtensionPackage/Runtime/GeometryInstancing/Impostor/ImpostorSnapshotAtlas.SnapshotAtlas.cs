namespace UnityEngine.Rendering.Universal
{
    public  partial class ImpostorSnapshotAtlas
    {
        public class SnapshotAtlas
        {
            public RenderTexture colorRT;
            public RenderTexture depthRT;
        }
        
        public class Snapshot
        {
            public int index;
            public float u;
            public float v;
            public float size;
        }
    }
}