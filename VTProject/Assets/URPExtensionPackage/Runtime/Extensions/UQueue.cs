using System.Collections.Generic;

namespace UnityEngine.Rendering.Universal
{
    public class UQueue<T>
    {
        private Queue<T> m_frontQueue = new Queue<T>();
        private Queue<T> m_backQueue = new Queue<T>();

        private readonly object _synLock = new object();

        public void Enqueue(T item)
        {
            lock (_synLock)
            {
                m_frontQueue.Enqueue(item);
            }
        }

        public Queue<T> Switch()
        {
            lock (_synLock)
            {
                if (m_backQueue.Count == 0)
                {
                    Queue<T> tmp = m_frontQueue;
                    m_frontQueue = m_backQueue;
                    m_backQueue = tmp;
                }
            }

            return m_backQueue;
        }

        public void Clear()
        {
            lock (_synLock)
            {
                m_frontQueue.Clear();
                m_backQueue.Clear();
            }
        }
        
        public bool IsEmpty
        {
            get
            {
                lock (_synLock)
                {
                    return m_frontQueue.Count == 0;
                }
            }
        }

        public int Count
        {
            get
            {
                lock (_synLock)
                {
                    return m_frontQueue.Count;
                }
            }
        }
    }
}