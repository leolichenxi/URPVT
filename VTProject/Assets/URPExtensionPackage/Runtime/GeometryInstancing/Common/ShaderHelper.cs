using System.Collections.Generic;

namespace UnityEngine.Rendering.Universal
{
    public static class ShaderHelper
    {
        private static Dictionary<string, int> materialPropertyIds = new Dictionary<string, int>(32);
        public static int GetPropertyID(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                return -1;
            }
            int ret = 0;
            if (!materialPropertyIds.TryGetValue(propertyName, out ret))
            {
                ret = Shader.PropertyToID(propertyName);
                materialPropertyIds.Add(propertyName, ret);
            }
            return ret;
        }

    }
}