using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CrudApp.Repositories;
using CrudApp.Services;

namespace CrudApp.Forms
{
    public class FormDashboardUser : Form
    {
        private readonly ParcelaRepository parcelaRepo = new ParcelaRepository();
        public FormDashboardUser()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
        }

        private void InitializeComponent()
        {
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.AutoScaleMode = AutoScaleMode.None;
            this.AutoSize = false;

            Text = $"SiS Cred de Gerenciamento de Crédito - Dashboard - Usuario: {Session.CurrentUsername}";
            ClientSize = new Size(1000, 650);
            BackColor = Color.FromArgb(240, 242, 245);

            Panel spacer = new Panel
            {
                Dock = DockStyle.Top,
                Height = 10
            };

            // ===== MENU STRIP =====
            this.Icon = new Icon("app.ico");
            MenuStrip menu = new MenuStrip();
            menu.Dock = DockStyle.Top;

            ToolStripMenuItem menuClientes = new ToolStripMenuItem("Clientes");
            menuClientes.DropDownItems.Add("Novo Cliente", null, (s, e) => new FormNovoCliente().Show());
            menuClientes.DropDownItems.Add("Lista de Clientes", null, (s, e) => new FormMain().Show());

            ToolStripMenuItem menuServicos = new ToolStripMenuItem("Serviços");
            menuServicos.DropDownItems.Add("Novo Serviço", null, (s, e) => new FormCadastroServico().Show());
            menuServicos.DropDownItems.Add("Lista de Serviços", null, (s, e) => new FormListaServicos().Show()); 

            ToolStripMenuItem menuFerramentas = new ToolStripMenuItem("Ferramentas");
            menuFerramentas.DropDownItems.Add("Simulador de Contrato e Parcelas", null, (s, e) => new FormCalcularParcelas().Show());
            menuFerramentas.DropDownItems.Add("Calculadora Valor Liberado", null, (s, e) => new FormFerramentaLiberado().Show());
            menuFerramentas.DropDownItems.Add("Calculadora Parcela Em Atraso", null, (s, e) => new FormFerramentaCalcularAtraso().Show());
            menuFerramentas.DropDownItems.Add(new ToolStripSeparator());
            menuFerramentas.DropDownItems.Add("Restauração e Backup", null, (s, e) => new FormBackup().Show());
            menuFerramentas.DropDownItems.Add("Sincronizar Data Base", null, (s, e) => new FormSyncDB().Show());
            menuFerramentas.DropDownItems.Add("Logout", null, btnLogout_Click);
            menuFerramentas.DropDownItems.Add(new ToolStripSeparator());
            menuFerramentas.DropDownItems.Add("Sobre", null, (s, e) => new FormAbout().ShowDialog());

            menu.Items.Add(menuClientes);
            menu.Items.Add(menuServicos);
            menu.Items.Add(menuFerramentas);

            this.MainMenuStrip = menu;
            this.Controls.Add(spacer);
            this.Controls.Add(menu);

   
            // ===== GRID DO DASHBOARD =====
            TableLayoutPanel grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                 // espaço no topo
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(15)
            };

            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            Panel pMes = CriarBlocoEntradasMes();
            Panel pAmortJuros = CriarBlocoAmortizacaoJuros();
            Panel pCaixa = CriarBlocoCaixa();
            Panel pParcelasAtraso  = CriarBlocoVencimentos();

            grid.Controls.Add(pMes, 0, 0);
            grid.Controls.Add(pAmortJuros, 1, 0);
            grid.Controls.Add(pParcelasAtraso , 1, 1);
            grid.Controls.Add(pCaixa, 0, 1);
            
