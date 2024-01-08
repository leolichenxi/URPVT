using System.Collections.Generic;
using Unity.VisualScripting;
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
        private List<InstancePassInfo> m_passInfos = new List<InstancePassInfo>();
        
        public uint BoundsCheckCode = 0;

        private int m_refreshSnapshot = 1;
        private void UpdateBoundsCheckCode()
        {
            BoundsCheckCode++;
            if (BoundsCheckCode == uint.MaxValue - 100)
            {
                BoundsCheckCode = 0;
            }
        }
        
        
        public void RefreshImpostor(int refresh = 1)
        {
            m_refreshSnapshot = refresh;
        }

        internal InstancePassInfo SafeGetPassInfo(Camera cam, EInstancePassType passType, int cascadeIndex = 0)
        {
            int num = m_passInfos.Count;
            for (int i = 0; i < num; i++)
            {
                var passInfo = m_passInfos[i];
                if (passInfo.camera == cam && passInfo.passType == passType && passInfo.cascadeIndex == cascadeIndex)
                {
                    passInfo.BoundsCheckCode = BoundsCheckCode + (uint)i;// 为了让每个Pass的CheckCode唯一
                    return passInfo;
                }
            }
            InstancePassInfo p = new InstancePassInfo();
            p.camera = cam;
            p.passType = passType;
            p.cascadeIndex = cascadeIndex;
            p.index = num;
            p.BoundsCheckCode = BoundsCheckCode + (uint)num;// 为了让每个Pass的CheckCode唯一
            m_passInfos.Add(p);
            return p;
        }
    }
}