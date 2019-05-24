using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace DataDog.Tracing.Sql
{
    public class TraceDbCommand : DbCommand
    {
        private const string ServiceName = "sql";
        private readonly DbCommand _command;
        private readonly ISpanSource _spanSource;

        public TraceDbCommand(DbCommand command)
            : this(command, TraceContextSpanSource.Instance)
        {
        }

        public TraceDbCommand(DbCommand command, ISpanSource spanSource)
        {
            _command = command;
            _spanSource = spanSource;
        }

        public IDbCommand InnerCommand => _command;
        
        #region Overrides

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

        protected override DbParameterCollection DbParameterCollection
        {
            get => _command.Parameters;
        }

        protected override DbTransaction DbTransaction
        {
            get => _command.Transaction;
            set => _command.Transaction = value;
        }

        public override bool DesignTimeVisible
        {
            get => _command.DesignTimeVisible;
            set => _command.DesignTimeVisible = value;
        }

        public override UpdateRowSource UpdatedRowSource
        {
            get => _command.UpdatedRowSource;
            set => _command.UpdatedRowSource = value;
        }

        public override void Cancel()
        {
            _command.Cancel();
        }

        protected override DbParameter CreateDbParameter()
        {
            return _command.CreateParameter();
        }

        public override int ExecuteNonQuery()
        {
            const string name = "sql." + nameof(ExecuteNonQuery);

            var span = _spanSource.Begin(name, ServiceName, _command.Connection.Database, ServiceName);

            try
            {
                int result = _command.ExecuteNonQuery();

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

        public override async Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
        {
            const string name = "sql." + nameof(ExecuteNonQueryAsync);

            var span = _spanSource.Begin(name, ServiceName, _command.Connection.Database, ServiceName);

            try
            {
                int result = await _command.ExecuteNonQueryAsync(cancellationToken);

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

            ISpan span = _spanSource.Begin(name, ServiceName, _command.Connection.Database, ServiceName);

            try
            {
                if (span != null)
                {
                    SetMeta(span);
                }

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

        public override async Task<object> ExecuteScalarAsync(CancellationToken cancellationToken)
        {
            const string name = "sql." + nameof(ExecuteScalarAsync);

            ISpan span = _spanSource.Begin(name, ServiceName, _command.Connection.Database, ServiceName);

            try
            {
                if (span != null)
                {
                    SetMeta(span);
                }

                return await _command.ExecuteScalarAsync(cancellationToken);
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
        
        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            const string name = "sql." + nameof(ExecuteDbDataReader);

            ISpan span = _spanSource.Begin(name, ServiceName, _command.Connection.Database, ServiceName);

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

        protected override async Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
        {
            const string name = "sql." + nameof(ExecuteDbDataReaderAsync);

            ISpan span = _spanSource.Begin(name, ServiceName, _command.Connection.Database, ServiceName);

            try
            {
                if (span != null)
                {
                    const string metaKey = "sql." + nameof(CommandBehavior);

                    span.SetMeta(metaKey, behavior.ToString("x"));
                    SetMeta(span);
                }

                return await _command.ExecuteReaderAsync(behavior, cancellationToken);
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

        public override void Prepare()
        {
            _command.Prepare();
        }

        #endregion

        #region Private Methods
        
        private void SetMeta(ISpan span)
        {
            span.SetMeta("sql.CommandText", CommandText);
            span.SetMeta("sql.CommandType", CommandType.ToString());
        }

        #endregion
        
        #region Dispose Pattern

        private bool _disposed;

        protected override void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                _command?.Dispose();
            }

            _disposed = true;

            base.Dispose(disposing);
        }

        #endregion
    }
}
