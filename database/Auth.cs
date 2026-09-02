using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using CrudApp.Services;
using System.Data.SQLite;

namespace CrudApp.Database
{
    public static class Auth
    {
 
        // HASH DA SENHA

        public static (string hash, string salt) CreateHashedPassword(
            string password,
            int iterations = 100_000,
            int saltSize = 16,
            int hashSize = 32)
        {
            using var rng = RandomNumberGenerator.Create();

            byte[] salt = new byte[saltSize];
            rng.GetBytes(salt);

            using var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256
            );

            byte[] hash = pbkdf2.GetBytes(hashSize);

            return (
                Convert.ToBase64String(hash),
                Convert.ToBase64String(salt)
            );
        }

        // VERIFICAR SENHA
  
        public static bool VerifyPassword(
            string password,
            string storedHashBase64,
            string storedSaltBase64,
            int iterations = 100_000,
            int hashSize = 32)
        {
            byte[] salt =
                Convert.FromBase64String(storedSaltBase64);

            using var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256
            );

            byte[] computed =
                pbkdf2.GetBytes(hashSize);

            byte[] storedHash =
                Convert.FromBase64String(storedHashBase64);

            return CryptographicOperations.FixedTimeEquals(
                computed,
                storedHash
            );
        }

        // LOGIN
        
        public static (
            bool ok,
            string role,
            int userId
        ) ValidateUser(
            string username,
            string password)
        {
            using var conn = Database.GetConnection();
            conn.Open();

            using var cmd = new SQLiteCommand(@"
                SELECT
                    id,
                    password_hash,
                    password_salt,
                    role
                FROM usuarios
                WHERE username = @u
            ", conn);

            cmd.Parameters.AddWithValue("@u", username);

            using var reader = cmd.ExecuteReader();

            if (!reader.Read())
                return (false, string.Empty, -1);

            int id = reader.GetInt32(0);

            string hash =
                reader.GetString(1);

            string salt =
                reader.GetString(2);

            string role =
                reader.GetString(3);

            bool ok =
                VerifyPassword(
                    password,
                    hash,
                    salt
                );

            return (
                ok,
                role,
                ok ? id : -1
            );
        }


        // LISTAR USUÁRIOS

        public static List<(int id, string username, string role)>
            GetAllUsers()
        {
            var list =
                new List<(int, string, string)>();

            using var conn =
                Database.GetConnection();

            conn.Open();

            using var cmd = new SQLiteCommand(@"
                SELECT
                    id,
                    username,
                    role
                FROM usuarios
                ORDER BY id
            ", conn);

            using var rdr =
                cmd.ExecuteReader();

            while (rdr.Read())
            {
                list.Add((
                    rdr.GetInt32(0),
                    rdr.GetString(1),
                    rdr.GetString(2)
                ));
            }

            return list;
        }

        // CRIAR USUÁRIO

        public static void CreateUser(
            string username,
            string password,
            string role)
        {
            var (hash, salt) =
                CreateHashedPassword(password);

            using var conn =
                Database.GetConnection();

            conn.Open();

            using var cmd = new SQLiteCommand(@"
                INSERT INTO usuarios
                (
                    username,
                    password_hash,
                    password_salt,
                    role
                )
                VALUES
                (
                    @u,
                    @h,
                    @s,
                    @r
                )
            ", conn);

            cmd.Parameters.AddWithValue("@u", username);
            cmd.Parameters.AddWithValue("@h", hash);
            cmd.Parameters.AddWithValue("@s", salt);
            cmd.Parameters.AddWithValue("@r", role);

            cmd.ExecuteNonQuery();
        }

        // EXCLUIR USUÁRIO

        public static void DeleteUser(int id)
        {
            using var conn =
                Database.GetConnection();

            conn.Open();

            using var cmd = new SQLiteCommand(
                "DELETE FROM usuarios WHERE id = @id",
                conn
            );

            cmd.Parameters.AddWithValue("@id", id);

            cmd.ExecuteNonQuery();

            SyncService.Registrar(
                "usuarios",
                "DELETE",
                new { id }
            );
        }

        // ALTERAR ROLE

        public static void UpdateUserRole(
            int id,
            string role)
        {
            using var conn =
                Database.GetConnection();

            conn.Open();

            using var cmd = new SQLiteCommand(@"
                UPDATE usuarios
                SET role = @r
                WHERE id = @id
            ", conn);

            cmd.Parameters.AddWithValue("@r", role);
            cmd.Parameters.AddWithValue("@id", id);

            cmd.ExecuteNonQuery();
        }

        // ALTERAR NOME DE USUÁRIO

        public static void UpdateUsername(
            int id,
            string username)
        {
            using var conn =
                Database.GetConnection();

            conn.Open();

            using var cmd = new SQLiteCommand(@"
                UPDATE usuarios
                SET username = @u
                WHERE id = @id
            ", conn);

            cmd.Parameters.AddWithValue("@u", username);
            cmd.Parameters.AddWithValue("@id", id);

            cmd.ExecuteNonQuery();

            SyncService.Registrar(
                "usuarios",
                "UPDATE",
                new
                {
                    id,
                    username
                }
            );
        }

        // ALTERAR SENHA

        public static void ChangePassword(
            int id,
            string newPassword)
        {
            var (hash, salt) =
                CreateHashedPassword(newPassword);

            using var conn =
                Database.GetConnection();

            conn.Open();

            using var cmd = new SQLiteCommand(@"
                UPDATE usuarios
                SET
                    password_hash = @h,
                    password_salt = @s
                WHERE id = @id
            ", conn);

            cmd.Parameters.AddWithValue("@h", hash);
            cmd.Parameters.AddWithValue("@s", salt);
            cmd.Parameters.AddWithValue("@id", id);

            cmd.ExecuteNonQuery();
        }
    }
}
