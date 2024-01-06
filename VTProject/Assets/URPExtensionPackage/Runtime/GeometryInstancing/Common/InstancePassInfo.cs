namespace UnityEngine.Rendering.Universal
{
    public enum EInstancePassType
    {
        ShadowCaster = 0,
        RenderingOpaque = 1,
        RenderingTransparent = 2,
        PreZ = 3,
    }
    
    public struct DrawPrefabSetting
    {
        public bool HasShadow;
        public bool HasPreZ;
        public bool UseImpostor;
        public bool IsTransparent;
    }

    public class InstancePassInfo
    {
        public Camera camera;
        public EInstancePassType passType;
        public int cascadeIndex = 0;
        public Matrix4x4 matView = Matrix4x4.identity;
        public Matrix4x4 matProj = Matrix4x4.identity;
        public int index;
        public Vector3 CameraPos = Vector3.zero;
        public Plane[] CameraPlanes = new Plane[6];
        public uint BoundsCheckCode = 0;

        public void UpdateCameraPlanes(Camera cam)
        {
            matView = cam.worldToCameraMatrix;
            matProj = cam.projectionMatrix;
            CameraPos = cam.transform.position;
            GeometryUtility.CalculateFrustumPlanes(cam, CameraPlanes);
        }

        public void UpdateCameraPlanes(Matrix4x4 view, Matrix4x4 proj)
        {
            matView = view;
            matProj = proj;

            if (camera != null)
            {
                CameraPos = camera.transform.position;
            }
            else
            {
                // 投影时这里会偏差比较大，默认还是使用的camera的position
                CameraPos.x = view.m03;
                CameraPos.y = view.m13;
                CameraPos.z = view.m23;
            }

            GeometryUtility.CalculateFrustumPlanes(proj * view, CameraPlanes);
        }
    }
}