using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using System.Threading.Tasks;
using CrudApp.Services;
using System.Data.SQLite;
using CrudApp.Database;

namespace CrudApp.Forms
{
    public class FormBackup : Form
    {
        private DataGridView grid;
        private Button btnBackup;
        private Button btnSelecionarArquivo;
        private Button btnRestaurar;
        private Button btnExcluir;
        private Button btnExportarSync;
        private Button btnImportarSync;
        private Label lblInfo, lblProgress;
        private Panel panelGrid;
        private ProgressBar progressBar;

        private readonly string pastaBackups = @"C:\SiS_Cred_Backup";
        private readonly string pastaSupport = @"C:\SiS_Cred\Support";

        public FormBackup()
        {
            InitializeComponent();
            CarregarBackups();
        }

        private void InitializeComponent()
        {
            this.Icon = new Icon("app.ico");

            Text = "SiS Cred - Backup Restauração do Sistema";
            Width = 650;
            Height = 500;
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            panelGrid = new Panel
            {
                Dock = DockStyle.Top,
                Height = 260,
                Padding = new Padding(10)
            };

            grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            btnBackup = new Button
            {
                Text = "Fazer Backup",
                Width = 180,
                Height = 40,
                Top = 280,
                Left = 30
            };
            btnBackup.Click += BtnBackup_Click;

            btnRestaurar = new Button
            {
                Text = "Restaurar",
                Width = 180,
                Height = 40,
                Top = 280,
                Left = 235
            };
            btnRestaurar.Click += BtnRestaurar_Click;

            btnSelecionarArquivo = new Button
            {
                Text = "Selecionar Backup",
                Width = 180,
                Height = 40,
                Top = 330,
                Left = 235
            };
            btnSelecionarArquivo.Click += BtnSelecionarArquivo_Click;

            btnExcluir = new Button
            {
                Text = "Excluir",
                Width = 180,
                Height = 40,
                Top = 280,
                Left = 440
            };
            btnExcluir.Click += BtnExcluir_Click;

            btnExportarSync = new Button
            {
                Text = "Exportar Alterações",
                Width = 180,
                Height = 40,
                Top = 330,
                Left = 30
            };
            btnExportarSync.Click += BtnExportarSync_Click;

            btnImportarSync = new Button
            {
                Text = "Importar Alterações",
                Width = 180,
                Height = 40,
                Top = 330,
                Left = 440
            };
            btnImportarSync.Click += BtnImportarSync_Click;

            lblInfo = new Label
            {
                Text = "⚠ Ao restaurar, o sistema será Reiniciado automaticamente.",
                Dock = DockStyle.Bottom,
                Height = 30,
                TextAlign = ContentAlignment.MiddleCenter
            };

            progressBar = new ProgressBar
            {
                Left = 20,
                Top = 385,
                Width = 590,
                Height = 18,
                Minimum = 0,
                Maximum = 100,
                Visible = false
            };

            lblProgress = new Label
            {
                Left = 30,
                Top = 400,
                Width = 590,
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = false
            };

            panelGrid.Controls.Add(grid);

            Controls.Add(panelGrid);
            Controls.Add(btnBackup);
            Controls.Add(btnRestaurar);
            Controls.Add(btnExcluir);
            Controls.Add(btnSelecionarArquivo);
            Controls.Add(lblInfo);
            Controls.Add(progressBar);
            Controls.Add(lblProgress);
        }

        private void CarregarBackups()
        {
            if (!Directory.Exists(pastaBackups))
                Directory.CreateDirectory(pastaBackups);

            grid.Columns.Clear();

            grid.Columns.Add("data", "Data");
            grid.Columns.Add("tamanho", "Tamanho");
            grid.Columns.Add("arquivo", "Arquivo");

            grid.Rows.Clear();

            var arquivos = Directory.GetFiles(pastaBackups, "*.zip")
                .OrderByDescending(File.GetCreationTime);

            foreach (var file in arquivos)
            {
                FileInfo info = new FileInfo(file);

                grid.Rows.Add(
                    info.CreationTime.ToString("dd/MM/yyyy HH:mm"),
                    $"{info.Length / 1024 / 1024} MB",
                    info.Name
                );
            }
        }

