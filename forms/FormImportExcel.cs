using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using OfficeOpenXml;
using System.Data.SQLite;
using CrudApp.Database;

namespace CrudApp.Forms
{
    public class FormImportarExcel : Form
    {
        private TextBox txtArquivo;
        private Button btnSelecionar;
        private Button btnImportar;

        public FormImportarExcel()
        {
            InitializeComponent();
            Database.Database.Initialize();
        }

        private void InitializeComponent()
        {
            this.Icon = new Icon("app.ico");
            this.Text = "Importar dados do Excel";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ClientSize = new Size(420, 160);

            txtArquivo = new TextBox
            {
                Left = 20,
                Top = 20,
                Width = 280,
                ReadOnly = true
            };

            btnSelecionar = new Button
            {
                Left = 310,
                Top = 18,
                Width = 80,
                Text = "Arquivo"
            };

            btnSelecionar.Click += BtnSelecionar_Click;

            btnImportar = new Button
            {
                Left = 20,
                Top = 70,
                Width = 370,
                Height = 40,
                Text = "Importar Clientes, Empréstimos e Parcelas"
            };

            btnImportar.Click += BtnImportar_Click;

            Controls.AddRange(new Control[]
            {
                txtArquivo,
                btnSelecionar,
                btnImportar
            });
        }

        private void BtnSelecionar_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Filter = "Excel (*.xlsm)|*.xlsm"
            };

