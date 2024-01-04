namespace UnityEngine.Rendering.Universal
{
    public static class InstanceConst
    {
        public const string GeometryInstancingPassTag = "GeometryInstancingPass";
        public const int MaxPropertyCount = 32;
        /// <summary>
        /// 每组最大提交个数
        /// </summary>
        public const int MaxGeometryInstanceDrawCount = 200;
        /// <summary>
        /// Batch 最多
        /// </summary>
        public const int MaxGeometryInstanceArrayCount = 1024;

        #region PassName 定义

        public const string PassAfterZ = "AfterZ";
        public const string PassForwardLit = "ForwardLit";
        public const string PassMain = "Main";
        public const string PassShadowCaster = "ShadowCaster";
        public const string PassImpostor = "Impostor";
        public const string PassPreZ = "PreZ";

        #endregion

    }
}