using System;
using System.Data;
using System.Diagnostics;

namespace ExoLive.Server.Common.Providers
{
    /// <summary>
    /// DataStateObject interchange interface
    /// </summary>
    public interface IDataContextManagerInterchange
    {
        object DataStateObj { get; }
    }

    /// <summary>
    /// Data context class for managing connection and transaction in hierarchy of calls
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DataContextManager<T> : IDataContextManagerInterchange, IDisposable
    {
        private readonly Func<IDbConnection> _funcCreateConnection;
        private IDbTransaction _txn;
        private IDbConnection _cnn;
        private bool _selfConnection;
        private bool _selfTransaction;

        private IDbConnection InternalCreateConnection()
        {
            return _funcCreateConnection != null ? _funcCreateConnection() : null;
        }

        public DataContextManager(object dataStateObj, Func<IDbConnection> createConnection, bool transactionRequired = true)
        {
            var dataState = dataStateObj;
            var sourceDataState = dataStateObj as IDataContextManagerInterchange;
            if (sourceDataState != null) dataState = sourceDataState.DataStateObj;

            _funcCreateConnection = createConnection;
            _txn = dataState as IDbTransaction;

            if (_txn != null)
                _cnn = _txn.Connection;
            else
                _cnn = dataState as IDbConnection;

            _selfConnection = _cnn == null;
            _selfTransaction = _txn == null;

            if (_selfConnection) _cnn = InternalCreateConnection();
            if (_selfTransaction && transactionRequired)
            {
                Debug.Assert(_cnn != null, "_cnn != null");
                _txn = _cnn.BeginTransaction();
            }
        }

        /// <summary>
        /// Result of Run operation
        /// </summary>
        public T Result { get; set; }

        /// <summary>
        /// Connection object
        /// </summary>
        public IDbConnection Connection
        {
            get { return _cnn; }
        }

        /// <summary>
        /// Transaction object
        /// </summary>
        public IDbTransaction Transaction
        {
            get { return _txn; }
        }

        /// <summary>
        /// Current DataObject
        /// </summary>
        public object DataStateObj
        {
            get
            {
                if (_txn != null) return _txn;
                if (_cnn != null) return _cnn;
                return null;
            }
        }

        /// <summary>
        /// Safe commit transaction if transaction can commited in this context
        /// </summary>
        public void SafeCommit()
        {
            if (_selfTransaction && _txn != null)
                _txn.Commit();
        }

        /// <summary>
        /// Safe rollback transaction if transaction can be rolled back in this context
        /// </summary>
        public void SafeRollback()
        {
            if (_selfTransaction && _txn != null)
                _txn.Rollback();
        }

        /// <summary>
        /// Primary function for Run data operations
        /// </summary>
        /// <param name="runAction"></param>
        [DebuggerStepThrough]
        public void Run(Action<DataContextManager<T>> runAction)
        {
            try
            {
                runAction(this);
            }
            finally
            {
                CleanUp();
            }
        }

        //[DebuggerStepThrough]
        private void CleanUp()
        {
            if (_selfTransaction && _txn != null)
            {
                _txn.Dispose();
                _txn = null;
            }
            if (_selfConnection && _cnn != null)
            {
                _cnn.Close();
                _cnn.Dispose();
                _cnn = null;
            }
        }

        public void Dispose()
        {
            CleanUp();
        }

    }
}
