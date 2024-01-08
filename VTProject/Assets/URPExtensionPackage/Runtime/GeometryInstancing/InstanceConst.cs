namespace UnityEngine.Rendering.Universal
{
    public static class InstanceConst
    {
        public const string GEOMETRY_INSTANCING_PASS_TAG = "GeometryInstancingPass";
        public const string IMPOSTOR_SNAPSHOT_PASS = "ImpostorSnapshotPass";
        public const int MAX_PROPERTY_COUNT = 32;
        /// <summary>
        /// 每组最大提交个数
        /// </summary>
        public const int MAX_GEOMETRY_INSTANCE_DRAW_COUNT = 200;
        public const int MAX_BATCH_DRAW_COUNT = 256;
        /// <summary>
        /// Batch 最多
        /// </summary>
        public const int MAX_GEOMETRY_INSTANCE_ARRAY_COUNT = 1024;


        // public static RenderTextureFormat Atlas_Color_Format = SystemInfo.SupportsTextureFormat(TextureFormat.ARGB32) ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.ARGB32;
        public static RenderTextureFormat AtlasColorFormat = RenderTextureFormat.ARGB32;
        public static RenderTextureFormat AtlasShadowFormat = RenderTextureFormat.RHalf;
        
        #region PassName 定义

        public static readonly string PassAfterZ = "AfterZ";
        public static readonly string PassForwardLit = "ForwardLit";
        public static readonly string PassMain = "Main";
        public static readonly string PassShadowCaster = "ShadowCaster";
        public static readonly string PassImpostor = "Impostor";
        public static readonly string PassPreZ = "PreZ";

        public static int AtlasRTSize = 1024; // 512 *2
        public static int SnapRTSize = 256 ; // 128 *2 
        
        
        public const float IMPOSTOR_PROJECT_NEAR = -15.0f;
        public const float IMPOSTOR_PROJECT_FAR = 15.0f;
        
        #endregion
        #region PropertyName 定义

        public const string PROPERTY_IMPOSTOR_TEX = "_ImpostorTex";
        public const string PROPERTY_IMPOSTOR_DEPTH_TEX = "_ImpostorDepthTex";

        #endregion
        
        
        public static int ImpostorZBufferParam = Shader.PropertyToID("_ImpostorZBufferParam");
        public static int VBufferDepthEncodingParams = Shader.PropertyToID("_VBufferDepthEncodingParams");
    }
}