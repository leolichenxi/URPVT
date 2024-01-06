using Unity.Mathematics;

namespace UnityEngine.Rendering.Universal
{
    public static class DrawBatchUtility
    {
        public static Mesh CreateImpostorMesh(Bounds bounds,ImpostorSnapshotAtlas.Snapshot snapshotRT)
        {
            Vector3 extents = bounds.extents;

            // 顶点
            Vector3[] vertices = new Vector3[4];
            // 计算长宽比
            float x = 1.0f;
            float y = 1.0f;
            float scale = 1.0f;
            if (extents.x < extents.y)
            {
                // 宽高比，为了减少面板面积
                x = extents.x / extents.y;
            }

            if (extents.x > extents.y)
            {
                // 宽高比，为了减少面板面积
                //y = extents.y / extents.x;
                // 由于正交投影和从斜上方投影，造成矮宽的物体Impostor大一点，这里特殊修正
                y = extents.y * 1.414f / extents.x;
                if (y > 1) y = 1;
                scale = y;
            }

            scale *= 0.5f; // 因为属性"_Size"默认是1，所以这里乘0.5

            vertices[0] = new Vector3(-x * scale, -y * scale, 0);
            vertices[1] = new Vector3(x * scale, -y * scale, 0);
            vertices[2] = new Vector3(x * scale, +y * scale, 0);
            vertices[3] = new Vector3(-x * scale, +y * scale, 0);

            // UV
            Vector2[] uvs = new Vector2[4];
            uvs[0] = new Vector2(0, 0);
            uvs[1] = new Vector2(1, 0);
            uvs[2] = new Vector2(1, 1);
            uvs[3] = new Vector2(0, 1);

            if (extents.x < extents.y)
            {
                uvs[0] = new Vector2(0.5f - x * 0.5f, 0);
                uvs[1] = new Vector2(0.5f + x * 0.5f, 0);
                uvs[2] = new Vector2(0.5f + x * 0.5f, 1);
                uvs[3] = new Vector2(0.5f - x * 0.5f, 1);
            }

            if (extents.x > extents.y)
            {
                uvs[0] = new Vector2(0, 0.5f - y * 0.5f);
                uvs[1] = new Vector2(1, 0.5f - y * 0.5f);
                uvs[2] = new Vector2(1, 0.5f + y * 0.5f);
                uvs[3] = new Vector2(0, 0.5f + y * 0.5f);
            }

            // uv在图集中的位置
            for (int i = 0; i < 4; i++)
            {
                uvs[i].x = snapshotRT.u + uvs[i].x * snapshotRT.size;
                uvs[i].y = snapshotRT.v + uvs[i].y * snapshotRT.size;
            }


            // 顶点索引
            int[] indices = new int[6];
            indices[0] = 0;
            indices[1] = 2;
            indices[2] = 1;
            indices[3] = 0;
            indices[4] = 3;
            indices[5] = 2;

            Mesh mesh = new Mesh();
            mesh.name = "ImpostorMesh";
            mesh.vertices = vertices;
            mesh.uv = uvs;
            // 某些老机型使用uv2有问题，比如红米Note3的uv2无效
            //mesh.uv2 = uvs2;
            mesh.SetIndices(indices, MeshTopology.Triangles, 0, false);

            return mesh;
        }
    }
}