using System;
using System.Collections.Generic;
using System.Data.SQLite;
using CrudApp.Models;
using CrudApp.Services;
using CrudApp.Database;

namespace CrudApp.Repositories
{
    public class ClienteRepository
    {
        // 🔄 LISTAR TODOS
        public List<Cliente> GetAll()
        {
            var list = new List<Cliente>();

            using var conn = Database.Database.GetConnection();
            conn.Open();

            using var cmd = new SQLiteCommand(
                "SELECT * FROM clientes ORDER BY id DESC", conn);

            using var rd = cmd.ExecuteReader();
            while (rd.Read())
                list.Add(Map(rd));

            return list;
        }

        // 🔍 BUSCAR POR ID
        public Cliente? GetById(int id)
        {
            using var conn = Database.Database.GetConnection();
            conn.Open();

            using var cmd = new SQLiteCommand(
                "SELECT * FROM clientes WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var rd = cmd.ExecuteReader();
            return rd.Read() ? Map(rd) : null;
        }

        // ➕ INSERIR
        public int Add(Cliente c)
        {
            using var conn = Database.Database.GetConnection();
            conn.Open();

            var sql = @"INSERT INTO clientes (
                foto, nome, rg, cpf, email, telefone,
                estado, cidade, ocupacao, rua, numero,
                complemento, cep, banco, agencia, conta,
                qtd_contratos, cliente_score, bairro,
                nacionalidade, cel_pix, estado_civil
            )
            VALUES (
                @foto, @nome, @rg, @cpf, @email, @tel,
                @estado, @cidade, @ocupacao, @rua, @numero,
                @comp, @cep, @banco, @agencia, @conta,
                @qtd, @score, @bairro,
                @nacionalidade, @cel_pix, @estado_civil
            )
            RETURNING id;";

            using var cmd = new SQLiteCommand(sql, conn);
            BindParams(cmd, c);

            int id = Convert.ToInt32(cmd.ExecuteScalar());

            // 🔹 atualiza o objeto
            c.Id = id;

            // 🔹 registra sync correto
            SyncService.Registrar("clientes", "INSERT", c);

            return id;
        }

        // ✏️ ATUALIZAR
        public void Update(Cliente c)
        {
            using var conn = Database.Database.GetConnection();
            conn.Open();

            var sql = @"
                UPDATE clientes SET
                    foto=@foto,
                    nome=@nome,
                    rg=@rg,
                    ocupacao=@ocupacao,
                    cpf=@cpf,
                    email=@email,
                    telefone=@tel,
                    estado=@estado,
                    cidade=@cidade,
                    rua=@rua,
                    numero=@numero,
                    complemento=@comp,
                    cep=@cep,
                    banco=@banco,
                    agencia=@agencia,
                    conta=@conta,
                    qtd_contratos=@qtd,
                    cliente_score=@score,
                    bairro=@bairro,
                    nacionalidade=@nacionalidade,
                    cel_pix=@cel_pix,
                    estado_civil=@estado_civil
                WHERE id=@id;
            ";

            using var cmd = new SQLiteCommand(sql, conn);
            BindParams(cmd, c);
            cmd.Parameters.AddWithValue("@id", c.Id);

            cmd.ExecuteNonQuery();

            SyncService.Registrar("clientes", "UPDATE", c);
        }

        // ❌ EXCLUIR
        public void Delete(int id)
        {
            using var conn = Database.Database.GetConnection();
            conn.Open();

            using var cmd = new SQLiteCommand(
                "DELETE FROM clientes WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);

            cmd.ExecuteNonQuery();

            SyncService.Registrar( "clientes", "DELETE", new { id = id });
        }

        // 🔎 NOME POR ID
        public static string? GetNameById(int id)
        {
            using var conn = Database.Database.GetConnection();
            conn.Open();

            using var cmd = new SQLiteCommand(
                "SELECT nome FROM clientes WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);

            return cmd.ExecuteScalar()?.ToString();
        }

        // ================== HELPERS ==================

        private Cliente Map(SQLiteDataReader rd)
        {
            return new Cliente
            {
                Id = rd.GetInt32(rd.GetOrdinal("id")),
                Foto = rd["foto"] == DBNull.Value ? null : (byte[])rd["foto"],
                Nome = rd["nome"]?.ToString() ?? "",
                Rg = rd["rg"]?.ToString(),
                Ocupacao = rd["ocupacao"]?.ToString(),
                Cpf = rd["cpf"]?.ToString(),
                Email = rd["email"]?.ToString(),
                Telefone = rd["telefone"]?.ToString(),
                Bairro = rd["bairro"]?.ToString(),
                Nacionalidade = rd["nacionalidade"]?.ToString(),
                Estado_civil = rd["estado_civil"]?.ToString(),
                Cel_pix = rd["cel_pix"]?.ToString(),
                Estado = rd["estado"]?.ToString(),
                Cidade = rd["cidade"]?.ToString(),
                Rua = rd["rua"]?.ToString(),
                Numero = rd["numero"]?.ToString(),
                Complemento = rd["complemento"]?.ToString(),
                Cep = rd["cep"]?.ToString(),
                Banco = rd["banco"]?.ToString(),
                Agencia = rd["agencia"]?.ToString(),
                Conta = rd["conta"]?.ToString(),
                QtdContratos = rd["qtd_contratos"] == DBNull.Value ? 0 : Convert.ToInt32(rd["qtd_contratos"]),
                ClienteScore = rd["cliente_score"]?.ToString()
            };
        }

        private void BindParams(SQLiteCommand cmd, Cliente c)
        {
            cmd.Parameters.AddWithValue("@foto", (object?)c.Foto ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@nome", c.Nome);
            cmd.Parameters.AddWithValue("@rg", (object?)c.Rg ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ocupacao", (object?)c.Ocupacao ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@cpf", (object?)c.Cpf ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@email", (object?)c.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@tel", (object?)c.Telefone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@bairro", (object?)c.Bairro ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@nacionalidade", (object?)c.Nacionalidade ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@estado_civil", (object?)c.Estado_civil ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@cel_pix", (object?)c.Cel_pix ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@estado", (object?)c.Estado ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@cidade", (object?)c.Cidade ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@rua", (object?)c.Rua ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@numero", (object?)c.Numero ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@comp", (object?)c.Complemento ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@cep", (object?)c.Cep ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@banco", (object?)c.Banco ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@agencia", (object?)c.Agencia ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@conta", (object?)c.Conta ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@qtd", c.QtdContratos);
            cmd.Parameters.AddWithValue("@score", (object?)c.ClienteScore ?? DBNull.Value);
        }
    }
}
