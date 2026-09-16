using System;
using System.Collections.Generic;
using System.Data.SQLite;
using CrudApp.Models;
using CrudApp.Services;
using CrudApp.Database;

namespace CrudApp.Repositories
{
    public class EmprestimoRepository
    {
        // ================== LISTAR TODOS ==================

        public List<Emprestimo> GetAll()
        {
            var list = new List<Emprestimo>();

            using var conn = Database.Database.GetConnection();
            conn.Open();

            using var cmd = new SQLiteCommand(
                "SELECT * FROM emprestimos ORDER BY id DESC",
                conn);

            using var rd = cmd.ExecuteReader();

            while (rd.Read())
                list.Add(Map(rd));

            return list;
        }

        // ================== POR CLIENTE ==================

        public List<Emprestimo> GetByCliente(int clienteId)
        {
            var list = new List<Emprestimo>();

            using var conn = Database.Database.GetConnection();
            conn.Open();

            using var cmd = new SQLiteCommand(
                "SELECT * FROM emprestimos WHERE cliente_id = @cid ORDER BY id DESC",
                conn);

            cmd.Parameters.AddWithValue("@cid", clienteId);

            using var rd = cmd.ExecuteReader();

            while (rd.Read())
                list.Add(Map(rd));

            return list;
        }

        // ================== POR ID ==================

        public Emprestimo? GetById(int id)
        {
            using var conn = Database.Database.GetConnection();
            conn.Open();

            using var cmd = new SQLiteCommand(
                "SELECT * FROM emprestimos WHERE id = @id",
                conn);

            cmd.Parameters.AddWithValue("@id", id);

            using var rd = cmd.ExecuteReader();

            return rd.Read() ? Map(rd) : null;
        }

        // ================== INSERIR ==================

        public int AddAndReturnId(Emprestimo e)
        {
            return Add(e);
        }

        public int Add(Emprestimo e)
        {
            using var conn = Database.Database.GetConnection();
            conn.Open();

            var sql = @"
                INSERT INTO emprestimos
                (
                    cliente_id,
                    contrato,
                    liberado,
                    valor_contrato,
                    total_juros,
                    total_amortizacao,
                    agio,
                    parcelas,
                    codigo,
                    averbacao,
                    vencimento,
                    tipo,
                    situacao,
                    quitacao,
                    tipo_quitacao,
                    abatimento,
                    atualizado_em
                )
                VALUES
                (
                    @cid,
                    @contrato,
                    @lib,
                    @valor_contrato,
                    @total_juros,
                    @total_amortizacao,
                    @agio,
                    @parcelas,
                    @codigo,
                    @averb,
                    @venc,
                    @tipo,
                    @sit,
                    @quit,
                    @tipoq,
                    @abat,
                    @upd
                )
                RETURNING id;
            ";

            using var cmd = new SQLiteCommand(sql, conn);

            BindParams(cmd, e);

            int id = Convert.ToInt32(cmd.ExecuteScalar());

            e.Id = id;

            SyncService.Registrar(
                "emprestimos",
                "INSERT",
                e
            );

            return id;
        }

        // ================== ATUALIZAR ==================

        public void Update(Emprestimo e)
        {
            using var conn = Database.Database.GetConnection();
            conn.Open();

            var sql = @"
                UPDATE emprestimos SET

                    cliente_id        = @cid,
                    contrato          = @contrato,
                    liberado          = @lib,
                    valor_contrato    = @valor_contrato,
                    total_juros       = @total_juros,
                    total_amortizacao = @total_amortizacao,
                    agio              = @agio,
                    parcelas          = @parcelas,
                    codigo            = @codigo,
                    averbacao         = @averb,
                    vencimento        = @venc,
                    tipo              = @tipo,
                    situacao          = @sit,
                    quitacao          = @quit,
                    tipo_quitacao     = @tipoq,
                    abatimento        = @abat,
                    atualizado_em     = @upd

                WHERE id = @id;
            ";

            using var cmd = new SQLiteCommand(sql, conn);

            cmd.Parameters.AddWithValue("@id", e.Id);

            BindParams(cmd, e);

            cmd.ExecuteNonQuery();

            SyncService.Registrar(
                "emprestimos",
                "UPDATE",
                e
            );
        }

        // ================== EXCLUIR ==================

        public void Delete(int id)
        {
            using var conn = Database.Database.GetConnection();
            conn.Open();

            using var cmd = new SQLiteCommand(
                "DELETE FROM emprestimos WHERE id = @id",
                conn);

            cmd.Parameters.AddWithValue("@id", id);

            cmd.ExecuteNonQuery();

            SyncService.Registrar(
                "emprestimos",
                "DELETE",
                new { id }
            );
        }

        // ================== MAP ==================

