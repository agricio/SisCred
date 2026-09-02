using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using CrudApp.Database;
using CrudApp.Services;
using CrudApp.Models;

namespace CrudApp.Repositories
{
    public class ServicoRepository
    {
        // ============================
        // LISTAR TODOS
        // ============================

        public List<Servico> GetAll()
        {
            var lista = new List<Servico>();

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
                FROM servicos
                ORDER BY vencimento;
            ";

            using var cmd = new SQLiteCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(Map(reader));
            }

            return lista;
        }

        // ============================
        // INSERIR
        // ============================

        public void Insert(Servico d)
        {
            using var conn = Database.Database.GetConnection();
            conn.Open();

            string sql = @"
                INSERT INTO servicos
                (
                    tipo,
                    valor,
                    vencimento,
                    data_pagamento,
                    situacao
                )
                VALUES
                (
                    @tipo,
                    @valor,
                    @vencimento,
                    @data_pagamento,
                    @situacao
                )
                RETURNING id;
            ";

            using var cmd = new SQLiteCommand(sql, conn);

            cmd.Parameters.AddWithValue(
                "@tipo",
                d.Tipo ?? ""
            );

            cmd.Parameters.AddWithValue(
                "@valor",
                d.Valor
            );

            cmd.Parameters.AddWithValue(
                "@vencimento",
                ToDbDate(d.Vencimento)
            );

            cmd.Parameters.AddWithValue(
                "@data_pagamento",
                ToDbDate(d.DataPagamento)
            );

            cmd.Parameters.AddWithValue(
                "@situacao",
                d.Situacao ?? ""
            );

            int id = Convert.ToInt32(
                cmd.ExecuteScalar()
            );

            d.Id = id;

            SyncService.Registrar(
                "servicos",
                "INSERT",
                d
            );
        }

        // ============================
        // ATUALIZAR
        // ============================

        public void Update(Servico d)
        {
            using var conn = Database.Database.GetConnection();
            conn.Open();

            string sql = @"
                UPDATE servicos SET
                    tipo = @tipo,
                    valor = @valor,
                    vencimento = @venc,
                    data_pagamento = @pag,
                    situacao = @sit
                WHERE id = @id;
            ";

            using var cmd = new SQLiteCommand(sql, conn);

            cmd.Parameters.AddWithValue(
                "@tipo",
                d.Tipo ?? ""
            );

            cmd.Parameters.AddWithValue(
                "@valor",
                d.Valor
            );

            cmd.Parameters.AddWithValue(
                "@venc",
                ToDbDate(d.Vencimento)
            );

            cmd.Parameters.AddWithValue(
                "@pag",
                ToDbDate(d.DataPagamento)
            );

            cmd.Parameters.AddWithValue(
                "@sit",
                d.Situacao ?? ""
            );

            cmd.Parameters.AddWithValue(
                "@id",
                d.Id
            );

            cmd.ExecuteNonQuery();

            SyncService.Registrar(
                "servicos",
                "UPDATE",
                d
            );
        }

        // ============================
        // EXCLUIR
        // ============================

        public void Delete(int id)
        {
            using var conn = Database.Database.GetConnection();
            conn.Open();

            string sql = @"
                DELETE FROM servicos
                WHERE id = @id;
            ";

            using var cmd = new SQLiteCommand(sql, conn);

            cmd.Parameters.AddWithValue(
                "@id",
                id
            );

            cmd.ExecuteNonQuery();

            SyncService.Registrar(
                "servicos",
                "DELETE",
                new { id }
            );
        }

        // ============================
        // ATRASADAS
        // ============================

        public List<Servico> GetAtrasadas()
        {
            return GetAll()
                .Where(d =>
                    string.Equals(
                        d.Situacao,
                        "Atrasada",
                        StringComparison.OrdinalIgnoreCase
                    )
                    &&
                    d.Vencimento.Date < DateTime.Today
                )
                .ToList();
        }

        // ============================
        // TOTAL DO PERÍODO
        // ============================

        public decimal GetTotalPeriodo(
            DateTime inicio,
            DateTime fim)
        {
            using var conn = Database.Database.GetConnection();
            conn.Open();

            string sql = @"
                SELECT COALESCE(SUM(valor), 0)
                FROM servicos
                WHERE date(vencimento)
                      BETWEEN date(@i) AND date(@f);
            ";

            using var cmd = new SQLiteCommand(sql, conn);

            cmd.Parameters.AddWithValue(
                "@i",
                inicio.ToString("yyyy-MM-dd")
            );

            cmd.Parameters.AddWithValue(
                "@f",
                fim.ToString("yyyy-MM-dd")
            );

            object resultado = cmd.ExecuteScalar();

            return resultado == null ||
                   resultado == DBNull.Value
                ? 0m
                : Convert.ToDecimal(resultado);
        }

        // ============================
        // POR MÊS
        // ============================

        public List<Servico> GetPorMes(
            int ano,
            int mes)
        {
            var lista = new List<Servico>();

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
                FROM servicos
                WHERE strftime('%Y', vencimento) = @ano
                  AND strftime('%m', vencimento) = @mes
                ORDER BY vencimento;
            ";

            using var cmd = new SQLiteCommand(sql, conn);

            cmd.Parameters.AddWithValue(
                "@ano",
                ano.ToString("D4")
            );

            cmd.Parameters.AddWithValue(
                "@mes",
                mes.ToString("D2")
            );

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(Map(reader));
            }

            return lista;
        }

        // ============================
        // BUSCAR POR ID
        // ============================

        public Servico? GetById(int id)
        {
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
                FROM servicos
                WHERE id = @id;
            ";

            using var cmd = new SQLiteCommand(sql, conn);

            cmd.Parameters.AddWithValue(
                "@id",
                id
            );

            using var r = cmd.ExecuteReader();

            if (!r.Read())
                return null;

            return Map(r);
        }

        // ============================
        // MAP
        // ============================

        private Servico Map(SQLiteDataReader reader)
        {
            return new Servico
            {
                Id = Convert.ToInt32(
                    reader["id"]
                ),

                Tipo = reader["tipo"] == DBNull.Value
                    ? ""
                    : reader["tipo"]?.ToString() ?? "",

                Valor = reader["valor"] == DBNull.Value
                    ? 0m
                    : Convert.ToDecimal(reader["valor"]),

                Vencimento = TryDate(
                    reader["vencimento"]
                ) ?? DateTime.MinValue,

                DataPagamento = TryDate(
                    reader["data_pagamento"]
                ),

                Situacao = reader["situacao"] == DBNull.Value
                    ? ""
                    : reader["situacao"]?.ToString() ?? "",

                CriadoEm = TryDate(
                    reader["criado_em"]
                ) ?? DateTime.MinValue
            };
        }

        // ============================
        // CONVERTER DATA DO SQLITE
        // ============================

        private static DateTime? TryDate(object value)
        {
            if (value == null || value == DBNull.Value)
                return null;

            if (value is DateTime dateTime)
                return dateTime;

            return DateTime.TryParse(
                value.ToString(),
                out var data
            )
                ? data
                : null;
        }

        // ============================
        // CONVERTER DATA PARA SQLITE
        // ============================

        private static object ToDbDate(DateTime? date)
        {
            return date.HasValue
                ? date.Value.ToString("yyyy-MM-dd HH:mm:ss")
                : DBNull.Value;
        }

        private static object ToDbDate(DateTime date)
        {
            return date.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}