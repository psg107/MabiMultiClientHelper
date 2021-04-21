using System.Collections.Generic;

namespace MabiMultiClientHelper
{
    public class ThreadSafeList<T> : List<T>
    {
        private readonly object locker = new object();

        public new void Add(T item)
        {
            lock (locker)
            {
                base.Add(item);
            }
        }

        public new void Remove(T item)
        {
            lock (locker)
            {
                base.Remove(item);
            }
        }
    }
}