            if (dlg.ShowDialog() == DialogResult.OK)
                txtArquivo.Text = dlg.FileName;
        }

        private void BtnImportar_Click(object sender, EventArgs e)
        {
            if (!File.Exists(txtArquivo.Text))
            {
                MessageBox.Show("Selecione um arquivo válido.");
                return;
            }

            try
            {
                ImportarExcel(txtArquivo.Text);

                MessageBox.Show(
                    "✅ Importação concluída com sucesso!",
                    "Sucesso",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "❌ Erro ao importar:\n" + ex.Message,
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void ImportarExcel(string caminho)
        {
            using var package = new ExcelPackage(new FileInfo(caminho));

            var wsCli = package.Workbook.Worksheets
                .FirstOrDefault(w =>
                    w.Name.ToUpper().Contains("CLIENT"));

            var wsEmp = package.Workbook.Worksheets
                .FirstOrDefault(w =>
                    w.Name.ToUpper().Contains("EMPRE"));

            var wsPar = package.Workbook.Worksheets
                .FirstOrDefault(w =>
                    w.Name.ToUpper().Contains("PARCEL"));

            if (wsCli == null || wsEmp == null || wsPar == null)
                throw new Exception(
                    "Uma ou mais abas não foram encontradas no Excel.");

            using var conn = Database.Database.GetConnection();
            conn.Open();

            using var tran = conn.BeginTransaction();

            try
            {
                // =====================================================
                // CLIENTES + EMPRÉSTIMOS
                // =====================================================

                for (int i = 2; i <= wsEmp.Dimension.Rows; i++)
                {
                    string nome = wsCli.Cells[i, 1].Text;
                    string cpf = wsCli.Cells[i, 2].Text;

                    if (string.IsNullOrWhiteSpace(cpf))
                        continue;

                    int clienteId = ObterOuCriarCliente(
                        conn,
                        nome,
                        cpf,
                        wsCli.Cells[i, 3].Text,
                        wsCli.Cells[i, 4].Text,
                        wsCli.Cells[i, 5].Text
                    );

                    int emprestimoId = CriarEmprestimo(
                        conn,
                        clienteId,
                        int.Parse(wsEmp.Cells[i, 2].Text),
                        decimal.Parse(wsEmp.Cells[i, 3].Text),
                        int.Parse(wsEmp.Cells[i, 4].Text),
                        wsEmp.Cells[i, 5].Text
                    );

                    // =================================================
                    // PARCELAS DO EMPRÉSTIMO
                    // =================================================

                    for (int p = 2; p <= wsPar.Dimension.Rows; p++)
                    {
                        // Coluna 1 = número do contrato
                        if (wsPar.Cells[p, 1].Text !=
                            wsEmp.Cells[i, 2].Text)
                        {
                            continue;
                        }

                        CriarParcela(
                            conn,
                            emprestimoId,
                            int.Parse(wsPar.Cells[p, 2].Text),
                            decimal.Parse(wsPar.Cells[p, 3].Text),
                            decimal.Parse(wsPar.Cells[p, 4].Text),
                            decimal.Parse(wsPar.Cells[p, 5].Text),
                            DateTime.Parse(wsPar.Cells[p, 6].Text)
                        );
                    }
                }

                tran.Commit();
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        // =============================================================
        // CLIENTES
        // =============================================================

        private void ImportarClientes(
            ExcelWorksheet ws,
            SQLiteConnection conn)
        {
            int rows = ws.Dimension.Rows;

            for (int i = 2; i <= rows; i++)
            {
                string nome = ws.Cells[i, 1].Text;
                string cpf = ws.Cells[i, 2].Text;

                if (string.IsNullOrWhiteSpace(cpf))
                    continue;

                string sql = @"
                    INSERT INTO clientes
                        (nome, cpf)
                    VALUES
                        (@nome, @cpf)
                    ON CONFLICT(cpf) DO NOTHING;
                ";

                using var cmd = new SQLiteCommand(sql, conn);

                cmd.Parameters.AddWithValue("@nome", nome);
                cmd.Parameters.AddWithValue("@cpf", cpf);

                cmd.ExecuteNonQuery();
            }
        }

        // =============================================================
        // OBTER OU CRIAR CLIENTE
        // =============================================================

        private int ObterOuCriarCliente(
            SQLiteConnection conn,
            string nome,
            string cpf,
            string rg,
            string telefone,
            string cidade)
        {
            string select = @"
                SELECT id
                FROM clientes
                WHERE cpf = @cpf;
            ";

            using var check = new SQLiteCommand(select, conn);

            check.Parameters.AddWithValue("@cpf", cpf);

            var result = check.ExecuteScalar();

            if (result != null && result != DBNull.Value)
                return Convert.ToInt32(result);

            string insert = @"
                INSERT INTO clientes
                    (nome, cpf, rg, telefone, cidade)
                VALUES
                    (@nome, @cpf, @rg, @telefone, @cidade);

                SELECT last_insert_rowid();
            ";

            using var cmd = new SQLiteCommand(insert, conn);

            cmd.Parameters.AddWithValue("@nome", nome ?? "");
            cmd.Parameters.AddWithValue("@cpf", cpf ?? "");
            cmd.Parameters.AddWithValue("@rg", rg ?? "");
            cmd.Parameters.AddWithValue("@telefone", telefone ?? "");
            cmd.Parameters.AddWithValue("@cidade", cidade ?? "");

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        // =============================================================
        // EMPRÉSTIMOS
        // =============================================================

        private int CriarEmprestimo(
            SQLiteConnection conn,
            int clienteId,
            int contrato,
            decimal valor,
            int parcelas,
            string tipo)
        {
            string sql = @"
                INSERT INTO emprestimos
                (
                    cliente_id,
                    contrato,
                    valor_contrato,
                    parcelas,
                    tipo,
                    situacao
                )
                VALUES
                (
                    @cliente,
                    @contrato,
                    @valor,
                    @parcelas,
                    @tipo,
                    'Ativo'
                );

                SELECT last_insert_rowid();
            ";

            using var cmd = new SQLiteCommand(sql, conn);

            cmd.Parameters.AddWithValue("@cliente", clienteId);
            cmd.Parameters.AddWithValue("@contrato", contrato);
            cmd.Parameters.AddWithValue("@valor", valor);
            cmd.Parameters.AddWithValue("@parcelas", parcelas);
            cmd.Parameters.AddWithValue("@tipo", tipo ?? "");

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        // =============================================================
        // PARCELAS
        // =============================================================

        private void CriarParcela(
            SQLiteConnection conn,
            int emprestimoId,
            int numero,
            decimal amortizacao,
            decimal juros,
            decimal valor,
            DateTime vencimento)
        {
            string sql = @"
                INSERT INTO parcelas
                (
                    emprestimos_id,
                    n_parcelas,
                    amortizacao,
                    juros,
                    valor_prestacao,
                    vencimento,
                    situacao
                )
                VALUES
                (
                    @emp,
                    @num,
                    @amort,
                    @juros,
                    @valor,
                    @venc,
                    'Aberta'
                );
            ";

            using var cmd = new SQLiteCommand(sql, conn);

            cmd.Parameters.AddWithValue("@emp", emprestimoId);
            cmd.Parameters.AddWithValue("@num", numero);
            cmd.Parameters.AddWithValue("@amort", amortizacao);
            cmd.Parameters.AddWithValue("@juros", juros);
            cmd.Parameters.AddWithValue("@valor", valor);

            // SQLite está usando TEXT para datas
            cmd.Parameters.AddWithValue(
                "@venc",
                vencimento.ToString("yyyy-MM-dd HH:mm:ss")
            );

            cmd.ExecuteNonQuery();
        }
    }
}