        private Emprestimo Map(SQLiteDataReader rd)
        {
            int idContrato = rd.GetOrdinal("contrato");
            int idLiberado = rd.GetOrdinal("liberado");
            int idValorContrato = rd.GetOrdinal("valor_contrato");
            int idTotalJuros = rd.GetOrdinal("total_juros");
            int idTotalAmortizacao = rd.GetOrdinal("total_amortizacao");
            int idAgio = rd.GetOrdinal("agio");
            int idParcelas = rd.GetOrdinal("parcelas");
            int idCodigo = rd.GetOrdinal("codigo");

            int idAverbacao = rd.GetOrdinal("averbacao");
            int idVencimento = rd.GetOrdinal("vencimento");
            int idQuitacao = rd.GetOrdinal("quitacao");

            int idAbatimento = rd.GetOrdinal("abatimento");
            int idAtualizado = rd.GetOrdinal("atualizado_em");

            return new Emprestimo
            {
                Id = Convert.ToInt32(rd["id"]),

                ClienteId = Convert.ToInt32(rd["cliente_id"]),

                Contrato = rd.IsDBNull(idContrato) ? "" : rd["contrato"].ToString(),

                Liberado = rd.IsDBNull(idLiberado)
                    ? 0
                    : Convert.ToDouble(rd["liberado"]),

                ValorContrato = rd.IsDBNull(idValorContrato)
                    ? 0
                    : Convert.ToDouble(rd["valor_contrato"]),

                TotalJuros = rd.IsDBNull(idTotalJuros)
                    ? 0
                    : Convert.ToDouble(rd["total_juros"]),

                TotalAmortizacao = rd.IsDBNull(idTotalAmortizacao)
                    ? 0
                    : Convert.ToDouble(rd["total_amortizacao"]),

                Agio = rd.IsDBNull(idAgio)
                    ? 0
                    : Convert.ToDouble(rd["agio"]),

                Parcelas = rd.IsDBNull(idParcelas)
                    ? 0
                    : Convert.ToInt32(rd["parcelas"]),

                Codigo = rd.IsDBNull(idCodigo)
                    ? 0
                    : Convert.ToInt32(rd["codigo"]),

                Averbacao = LerData(rd, idAverbacao),

                Vencimento = LerData(rd, idVencimento),

                Quitacao = LerData(rd, idQuitacao),

                Tipo = rd["tipo"]?.ToString() ?? "",

                Situacao = rd["situacao"]?.ToString() ?? "",

                TipoQuitacao = rd["tipo_quitacao"]?.ToString() ?? "",

                Abatimento = rd.IsDBNull(idAbatimento)
                    ? null
                    : Convert.ToDouble(rd["abatimento"]),

                AtualizadoEm = LerData(rd, idAtualizado)
            };
        }

        // ================== LEITURA DE DATA ==================

        private DateTime? LerData(SQLiteDataReader rd, int ordinal)
        {
            if (rd.IsDBNull(ordinal))
                return null;

            var valor = rd.GetValue(ordinal);

            if (valor is DateTime data)
                return data;

            if (DateTime.TryParse(
                valor?.ToString(),
                out DateTime resultado))
            {
                return resultado;
            }

            return null;
        }

        // ================== PARAMETROS ==================

        private void BindParams(SQLiteCommand cmd, Emprestimo e)
        {
            cmd.Parameters.AddWithValue(
                "@cid",
                e.ClienteId
            );

            cmd.Parameters.AddWithValue(
                "@contrato",
                e.Contrato
            );

            cmd.Parameters.AddWithValue(
                "@lib",
                e.Liberado
            );

            cmd.Parameters.AddWithValue(
                "@valor_contrato",
                e.ValorContrato
            );

            cmd.Parameters.AddWithValue(
                "@total_juros",
                e.TotalJuros
            );

            cmd.Parameters.AddWithValue(
                "@total_amortizacao",
                e.TotalAmortizacao
            );

            cmd.Parameters.AddWithValue(
                "@agio",
                e.Agio
            );

            cmd.Parameters.AddWithValue(
                "@parcelas",
                e.Parcelas
            );

            cmd.Parameters.AddWithValue(
                "@codigo",
                e.Codigo
            );

            cmd.Parameters.AddWithValue(
                "@averb",
                e.Averbacao.HasValue
                    ? e.Averbacao.Value.ToString("yyyy-MM-dd HH:mm:ss")
                    : (object)DBNull.Value
            );

            cmd.Parameters.AddWithValue(
                "@venc",
                e.Vencimento.HasValue
                    ? e.Vencimento.Value.ToString("yyyy-MM-dd HH:mm:ss")
                    : (object)DBNull.Value
            );

            cmd.Parameters.AddWithValue(
                "@quit",
                e.Quitacao.HasValue
                    ? e.Quitacao.Value.ToString("yyyy-MM-dd HH:mm:ss")
                    : (object)DBNull.Value
            );

            cmd.Parameters.AddWithValue(
                "@tipo",
                e.Tipo ?? ""
            );

            cmd.Parameters.AddWithValue(
                "@sit",
                e.Situacao ?? ""
            );

            cmd.Parameters.AddWithValue(
                "@tipoq",
                e.TipoQuitacao ?? ""
            );

            cmd.Parameters.AddWithValue(
                "@abat",
                e.Abatimento.HasValue
                    ? e.Abatimento.Value
                    : (object)DBNull.Value
            );

            cmd.Parameters.AddWithValue(
                "@upd",
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            );
        }
    }
}