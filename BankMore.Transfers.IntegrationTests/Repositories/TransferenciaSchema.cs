using System.Data;
using Dapper;

namespace BankMore.Transfers.IntegrationTests.Repositories;

public static class TransferenciaSchema
{
    public static void Create(IDbConnection connection)
    {
        connection.Execute("""
            CREATE TABLE transferencia (
                idtransferencia TEXT PRIMARY KEY,
                idcontacorrente_origem TEXT NOT NULL,
                idcontacorrente_destino TEXT NOT NULL,
                datamovimento TEXT NOT NULL,
                valor REAL NOT NULL
            );
        """);
    }
}
