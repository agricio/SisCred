using System.Text.Json;
using System.Data.SQLite;
using CrudApp.Database;

namespace CrudApp.Services
{
    public static class SyncService
    {
        public static void Registrar(string tabela, string operacao, object dados)
        {
            using var conn = Database.Database.GetConnection();
            conn.Open();

            string json = JsonSerializer.Serialize(dados);

            string sql = @"
            INSERT INTO sync_log (tabela, operacao, dados, usuario)
            VALUES (@tabela, @operacao, @dados, @usuario)";

            using var cmd = new SQLiteCommand(sql, conn);

            cmd.Parameters.AddWithValue("tabela", tabela);
            cmd.Parameters.AddWithValue("operacao", operacao);
            cmd.Parameters.AddWithValue("@dados", json);
            cmd.Parameters.AddWithValue("usuario", Session.CurrentUsername);

            cmd.ExecuteNonQuery();
        }
    }
}