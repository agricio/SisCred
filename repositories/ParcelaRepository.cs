using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using CrudApp.Models;
using CrudApp.Services;
using CrudApp.Database;

namespace CrudApp.Repositories
{
    public class ParcelaRepository
    {
        // ================== LISTAR TODAS AS PARCELAS ==================

        public List<Parcela> GetAll()
        {
            var list = new List<Parcela>();

            using var conn = Database.Database.GetConnection();
            conn.Open();

            using var cmd = new SQLiteCommand(
                "SELECT * FROM parcelas ORDER BY vencimento",
                conn);

            using var rd = cmd.ExecuteReader();

            while (rd.Read())
                list.Add(Map(rd));

            return list;
        }

        // ================== BUSCAR POR ID ==================

        public Parcela? GetById(int id)
        {
            using var conn = Database.Database.GetConnection();
            conn.Open();

            using var cmd = new SQLiteCommand(
                "SELECT * FROM parcelas WHERE id = @id",
                conn);

            cmd.Parameters.AddWithValue("@id", id);

            using var rd = cmd.ExecuteReader();

            return rd.Read() ? Map(rd) : null;
        }

        // ================== BUSCAR PARCELAS DE UM EMPRÉSTIMO ==================

        public List<Parcela> GetByEmprestimo(int emprestimoId)
        {
            var list = new List<Parcela>();

            using var conn = Database.Database.GetConnection();
            conn.Open();

            using var cmd = new SQLiteCommand(
                "SELECT * FROM parcelas WHERE emprestimos_id = @id ORDER BY n_parcelas",
                conn);

            cmd.Parameters.AddWithValue("@id", emprestimoId);

            using var rd = cmd.ExecuteReader();

            while (rd.Read())
                list.Add(Map(rd));

            return list;
        }

        // ================== INSERIR ==================

        public void Insert(Parcela p)
        {
            using var conn = Database.Database.GetConnection();
            conn.Open();

            var sql = @"
                INSERT INTO parcelas
                (
                    emprestimos_id,
                    n_parcelas,
                    amortizacao,
                    juros,
                    valor_prestacao,
                    saldo,
                    vencimento,
                    pagamento,
                    forma_pagamento,
                    agio,
                    situacao
                )
                VALUES
                (
                    @eid,
                    @num,
                    @amort,
                    @juros,
                    @valor,
                    @saldo,
                    @venc,
                    @pag,
                    @forma,
                    @agio,
                    @sit
                )
                RETURNING id;
            ";

            using var cmd = new SQLiteCommand(sql, conn);

            cmd.Parameters.AddWithValue("@eid", p.EmprestimosId);
            cmd.Parameters.AddWithValue("@num", p.NParcelas);

            cmd.Parameters.AddWithValue("@amort", p.Amortizacao);
            cmd.Parameters.AddWithValue("@juros", p.Juros);
            cmd.Parameters.AddWithValue("@valor", p.ValorPrestacao);
            cmd.Parameters.AddWithValue("@saldo", p.Saldo);

            cmd.Parameters.AddWithValue(
                "@venc",
                ToDbDate(p.Vencimento)
            );

            cmd.Parameters.AddWithValue(
                "@pag",
                ToDbDate(p.Pagamento)
            );

            cmd.Parameters.AddWithValue(
                "@forma",
                (object?)p.FormaPagamento ?? DBNull.Value
            );

            cmd.Parameters.AddWithValue(
                "@agio",
                p.Agio.HasValue
                    ? p.Agio.Value
                    : (object)DBNull.Value
            );

            cmd.Parameters.AddWithValue(
                "@sit",
                p.Situacao ?? ""
            );

            int id = Convert.ToInt32(cmd.ExecuteScalar());

            p.Id = id;

            SyncService.Registrar(
                "parcelas",
                "INSERT",
                p
            );
        }

        // ================== ATUALIZAR ==================

