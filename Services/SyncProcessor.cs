using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;
using System.Text.Json;

public static class SyncProcessor
{
    public static void Aplicar(
        SQLiteConnection conn,
        string tabela,
        string operacao,
        string json)
    {
        var dados = JsonDocument.Parse(json).RootElement;

        int id = GetId(dados);

        // ============================================================
        // DELETE
        // ============================================================

        if (operacao == "DELETE")
        {
            using var delete = new SQLiteCommand(
                $"DELETE FROM {tabela} WHERE id = @id",
                conn);

            delete.Parameters.AddWithValue("@id", id);
            delete.ExecuteNonQuery();

            return;
        }

        // ============================================================
        // PREPARA COLUNAS E PARÂMETROS
        // ============================================================

        var colunas = new List<string>();
        var parametros = new List<string>();
        var updates = new List<string>();

        using var cmd = new SQLiteCommand();
        cmd.Connection = conn;

        foreach (var prop in dados.EnumerateObject())
        {
            string coluna = ToSnakeCase(prop.Name);

            // Ignora colunas que não existem no banco
            if (!ColunaExiste(conn, tabela, coluna))
                continue;

            colunas.Add(coluna);
            parametros.Add("@" + coluna);

            if (coluna != "id")
                updates.Add($"{coluna} = @{coluna}");

            object valor = GetValue(prop.Value);

            // ========================================================
            // FOTO - BASE64 PARA BLOB
            // ========================================================

            if (coluna == "foto" &&
                valor != DBNull.Value &&
                valor is string base64)
            {
                try
                {
                    valor = Convert.FromBase64String(base64);
                }
                catch
                {
                    // Se não for Base64 válido, mantém o valor original
                }
            }

            cmd.Parameters.AddWithValue("@" + coluna, valor);
        }

        // ============================================================
        // VERIFICA SE O REGISTRO JÁ EXISTE
        // ============================================================

        using var check = new SQLiteCommand(
            $"SELECT 1 FROM {tabela} WHERE id = @id",
            conn);

        check.Parameters.AddWithValue("@id", id);

        bool existe = check.ExecuteScalar() != null;

        string sql;

        // ============================================================
        // UPDATE
        // ============================================================

        if (existe)
        {
            if (updates.Count == 0)
                return;

            sql = $@"
                UPDATE {tabela}
                SET {string.Join(", ", updates)}
                WHERE id = @id;
            ";
        }
        // ============================================================
        // INSERT
        // ============================================================

        else
        {
            sql = $@"
                INSERT INTO {tabela}
                    ({string.Join(", ", colunas)})
                VALUES
                    ({string.Join(", ", parametros)});
            ";
        }

        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }

    // ================================================================
    // OBTÉM ID DO JSON
    // ================================================================

    private static int GetId(JsonElement dados)
    {
        foreach (var p in dados.EnumerateObject())
        {
            if (p.Name.Equals(
                "id",
                StringComparison.OrdinalIgnoreCase))
            {
                if (p.Value.ValueKind == JsonValueKind.Number &&
                    p.Value.TryGetInt32(out int id))
                {
                    return id;
                }

                if (p.Value.ValueKind == JsonValueKind.String &&
                    int.TryParse(p.Value.GetString(), out id))
                {
                    return id;
                }
            }
        }

        throw new Exception("ID não encontrado no JSON.");
    }

    // ================================================================
    // CONVERTE JSON PARA TIPO DO BANCO
    // ================================================================

    private static object GetValue(JsonElement el)
    {
        switch (el.ValueKind)
        {
            case JsonValueKind.String:

                string? texto = el.GetString();

                if (string.IsNullOrWhiteSpace(texto))
                    return "";

                // Datas são armazenadas como TEXT no SQLite
                if (DateTime.TryParse(
                    texto,
                    out DateTime dt))
                {
                    return dt.ToString(
                        "yyyy-MM-dd HH:mm:ss");
                }

                return texto;

            case JsonValueKind.Number:

                if (el.TryGetInt32(out int i))
                    return i;

                if (el.TryGetInt64(out long l))
                    return l;

                if (el.TryGetDecimal(out decimal d))
                    return d;

                return el.GetDouble();

            case JsonValueKind.True:
                return 1;

            case JsonValueKind.False:
                return 0;

            case JsonValueKind.Null:
                return DBNull.Value;

            default:
                return el.ToString();
        }
    }

    // ================================================================
    // VERIFICA SE A COLUNA EXISTE NO SQLITE
    // ================================================================

    private static bool ColunaExiste(
        SQLiteConnection conn,
        string tabela,
        string coluna)
    {
        // PRAGMA não aceita parâmetros diretamente para o nome
        // da tabela, então validamos o nome antes.

        if (!NomeSqlSeguro(tabela) ||
            !NomeSqlSeguro(coluna))
        {
            return false;
        }

        using var cmd = new SQLiteCommand(
            $"PRAGMA table_info({tabela});",
            conn);

        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            string nomeColuna =
                reader["name"]?.ToString() ?? "";

            if (nomeColuna.Equals(
                coluna,
                StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    // ================================================================
    // VALIDA NOME DE TABELA/COLUNA
    // ================================================================

    private static bool NomeSqlSeguro(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            return false;

        foreach (char c in nome)
        {
            if (!char.IsLetterOrDigit(c) && c != '_')
                return false;
        }

        return true;
    }

    // ================================================================
    // CAMELCASE → SNAKE_CASE
    // ================================================================

    private static string ToSnakeCase(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        var sb = new StringBuilder();

        for (int i = 0; i < text.Length; i++)
        {
            if (char.IsUpper(text[i]) && i > 0)
                sb.Append('_');

            sb.Append(char.ToLower(text[i]));
        }

        return sb.ToString();
    }
}