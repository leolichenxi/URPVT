using System;
using System.Collections.Generic;

namespace UnityEngine.Rendering.Universal
{
    public class InstancingMaterialProperty
    {
        public readonly List<int> FloatPropertyName = new List<int>();
        public readonly List<int> VectorPropertyName = new List<int>();
        public readonly Dictionary<int, Texture> TextureProperty = new Dictionary<int, Texture>();

        /// <summary>
        /// Register float property
        /// </summary>
        /// <param name="name"></param>
        public void RegisterFloatProperty(string name)
        {
            if (FloatPropertyName.Count >= ImpostorConst.MAX_PROPERTY_COUNT)
            {
                throw new NotSupportedException("Number of Property is [0, 32]");
            }
            int id = ShaderHelper.GetPropertyID(name);
            if (!FloatPropertyName.Contains(id))
            {
                FloatPropertyName.Add(id);
            }
        }

        /// <summary>
        /// Register vector property
        /// </summary>
        /// <param name="name"></param>
        public void RegisterVectorProperty(string name)
        {
            if (VectorPropertyName.Count >= ImpostorConst.MAX_PROPERTY_COUNT)
            {
                throw new NotSupportedException("Number of Property is [0, 32]");
            }
            int id = ShaderHelper.GetPropertyID(name);
            if (!VectorPropertyName.Contains(id))
            {
                VectorPropertyName.Add(id);
            }
        }

        /// <summary>
        /// Deregister float property
        /// </summary>
        /// <param name="name"></param>
        public void DeregisterFloatProperty(string name)
        {
            int id = ShaderHelper.GetPropertyID(name);
            FloatPropertyName.RemoveValueSwapAt(id);
        }

        /// <summary>
        /// Deregister vector property
        /// </summary>
        /// <param name="name"></param>
        public void DeregisterVectorProperty(string name)
        {
            int id = ShaderHelper.GetPropertyID(name);
            VectorPropertyName.RemoveValueSwapAt(id);
        }

        public void SetTexture(string name, Texture value)
        {
            int id = ShaderHelper.GetPropertyID(name);
            if (TextureProperty.ContainsKey(id))
            {
                TextureProperty[id] = value;
            }
            else
            {
                TextureProperty.Add(id, value);
            }
        }
    }
}