using System;
using System.Data;

namespace DataDog.Tracing.Sql
{
    public class TraceDbCommand : IDbCommand
    {
        private const string ServiceName = "sql";
        private readonly IDbCommand _command;
        private readonly ISpanSource _spanSource;

        public TraceDbCommand(IDbCommand command)
            : this(command, TraceContextSpanSource.Instance)
        {
        }

        public TraceDbCommand(IDbCommand command, ISpanSource spanSource)
        {
            _command = command;
            _spanSource = spanSource;
        }

        public void Dispose() => _command.Dispose();

        public void Cancel() => _command.Cancel();

        public IDbDataParameter CreateParameter() => _command.CreateParameter();

        private void SetMeta(ISpan span)
        {
            span.SetMeta("sql.CommandText", CommandText);
            span.SetMeta("sql.CommandType", CommandType.ToString());
        }

        public int ExecuteNonQuery()
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

        public IDataReader ExecuteReader() => ExecuteReader(CommandBehavior.Default);

        public IDataReader ExecuteReader(CommandBehavior behavior)
        {
            const string name = "sql." + nameof(ExecuteReader);
            var span = _spanSource.Begin(name, ServiceName, _command.Connection.Database, ServiceName);
            try
            {
                if (span != null)
                {
                    const string metaKey = "sql." + nameof(CommandBehavior);
                    span.SetMeta(metaKey, ((int)behavior).ToString("x"));
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

        public object ExecuteScalar()
        {
            const string name = "sql." + nameof(ExecuteScalar);
            var span = _spanSource.Begin(name, ServiceName, _command.Connection.Database, ServiceName);
            try
            {
                if (span != null) SetMeta(span);
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

        public void Prepare() => _command.Prepare();

        public string CommandText
        {
            get => _command.CommandText;
            set => _command.CommandText = value;
        }

        public int CommandTimeout
        {
            get => _command.CommandTimeout;
            set => _command.CommandTimeout = value;
        }

        public CommandType CommandType
        {
            get => _command.CommandType;
            set => _command.CommandType = value;
        }

        public IDbConnection Connection
        {
            get => _command.Connection;
            set => _command.Connection = value;
        }

        public IDataParameterCollection Parameters => _command.Parameters;

        public IDbTransaction Transaction
        {
            get => _command.Transaction;
            set => _command.Transaction = value;
        }

        public UpdateRowSource UpdatedRowSource
        {
            get => _command.UpdatedRowSource;
            set => _command.UpdatedRowSource = value;
        }
    }
}
