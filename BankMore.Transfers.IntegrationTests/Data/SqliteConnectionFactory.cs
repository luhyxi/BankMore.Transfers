using System.Data;
using System.Data.Common;
using BankMore.Transfers.Infrastructure.Data;
using Microsoft.Data.Sqlite;

namespace BankMore.Transfers.IntegrationTests.Data;

public sealed class SqliteConnectionFactory : IDbConnectionFactory
{
    private readonly SqliteConnection _connection;

    public SqliteConnectionFactory(SqliteConnection connection)
    {
        _connection = connection;
    }

    public ValueTask<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
        => ValueTask.FromResult<DbConnection>(new NonDisposableConnectionWrapper(_connection));

    private sealed class NonDisposableConnectionWrapper : DbConnection
    {
        private readonly DbConnection _inner;

        public NonDisposableConnectionWrapper(DbConnection inner) => _inner = inner;

        public override string ConnectionString
        {
            get => _inner.ConnectionString;
            set => _inner.ConnectionString = value;
        }

        public override string Database => _inner.Database;
        public override string DataSource => _inner.DataSource;
        public override string ServerVersion => _inner.ServerVersion;
        public override ConnectionState State => _inner.State;

        public override void ChangeDatabase(string databaseName) => _inner.ChangeDatabase(databaseName);
        public override void Close() { }
        public override void Open() => _inner.Open();
        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
            => _inner.BeginTransaction(isolationLevel);
        protected override DbCommand CreateDbCommand() => _inner.CreateCommand();
        protected override void Dispose(bool disposing) { }
    }
}
