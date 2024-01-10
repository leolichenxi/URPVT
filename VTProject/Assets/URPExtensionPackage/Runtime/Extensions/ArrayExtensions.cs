using System.Collections.Generic;

namespace UnityEngine.Rendering.Universal
{
    public static class ArrayExtensions
    {
        public static void RemoveSwapAt<T>(this DynamicArray<T> array, int arrayIndex) where T : new()
        {
            if (arrayIndex >= array.size || arrayIndex < 0)
            {
                Debug.LogError("arrayIndex out of range.");
            }
            int lastIndex = array.size - 1;
            if (arrayIndex != lastIndex)
            {
                array[arrayIndex] = array[lastIndex];
            }
            array.RemoveAt(lastIndex);
        }
        
        public static void RemoveValueSwapAt<T>(this List<T> array, T value)   //where T : new()
        {
            int arrayIndex = array.IndexOf(value);
            if (arrayIndex >= array.Count || arrayIndex < 0)
            {
                Debug.LogError("arrayIndex out of range.");
            }
            int lastIndex = array.Count - 1;
            if (arrayIndex != lastIndex)
            {
                array[arrayIndex] = array[lastIndex];
            }
            array.RemoveAt(lastIndex);
        }
        
        public static void RemoveSwapAt<T>(this List<T> array, int arrayIndex) //where T : new()
        {
            if (arrayIndex >= array.Count || arrayIndex < 0)
            {
                Debug.LogError($"arrayIndex out of range. array count = {array.Count}, arrayIndex = {arrayIndex}");
            }
            int lastIndex = array.Count - 1;
            if (arrayIndex != lastIndex)
            {
                array[arrayIndex] = array[lastIndex];
            }
            array.RemoveAt(lastIndex);
        }
    }
}