using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Rendering.Universal.Internal
{
    public class GeometryInstancingPass : ScriptableRenderPass
    {
        public const string Tag = "GeometryInstancingPass";
        public EInstancePassType InstancePassType { get; private set; }
        public GeometryInstancingPass(RenderPassEvent renderPassEvent, EInstancePassType type)
        {
            this.renderPassEvent = renderPassEvent;
            this.InstancePassType = type;
        }
            
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (!Application.isPlaying)
            {
                return;
            }
            Debug.Log("GeometryInstancingPass Execute");
        }
    }
}