        public void Update(Parcela p)
        {
            using var conn = Database.Database.GetConnection();
            conn.Open();

            var sql = @"
                UPDATE parcelas SET

                    emprestimos_id  = @eid,
                    n_parcelas      = @num,
                    amortizacao     = @amort,
                    juros           = @juros,
                    valor_prestacao = @valor,
                    saldo           = @saldo,
                    vencimento      = @venc,
                    pagamento       = @pag,
                    forma_pagamento = @forma,
                    agio            = @agio,
                    situacao        = @sit

                WHERE id = @id;
            ";

            using var cmd = new SQLiteCommand(sql, conn);

            cmd.Parameters.AddWithValue("@id", p.Id);
            cmd.Parameters.AddWithValue("@eid", p.EmprestimosId);
            cmd.Parameters.AddWithValue("@num", p.NParcelas);

            cmd.Parameters.AddWithValue("@amort", p.Amortizacao);
            cmd.Parameters.AddWithValue("@juros", p.Juros);
            cmd.Parameters.AddWithValue("@valor", p.ValorPrestacao);
            cmd.Parameters.AddWithValue("@saldo", p.Saldo);

            cmd.Parameters.AddWithValue(
                "@venc",
                ToDbDate(p.Vencimento)
            );

            cmd.Parameters.AddWithValue(
                "@pag",
                ToDbDate(p.Pagamento)
            );

            cmd.Parameters.AddWithValue(
                "@forma",
                (object?)p.FormaPagamento ?? DBNull.Value
            );

            cmd.Parameters.AddWithValue(
                "@agio",
                p.Agio.HasValue
                    ? p.Agio.Value
                    : (object)DBNull.Value
            );

            cmd.Parameters.AddWithValue(
                "@sit",
                p.Situacao ?? ""
            );

            cmd.ExecuteNonQuery();

            SyncService.Registrar(
                "parcelas",
                "UPDATE",
                p
            );
        }

        // ================== EXCLUIR PARCELA ==================

        public void Delete(int id)
        {
            using var conn = Database.Database.GetConnection();
            conn.Open();

            using var cmd = new SQLiteCommand(
                "DELETE FROM parcelas WHERE id = @id",
                conn);

            cmd.Parameters.AddWithValue("@id", id);

            cmd.ExecuteNonQuery();

            SyncService.Registrar(
                "parcelas",
                "DELETE",
                new { id }
            );
        }

        // ================== EXCLUIR TODAS AS PARCELAS DE UM EMPRÉSTIMO ==================

        public void DeleteByEmprestimo(int emprestimoId)
        {
            using var conn = Database.Database.GetConnection();
            conn.Open();

            using var cmd = new SQLiteCommand(
                "DELETE FROM parcelas WHERE emprestimos_id = @id",
                conn);

            cmd.Parameters.AddWithValue("@id", emprestimoId);

            cmd.ExecuteNonQuery();
        }

        // ================== MAP ==================

        private Parcela Map(SQLiteDataReader rd)
        {
            int id = rd.GetOrdinal("id");
            int idEmprestimo = rd.GetOrdinal("emprestimos_id");
            int idNParcelas = rd.GetOrdinal("n_parcelas");
            int idAmortizacao = rd.GetOrdinal("amortizacao");
            int idJuros = rd.GetOrdinal("juros");
            int idValor = rd.GetOrdinal("valor_prestacao");
            int idSaldo = rd.GetOrdinal("saldo");
            int idAgio = rd.GetOrdinal("agio");
            int idSituacao = rd.GetOrdinal("situacao");
            int idVencimento = rd.GetOrdinal("vencimento");
            int idPagamento = rd.GetOrdinal("pagamento");
            int idForma = rd.GetOrdinal("forma_pagamento");

            return new Parcela
            {
                Id = Convert.ToInt32(rd[id]),

                EmprestimosId = Convert.ToInt32(
                    rd[idEmprestimo]
                ),

                NParcelas = rd.IsDBNull(idNParcelas)
                    ? 0
                    : Convert.ToInt32(rd[idNParcelas]),

                Amortizacao = rd.IsDBNull(idAmortizacao)
                    ? 0
                    : Convert.ToDouble(rd[idAmortizacao]),

                Juros = rd.IsDBNull(idJuros)
                    ? 0
                    : Convert.ToDouble(rd[idJuros]),

                ValorPrestacao = rd.IsDBNull(idValor)
                    ? 0
                    : Convert.ToDouble(rd[idValor]),

                Saldo = rd.IsDBNull(idSaldo)
                    ? 0
                    : Convert.ToDouble(rd[idSaldo]),

                Agio = rd.IsDBNull(idAgio)
                    ? null
                    : Convert.ToDouble(rd[idAgio]),

                Situacao = rd.IsDBNull(idSituacao)
                    ? ""
                    : rd[idSituacao]?.ToString() ?? "",

                Vencimento = TryDate(
                    rd[idVencimento]
                ),

                Pagamento = TryDate(
                    rd[idPagamento]
                ),

                FormaPagamento = rd.IsDBNull(idForma)
                    ? null
                    : rd[idForma]?.ToString()
            };
        }

