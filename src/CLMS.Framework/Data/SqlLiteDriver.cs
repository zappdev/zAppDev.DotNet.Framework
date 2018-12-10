using System.Data;
using System.Data.Common;
using NHibernate.Driver;

namespace CLMS.Framework.Data
{
    public class SqlLiteDriver : ReflectionBasedDriver
    {
        public SqlLiteDriver() : base(
            "Microsoft.Data.Sqlite",
            "Microsoft.Data.Sqlite",
            "Microsoft.Data.Sqlite.SqliteConnection",
            "Microsoft.Data.Sqlite.SqliteCommand")
        {
        }

        public override DbConnection CreateConnection()
        {
            var connection = base.CreateConnection();
            connection.StateChange += Connection_StateChange;
            return connection;
        }

        private static void Connection_StateChange(object sender, StateChangeEventArgs e)
        {
            if ((e.OriginalState != ConnectionState.Broken && e.OriginalState != ConnectionState.Closed &&
                 e.OriginalState != ConnectionState.Connecting) || e.CurrentState != ConnectionState.Open) return;
            var connection = (DbConnection)sender;
            using (var command = connection.CreateCommand())
            {
                // Activated foreign keys if supported by SQLite.  Unknown pragmas are ignored.
                command.CommandText = "PRAGMA foreign_keys = ON";
                command.ExecuteNonQuery();
            }
        }

        public override IResultSetsCommand GetResultSetsCommand(NHibernate.Engine.ISessionImplementor session)
        {
            return new BasicResultSetsCommand(session);
        }

        public override bool UseNamedPrefixInSql => true;

        public override bool UseNamedPrefixInParameter => true;

        public override string NamedPrefix => "@";

        public override bool SupportsMultipleOpenReaders => false;

        public override bool SupportsMultipleQueries => true;

        public override bool SupportsNullEnlistment => false;

        public override bool HasDelayedDistributedTransactionCompletion => true;
    }
}
