using System;
using System.Collections.Generic;
using System.Collections;


namespace Lidgren.Network
{
    // Hypnotron - March.28.2012 - Simply added locks around the main accessors lock (mLock) {}
    //             and this seems to fix the issue... so far...
    // Hypnotron - May.17.2011 - TODO: this class is not thread safe and causes.  SetCapacity() occassionally
    // causes ""Source array was not long enough" exception.
    // see-> http://code.google.com/p/lidgren-network/issues/detail?id=33#c2
    // Rather than go crazy with synclocks and bottle necking this queue, we need to find a
    // high performance threadsafe queue that is is ideally lockless.
    // RainceC points to this
    // http://www.truevision3d.com/forums/design_and_theory/lockless_queue_implementation-t18149.0.html;msg125979#msg125979
    // It is based on the lockfree queue implemented here:
    // http://www.codeproject.com/KB/cs/managediocp-part-2.aspx
    // but i dont see such a class listed unless it's part of one of the other projects.
    // http://www.codeproject.com/KB/cs/managediocp-part-2.aspx#2  <-- also read that thread in the talkback section
    // as it indicates bugs in that queue which RaineC likely carried over.
    // PFX framework (Parallels for .NET) for .net 4.0 seems to have a "ConcurrentQueue" class
    // which is supposed to be lockless in it's current implementation? (wasn't in the beta parallel extensions i think?)
    //
    // Actually finally found the pdf from Maged Michael & Scott's lockless queue
    // http://www.research.ibm.com/people/m/michael/podc-1996.pdf
    // it has pseudo code which seems easy enough
    // WARNING: I dont think its possible at all in c# using Maged Michael's implementation
    // it's explained very well in this thread http://www.codeproject.com/Messages/2497338/Still-a-bug-modified.aspx
    // "maybe" it can be done in unsafe code, but im not sure.
    // maybe http://research.microsoft.com/en-us/um/redmond/projects/invisible/src/queue/queue.c.htm
    //
    // NOTE: Lidgren gen 3 he just switched to using locks
    // http://code.google.com/p/lidgren-network-gen3/source/browse/trunk/Lidgren.Network/NetQueue.cs

	/// <summary>
	/// Simplified System.Collection.Generics.Queue with a few modifications:
	/// Doesn't cast exceptions when failing to Peek() or Dequeue()
	/// EnqueueFirst() to push an item on the beginning of the queue
	/// </summary>
	public sealed class NetQueue<T> : IEnumerable<T> where T : class
	{
        private object mLock;
		private const int m_defaultCapacity = 4;

		private int m_head;
		private int m_size;
		private int m_tail;
		private T[] m_items;

		/// <summary>
		/// Gets the number of items in the queue
		/// </summary>
		public int Count { get { return m_size; } }
		
		/// <summary>
		/// Initializes a new instance of the NetQueue class that is empty and has the default capacity
		/// </summary>
		public NetQueue()
		{
			m_items = new T[m_defaultCapacity];
            mLock = new object();
		}

		/// <summary>
		/// Initializes a new instance of the NetQueue class that is empty and has the specified capacity
		/// </summary>
		public NetQueue(int capacity)
		{
			if (capacity < 0)
				throw new ArgumentException("capacity must be positive", "capacity");
			m_items = new T[capacity];
            mLock = new object();

			m_head = 0;
			m_tail = 0;
			m_size = 0;
		}

		/// <summary>
		/// Removes all objects from the queue
		/// </summary>
		public void Clear()
		{
            lock (mLock)
            {
                Array.Clear(m_items, 0, m_items.Length);
                m_head = 0;
                m_tail = 0;
                m_size = 0;
            }
		}

		/// <summary>
		/// Determines whether an element is in the queue
		/// </summary>
		public bool Contains(T item)
		{
            lock (mLock)
            {
                int index = m_head;
                int num2 = m_size;
                EqualityComparer<T> comparer = EqualityComparer<T>.Default;
                while (num2-- > 0)
                {
                    if (item == null)
                    {
                        if (m_items[index] == null)
                        {
                            return true;
                        }
                    }
                    else if ((m_items[index] != null) && comparer.Equals(m_items[index], item))
                    {
                        return true;
                    }
                    index = (index + 1) % m_items.Length;
                }
			    return false;
            }
		}

