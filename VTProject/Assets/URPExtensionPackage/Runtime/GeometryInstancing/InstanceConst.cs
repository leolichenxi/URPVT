namespace UnityEngine.Rendering.Universal
{
    public static class InstanceConst
    {
        public const string GEOMETRY_INSTANCING_PASS_TAG = "GeometryInstancingPass";
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

        #region PassName 定义

        public static readonly string PassAfterZ = "AfterZ";
        public static readonly string PassForwardLit = "ForwardLit";
        public static readonly string PassMain = "Main";
        public static readonly string PassShadowCaster = "ShadowCaster";
        public static readonly string PassImpostor = "Impostor";
        public static readonly string PassPreZ = "PreZ";

        #endregion
        #region PropertyName 定义

        public const string PROPERTY_IMPOSTOR_TEX = "_ImpostorTex";
        public const string PROPERTY_IMPOSTOR_DEPTH_TEX = "_ImpostorDepthTex";

        #endregion
    }
}