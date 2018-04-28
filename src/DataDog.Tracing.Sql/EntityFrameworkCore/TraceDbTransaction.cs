using System;
using System.Data;
using System.Data.Common;

namespace DataDog.Tracing.Sql.EntityFrameworkCore
{
    // Entity Framework has a check like this:
    // if (connection.DbConnection != transaction.Connection)
    //     throw new InvalidOperationException(RelationalStrings.TransactionAssociatedWithDifferentConnection);
    // Where connection.DbConnection is of type TraceDbConnection and transaction.Connection is of type SqlConnection
    // Because of this we need to implement TraceDbTransaction
    public class TraceDbTransaction : DbTransaction
    {
        private const string ServiceName = "sql";

        private readonly ISpanSource _spanSource;

        protected override DbConnection DbConnection { get; }

        public DbTransaction Transaction { get; }

        public override IsolationLevel IsolationLevel => Transaction.IsolationLevel;

        public TraceDbTransaction(DbConnection connection, DbTransaction transaction)
            : this(connection, transaction, TraceContextSpanSource.Instance) { }

        public TraceDbTransaction(DbConnection connection, DbTransaction transaction, ISpanSource spanSource)
        {
            DbConnection = connection ?? throw new ArgumentNullException(nameof(connection));
            Transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
            _spanSource = spanSource ?? throw new ArgumentNullException(nameof(spanSource));
        }

        public override void Commit()
        {
            const string name = "sql." + nameof(Commit);
            var span = _spanSource.Begin(name, ServiceName, Transaction.Connection.Database, ServiceName);
            try
            {
                Transaction.Commit();
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

        public override void Rollback()
        {
            const string name = "sql." + nameof(Rollback);
            var span = _spanSource.Begin(name, ServiceName, Transaction.Connection.Database, ServiceName);
            try
            {
                Transaction.Rollback();
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