		/// <summary>
		/// Removes and returns an object from the beginning of the queue
		/// </summary>
		public T Dequeue()
		{
            lock (mLock)
            {
                if (m_size == 0)
                    return null;

                T local = m_items[m_head];
                m_items[m_head] = default(T);
                m_head = (m_head + 1) % m_items.Length;
                m_size--;

                System.Diagnostics.Debug.Assert(local != null); // Hypnotron - Sept.23.2011
                // TODO: it appears there's an issue with null items being enqueued somehow
                // but is this now fixed since i added lock (mLock) in all relevant places in this class?
                return local;
            }
		}

		/// <summary>
		/// Removes and returns an object from the beginning of the queue
		/// </summary>
		public T Dequeue(int stepsForward)
		{
            lock (mLock)
            {
                if (stepsForward == 0)
                    return Dequeue();

                if (stepsForward > m_size - 1)
                    return null; // outside valid range

                int ptr = (m_head + stepsForward) % m_items.Length;
                T local = m_items[ptr];

                while (ptr != m_head)
                {
                    m_items[ptr] = m_items[ptr - 1];
                    ptr--;
                    if (ptr < 0)
                        ptr = m_items.Length - 1;
                }
                m_items[ptr] = default(T);
                m_head = (m_head + 1) % m_items.Length;
                m_size--;

                return local;
            }
		}

		/// <summary>
		/// Adds an object to the end of the queue
		/// </summary>
		public void Enqueue(T item)
		{
            lock (mLock)
            {
                // TODO: how can an item being dequeued be null if it was
                // enqueued NOT null?  
                System.Diagnostics.Debug.Assert(item != null);  // Hypnotron - Sept.23.2011
                if (item == null) return;

                if (m_size == m_items.Length)
                {
                    int capacity = (int)((m_items.Length * 200L) / 100L);
                    if (capacity < (m_items.Length + 4))
                    {
                        capacity = m_items.Length + 4;
                    }
                    SetCapacity(capacity);
                }
                m_items[m_tail] = item;
                m_tail = (m_tail + 1) % m_items.Length;
                m_size++;
            }
		}

		/// <summary>
		/// Adds an object to the beginning of the queue
		/// </summary>
		public void EnqueueFirst(T item)
		{
            lock (mLock)
            {
                System.Diagnostics.Debug.Assert(item != null);  // Hypnotron - March.28.2012
                if (m_size == m_items.Length)
                {
                    int capacity = (int)((m_items.Length * 200L) / 100L);
                    if (capacity < (m_items.Length + 4))
                    {
                        capacity = m_items.Length + 4;
                    }
                    SetCapacity(capacity);
                }

                m_head--;
                if (m_head < 0)
                    m_head = m_items.Length - 1;
                m_items[m_head] = item;
                m_size++;
            }
		}

		public T Peek(int stepsForward)
		{
            lock (mLock)
            {
                return m_items[(m_head + stepsForward) % m_items.Length];
            }
		}

		/// <summary>
		/// Returns the object at the beginning of the queue without removing it
		/// </summary>
		public T Peek()
		{
            lock (mLock)
            {
                if (m_size == 0)
                    return null;
                return m_items[m_head];
            }
		}

        
        private void SetCapacity(int capacity)
		{
            try
            {
                T[] destinationArray = new T[capacity];
                if (m_size > 0)
                {
                    if (m_head < m_tail)
                    {
                        Array.Copy(m_items, m_head, destinationArray, 0, m_size);
                    }
                    else
                    {
                        Array.Copy(m_items, m_head, destinationArray, 0, m_items.Length - m_head);
                        Array.Copy(m_items, 0, destinationArray, m_items.Length - m_head, m_tail);
                    }
                }
                m_items = destinationArray;
                m_head = 0;
                m_tail = (m_size == capacity) ? 0 : m_size;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("NetQueue.SetCapacity() - " + ex.Message + " +++++stack trace ++++" + ex.StackTrace);
                System.Diagnostics.Debug.WriteLine(String.Format("capacity = {0}  m_head = {1} m_tail = {2} m_items.Length = {3} m_size = {4}", capacity, m_head, m_tail, m_items.Length, m_size));
            }
		}

		public IEnumerator<T> GetEnumerator()
		{
            lock (mLock)
            {
                int bufLen = m_items.Length;

                int ptr = m_head;
                int len = m_size;
                while (len-- > 0)
                {
                    yield return m_items[ptr];
                    ptr++;
                    if (ptr >= bufLen)
                        ptr = 0;
                }
            }
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
            lock (mLock)
            {
                int bufLen = m_items.Length;

                int ptr = m_head;
                int len = m_size;
                while (len-- > 0)
                {
                    yield return m_items[ptr];
                    ptr++;
                    if (ptr >= bufLen)
                        ptr = 0;
                }
            }
		}
	}
}