namespace UnityEngine.Rendering.Universal
{
    public class InstanceObject
    {
        public int DrawIndex { get; private set; }
        private Matrix4x4 m_drawMatrix;

        public void SetDrawIndex(int index)
        {
            DrawIndex = index;
        }
        
        public void UpdateMatrix(Matrix4x4 matrix)
        {
            m_drawMatrix = matrix;
        }
    }
}