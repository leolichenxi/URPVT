using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace UnityEngine.Rendering.Universal
{
    public  partial class ImpostorSnapshotAtlas
    {
        public enum EBlitMode
        {
            BLIT_COLOR,
            BLIT_DEPTH
        }
        
        public   class SnapshotAtlas
        {
            public RenderTexture ColorRT { get; private set; }
            public RenderTexture DepthRT { get; private set; }

            private int m_index;
            private int m_blockX;
            private int m_blockY;
            private int m_blockNum;
            private bool[] m_objRef;
            private int m_objCount;
            public SnapshotAtlas(int blockX, int blockY, int indexInList)
            {
                m_blockX = blockX;
                m_blockY = blockY;
                m_index = indexInList;
                m_blockNum = m_blockX * m_blockY;

                ColorRT = DrawBatchUtility.CreateSnapColorRT("SnapshotAtlas" + indexInList);
                DepthRT = DrawBatchUtility.CreateSnapShadowRT("SnapshotDepthAtlas" + indexInList);
                
                m_objRef = new bool[m_blockNum];
                for (int i = 0; i < m_blockNum; i++)
                {
                    m_objRef[i] = false;
                }
            }


            public bool TryBindSnapShot(Snapshot snapshot)
            {
                 
                int index = -1;
                for (int i = 0; i < m_blockNum; i++)
                {
                    if (!m_objRef[i])
                    {
                        m_objRef[i] = true;
                        index = i;
                        break;
                    }
                }

                if (index >= 0)
                {
                    snapshot.index = index;
                    snapshot.u = ((float)(index / m_blockX)) / this.m_blockX;
                    snapshot.v = ((float)(index % m_blockX)) / this.m_blockY;
                    snapshot.size = 1.0f / this.m_blockX;
                    m_objCount++;
                    return false;
                }
                return true;
            }
            
            public void BlitColor(CommandBuffer cmd,Snapshot snapshot,RenderTexture src)
            {
                cmd.SetRenderTarget(ColorRT);
                Rect viewPort = new Rect();
                viewPort.Set(snapshot.u * InstanceConst.AtlasRTSize, snapshot.v * InstanceConst.AtlasRTSize, InstanceConst.SnapRTSize, InstanceConst.SnapRTSize);
                cmd.SetViewport(viewPort);
                DrawBatchUtility.DoCopyColor(cmd, src);
            }

            public void BlitDepth(CommandBuffer cmd, Snapshot snapshot,RenderTexture src)
            {
                cmd.SetRenderTarget(DepthRT);
                Rect viewPort = new Rect();
                viewPort.Set(snapshot.u * InstanceConst.AtlasRTSize, snapshot.v * InstanceConst.AtlasRTSize, InstanceConst.SnapRTSize, InstanceConst.SnapRTSize);
                cmd.SetViewport(viewPort);
                //GraphicsEx.DrawTextureCB(cmd, source);
                DrawBatchUtility.DoCopyColor(cmd, src);
            }
            
            public void Clear()
            {
                {
                    UnityEngine.Object.Destroy(ColorRT);
                    ColorRT = null;
                }
                if (DepthRT)
                {
                    UnityEngine.Object.Destroy(DepthRT);
                    DepthRT = null;
                }
            }
        }
        
        public class Snapshot
        {
            public int index;
            public float u;
            public float v;
            public float size;
            public SnapshotAtlas SnapshotAtlas { get; private set; }

            public Snapshot(SnapshotAtlas atlas)
            {
                SnapshotAtlas = atlas;
                this.index = 0;
                atlas.TryBindSnapShot(this);
            }

            public void Blit(CommandBuffer cmd, RenderTexture rt, EBlitMode blitMode)
            {
                if (SnapshotAtlas!=null)
                {
                    switch (blitMode)
                    {
                        case EBlitMode.BLIT_COLOR:
                            BlitColor(cmd, rt);
                            break;
                        case EBlitMode.BLIT_DEPTH:
                            BlitDepth(cmd,rt);
                            break;
                        default:
                            break;
                    }
                }
            }
            
            private void BlitColor(CommandBuffer cmd, RenderTexture source)
            {
                SnapshotAtlas.BlitColor(cmd,this,source);
            }
            
            private void BlitDepth(CommandBuffer cmd, RenderTexture source)
            {
                SnapshotAtlas.BlitDepth(cmd,this,source);
            }

            public void Clear()
            {
                SnapshotAtlas = null;
            }
        }


    
    }
    
    public partial class ImpostorSnapshotAtlas
    {
        private List<SnapshotAtlas> m_listAtlas = new List<SnapshotAtlas>();
    }
    
}