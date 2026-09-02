
using System;
using System.IO;
using System.Data.SQLite;

namespace CrudApp.Database
{
    public static class Database
    {
        private const string DB_NAME = "Speed_cred.db";

        public static string DbPath =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DB_NAME);

        public static string ConnectionString =>
            $"Data Source={DbPath};Version=3;foreign keys=true;";

        public static SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(ConnectionString);
        }

        public static void Initialize()
        {
            // =========================================================
            // CRIAR BANCO
            // =========================================================

            if (!File.Exists(DbPath))
            {
                SQLiteConnection.CreateFile(DbPath);
            }

            using var conn = GetConnection();
            conn.Open();

            // =========================================================
            // FOREIGN KEYS
            // =========================================================

            using (var pragma = new SQLiteCommand(
                "PRAGMA foreign_keys = ON;", conn))
            {
                pragma.ExecuteNonQuery();
            }

            // =========================================================
            // CLIENTES
            // =========================================================

            string sqlClientes = @"
                CREATE TABLE IF NOT EXISTS clientes (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    foto BLOB,
                    nome TEXT NOT NULL,
                    rg TEXT,
                    ocupacao TEXT,
                    cpf TEXT UNIQUE,
                    email TEXT,
                    telefone TEXT,
                    cel_pix TEXT,
                    estado TEXT,
                    cidade TEXT,
                    rua TEXT,
                    numero TEXT,
                    complemento TEXT,
                    cep TEXT,
                    banco TEXT,
                    agencia TEXT,
                    conta TEXT,
                    bairro TEXT,
                    nacionalidade TEXT,
                    estado_civil TEXT,
                    qtd_contratos INTEGER DEFAULT 0,
                    cliente_score TEXT,
                    criado_em TEXT DEFAULT CURRENT_TIMESTAMP,
                    atualizado_em TEXT DEFAULT CURRENT_TIMESTAMP
                );
            ";

            // =========================================================
            // EMPRESTIMOS
            // =========================================================

            string sqlEmprestimos = @"
                CREATE TABLE IF NOT EXISTS emprestimos (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    cliente_id INTEGER NOT NULL,
                    contrato INTEGER,
                    liberado REAL,
                    agio REAL,
                    valor_contrato REAL,
                    total_juros REAL,
                    total_amortizacao REAL,
                    parcelas INTEGER,
                    codigo INTEGER,
                    averbacao TEXT,
                    vencimento TEXT,
                    quitacao TEXT,
                    tipo TEXT,
                    situacao TEXT,
                    tipo_quitacao TEXT,
                    abatimento REAL,
                    criado_em TEXT DEFAULT CURRENT_TIMESTAMP,
                    atualizado_em TEXT DEFAULT CURRENT_TIMESTAMP,

                    FOREIGN KEY (cliente_id)
                        REFERENCES clientes(id)
                        ON DELETE CASCADE
                );
            ";

            // =========================================================
            // PARCELAS
            // =========================================================

            string sqlParcelas = @"
                CREATE TABLE IF NOT EXISTS parcelas (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    emprestimos_id INTEGER NOT NULL,
                    n_parcelas INTEGER,
                    amortizacao REAL,
                    juros REAL,
                    valor_prestacao REAL,
                    saldo REAL,
                    forma_pagamento TEXT,
                    vencimento TEXT,
                    pagamento TEXT,
                    agio REAL,
                    situacao TEXT,
                    criado_em TEXT DEFAULT CURRENT_TIMESTAMP,
                    atualizado_em TEXT DEFAULT CURRENT_TIMESTAMP,

                    FOREIGN KEY (emprestimos_id)
                        REFERENCES emprestimos(id)
                        ON DELETE CASCADE
                );
            ";

            // =========================================================
            // DESPESAS
            // =========================================================

            string sqlDespesas = @"
                CREATE TABLE IF NOT EXISTS despesas (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    tipo TEXT NOT NULL,
                    valor REAL NOT NULL,
                    vencimento TEXT NOT NULL,
                    data_pagamento TEXT,
                    situacao TEXT NOT NULL,
                    criado_em TEXT DEFAULT CURRENT_TIMESTAMP
                );
            ";

            // =========================================================
            // SERVICOS
            // =========================================================

            string sqlServicos = @"
                CREATE TABLE IF NOT EXISTS servicos (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    tipo TEXT NOT NULL,
                    valor REAL NOT NULL,
                    vencimento TEXT NOT NULL,
                    data_pagamento TEXT,
                    situacao TEXT NOT NULL,
                    criado_em TEXT DEFAULT CURRENT_TIMESTAMP,
                    atualizado_em TEXT DEFAULT CURRENT_TIMESTAMP
                );
            ";

            // =========================================================
            // USUARIOS
            // =========================================================

            string sqlUsuarios = @"
                CREATE TABLE IF NOT EXISTS usuarios (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    username TEXT NOT NULL UNIQUE,
                    password_hash TEXT NOT NULL,
                    password_salt TEXT NOT NULL,
                    role TEXT NOT NULL,
                    criado_em TEXT DEFAULT CURRENT_TIMESTAMP,
                    atualizado_em TEXT DEFAULT CURRENT_TIMESTAMP
                );
            ";

            // =========================================================
            // SYNC LOG
            // =========================================================

            string sqlSyncLog = @"
                CREATE TABLE IF NOT EXISTS sync_log (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    tabela TEXT NOT NULL,
                    operacao TEXT NOT NULL,
                    dados TEXT,
                    data TEXT DEFAULT CURRENT_TIMESTAMP,
                    usuario TEXT,
                    sincronizado INTEGER DEFAULT 0
                );
            ";

            // =========================================================
            // EXECUTAR CRIAÇÃO DAS TABELAS
            // =========================================================

            ExecutarComando(conn, sqlClientes);
            ExecutarComando(conn, sqlEmprestimos);
            ExecutarComando(conn, sqlParcelas);
            ExecutarComando(conn, sqlDespesas);
            ExecutarComando(conn, sqlServicos);
            ExecutarComando(conn, sqlUsuarios);
            ExecutarComando(conn, sqlSyncLog);

            // =========================================================
            // ÍNDICES
            // =========================================================

            ExecutarComando(conn,
                "CREATE INDEX IF NOT EXISTS idx_clientes_cpf ON clientes(cpf);");

            ExecutarComando(conn,
                "CREATE INDEX IF NOT EXISTS idx_emprestimos_cliente ON emprestimos(cliente_id);");

            ExecutarComando(conn,
                "CREATE INDEX IF NOT EXISTS idx_parcelas_emprestimo ON parcelas(emprestimos_id);");

            ExecutarComando(conn,
                "CREATE INDEX IF NOT EXISTS idx_sync_nao_enviado ON sync_log(sincronizado);");

            // =========================================================
            // USUÁRIO ADMIN
            // =========================================================

            using (var check = new SQLiteCommand(
                "SELECT COUNT(*) FROM usuarios WHERE username = @u;",
                conn))
            {
                check.Parameters.AddWithValue("@u", "admin");

                long count = Convert.ToInt64(check.ExecuteScalar());

                if (count == 0)
                {
                    var (hash, salt) =
                        Auth.CreateHashedPassword("123");

                    using var insert = new SQLiteCommand(@"
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
                        );
                    ", conn);

                    insert.Parameters.AddWithValue("@u", "admin");
                    insert.Parameters.AddWithValue("@h", hash);
                    insert.Parameters.AddWithValue("@s", salt);
                    insert.Parameters.AddWithValue("@r", "admin");

                    insert.ExecuteNonQuery();
                }
            }
        }

        // =============================================================
        // MÉTODO AUXILIAR
        // =============================================================

        private static void ExecutarComando(
            SQLiteConnection conn,
            string sql)
        {
            using var cmd = new SQLiteCommand(sql, conn);
            cmd.ExecuteNonQuery();
        }
    }
}
