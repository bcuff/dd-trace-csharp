using System;
using System.Data;

namespace DataDog.Tracing.Sql
{
    public class TraceDbConnection : IDbConnection
    {
        const string ServiceName = "sql";

        readonly ISpanSource _spanSource;
        readonly IDbConnection _connection;

        public TraceDbConnection(IDbConnection connection)
            : this(connection, TraceContextSpanSource.Instance)
        {
        }

        public TraceDbConnection(IDbConnection connection, ISpanSource spanSource)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _spanSource = spanSource ?? throw new ArgumentNullException(nameof(spanSource));
        }

        public void Dispose() => _connection.Dispose();

        // todo - span around transactions
        public IDbTransaction BeginTransaction() => _connection.BeginTransaction();

        public IDbTransaction BeginTransaction(IsolationLevel il) => _connection.BeginTransaction(il);

        public void ChangeDatabase(string databaseName) => _connection.ChangeDatabase(databaseName);

        public void Close() => _connection.Close();

        public IDbCommand CreateCommand() => new TraceDbCommand(_connection.CreateCommand(), _spanSource);

        public void Open()
        {
            var span = _spanSource.Begin("sql.connect", ServiceName, _connection.Database, ServiceName);
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

        public string ConnectionString
        {
            get => _connection.ConnectionString;
            set => _connection.ConnectionString = value;
        }

        public int ConnectionTimeout => _connection.ConnectionTimeout;

        public string Database => _connection.Database;

        public ConnectionState State => _connection.State;
    }
}
