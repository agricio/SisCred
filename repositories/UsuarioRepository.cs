using System;
using System.Collections.Generic;
using System.Data.SQLite;
using CrudApp.Models;
using CrudApp.Services;
using CrudApp.Database;

namespace CrudApp.Repositories
{
    public class UsuarioRepository
    {
        public Usuario? Login(string username, string password)
        {
            using var conn = Database.Database.GetConnection();
            conn.Open();

            var sql = @"
                SELECT 
                    id,
                    username,
                    password_hash,
                    role
                FROM usuarios
                WHERE username = @u;
            ";

            using var cmd = new SQLiteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@u", username);

            using var rd = cmd.ExecuteReader();

            if (!rd.Read())
                return null;

            string passwordHash = rd["password_hash"]?.ToString() ?? "";

            if (!Auth.Equals(password, passwordHash))
                return null;

            return new Usuario
            {
                Id = Convert.ToInt32(rd["id"]),
                Username = rd["username"]?.ToString() ?? "",
                Role = rd["role"]?.ToString() ?? ""
            };
        }

        public int CriarUsuario(Usuario u)
        {
            using var conn = Database.Database.GetConnection();
            conn.Open();

            var sql = @"
                INSERT INTO usuarios
                    (username, password_hash, role)
                VALUES
                    (@u, @p, @r)
                RETURNING id;
            ";

            using var cmd = new SQLiteCommand(sql, conn);

            cmd.Parameters.AddWithValue("@u", u.Username ?? "");
            cmd.Parameters.AddWithValue("@p", u.PasswordHash ?? "");
            cmd.Parameters.AddWithValue("@r", u.Role ?? "");

            int id = Convert.ToInt32(cmd.ExecuteScalar());

            u.Id = id;

            SyncService.Registrar(
                "usuarios",
                "INSERT",
                u
            );

            return id;
        }

        public List<Usuario> ListarTodos()
        {
            var list = new List<Usuario>();

            using var conn = Database.Database.GetConnection();
            conn.Open();

            var sql = @"
                SELECT
                    id,
                    username,
                    password_hash,
                    role
                FROM usuarios
                ORDER BY username;
            ";

            using var cmd = new SQLiteCommand(sql, conn);
            using var rd = cmd.ExecuteReader();

            while (rd.Read())
            {
                list.Add(new Usuario
                {
                    Id = Convert.ToInt32(rd["id"]),

                    Username = rd["username"]?.ToString() ?? "",

                    PasswordHash = rd["password_hash"]?.ToString() ?? "",

                    Role = rd["role"]?.ToString() ?? ""
                });
            }

            return list;
        }

        public void Delete(int id)
        {
            using var conn = Database.Database.GetConnection();
            conn.Open();

            var sql = @"
                DELETE FROM usuarios
                WHERE id = @id;
            ";

            using var cmd = new SQLiteCommand(sql, conn);

            cmd.Parameters.AddWithValue("@id", id);

            cmd.ExecuteNonQuery();

            SyncService.Registrar(
                "usuarios",
                "DELETE",
                new { id }
            );
        }
    }
}