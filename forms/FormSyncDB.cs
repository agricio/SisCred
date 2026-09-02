using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using System.Data.SQLite;
using CrudApp.Database;
using CrudApp.Services;

namespace CrudApp.Forms
{
    public class FormSyncDB : Form
    {
        private DataGridView grid;
        private Panel panelGrid;
        private Button btnExportar;
        private Button btnImportar;
        private Button btnExcluir;

        private readonly string pastaSync = @"C:\Speed_Cred_Backup\Sync";

        public FormSyncDB()
        {
            InitializeComponent();
            CarregarArquivos();
        }

        private void InitializeComponent()
        {
            this.Icon = new Icon("app.ico");
            Text = "SiS Cred - Sincronização de Dados";
            Width = 600;
            Height = 420;
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Padding = new Padding(10);

            grid = new DataGridView
            {
                Dock = DockStyle.Top,
                Height = 250,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            btnExportar = new Button
            {
                Text = "Exportar Alterações",
                Width = 170,
                Height = 40,
                Top = 270,
                Left = 30
            };
            btnExportar.Click += BtnExportar_Click;

            btnImportar = new Button
            {
                Text = "Importar Alterações",
                Width = 170,
                Height = 40,
                Top = 270,
                Left = 210
            };
            btnImportar.Click += BtnImportar_Click;

            btnExcluir = new Button
            {
                Text = "Excluir Arquivo",
                Width = 170,
                Height = 40,
                Top = 270,
                Left = 390
            };
            btnExcluir.Click += BtnExcluir_Click;

            Controls.Add(grid);
            Controls.Add(btnExportar);
            Controls.Add(btnImportar);
            Controls.Add(btnExcluir);
        }

        private void CarregarArquivos()
        {
            if (!Directory.Exists(pastaSync))
                Directory.CreateDirectory(pastaSync);

            grid.Columns.Clear();
            grid.Columns.Add("arquivo", "Arquivo");
            grid.Columns.Add("data", "Data");
            grid.Columns.Add("tamanho", "Tamanho");
            

            grid.Rows.Clear();

            var arquivos = Directory.GetFiles(pastaSync, "*.sync")
                .OrderByDescending(File.GetCreationTime);

            foreach (var file in arquivos)
            {
                FileInfo info = new FileInfo(file);

                grid.Rows.Add(
                    info.Name,
                    info.CreationTime.ToString("dd/MM/yyyy HH:mm"),
                    $"{(info.Length / 1024.0 / 1024.0):0.00} MB"
                    
                );
            }
        }

        private void BtnExportar_Click(object sender, EventArgs e)
        {
            try
            {
                using var conexao = Database.Database.GetConnection();
                conexao.Open();

                string arquivo = Path.Combine(
                    pastaSync,
                    $"Sync_{Session.CurrentUsername}_{Environment.MachineName}_{DateTime.Now:dd-MM-yyyy_HH-mm-ss}.sync"
                );

                using var cmd = new SQLiteCommand(
                    "SELECT id,tabela,operacao,dados FROM sync_log WHERE sincronizado=false ORDER BY id",
                    conexao);

                using var reader = cmd.ExecuteReader();
                using var sw = new StreamWriter(arquivo);

                while (reader.Read())
                {
                    string linha =
                        $"{reader.GetInt32(0)}§{reader.GetString(1)}§{reader.GetString(2)}§{reader.GetValue(3)}";

                    sw.WriteLine(linha);
                }

                reader.Close();

                new SQLiteCommand(
                    "UPDATE sync_log SET sincronizado=true WHERE sincronizado=false",
                    conexao).ExecuteNonQuery();

                MessageBox.Show("Alterações exportadas com sucesso!");

                CarregarArquivos();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro: " + ex.Message);
            }
        }

        private void BtnImportar_Click(object sender, EventArgs e)
        {
            using OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Sync (*.sync)|*.sync";

            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                using var conexao = Database.Database.GetConnection();
                conexao.Open();

                foreach (var linha in File.ReadAllLines(ofd.FileName))
                {
                    var partes = linha.Split('§');

                    string tabela = partes[1];
                    string operacao = partes[2];
                    string dados = partes[3];

                    SyncProcessor.Aplicar(conexao, tabela, operacao, dados);

                    using var cmd = new SQLiteCommand(
                        @"INSERT INTO sync_log (tabela,operacao,dados,sincronizado)
                          VALUES (@t,@o,@d::jsonb,true)",
                        conexao);

                    cmd.Parameters.AddWithValue("t", tabela);
                    cmd.Parameters.AddWithValue("o", operacao);
                    cmd.Parameters.AddWithValue("d", dados);

                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Alterações sincronizadas!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao importar: " + ex.Message);
            }
        }

        private void BtnExcluir_Click(object sender, EventArgs e)
        {
            if (grid.SelectedRows.Count == 0)
                return;

            string arquivo = grid.SelectedRows[0].Cells[0].Value.ToString();
            string caminho = Path.Combine(pastaSync, arquivo);

            if (MessageBox.Show(
                "Deseja excluir este arquivo?",
                "Confirmar",
                MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;

            File.Delete(caminho);

            CarregarArquivos();
        }
    }
}