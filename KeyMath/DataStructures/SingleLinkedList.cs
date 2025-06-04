using System;
using System.Collections.Generic ;

namespace keymath.DataStructures
{
    /// <summary>
    /// A (hopefully) lightweight generic, singlely linked list primarily for renderable entity node lists.
    /// The .net linked list is a doublely linked list and consumes more memory and has a higher GC burden.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class SingleLinkedList<T> : ICollection<T>
    {
        private LinkedItem<T> _first;
        private LinkedItem<T> _last;
        private int _count;
        private object _courseGrainedLock;

        public SingleLinkedList()
        {
            _courseGrainedLock = new object();
        }

        /// <summary>
        /// This class holds one item in the list.
        /// </summary>
        private class LinkedItem<K>
        {
            // only Next node ref is stored, not Previous. Thus this is referred to as Singly Linked List
            public LinkedItem<K> Next;  
            public K Value;

            /// <summary>
            /// Returns the string representation of the class.
            /// </summary>
            public override string ToString()
            {
                if (Value == null)
                    return string.Empty;
                return Value.ToString();
            }
        }
        

        /// <summary>
        /// Returns a string representation of the class.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            lock (_courseGrainedLock)
            {
                if (_first == null)
                    return string.Empty;

                System.Text.StringBuilder result = new System.Text.StringBuilder();

                // NOTE: If lock here and in Enumerator, this will deadlock
                foreach (T value in this)
                {
                    if (result.Length > 0)
                        result.Append(" -> ");
                    result.Append(value);
                }

                return result.ToString();
            }
        }

        // IMPORTANT: We only need to make ADDs thread safe so that multiple threads attempting to
        // Add a new node at the same time, do not 
        // TODO: no thread safe, it seems to me it's possible to lose list items and have mem leaks
        // when adding items in multithreaded scenario
        // a version of Add where the item is sorted using the IComparer specified
        public void Add(T item, System.Collections.Generic.IComparer<T> comparer)
        {
            if (item == null || comparer == null) throw new ArgumentNullException();

            var node = new LinkedItem<T>() { Value = item };

            lock (_courseGrainedLock)
            {
                if (_first == null)
                {
                    _first = node;
                    _last = node;
                    _count++;
                    return;
                }

                var searchNode = _first;
                LinkedItem<T> prevNode = null; ;

                while (searchNode != null)
                {
                    // if compare passes, break and insert that node immediately. 
                    if (comparer.Compare(node.Value, searchNode.Value) == 1)
                        break;

                    // move to the next item.
                    prevNode = searchNode;
                    searchNode = searchNode.Next;
                } // end while

                if (prevNode == null)
                {
                    prevNode = node;
                    prevNode.Next = _first;
                    _first = prevNode;
                }
                else
                {
                    var tmp = searchNode;
                    prevNode.Next = node;
                    node.Next = tmp;
                    if (tmp == null)
                        _last = node;
                }

                _count++;
            } // end lock()
        }

        #region ICollection<T> Members
        public void Add(T item)
        {
            if (item == null)
                throw new ArgumentNullException("value");

            var node = new LinkedItem<T>() { Value = item };

            lock (_courseGrainedLock)
            {
                // check if we need to update the first and last
                // fields.
                if (_first == null)
                    _first = node;
                if (_last != null)
                    _last.Next = node;

                _last = node;
                _count++;
            }
        }

        /// <summary>
        /// Gets the item at the given index.
        /// </summary>
        /// <param name="index">The index of the item that is
        /// returned.</param>
        public T this[int index]
        {
            get
            {
                // throw an exception if the index is not valid.
                if (index < 0)
                    throw new ArgumentOutOfRangeException();

                lock (_courseGrainedLock)
                {
                    // get the first item.
                    var node = _first;

                    // loop until we reach the item we want.
                    for (int i = 0; i < index; i++)
                    {
                        // if there is no next item throw an exception.
                        if (node.Next == null)
                            throw new ArgumentOutOfRangeException();

                        // go to the next item.
                        node = node.Next;
                    }

                    // return the value of the item.
                    return node.Value;
                }
            }
        }

        /// <summary>
        /// Clears the list.
        /// </summary>
        public void Clear()
        {
            lock (_courseGrainedLock)
            {
                _first = null;
                _last = null;
                _count = 0;
            }
        }

        /// <summary>
        /// Returns whether the list holds the given value.
        /// </summary>
        /// <param name="value">The value that is checked for.</param>
        public bool Contains(T value)
        {
            lock (_courseGrainedLock)
            {
                var node = _first;

                // loop until we reach the end of the list.
                while (node != null)
                {
                    // check if we match.
                    if (node.Value.Equals(value))
                        return true;

                    // otherwise go on.
                    node = node.Next;
                }
                return false;
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
        	lock (_courseGrainedLock)
            {
	        	if (_count == 0) return;
	        	if (array.Length < _count) throw new ArgumentOutOfRangeException();
	        	
	        	int i = 0;
	        	var node = _first;
	        	while (node != null)
	            {
	        		array[i++] = node.Value;
	                node = node.Next;
	            }
        	}
        }

        /// <summary>
        /// Gets the amount of items in the list.
        /// </summary>
        public int Count
        {
            get { return _count; }
        }

        /// <summary>
        /// Gets whether the list is read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Removes an item from the list.
        /// </summary>
        /// <param name="value">The item to remove.</param>
        public bool Remove(T value)
        {
            lock (_courseGrainedLock)
            {
                if (_first == null)
                    return false;

                // get the first item.
                var node = _first;

                // check if the first item matches.
                if (node.Value.Equals(value))
                {
                    // if so move the next item to the first one.
                    _first = node.Next;
                    _count--;

                    // if first is null the list has been cleared.
                    // last is also to set null.
                    if (_first == null)
                        _last = null;

                    return true;
                }
                else
                {
                    // loop until we reach the end.
                    while (node.Next != null)
                    {
                        // check the values.
                        if (node.Next.Value.Equals(value))
                        {
                            // if we match store the one after the next
                            // as the next one.
                            node.Next = node.Next.Next;
                            _count--;

                            // if the next item is null, set this item
                            // as the last one.
                            if (node.Next == null)
                                _last = node;
                            return true;
                        }

                        // move to the next item.
                        node = node.Next;
                    }
                }

                return false;
            }
        }
        #endregion

        #region IEnumerable<T> Members
        public IEnumerator<T> GetEnumerator()
        {
            var node = _first;

            // TODO: there is no way to lock this enumerate and still be able to add
            // or remove items or iterate within those using foreach that requires a call to GetEnumerator!
            // things would deadlock
            while (node != null)
            {
                yield return node.Value;
                node = node.Next;
            }

            //if (_last != null)
            //{
            //    var current = _last;
            //    do
            //    {
            //        current = current.Next;
            //        yield return current.Value;
            //    } while (current != _last);
            //}


        }

        #endregion

        #region IEnumerable Members
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
