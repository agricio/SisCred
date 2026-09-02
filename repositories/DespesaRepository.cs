using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using CrudApp.Database;
using CrudApp.Models;
using CrudApp.Services;

namespace CrudApp.Repositories
{
    public class DespesaRepository
    {
        public List<Despesa> GetAll()
        {
            var lista = new List<Despesa>();

            using var conn = Database.Database.GetConnection();
            conn.Open();

            string sql = @"
                SELECT
                    id,
                    tipo,
                    valor,
                    vencimento,
                    data_pagamento,
                    situacao,
                    criado_em
                FROM despesas
                ORDER BY vencimento;
            ";

            using var cmd = new SQLiteCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(new Despesa
                {
                    Id = reader.GetInt32(0),
                    Tipo = reader.GetString(1),
                    Valor = reader.GetDecimal(2),
                    Vencimento = reader.GetDateTime(3),
                    DataPagamento = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                    Situacao = reader.GetString(5),
                    CriadoEm = reader.GetDateTime(6)
                });
            }

            return lista;
        }

        public void Insert(Despesa d)
        {
            using var conn = Database.Database.GetConnection();
            conn.Open();

            string sql = @"
                INSERT INTO despesas
                (tipo, valor, vencimento, data_pagamento, situacao)
                VALUES
                (@tipo, @valor, @vencimento, @data_pagamento, @situacao)
                RETURNING id;
            ";

            using var cmd = new SQLiteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@tipo", d.Tipo);
            cmd.Parameters.AddWithValue("@valor", d.Valor);
            cmd.Parameters.AddWithValue("@vencimento", d.Vencimento);
            cmd.Parameters.AddWithValue("@data_pagamento",
                d.DataPagamento.HasValue ? d.DataPagamento.Value : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@situacao", d.Situacao);

            int id = Convert.ToInt32(cmd.ExecuteScalar());

            // atualiza o objeto
            d.Id = id;

            // registra no sync
            SyncService.Registrar("despesas", "INSERT", d);
        }

        public void Update(Despesa d)
        {
            using var conn = Database.Database.GetConnection();
            conn.Open();

            string sql = @"
                UPDATE despesas SET
                    tipo = @tipo,
                    valor = @valor,
                    vencimento = @venc,
                    data_pagamento = @pag,
                    situacao = @sit
                WHERE id = @id
            ";

            using var cmd = new SQLiteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@tipo", d.Tipo);
            cmd.Parameters.AddWithValue("@valor", d.Valor);
            cmd.Parameters.AddWithValue("@venc", d.Vencimento);
            cmd.Parameters.AddWithValue("@pag", (object?)d.DataPagamento ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@sit", d.Situacao);
            cmd.Parameters.AddWithValue("@id", d.Id);

            cmd.ExecuteNonQuery();
            SyncService.Registrar("despesas", "UPDATE", d);
        }

        public void Delete(int id)
        {
            using var conn = Database.Database.GetConnection();
            conn.Open();

            string sql = "DELETE FROM despesas WHERE id = @id";

            using var cmd = new SQLiteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
            
            SyncService.Registrar("despesas", "DELETE", new { id = id });
        }

        public List<Despesa> GetAtrasadas()
        {
            return GetAll()
                .Where(d => d.Situacao == "Atrasada" && d.Vencimento.Date < DateTime.Today)
                .ToList();
        }

        public decimal GetTotalPeriodo(DateTime inicio, DateTime fim)
        {
            using var conn = Database.Database.GetConnection();
            conn.Open();

            using var cmd = new SQLiteCommand(@"
                SELECT COALESCE(SUM(valor), 0)
                FROM despesas
                WHERE vencimento BETWEEN @i AND @f;
            ", conn);

            cmd.Parameters.AddWithValue("@i", inicio);
            cmd.Parameters.AddWithValue("@f", fim);

            return (decimal)cmd.ExecuteScalar();
        }

        public List<Despesa> GetPorMes(int ano, int mes)
        {
            var lista = new List<Despesa>();

            using var conn = Database.Database.GetConnection();
            conn.Open();

            string sql = @"
                SELECT id, tipo, valor, vencimento, data_pagamento, situacao, criado_em
                FROM despesas
                WHERE EXTRACT(YEAR FROM vencimento) = @ano
                AND EXTRACT(MONTH FROM vencimento) = @mes
                ORDER BY vencimento;
            ";

            using var cmd = new SQLiteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ano", ano);
            cmd.Parameters.AddWithValue("@mes", mes);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new Despesa
                {
                    Id = reader.GetInt32(0),
                    Tipo = reader.GetString(1),
                    Valor = reader.GetDecimal(2),
                    Vencimento = reader.GetDateTime(3),
                    DataPagamento = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                    Situacao = reader.GetString(5),
                    CriadoEm = reader.GetDateTime(6)
                });
            }
            return lista;
        }

        public Despesa GetById(int id)
        {
            using var conn = Database.Database.GetConnection();
            conn.Open();

            string sql = @"
                SELECT id, tipo, valor, vencimento, data_pagamento, situacao
                FROM despesas
                WHERE id = @id
            ";

            using var cmd = new SQLiteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var r = cmd.ExecuteReader();
            if (!r.Read()) return null;

            return new Despesa
            {
                Id = r.GetInt32(0),
                Tipo = r.GetString(1),
                Valor = r.GetDecimal(2),
                Vencimento = r.GetDateTime(3),
                DataPagamento = r.IsDBNull(4) ? null : r.GetDateTime(4),
                Situacao = r.GetString(5)
            };
        }

        

    }
}