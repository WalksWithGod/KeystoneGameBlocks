// (c) Copyright Crainiate Software 2009
// This source code is distributed under the terms of the GNU Lesser General Public License (LGPL).
// See http://www.gnu.org/licenses/lgpl-3.0.html and the (Licence).txt file for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Text;

namespace Open.Diagramming.Collections
{
    //Contains a list of reusable objects
    //Allocates new T's when needed
    //When cleared, keeps the memory but resets a pointer to the next free object
    //T requires a parameterless constructor for this to work
    internal class ListFactory<T>
        where T: IDisposable, new()
    {
        private List<T> _internalList;
        private int _pointer;

        public ListFactory()
        {
           _internalList = new List<T>();
            _pointer = 0;
        }

        public void Clear()
        {
            _pointer = 0;
        }

        public int Count
        {
            get
            {
                return _pointer;
            }
        }

        public List<T> InnerList
        {
            get
            {
                return _internalList;
            }
        }

        //Either return T form the list or add a new one
        //Clear T when it is being reused
        public T Create()
        {
            T t;

            //If the pointer is less than the object count then return a recycled object
            //Else return a new object 
            if (_pointer < _internalList.Count )
            {
                t = _internalList[_pointer];
                t.Dispose();
            }
            else
            {
                t = new T();
                _internalList.Add(t);
            }

            _pointer ++;
            return t;
        }
    }
}