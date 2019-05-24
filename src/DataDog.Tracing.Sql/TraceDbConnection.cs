using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using IsolationLevel = System.Data.IsolationLevel;

namespace DataDog.Tracing.Sql
{
    public class TraceDbConnection : DbConnection
    {
        private const string ServiceName = "sql";

        private readonly ISpanSource _spanSource;
        private readonly DbConnection _connection;

        public TraceDbConnection(DbConnection connection)
            : this(connection, TraceContextSpanSource.Instance)
        {
        }

        public TraceDbConnection(DbConnection connection, ISpanSource spanSource)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _spanSource = spanSource ?? throw new ArgumentNullException(nameof(spanSource));
        }

        public IDbConnection InnerConnection => _connection;
        
        #region Overrides

        public override string ConnectionString
        {
            get => _connection.ConnectionString;
            set => _connection.ConnectionString = value;
        }
        
        public override int ConnectionTimeout
        {
            get => _connection.ConnectionTimeout;
        }

        public override string Database
        {
            get => _connection.Database;
        }

        public override string DataSource
        {
            get => _connection.DataSource;
        }
        
        public override string ServerVersion 
        {
            get => _connection.ServerVersion;
        }

        public override ConnectionState State
        {
            get => _connection.State;
        }

        public override event StateChangeEventHandler StateChange
        {
            add => _connection.StateChange += value;
            remove => _connection.StateChange -= value;
        }
        
        // todo - span around transactions
        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            return _connection.BeginTransaction(isolationLevel);
        }
        
        public override void Close()
        {
            _connection.Close();
        }

        public override void ChangeDatabase(string databaseName)
        {
            _connection.ChangeDatabase(databaseName);
        }

        protected override DbCommand CreateDbCommand()
        {
            return new TraceDbCommand(_connection.CreateCommand(), _spanSource);
        }

        public override void EnlistTransaction(Transaction transaction)
        {
            _connection.EnlistTransaction(transaction);
        }

        public override DataTable GetSchema()
        {
            return _connection.GetSchema();
        }

        public override DataTable GetSchema(string collectionName)
        {
            return _connection.GetSchema(collectionName);
        }

        public override DataTable GetSchema(string collectionName, string[] restrictionValues)
        {
            return _connection.GetSchema(collectionName, restrictionValues);
        }

        public override void Open()
        {
            ISpan span = _spanSource.Begin("sql.connect", ServiceName, _connection.Database, ServiceName);

            try
            {
                _connection.Open();
            }
            catch (Exception ex)
            {
                span?.SetError(ex);
                throw;
            }
            finally
            {
                span?.Dispose();
            }
        }

        public override async Task OpenAsync(CancellationToken cancellationToken)
        {
            ISpan span = _spanSource.Begin("sql.connect", ServiceName, _connection.Database, ServiceName);

            try
            {
                await _connection.OpenAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                span?.SetError(ex);
                throw;
            }
            finally
            {
                span?.Dispose();
            }
        }

        #endregion
        
        #region Dispose Pattern

        private bool _disposed;

        protected override void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                _connection?.Dispose();
            }

            _disposed = true;

            base.Dispose(disposing);
        }

        #endregion
    }
}