        // ================== VERIFICA PARCELA EM ATRASO ==================

        public bool TemParcelaEmAtraso(int emprestimoId)
        {
            var parcelas = GetByEmprestimo(emprestimoId);

            return parcelas.Any(p =>
                p.Vencimento.HasValue &&
                p.Vencimento.Value.Date < DateTime.Today &&
                !EhParcelaQuitada(p.Situacao)
            );
        }

        // ================== CONVERSÃO DE DATA ==================

        private static DateTime? TryDate(object value)
        {
            if (value == null || value == DBNull.Value)
                return null;

            if (value is DateTime dateTime)
                return dateTime;

            return DateTime.TryParse(
                value.ToString(),
                out var d)
                    ? d
                    : null;
        }

        private static object ToDbDate(DateTime? date)
        {
            return date.HasValue
                ? date.Value.ToString("yyyy-MM-dd HH:mm:ss")
                : DBNull.Value;
        }

        // ================== SITUAÇÃO DA PARCELA ==================

        private static bool EhParcelaQuitada(string? situacao)
        {
            if (string.IsNullOrWhiteSpace(situacao))
                return false;

            string s = situacao.Trim();

            return
                s.Equals("Paga", StringComparison.OrdinalIgnoreCase) ||
                s.Equals("Paga em Atraso", StringComparison.OrdinalIgnoreCase) ||
                s.Equals("Paga Antecipado", StringComparison.OrdinalIgnoreCase) ||
                s.Equals("Refinanciada", StringComparison.OrdinalIgnoreCase);
        }

        // ================== SITUAÇÃO DO EMPRÉSTIMO ==================

        public string ObterSituacaoDoEmprestimo(int emprestimoId)
        {
            var parcelas = GetByEmprestimo(emprestimoId);

            if (parcelas == null || parcelas.Count == 0)
                return "A Vencer";

            // Existe alguma parcela atrasada?
            bool existeAtraso = parcelas.Any(p =>
                p.Vencimento.HasValue &&
                p.Vencimento.Value.Date < DateTime.Today &&
                !EhParcelaQuitada(p.Situacao)
            );

            if (existeAtraso)
                return "Em Atraso";

            // Todas as parcelas foram quitadas?
            bool todasQuitadas = parcelas.All(p =>
                EhParcelaQuitada(p.Situacao)
            );

            if (todasQuitadas)
                return "Quitado";

            return "A Vencer";
        }

        // ================== PARCELAS PARA GRID ==================

        public List<Parcela> GetParaGrid()
        {
            var list = new List<Parcela>();

            using var conn = Database.Database.GetConnection();
            conn.Open();

            using var cmd = new SQLiteCommand(
                "SELECT * FROM parcelas ORDER BY vencimento",
                conn);

            using var rd = cmd.ExecuteReader();

            while (rd.Read())
            {
                var p = Map(rd);

                // ==========================
                // REGRA DE NEGÓCIO
                // ==========================

                if (EhParcelaQuitada(p.Situacao))
                {
                    p.Situacao = "Quitado";
                }
                else if (
                    p.Vencimento.HasValue &&
                    p.Vencimento.Value.Date < DateTime.Today)
                {
                    p.Situacao = "Vencido";
                }
                else
                {
                    p.Situacao = "A Vencer";
                }

                list.Add(p);
            }

            return list;
        }

        // ================== SOMENTE ATRASADAS ==================

        public List<Parcela> GetSomenteAtrasadas()
        {
            var list = new List<Parcela>();

            using var conn = Database.Database.GetConnection();
            conn.Open();

            // SQLite não possui:
            // vencimento::date
            // CURRENT_DATE do PostgreSQL com esse comportamento.
            //
            // Como as datas estão armazenadas no padrão:
            // yyyy-MM-dd HH:mm:ss
            //
            // podemos comparar diretamente com:
            // date('now')

            var sql = @"
                SELECT *
                FROM parcelas
                WHERE vencimento IS NOT NULL
                  AND date(vencimento) < date('now')
                  AND (
                        situacao IS NULL
                        OR (
                            lower(trim(situacao)) NOT LIKE 'paga%'
                            AND lower(trim(situacao)) <> 'refinanciada'
                        )
                  )
                ORDER BY vencimento;
            ";

            using var cmd = new SQLiteCommand(sql, conn);

            using var rd = cmd.ExecuteReader();

            while (rd.Read())
            {
                var p = Map(rd);

                // Força a situação correta para o grid.
                p.Situacao = "Vencido";

                list.Add(p);
            }

            return list;
        }
    }
}