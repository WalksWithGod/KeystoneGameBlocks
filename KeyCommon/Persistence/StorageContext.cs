using System;
using System.Collections.Generic;
using Npgsql;
using System.Data;

namespace KeyCommon.Persistence
{
    /// <summary>
    /// Abstract class that all storage providers must inherit in order to
    /// fascilitate storage and retreival of domain entities (entities that can exist on both client and server)
    /// to remote (e.g. SQL db) or local (e.g client side XML or flat files) storage.
    /// Specific derived implementations of StorageContext will shoulder the responsibility of knowing
    /// how to read and write entities to the underlying storage solution.
    /// </summary>
    /// <remarks></remarks>
    public abstract class StorageContext : IDisposable
    {
        private bool mIsDisposed;

        public abstract int BeginTransaction();

        public abstract void EndTransaction(int transactionID);

        /// <summary>
        /// Store the object to the data store.
        /// </summary>
        /// <param name="value"></param>
        /// <remarks></remarks>
        public abstract void Store(object value);

        public abstract void Store(object value, int transactionID);

        /// <summary>
        /// Delete the object from the data store.
        /// </summary>
        /// <param name="value"></param>
        /// <remarks></remarks>
        public abstract void Delete(object value);
        public abstract void Delete(object value, int transactionID);

        /// <summary>
        /// Retreives a single item of the specified type, from the data store with the matching field and field value
        /// </summary>
        /// <param name="type"></param>
        /// <param name="field"></param>
        /// <param name="fieldTYpe"></param>
        /// <param name="fieldValue"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract object Retreive(Type type, string field, DbType fieldTYpe, object fieldValue);
        public abstract object Retreive(Type type, string field, DbType fieldTYpe, object fieldValue, int transactionID);

        /// <summary>
        /// Retreive a single item of the specified type, from the data store with the matching 'id'
        /// </summary>
        /// <param name="type"></param>
        /// <param name="primaryKey"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract object RetreiveByID(Type type, long primaryKey);
        public abstract object RetreiveByID(Type type, long primaryKey, int transactionID);

        /// <summary>
        /// Retreives all items from the data store that are of the same type.
        /// </summary>
        public abstract object[] RetreiveList(Type type, string field, System.Data.DbType fieldType, object fieldValue);
        public abstract object[] RetreiveList(Type type, string field, System.Data.DbType fieldType, object fieldValue, int transactionID);

        #region "IDisposable"
        ~StorageContext()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) here is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // pass true if managed resources should be disposed. Otherwise false.
        private void Dispose(bool disposing)
        {

            if ((!IsDisposed))
            {
                if ((disposing))
                {

                    DisposeManagedResources();

                    mIsDisposed = true;
                }
                DisposeUnmanagedResources();
            }
        }

        protected virtual void DisposeManagedResources()
        {
            // this is one of two subs that should ever need overriding by dervided types
        }

        protected virtual void DisposeUnmanagedResources()
        {
            // this is one of two subs that should ever need overriding by dervided types
        }

        protected void CheckDisposed()
        {
            if ((IsDisposed)) throw new ObjectDisposedException(this.GetType().Name + " is disposed.");
        }

        protected bool IsDisposed
        {
            get { return mIsDisposed; }
        }
        #endregion
    }
}
