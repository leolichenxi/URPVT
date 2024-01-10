using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace URPExtensionPackage.Runtime
{
    public class ImpostorMonitor : MonoBehaviour
    {
        public List<RenderTexture> RenderTextures = new List<RenderTexture>();

        private void Update()
        {
            RenderTextures.Clear();
            foreach (var snapshot in GeometryInstancingManager.Instance.SnapshotAtlas.Atlases)
            {
                RenderTextures.Add(snapshot.ColorRT);
                RenderTextures.Add(snapshot.DepthRT);
            }

            // RenderTextures.AddRange( Resources.FindObjectsOfTypeAll<RenderTexture>());;
        }
    }
}