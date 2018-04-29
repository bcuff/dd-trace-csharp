using System;
using System.Data;
using System.Data.Common;

namespace DataDog.Tracing.Sql.EntityFrameworkCore
{
    public class TraceDbConnection : DbConnection
    {
        private const string DefaultServiceName = "sql";
        private const string TypeName = "sql";

        private string ServiceName { get; }

        private readonly ISpanSource _spanSource;
        private readonly DbConnection _connection;

        public IDbConnection InnerConnection => _connection;

        public override int ConnectionTimeout => _connection.ConnectionTimeout;

        public override string Database => _connection.Database;

        public override string DataSource => _connection.DataSource;

        public override string ServerVersion => _connection.ServerVersion;

        public override ConnectionState State => _connection.State;

        public override string ConnectionString
        {
            get => _connection.ConnectionString;
            set => _connection.ConnectionString = value;
        }

        public TraceDbConnection(DbConnection connection)
            : this(connection, DefaultServiceName, TraceContextSpanSource.Instance) { }

        public TraceDbConnection(DbConnection connection, string serviceName)
            : this(connection, serviceName, TraceContextSpanSource.Instance) { }

        public TraceDbConnection(DbConnection connection, ISpanSource spanSource)
            : this(connection, DefaultServiceName, spanSource) { }

        public TraceDbConnection(DbConnection connection, string serviceName, ISpanSource spanSource)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _spanSource = spanSource ?? throw new ArgumentNullException(nameof(spanSource));

            ServiceName = string.IsNullOrWhiteSpace(serviceName)
                ? DefaultServiceName
                : serviceName;
        }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
            => new TraceDbTransaction(this, _connection.BeginTransaction(isolationLevel), ServiceName, _spanSource);

        protected override DbCommand CreateDbCommand()
            => new TraceDbCommand(_connection.CreateCommand(), ServiceName, _spanSource);

        public new void Dispose()
            => _connection.Dispose();

        public override void ChangeDatabase(string databaseName)
            => _connection.ChangeDatabase(databaseName);

        public override void Close()
            => _connection.Close();

        public override void Open()
        {
            var span = _spanSource.Begin("sql.connect", ServiceName, _connection.Database, TypeName);
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
    }
}
