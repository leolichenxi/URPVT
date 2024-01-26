namespace UnityEngine.Rendering.Universal
{
    // 二级缓存
    public class BatchGroup
    {
        public bool HasElement => ValidLength > 0;
        public bool HasSpace => ValidLength < Capacity;
        public Matrix4x4[] MatrixBuffer => m_buffer;
        public int ValidLength { get; private set; }
        public int Capacity { get; private set; }
        private Matrix4x4[] m_buffer;
        private int[] m_objects;
        private int m_MaterialNum;
        public BatchGroup(int capacity)
        {
            Capacity = GetCapacity(capacity);
            ValidLength = 0;
            m_buffer = new Matrix4x4[Capacity];
            m_objects = new int[Capacity];
        }
        /// <summary>
        /// 添加 返回当前值所在的下标
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="matrix4X4"></param>
        /// <returns></returns>
        public int AddMatrix(int uid, Matrix4x4 matrix4X4)
        {
            if (HasSpace)
            {
                int l = ValidLength;
                m_buffer[ValidLength] = matrix4X4;
                m_objects[ValidLength] = uid;
                ValidLength++;
                return l;
            }

            return -1;
        }

        public bool TryAddMatrix(int uid, Matrix4x4 drawMatrix, ref int addIndex)
        {
            addIndex = AddMatrix(uid, drawMatrix);
            return addIndex >= 0;
        }

        /// <summary>
        /// 直接根据下表设置， 没有数据则抛error 或异常
        /// </summary>
        /// <param name="dataIndex"></param>
        /// <param name="matrix4X4"></param>
        public void SetMatrixAt(int dataIndex, Matrix4x4 matrix4X4)
        {
            if (dataIndex >= Capacity || dataIndex > ValidLength)
            {
                Debug.LogError("set data error. buff is full or not sequence!!");
                return;
            }

            m_buffer[dataIndex] = matrix4X4;
        }

        /// <summary>
        /// 根据下标删除数据
        /// </summary>
        /// <param name="dataIndex"></param>
        /// <param name="changedObjectUid"></param>
        /// <param name="changedToIndex">new data index</param>
        /// <returns></returns>
        public bool RemoveAt(int dataIndex, out int changedObjectUid, out int changedToIndex)
        {
            changedToIndex = -1;
            changedObjectUid = -1;
            if (dataIndex >= ValidLength || ValidLength <= 0)
            {
                return false;
            }
            ValidLength--;
            if (dataIndex != ValidLength)
            {
                m_buffer[dataIndex] = m_buffer[ValidLength];
                //先赋值
                changedObjectUid = m_objects[ValidLength];
                m_objects[dataIndex] = m_objects[ValidLength];
                changedToIndex = dataIndex;
                return true;
            }

            return false;
        }

        public void Release()
        {
            this.ValidLength = 0;
        }

        public static int GetCapacity(int need)
        {
            if (need >= ImpostorConst.MAX_GEOMETRY_INSTANCE_DRAW_COUNT)
            {
                return ImpostorConst.MAX_GEOMETRY_INSTANCE_DRAW_COUNT;
            }
            return need;
        }
    }
}