using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Random = System.Random;

public class Test : MonoBehaviour
{
    public EInstanceRenderMode Mode = EInstanceRenderMode.CommandInstancing;
    public MeshRenderer InstancedMeshRender;
    public MeshFilter MeshFilter;
    // Start is called before the first frame update
    private IBatchGroupBuffer m_groupBuffer;
    void Start()
    {
        AddInstanced();
    }

    private void OnDestroy()
    {
        GeometryInstancingManager.Instance.RemoveGroupBuffer(m_groupBuffer);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddInstanced()
    {
        Debug.Log(Mode.ToString());
        var mats = InstancedMeshRender.sharedMaterials;
        var mesh = MeshFilter.sharedMesh;
        var buffer = GeometryInstancingManager.Instance.CreateGroupBuffer(Mode);
        m_groupBuffer = buffer;
        int[] matKey = new int[mats.Length];

        for (int i = 0; i < mats.Length; i++)
        {
            matKey[i] = mats[i].GetInstanceID();
        }

        // TestPass.RenderMesh = mesh;
        // TestPass.RenderMaterial = InstancedMeshRender.sharedMaterial;
        buffer.Init(mesh.GetInstanceID(),matKey,0,new DrawPrefabSetting()
        {
            HasPreZ = false,
            HasShadow = true,
            UseImpostor = false,
            IsTransparent = true,
        });
        buffer.SetRenderInfo(mesh,mats);
        int del = 5;
        int c = 100 * del;
        for (int x = -c; x <= c; x +=del)
        {
            for (int y = -c; y <= c; y+=del)
            {
                Vector3 position = transform.position;
                position += new Vector3(x, 0, y);
                buffer.AddInstanceObject(x *1000000 + y,float4x4.TRS(position,transform.rotation,transform.localScale));
            }
        }
        
    }
}
