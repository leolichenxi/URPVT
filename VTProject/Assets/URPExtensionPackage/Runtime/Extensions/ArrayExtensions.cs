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
    }
}