            Controls.Add(grid);
        }

        private Panel CriarBase(string titulo)
        {
            Panel p = new Panel
            {
                BackColor = Color.White,
                Margin = new Padding(8, 20, 8, 8),     //  espaço controlado
                Dock = DockStyle.Fill,        //  deixa o grid mandar
                //BorderStyle = BorderStyle.FixedSingle
            };

            Label lbl = new Label
            {
                Text = titulo,
                Height = 35,
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Padding = new Padding(10, 0, 0, 0),
                TextAlign = ContentAlignment.MiddleLeft
            };

            p.Controls.Add(lbl);
            return p;
        }

        // =============== BLOCO ENTRADAS POR MÊS ===============

        private Panel CriarBlocoEntradasMes()
        {
            Panel basePanel = CriarBase("");

            // painel principal interno
            

            return basePanel;
        }

        // =============== BLOCO AMORTIZAÇÃO E JUROS ===============
        private Panel CriarBlocoAmortizacaoJuros()
        {
            Panel p = CriarBase("");

            return p;
        }

      
        // =============== BLOCO PARCELAS A VENCER ===============
        private Panel CriarBlocoCaixa()
        {
            Panel p = CriarBase("Parcelas A Vencer");

            var parcelaRepo = new ParcelaRepository();
            var emprestimoRepo = new EmprestimoRepository();
            var clienteRepo = new ClienteRepository();

            // ===== FILTROS DE DATA =====
            Label lblInicio = new Label
            {
                Text = "Início:",
                Location = new Point(20, 50),
                AutoSize = true
            };

            DateTimePicker dtInicio = new DateTimePicker
            {
                Location = new Point(70, 45),
                Width = 120,
                Format = DateTimePickerFormat.Short
            };

            Label lblFim = new Label
            {
                Text = "Fim:",
                Location = new Point(210, 50),
                AutoSize = true
            };

            DateTimePicker dtFim = new DateTimePicker
            {
                Location = new Point(250, 45),
                Width = 120,
                Format = DateTimePickerFormat.Short
            };

            Button btnFiltrar = new Button
            {
                Text = "Pesquisar",
                Location = new Point(390, 44),
                Width = 80
            };

            // ===== TOTAL DO CAIXA =====
            Label lblTotal = new Label
            {
                Text = "Total no período: R$ 0,00",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(20, 80),
                AutoSize = true
            };

            // ===== GRID =====
            DataGridView grid = new DataGridView
            {
                Location = new Point(10, 115),
                Size = new Size(760, 300),
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                BorderStyle = BorderStyle.FixedSingle,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            grid.Columns.Add("Cliente", "Cliente");
            grid.Columns.Add("Contrato", "Contrato");
            grid.Columns.Add("Vencimento", "Vencimento");
            grid.Columns.Add("Parcela", "Parcela");
            grid.Columns.Add("Valor", "Valor");
            
            grid.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                    var row = grid.Rows[e.RowIndex];
                if (row.Tag == null) return;
                    int emprestimoId = Convert.ToInt32(row.Tag);
                    new FormEditarEmprestimoUserAreceber(emprestimoId).ShowDialog();
            };

            // ===== AÇÃO FILTRAR =====
            btnFiltrar.Click += (s, e) =>
            {
                grid.Rows.Clear();

                DateTime inicio = dtInicio.Value.Date;
                DateTime fim = dtFim.Value.Date;

                decimal total = 0;

                var parcelasPeriodo = parcelaRepo.GetAll()
                    .Where(p =>
                        p.Situacao != null &&
                        p.Situacao.Equals("A Vencer", StringComparison.OrdinalIgnoreCase) &&
                        p.Vencimento.HasValue &&
                        p.Vencimento.Value.Date >= inicio &&
                        p.Vencimento.Value.Date <= fim
                    )
                    .OrderBy(p => p.Vencimento)
                    .ToList();

                foreach (var parcela in parcelasPeriodo)
                {
                    var emprestimo = emprestimoRepo.GetById(parcela.EmprestimosId);
                    if (emprestimo == null) continue;

                    var cliente = clienteRepo.GetById(emprestimo.ClienteId);

                    total += (decimal)(parcela.ValorPrestacao ?? 0);

                    int rowIndex = grid.Rows.Add(
                        cliente?.Nome ?? "N/D",
                        emprestimo.Contrato,
                        parcela.NParcelas,
                        parcela.Vencimento?.ToString("dd/MM/yyyy"),
                        parcela.ValorPrestacao?.ToString("C2")
                    );

                    // 🔥 GUARDA O ID REAL DO EMPRÉSTIMO
                    grid.Rows[rowIndex].Tag = emprestimo.Id;
                }

                lblTotal.Text = $"Total A Receber No Período: {total:C2}";
            };

            // ===== VALOR PADRÃO: HOJE =====
            dtInicio.Value = DateTime.Today;
            dtFim.Value = DateTime.Today;
            btnFiltrar.PerformClick();

            // ===== ADD CONTROLES =====
            p.Controls.Add(lblInicio);
            p.Controls.Add(dtInicio);
            p.Controls.Add(lblFim);
            p.Controls.Add(dtFim);
            p.Controls.Add(btnFiltrar);
            p.Controls.Add(lblTotal);
            p.Controls.Add(grid);

            return p;
        }

        // =============== BLOCO PARCELAS EM ATRASO ===============
        private Panel CriarBlocoVencimentos()
        {
            Panel p = CriarBase("Parcelas Em Atraso");

            var parcelaRepo = new ParcelaRepository();
            var emprestimoRepo = new EmprestimoRepository();
            var clienteRepo = new ClienteRepository();

            DataGridView grid = new DataGridView
            {
                Location = new Point(10, 45),
                Size = new Size(760, 410),
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                BorderStyle = BorderStyle.FixedSingle,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            grid.ColumnHeadersHeightSizeMode =
                DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.ColumnHeadersHeight = 34;
            grid.RowTemplate.Height = 26;

            // ===== COLUNAS =====
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Cliente", Width = 220 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Contrato", Width = 90 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Parcela", Width = 80 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Vencimento", Width = 120 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Valor da Parcela", Width = 120 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Dias Atrassados", Width = 80 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "EmprestimoId", Visible = false }); //invisivel

            grid.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex < 0) return;

                int emprestimoId = (int)grid.Rows[e.RowIndex].Tag;
                new FormEditarEmprestimoUserAtrasados(emprestimoId).ShowDialog();
            };

            // ===== BUSCA PARCELAS EM ATRASO =====
            var parcelasAtrasadas = parcelaRepo.GetAll()
                .Where(p =>
                    p.Situacao != null &&
                    p.Situacao.Equals("Em Atraso", StringComparison.OrdinalIgnoreCase) &&
                    p.Vencimento.HasValue &&
                    p.Vencimento.Value.Date < DateTime.Today
                )
                .OrderBy(p => p.Vencimento)
                .ToList();

                

            foreach (var parcela in parcelasAtrasadas)
            {
                var emprestimo = emprestimoRepo.GetById(parcela.EmprestimosId);
                if (emprestimo == null) continue;

                var cliente = clienteRepo.GetById(emprestimo.ClienteId);

                int diasAtraso = (DateTime.Today - parcela.Vencimento.Value.Date).Days;

                int rowIndex = grid.Rows.Add(
                    cliente?.Nome ?? "N/D",
                    emprestimo.Contrato,
                    parcela.NParcelas,
                    parcela.Vencimento?.ToString("dd/MM/yyyy"),
                    parcela.ValorPrestacao?.ToString("C2"),
                    diasAtraso // 👈 AGORA SIM
                );

                grid.Rows[rowIndex].Tag = emprestimo.Id; // 🔥 guarda o ID real

                // destaque atraso > 30 dias
                if (diasAtraso > 20)
                    {
                        grid.Rows[rowIndex].DefaultCellStyle.BackColor = Color.MistyRose;
                        grid.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.DarkRed;
                        grid.Rows[rowIndex].DefaultCellStyle.Font =
                            new Font("Segoe UI", 9, FontStyle.Bold);
                    }
                }

                p.Controls.Add(grid);
                return p;
        }
        
        // ===== FAZER LOGOUT DO SISTEMA =====
        private void btnLogout_Click(object sender, EventArgs e)
        {
            Session.Logout();

            Close(); // ⬅️ ISSO É ESSENCIAL

            using (var login = new FormLogin())
            {
                if (login.ShowDialog() == DialogResult.OK)
                {
                    Session.Login(
                        login.LoggedUserId,
                        login.LoggedUsername,
                        login.LoggedUserRole
                    );

                    Form novoDashboard =
                        Session.CurrentUserRole == "admin"
                            ? new FormDashboard()
                            : new FormDashboardUser();

                    novoDashboard.Show();
                }
                else
                {
                        Application.Restart();
                        Environment.Exit(0);
                }
            }
        }
 

    }
}
