using System.Collections.Generic;

namespace WendySharp
{
    class FixedSizedQueue<T> : Queue<T>
    {
        public uint Limit { get; set; }

        public new void Enqueue(T obj)
        {
            base.Enqueue(obj);

            while (base.Count > Limit)
            {
                base.Dequeue();
            }
        }
    }
}
