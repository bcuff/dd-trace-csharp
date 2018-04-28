using System;
using System.Data;
using System.Data.Common;

namespace DataDog.Tracing.Sql.EntityFrameworkCore
{
    public class TraceDbCommand : DbCommand
    {
        private const string ServiceName = "sql";

        private readonly DbCommand _command;
        private readonly ISpanSource _spanSource;

        public IDbCommand InnerCommand => _command;

        protected override DbParameterCollection DbParameterCollection => _command.Parameters;

        public override bool DesignTimeVisible
        {
            get => _command.DesignTimeVisible;
            set => _command.DesignTimeVisible = value;
        }

        public override string CommandText
        {
            get => _command.CommandText;
            set => _command.CommandText = value;
        }

        public override int CommandTimeout
        {
            get => _command.CommandTimeout;
            set => _command.CommandTimeout = value;
        }

        public override CommandType CommandType
        {
            get => _command.CommandType;
            set => _command.CommandType = value;
        }

        protected override DbConnection DbConnection
        {
            get => _command.Connection;
            set => _command.Connection = value;
        }

        public override UpdateRowSource UpdatedRowSource
        {
            get => _command.UpdatedRowSource;
            set => _command.UpdatedRowSource = value;
        }

        protected override DbTransaction DbTransaction
        {
            get => _command.Transaction;
            set => _command.Transaction =
                value is TraceDbTransaction transaction
                    ? transaction.Transaction
                    : value;
        }

        public TraceDbCommand(DbCommand command)
            : this(command, TraceContextSpanSource.Instance) { }

        public TraceDbCommand(DbCommand command, ISpanSource spanSource)
        {
            _command = command ?? throw new ArgumentNullException(nameof(command));
            _spanSource = spanSource ?? throw new ArgumentNullException(nameof(spanSource));
        }

        public new void Dispose() => _command.Dispose();

        public override void Cancel() => _command.Cancel();

        public override void Prepare() => _command.Prepare();

        protected override DbParameter CreateDbParameter() => _command.CreateParameter();

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            const string name = "sql." + nameof(ExecuteReader);
            var span = _spanSource.Begin(name, ServiceName, _command.Connection.Database, ServiceName);
            try
            {
                if (span != null)
                {
                    const string metaKey = "sql." + nameof(CommandBehavior);
                    span.SetMeta(metaKey, behavior.ToString("x"));
                    SetMeta(span);
                }

                return _command.ExecuteReader(behavior);
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

        public override int ExecuteNonQuery()
        {
            const string name = "sql." + nameof(ExecuteNonQuery);
            var span = _spanSource.Begin(name, ServiceName, _command.Connection.Database, ServiceName);
            try
            {
                var result = _command.ExecuteNonQuery();
                if (span != null)
                {
                    span.SetMeta("sql.RowsAffected", result.ToString());
                    SetMeta(span);
                }

                return result;
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

        public override object ExecuteScalar()
        {
            const string name = "sql." + nameof(ExecuteScalar);
            var span = _spanSource.Begin(name, ServiceName, _command.Connection.Database, ServiceName);
            try
            {
                if (span != null)
                    SetMeta(span);

                return _command.ExecuteScalar();
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

        private void SetMeta(ISpan span)
        {
            span.SetMeta("sql.CommandText", CommandText);
            span.SetMeta("sql.CommandType", CommandType.ToString());
        }
    }
}
