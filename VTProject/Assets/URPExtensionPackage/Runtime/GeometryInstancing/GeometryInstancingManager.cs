using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UnityEngine.Rendering.Universal
{
    public class GeometryInstancingManager
    {
        private static GeometryInstancingManager m_instance;
        public static GeometryInstancingManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new GeometryInstancingManager();
                }
                return m_instance;
            }
        }

        public List<IBatchGroupBuffer> m_batchBuffer = new List<IBatchGroupBuffer>();
        public IReadOnlyList<IBatchGroupBuffer> BatchGroupBuffers => m_batchBuffer;
        public void Render(EInstancePassType passType)
        {
           
        }
        
    }
}