using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace UnityEngine.Rendering.Universal
{
    public partial class ImpostorSnapshotAtlas
    {
        public enum EBlitMode
        {
            BLIT_COLOR,
            BLIT_DEPTH
        }

        public class SnapshotAtlas
        {
            public bool IsFull
            {
                get { return m_objCount >= m_blockNum; }
            }

            public RenderTexture ColorRT { get; private set; }
            public RenderTexture DepthRT { get; private set; }
            public bool HasRef => m_snapshots.Count > 0;

            public int ObjCount => m_objCount;
            public int BlockNum => m_blockNum;

            private int m_index;
            private int m_blockX;
            private int m_blockY;
            private int m_blockNum;
            // private bool[] m_objRef;
            private int m_objCount;

            private List<Snapshot> m_snapshots;

            public SnapshotAtlas(int blockX, int blockY, int indexInList)
            {
                m_blockX = blockX;
                m_blockY = blockY;
                m_index = indexInList;
                m_blockNum = m_blockX * m_blockY;
                m_snapshots = new List<Snapshot>(m_blockNum);
                ColorRT = DrawBatchUtility.CreateSnapColorRT("SnapshotAtlas" + indexInList);
                DepthRT = DrawBatchUtility.CreateSnapShadowRT("SnapshotDepthAtlas" + indexInList);
            }

            public void AddSnapShot(Snapshot snapshot)
            {
                int index = -1;
                if (m_snapshots.Count< m_blockNum)
                {
                    index = m_snapshots.Count;
                    m_snapshots.Add(snapshot);
                }
                if (index >= 0)
                {
                    snapshot.index = index;
                    snapshot.u = ((float)(index / m_blockX)) / this.m_blockX;
                    snapshot.v = ((float)(index % m_blockX)) / this.m_blockY;
                    snapshot.size = 1.0f / this.m_blockX;
                    m_objCount = ObjCount + 1;
                }
                else
                {
                    Debug.LogError("BindSnapShot Fail");
                }
            }

            public void RemoveSnapShot(Snapshot snapshot)
            {
                int index = snapshot.index;
                m_snapshots.RemoveSwapAt(snapshot.index);
                if (index>0 )
                {
                    m_snapshots[index].index = index;
                }

                if (m_snapshots.Count == 0)
                {
                     // TODO 切场景 RT可能被清理 这里最好有个监控引用 或且场景暂停销毁
                     Graphics.SetRenderTarget(this.ColorRT);
                     GL.Clear(true, true, new Color(0, 0, 0, 0));
                     Graphics.SetRenderTarget(this.DepthRT);
                     GL.Clear(true, true, new Color(0, 0, 0, 0));
                }
            }

            public void BlitColor(CommandBuffer cmd, Snapshot snapshot, RenderTexture src)
            {
                cmd.SetRenderTarget(ColorRT);
                Rect viewPort = new Rect();
                viewPort.Set(snapshot.u * InstanceConst.AtlasRTSize, snapshot.v * InstanceConst.AtlasRTSize, InstanceConst.SnapRTSize, InstanceConst.SnapRTSize);
                cmd.SetViewport(viewPort);
                DrawBatchUtility.DoCopyColor(cmd, src);
            }

            public void BlitDepth(CommandBuffer cmd, Snapshot snapshot, RenderTexture src)
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
                if (ColorRT)
                {
                    UnityEngine.Object.Destroy(ColorRT);
                    ColorRT = null;
                }
                if (DepthRT)
                {
                    UnityEngine.Object.Destroy(DepthRT);
                    DepthRT = null;
                }
                m_snapshots.Clear();
            }
        }

        public class Snapshot
        {
            public int index;
            public float u;
            public float v;
            public float size;
            public SnapshotAtlas SnapshotAtlas { get; private set; }

            internal Snapshot(SnapshotAtlas atlas)
            {
                SnapshotAtlas = atlas;
                this.index = 0;
                atlas.AddSnapShot(this);
            }

            public void Blit(CommandBuffer cmd, RenderTexture rt, EBlitMode blitMode)
            {
                if (SnapshotAtlas != null)
                {
                    switch (blitMode)
                    {
                        case EBlitMode.BLIT_COLOR:
                            BlitColor(cmd, rt);
                            break;
                        case EBlitMode.BLIT_DEPTH:
                            BlitDepth(cmd, rt);
                            break;
                        default:
                            break;
                    }
                }
            }

            private void BlitColor(CommandBuffer cmd, RenderTexture source)
            {
                SnapshotAtlas.BlitColor(cmd, this, source);
            }

            private void BlitDepth(CommandBuffer cmd, RenderTexture source)
            {
                SnapshotAtlas.BlitDepth(cmd, this, source);
            }

            public void Clear()
            {
                SnapshotAtlas.RemoveSnapShot(this);
                SnapshotAtlas = null;
            }
        }
    }

    public partial class ImpostorSnapshotAtlas
    {
        // 图集管理器
        private List<SnapshotAtlas> m_listAtlas = new List<SnapshotAtlas>();
        public Snapshot Create()
        {
            SnapshotAtlas sa = GetAvailableAtlas();
            Snapshot s = new ImpostorSnapshotAtlas.Snapshot(sa);
            return s;
        }

        private SnapshotAtlas GetAvailableAtlas()
        {
            SnapshotAtlas sa;
            for (int i = m_listAtlas.Count - 1; i >= 0; i--)
            {
                sa = m_listAtlas[i];
                if (!sa.IsFull)
                {
                    return sa;
                }
            }
            sa = new SnapshotAtlas(InstanceConst.BLOCK_X, InstanceConst.BLOCK_Y, m_listAtlas.Count);
            m_listAtlas.Add(sa);
            return sa;
        }
    }
}