        private void BtnBackup_Click(object sender, EventArgs e)
        {
            try
            {
                string nomeBackup =
                    $"Backup_{Environment.MachineName}_{Session.CurrentUsername}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}";

                string pastaTemp = Path.Combine(pastaBackups, nomeBackup);

                Directory.CreateDirectory(pastaTemp);

                // ============================================================
                // OBTÉM O CAMINHO DO SQLITE ATRAVÉS DA CONEXÃO EXISTENTE
                // ============================================================

                string bancoOrigem;

                using (var conexao = Database.Database.GetConnection())
                {
                    var builder = new SQLiteConnectionStringBuilder(
                        conexao.ConnectionString
                    );

                    bancoOrigem = builder.DataSource;

                    if (!Path.IsPathRooted(bancoOrigem))
                    {
                        bancoOrigem = Path.GetFullPath(bancoOrigem);
                    }
                }

                if (!File.Exists(bancoOrigem))
                    throw new Exception(
                        "Banco de dados SQLite não encontrado:\n" + bancoOrigem
                    );

                // ============================================================
                // BACKUP DO BANCO SQLITE
                // ============================================================

                string bancoBackup = Path.Combine(
                    pastaTemp,
                    Path.GetFileName(bancoOrigem)
                );

                File.Copy(
                    bancoOrigem,
                    bancoBackup,
                    true
                );

                // ============================================================
                // SALVA INFORMAÇÕES DO BACKUP
                // ============================================================

                File.WriteAllText(
                    Path.Combine(pastaTemp, "info.txt"),
                    $"Usuário: {Session.CurrentUsername}\n" +
                    $"Computador: {Environment.MachineName}\n" +
                    $"Data: {DateTime.Now}"
                );

                // ============================================================
                // FOTOS
                // ============================================================

                string fotosOrigem = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Fotos"
                );

                if (Directory.Exists(fotosOrigem))
                {
                    CopiarDiretorio(
                        fotosOrigem,
                        Path.Combine(pastaTemp, "Fotos")
                    );
                }

                // ============================================================
                // ZIP
                // ============================================================

                string zipPath = pastaTemp + ".zip";

                ZipFile.CreateFromDirectory(
                    pastaTemp,
                    zipPath
                );

                Directory.Delete(
                    pastaTemp,
                    true
                );

                MessageBox.Show(
                    "✅ Backup criado com sucesso!"
                );

                CarregarBackups();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "❌ Erro no backup:\n" + ex.Message
                );
            }
        }

        private async void BtnRestaurar_Click(object sender, EventArgs e)
        {
            if (grid.SelectedRows.Count == 0)
                return;

            if (MessageBox.Show(
                "Deseja restaurar este backup?\nO sistema será reiniciado.",
                "Confirmação",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            ) != DialogResult.Yes)
                return;

            btnBackup.Enabled =
                btnRestaurar.Enabled =
                btnExcluir.Enabled =
                btnSelecionarArquivo.Enabled = false;

            progressBar.Visible =
                lblProgress.Visible = true;

            progressBar.Value = 0;

            lblProgress.Text =
                "Iniciando restauração...";

            try
            {
                string caminhoZip = Path.Combine(
                    pastaBackups,
                    grid.SelectedRows[0].Cells[2].Value.ToString()
                );

                string temp = Path.Combine(
                    Path.GetTempPath(),
                    "RestoreCrudApp"
                );

                if (Directory.Exists(temp))
                    Directory.Delete(temp, true);

                ZipFile.ExtractToDirectory(
                    caminhoZip,
                    temp
                );

                // ============================================================
                // LOCALIZA O BANCO SQLITE NO BACKUP
                // ============================================================

                string bancoBackup = Directory
                    .GetFiles(temp, "*.db", SearchOption.AllDirectories)
                    .FirstOrDefault();

                if (string.IsNullOrEmpty(bancoBackup))
                {
                    throw new Exception(
                        "Banco SQLite não encontrado no backup."
                    );
                }

                int total = 100;

                await Task.Run(() =>
                {
                    // ========================================================
                    // OBTÉM O CAMINHO DO BANCO ATUAL ATRAVÉS DO GetConnection()
                    // ========================================================

                    string bancoAtual;

                    using (var conexao = Database.Database.GetConnection())
                    {
                        var builder = new SQLiteConnectionStringBuilder(
                            conexao.ConnectionString
                        );

                        bancoAtual = builder.DataSource;

                        if (!Path.IsPathRooted(bancoAtual))
                        {
                            bancoAtual = Path.GetFullPath(bancoAtual);
                        }
                    }

                    // ========================================================
                    // LIMPA POOLS DO SQLITE
                    // ========================================================

                    SQLiteConnection.ClearAllPools();

                    // Pequena espera para garantir liberação dos arquivos
                    System.Threading.Thread.Sleep(500);

                    // ========================================================
                    // FAZ BACKUP DO BANCO ATUAL ANTES DA RESTAURAÇÃO
                    // ========================================================

                    if (File.Exists(bancoAtual))
                    {
                        string bancoAnterior = bancoAtual + ".antes_restore";

                        try
                        {
                            File.Copy(
                                bancoAtual,
                                bancoAnterior,
                                true
                            );
                        }
                        catch
                        {
                            // Não interrompe a restauração
                        }
                    }

                    // ========================================================
                    // SUBSTITUI O BANCO ATUAL PELO BANCO DO BACKUP
                    // ========================================================

                    File.Copy(
                        bancoBackup,
                        bancoAtual,
                        true
                    );

                    // ========================================================
                    // RESTAURA FOTOS
                    // ========================================================

                    string fotosBackup = Path.Combine(
                        temp,
                        "Fotos"
                    );

                    string fotosDestino = Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        "Fotos"
                    );

                    if (Directory.Exists(fotosBackup))
                    {
                        if (Directory.Exists(fotosDestino))
                        {
                            Directory.Delete(
                                fotosDestino,
                                true
                            );
                        }

                        CopiarDiretorio(
                            fotosBackup,
                            fotosDestino
                        );
                    }

                    Invoke(new Action(() =>
                    {
                        progressBar.Value = total;
                        lblProgress.Text =
                            "Restauração concluída...";
                    }));
                });

                MessageBox.Show(
                    "♻ Backup restaurado com sucesso.\nO sistema será reiniciado."
                );

                Application.Restart();
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "❌ Erro ao restaurar:\n" + ex.Message
                );
            }
            finally
            {
                btnBackup.Enabled =
                    btnRestaurar.Enabled =
                    btnExcluir.Enabled =
                    btnSelecionarArquivo.Enabled = true;

                progressBar.Visible =
                    lblProgress.Visible = false;
            }
        }

        private void BtnExcluir_Click(object sender, EventArgs e)
        {
            if (grid.SelectedRows.Count == 0)
                return;

            string arquivo =
                grid.SelectedRows[0].Cells[2].Value.ToString();

            string caminho =
                Path.Combine(pastaBackups, arquivo);

            if (MessageBox.Show(
                "Deseja excluir este backup?",
                "Confirmar",
                MessageBoxButtons.YesNo
            ) != DialogResult.Yes)
                return;

            File.Delete(caminho);

            CarregarBackups();
        }

        private void CopiarDiretorio(
            string origem,
            string destino)
        {
            Directory.CreateDirectory(destino);

            foreach (string arquivo in Directory.GetFiles(origem))
            {
                File.Copy(
                    arquivo,
                    Path.Combine(
                        destino,
                        Path.GetFileName(arquivo)
                    ),
                    true
                );
            }

            foreach (string dir in Directory.GetDirectories(origem))
            {
                CopiarDiretorio(
                    dir,
                    Path.Combine(
                        destino,
                        Path.GetFileName(dir)
                    )
                );
            }
        }

        private void BtnSelecionarArquivo_Click(
            object sender,
            EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title =
                    "Selecionar arquivo de backup";

                ofd.Filter =
                    "Backup (*.zip)|*.zip";

                ofd.Multiselect = false;

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    RestaurarBackupPorArquivo(
                        ofd.FileName
                    );
                }
            }
        }

        private async void RestaurarBackupPorArquivo(
            string caminhoZip)
        {
            if (!File.Exists(caminhoZip))
            {
                MessageBox.Show(
                    "Arquivo não encontrado."
                );

                return;
            }

            if (MessageBox.Show(
                "Deseja restaurar este backup?\nO sistema será reiniciado.",
                "Confirmação",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            ) != DialogResult.Yes)
                return;

            btnBackup.Enabled =
                btnRestaurar.Enabled =
                btnExcluir.Enabled =
                btnSelecionarArquivo.Enabled = false;

            progressBar.Visible =
                lblProgress.Visible = true;

            progressBar.Value = 0;

            lblProgress.Text =
                "Iniciando restauração...";

            try
            {
                string temp = Path.Combine(
                    Path.GetTempPath(),
                    "RestoreCrudApp"
                );

                if (Directory.Exists(temp))
                    Directory.Delete(temp, true);

                ZipFile.ExtractToDirectory(
                    caminhoZip,
                    temp
                );

                // ============================================================
                // LOCALIZA O BANCO SQLITE
                // ============================================================

                string bancoBackup = Directory
                    .GetFiles(
                        temp,
                        "*.db",
                        SearchOption.AllDirectories
                    )
                    .FirstOrDefault();

                if (string.IsNullOrEmpty(bancoBackup))
                {
                    throw new Exception(
                        "Banco SQLite não encontrado no backup."
                    );
                }

                await Task.Run(() =>
                {
                    // ========================================================
                    // OBTÉM O CAMINHO DO BANCO ATUAL
                    // ========================================================

                    string bancoAtual;

                    using (var conexao = Database.Database.GetConnection())
                    {
                        var builder =
                            new SQLiteConnectionStringBuilder(
                                conexao.ConnectionString
                            );

                        bancoAtual = builder.DataSource;

                        if (!Path.IsPathRooted(bancoAtual))
                        {
                            bancoAtual =
                                Path.GetFullPath(bancoAtual);
                        }
                    }

                    // ========================================================
                    // LIBERA CONEXÕES SQLITE
                    // ========================================================

                    SQLiteConnection.ClearAllPools();

                    System.Threading.Thread.Sleep(500);

                    // ========================================================
                    // RESTAURA BANCO
                    // ========================================================

                    File.Copy(
                        bancoBackup,
                        bancoAtual,
                        true
                    );

                    // ========================================================
                    // RESTAURA FOTOS
                    // ========================================================

                    string fotosBackup =
                        Path.Combine(temp, "Fotos");

                    string fotosDestino =
                        Path.Combine(
                            AppDomain.CurrentDomain.BaseDirectory,
                            "Fotos"
                        );

                    if (Directory.Exists(fotosBackup))
                    {
                        if (Directory.Exists(fotosDestino))
                        {
                            Directory.Delete(
                                fotosDestino,
                                true
                            );
                        }

                        CopiarDiretorio(
                            fotosBackup,
                            fotosDestino
                        );
                    }

                    Invoke(new Action(() =>
                    {
                        progressBar.Value = 100;

                        lblProgress.Text =
                            "Restauração concluída...";
                    }));
                });

                MessageBox.Show(
                    "♻ Backup restaurado com sucesso.\nO sistema será reiniciado."
                );

                Application.Restart();
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "❌ Erro ao restaurar:\n" + ex.Message
                );
            }
            finally
            {
                btnBackup.Enabled =
                    btnRestaurar.Enabled =
                    btnExcluir.Enabled =
                    btnSelecionarArquivo.Enabled = true;

                progressBar.Visible =
                    lblProgress.Visible = false;
            }
        }

        private void BtnExportarSync_Click(
            object sender,
            EventArgs e)
        {
            try
            {
                using var conexao =
                    Database.Database.GetConnection();

                conexao.Open();

                string arquivo = Path.Combine(
                    pastaBackups,
                    $"Sync_{Session.CurrentUsername}_{DateTime.Now:yyyyMMddHHmmss}.sync"
                );

                using var cmd = new SQLiteCommand(
                    @"SELECT id, tabela, operacao, dados
                      FROM sync_log
                      WHERE sincronizado = 0
                      ORDER BY id",
                    conexao
                );

                using var reader =
                    cmd.ExecuteReader();

                using var sw =
                    new StreamWriter(arquivo);

                while (reader.Read())
                {
                    string linha =
                        $"{reader.GetInt32(0)}§" +
                        $"{reader.GetString(1)}§" +
                        $"{reader.GetString(2)}§" +
                        $"{reader.GetValue(3)}";

                    sw.WriteLine(linha);
                }

                reader.Close();

                using var cmdUpdate =
                    new SQLiteCommand(
                        @"UPDATE sync_log
                          SET sincronizado = 1
                          WHERE sincronizado = 0",
                        conexao
                    );

                cmdUpdate.ExecuteNonQuery();

                MessageBox.Show(
                    "✅ Alterações exportadas com sucesso!"
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Erro ao exportar: " + ex.Message
                );
            }
        }

        private void BtnImportarSync_Click(
            object sender,
            EventArgs e)
        {
            using OpenFileDialog ofd =
                new OpenFileDialog();

            ofd.Filter =
                "Sync (*.sync)|*.sync";

            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                using var conexao =
                    Database.Database.GetConnection();

                conexao.Open();

                foreach (var linha in File.ReadAllLines(
                    ofd.FileName))
                {
                    var partes =
                        linha.Split('§');

                    if (partes.Length < 4)
                        continue;

                    int id =
                        int.Parse(partes[0]);

                    string tabela =
                        partes[1];

                    string operacao =
                        partes[2];

                    string dados =
                        partes[3];

                    // ========================================================
                    // APLICA ALTERAÇÃO
                    // ========================================================

                    SyncProcessor.Aplicar(
                        conexao,
                        tabela,
                        operacao,
                        dados
                    );

                    // ========================================================
                    // SALVA NO LOG
                    // SQLite utiliza TEXT para JSON
                    // ========================================================

                    using var cmd =
                        new SQLiteCommand(
                            @"INSERT INTO sync_log
                              (
                                  tabela,
                                  operacao,
                                  dados,
                                  sincronizado
                              )
                              VALUES
                              (
                                  @t,
                                  @o,
                                  @d,
                                  1
                              )",
                            conexao
                        );

                    cmd.Parameters.AddWithValue(
                        "@t",
                        tabela
                    );

                    cmd.Parameters.AddWithValue(
                        "@o",
                        operacao
                    );

                    cmd.Parameters.AddWithValue(
                        "@d",
                        dados
                    );

                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show(
                    "✅ Alterações sincronizadas!"
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Erro ao importar: " + ex.Message
                );
            }
        }
    }
}
