using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using CrudApp.Repositories;
using CrudApp.Services;

namespace CrudApp.Forms
{
    public class FormDashboard : Form
    {
        private readonly ParcelaRepository parcelaRepo = new ParcelaRepository();

        public FormDashboard()
        {
            InitializeComponent();

            this.Icon = new Icon("app.ico");
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
        }

        // ============================================================
        // INITIALIZE COMPONENT
        // ============================================================

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Icon = new Icon("app.ico");
            this.AutoScaleMode = AutoScaleMode.None;
            this.AutoSize = false;

            this.Text =
                $"SiS Cred Sistema de Gerenciamento de Crédito - Dashboard - Administrador: {Session.CurrentUsername}";

            this.ClientSize = new Size(1000, 650);
            this.BackColor = Color.FromArgb(240, 242, 245);

            // ========================================================
            // MENU
            // ========================================================

            MenuStrip menu = CriarMenu();

            // ========================================================
            // ÁREA PRINCIPAL
            // ========================================================

            TableLayoutPanel grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(10),
                BackColor = Color.FromArgb(240, 242, 245),
                Margin = new Padding(0)
            };

            grid.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 50F)
            );

            grid.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 50F)
            );

            grid.RowStyles.Add(
                new RowStyle(SizeType.Percent, 50F)
            );

            grid.RowStyles.Add(
                new RowStyle(SizeType.Percent, 50F)
            );

            // ========================================================
            // BLOCOS
            // ========================================================

            Panel pMes = CriarBlocoEntradasMes();
            Panel pAmortJuros = CriarBlocoAmortizacaoJuros();
            Panel pAtraso = CriarBlocoVencimentos();
            Panel pVencer = CriarBlocoCaixa();

            grid.Controls.Add(pMes, 0, 0);
            grid.Controls.Add(pAmortJuros, 1, 0);

            grid.Controls.Add(pAtraso, 0, 1);
            grid.Controls.Add(pVencer, 1, 1);

            // ========================================================
            // CONTROLES PRINCIPAIS
            // ========================================================

            this.Controls.Add(grid);
            this.Controls.Add(menu);

            this.MainMenuStrip = menu;

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        // ============================================================
        // MENU
        // ============================================================

        private MenuStrip CriarMenu()
        {
            MenuStrip menu = new MenuStrip
            {
                Dock = DockStyle.Top,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 9F)
            };

            // ========================================================
            // CLIENTES
            // ========================================================

            ToolStripMenuItem menuClientes =
                new ToolStripMenuItem("Clientes");

            menuClientes.DropDownItems.Add(
                "Novo Cliente",
                null,
                (s, e) => new FormNovoCliente().Show()
            );

            menuClientes.DropDownItems.Add(
                "Lista de Clientes",
                null,
                (s, e) => new FormMain().Show()
            );

            // ========================================================
            // SERVIÇOS
            // ========================================================

            ToolStripMenuItem menuServicos =
                new ToolStripMenuItem("Serviços");

            menuServicos.DropDownItems.Add(
                "Novo Serviço",
                null,
                (s, e) => new FormCadastroServico().Show()
            );

            menuServicos.DropDownItems.Add(
                "Lista de Serviços",
                null,
                (s, e) => new FormListaServicos().Show()
            );

            // ========================================================
            // CONTRATOS
            // ========================================================

            ToolStripMenuItem menuContratos =
                new ToolStripMenuItem("Contratos");

            menuContratos.DropDownItems.Add(
                "Novo Contrato",
                null,
                (s, e) => new FormNovoEmprestimo().Show()
            );

            menuContratos.DropDownItems.Add(
                "Contratos deste ano Corrente",
                null,
                (s, e) => new FormEmprestimosAnoAtual().Show()
            );

            menuContratos.DropDownItems.Add(
                "Contratos Gerais",
                null,
                (s, e) => new FormEmprestimos().Show()
            );

            // ========================================================
            // CAIXA
            // ========================================================

            ToolStripMenuItem menuCaixa =
                new ToolStripMenuItem("Caixa");

            menuCaixa.DropDownItems.Add(
                "Adicionar Despesa",
                null,
                (s, e) => new FormCadastroDespesa().Show()
            );

            menuCaixa.DropDownItems.Add(
                "Despesas Mensais",
                null,
                (s, e) => new FormListaDespesas().Show()
            );

            menuCaixa.DropDownItems.Add(
                new ToolStripSeparator()
            );

            menuCaixa.DropDownItems.Add(
                "Livro Caixa",
                null,
                (s, e) => new FormLivroCaixa().Show()
            );

            // ========================================================
            // FERRAMENTAS
            // ========================================================

            ToolStripMenuItem menuFerramentas =
                new ToolStripMenuItem("Ferramentas");

            menuFerramentas.DropDownItems.Add(
                "Gerenciamento de Usuários",
                null,
                (s, e) => new FormUsers().Show()
            );

            menuFerramentas.DropDownItems.Add(
                new ToolStripSeparator()
            );

            menuFerramentas.DropDownItems.Add(
                "Simulador de Contrato e Parcelas",
                null,
                (s, e) => new FormCalcularParcelas().Show()
            );

            menuFerramentas.DropDownItems.Add(
                "Calculadora Valor Liberado",
                null,
                (s, e) => new FormFerramentaLiberado().Show()
            );

            menuFerramentas.DropDownItems.Add(
                "Calculadora Parcela Em Atraso",
                null,
                (s, e) => new FormFerramentaCalcularAtraso().Show()
            );

            menuFerramentas.DropDownItems.Add(
                new ToolStripSeparator()
            );

            menuFerramentas.DropDownItems.Add(
                "Restauração e Backup",
                null,
                (s, e) => new FormBackup().Show()
            );

            menuFerramentas.DropDownItems.Add(
                "Sincronizar Data Base",
                null,
                (s, e) => new FormSyncDB().Show()
            );

            menuFerramentas.DropDownItems.Add(
                new ToolStripSeparator()
            );

            menuFerramentas.DropDownItems.Add(
                "Logout",
                null,
                btnLogout_Click
            );

            menuFerramentas.DropDownItems.Add(
                "Sobre",
                null,
                (s, e) => new FormAbout().ShowDialog()
            );

            menu.Items.Add(menuClientes);
            menu.Items.Add(menuServicos);
            menu.Items.Add(menuContratos);
            menu.Items.Add(menuCaixa);
            menu.Items.Add(menuFerramentas);

            return menu;
        }

        // ============================================================
        // BASE DOS BLOCOS
        // ============================================================

        private Panel CriarBase(string titulo, out Panel conteudo)
        {
            Panel card = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(5),
                BackColor = Color.White,
                Padding = new Padding(1)
            };

            // --------------------------------------------------------
            // CABEÇALHO
            // --------------------------------------------------------

            Panel header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 34,
                BackColor = Color.FromArgb(248, 249, 250)
            };

            Label lblTitulo = new Label
            {
                Text = titulo,
                Dock = DockStyle.Fill,
                Font = new Font(
                    "Segoe UI",
                    9F,
                    FontStyle.Bold
                ),
                ForeColor = Color.FromArgb(35, 35, 35),
                Padding = new Padding(10, 0, 0, 0),
                TextAlign = ContentAlignment.MiddleLeft
            };

            header.Controls.Add(lblTitulo);

            // --------------------------------------------------------
            // CONTEÚDO
            // --------------------------------------------------------

            conteudo = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(8)
            };

            card.Controls.Add(conteudo);
            card.Controls.Add(header);

            return card;
        }

        // ============================================================
        // ENTRADAS POR MÊS
        // ============================================================

        private Panel CriarBlocoEntradasMes()
        {
            Panel basePanel;
            Panel conteudo;

            basePanel = CriarBase(
                "Entradas por Mês",
                out conteudo
            );

            // --------------------------------------------------------
            // LAYOUT
            // --------------------------------------------------------

            TableLayoutPanel layout =
                new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 2,
                    RowCount = 1,
                    BackColor = Color.White,
                    Margin = new Padding(0),
                    Padding = new Padding(0)
                };

            layout.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    78F
                )
            );

            layout.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    22F
                )
            );

            // --------------------------------------------------------
            // GRÁFICO
            // --------------------------------------------------------

            Chart chart = new Chart
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            ChartArea area = new ChartArea();

            area.AxisX.LabelStyle.Format = "0.00";
            area.AxisX.Interval = 1;

            area.BackColor = Color.White;

            area.AxisX.MajorGrid.Enabled = false;
            area.AxisX.LineColor = Color.LightGray;
            area.AxisX.LabelStyle.Font =
                new Font("Segoe UI", 8F, FontStyle.Bold);

            area.AxisY.MajorGrid.LineColor =
                Color.Gainsboro;

            area.AxisY.LineColor =
                Color.LightGray;

            area.AxisY.LabelStyle.Font =
                new Font("Segoe UI", 8F);

            area.AxisY.LabelStyle.Format =
                "N2";

            chart.ChartAreas.Add(area);

            Legend legend = new Legend
            {
                Docking = Docking.Bottom,
                Alignment = StringAlignment.Center,
                Font = new Font("Segoe UI", 8F)
            };

            chart.Legends.Add(legend);

            Series sValor = new Series("Parcelas")
            {
                ChartType = SeriesChartType.Column,
                LegendText = "Parcelas"
            };

            Series sJuros = new Series("Juros")
            {
                ChartType = SeriesChartType.Column,
                LegendText = "Juros"
            };

            Series sAmort = new Series("Amortização")
            {
                ChartType = SeriesChartType.Column,
                LegendText = "Amortização"
            };

            sValor["PointWidth"] = "0.65";
            sJuros["PointWidth"] = "0.65";
            sAmort["PointWidth"] = "0.65";

            sValor.Color =
                Color.FromArgb(52, 152, 219);

            sJuros.Color =
                Color.FromArgb(231, 76, 60);

            sAmort.Color =
                Color.FromArgb(46, 204, 113);

            chart.Series.Add(sValor);
            chart.Series.Add(sJuros);
            chart.Series.Add(sAmort);

            // --------------------------------------------------------
            // PAINEL LATERAL
            // --------------------------------------------------------

            Panel lateral = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(8, 5, 5, 5),
                BackColor = Color.White
            };

            Label lblAno = new Label
            {
                Text = "Ano:",
                Location = new Point(5, 5),
                AutoSize = true,
                Font = new Font("Segoe UI", 8F),
                ForeColor = Color.FromArgb(40, 40, 40)
            };

            NumericUpDown numAno = new NumericUpDown
            {
                Minimum = 2000,
                Maximum = 2100,
                Value = DateTime.Now.Year,
                Location = new Point(5, 28),
                Width = 75,
                Height = 23,
                Font = new Font("Segoe UI", 8F)
            };

            Label lblTotalParcelas = new Label
            {
                AutoSize = true,
                Location = new Point(5, 75),
                Font = new Font( "Segoe UI", 8F, FontStyle.Bold ),
                ForeColor = Color.Gray
            };

            Label lblTotalJuros = new Label
            {
                AutoSize = true,
                Location = new Point(5, 94),
                Font = new Font( "Segoe UI", 8F, FontStyle.Bold ),
                ForeColor = Color.Gray
            };

            Label lblTotalAmort = new Label
            {
                AutoSize = true,
                Location = new Point(5, 113),
                Font = new Font( "Segoe UI", 8F, FontStyle.Bold ),
                ForeColor = Color.Gray
            };

            lateral.Controls.Add(lblAno);
            lateral.Controls.Add(numAno);
            lateral.Controls.Add(lblTotalParcelas);
            lateral.Controls.Add(lblTotalJuros);
            lateral.Controls.Add(lblTotalAmort);

            layout.Controls.Add(chart, 0, 0);
            layout.Controls.Add(lateral, 1, 0);

            conteudo.Controls.Add(layout);

            // --------------------------------------------------------
            // RECALCULAR
            // --------------------------------------------------------

            void Recalcular(int ano)
            {
                sValor.Points.Clear();
                sJuros.Points.Clear();
                sAmort.Points.Clear();

                var parcelas = parcelaRepo
                    .GetAll()
                    .Where(p =>
                        p.Pagamento.HasValue &&
                        p.Pagamento.Value.Year == ano &&
                        (
                            p.Situacao.Equals(
                                "Paga",
                                StringComparison.OrdinalIgnoreCase
                            )
                            ||
                            p.Situacao.Equals(
                                "Paga em Atraso",
                                StringComparison.OrdinalIgnoreCase
                            )
                            ||
                            p.Situacao.Equals(
                                "Paga Antecipado",
                                StringComparison.OrdinalIgnoreCase
                            )
                        )
                    )
                    .ToList();

                var porMes = parcelas
                    .GroupBy(p => p.Pagamento!.Value.Month)
                    .Select(g => new
                    {
                        Mes = g.Key,

                        Total = g.Sum(
                            x => x.ValorPrestacao ?? 0
                        ),

                        Juros = g.Sum(
                            x => x.Juros ?? 0
                        ),

                        Amort = g.Sum(
                            x => x.Amortizacao ?? 0
                        )
                    })
                    .ToDictionary(x => x.Mes);

                string[] meses =
                {
                    "Jan",
                    "Fev",
                    "Mar",
                    "Abr",
                    "Mai",
                    "Jun",
                    "Jul",
                    "Ago",
                    "Set",
                    "Out",
                    "Nov",
                    "Dez"
                };

                double totalParcelas = 0;
                double totalJuros = 0;
                double totalAmort = 0;

                for (int mes = 1; mes <= 12; mes++)
                {
                    double total = 0;
                    double juros = 0;
                    double amort = 0;

                    if (porMes.TryGetValue(mes, out var d))
                    {
                        total = d.Total;
                        juros = d.Juros;
                        amort = d.Amort;

                        totalParcelas += total;
                        totalJuros += juros;
                        totalAmort += amort;
                    }

                        sValor.Points.AddXY(
                            meses[mes - 1],
                            total
                        );

                        sJuros.Points.AddXY(
                            meses[mes - 1],
                            juros
                        );

                        sAmort.Points.AddXY(
                            meses[mes - 1],
                            amort
                        );

                        if (total > 0)
                        {
                            sValor.Points[mes - 1].Label =
                                total.ToString("C2");
                        }

                        if (juros > 0)
                        {
                            sJuros.Points[mes - 1].Label =
                                juros.ToString("C2");
                        }

                        if (amort > 0)
                        {
                        sAmort.Points[mes - 1].Label =
                            amort.ToString("C2");
                    }
                }

                double maxValor = new[]
                {
                    sValor.Points
                        .Select(p => p.YValues[0])
                        .DefaultIfEmpty(0)
                        .Max(),

                    sJuros.Points
                        .Select(p => p.YValues[0])
                        .DefaultIfEmpty(0)
                        .Max(),

                    sAmort.Points
                        .Select(p => p.YValues[0])
                        .DefaultIfEmpty(0)
                        .Max()
                }.Max();

                area.AxisY.Minimum = 0;

                if (maxValor <= 0)
                    area.AxisY.Maximum = 10;
                else
                    area.AxisY.Maximum = maxValor * 1.25;

                lblTotalParcelas.Text =
                    $"Total Parcelas: {totalParcelas:C2}";

                lblTotalJuros.Text =
                    $"Total Juros: {totalJuros:C2}";

                lblTotalAmort.Text =
                    $"Total Amortização: {totalAmort:C2}";
            }

            numAno.ValueChanged += (s, e) =>
            {
                Recalcular((int)numAno.Value);
            };

            Recalcular(DateTime.Now.Year);

            return basePanel;
        }

        // ============================================================
        // AMORTIZAÇÃO X JUROS
        // ============================================================

        private Panel CriarBlocoAmortizacaoJuros()
        {
            Panel basePanel;
            Panel conteudo;

            basePanel = CriarBase(
                "Amortização x Juros",
                out conteudo
            );

            // --------------------------------------------------------
            // TOPO
            // --------------------------------------------------------

            Panel topo = new Panel
            {
                Dock = DockStyle.Top,
                Height = 34,
                BackColor = Color.White
            };

            Label lblAno = new Label
            {
                Text = "Ano:",
                Location = new Point(5, 8),
                AutoSize = true,
                Font = new Font("Segoe UI", 8F)
            };

            NumericUpDown numAno = new NumericUpDown
            {
                Minimum = 2000,
                Maximum = 2100,
                Value = DateTime.Now.Year,
                Location = new Point(45, 5),
                Width = 75,
                Height = 23,
                Font = new Font("Segoe UI", 8F)
            };

            topo.Controls.Add(lblAno);
            topo.Controls.Add(numAno);

            // --------------------------------------------------------
            // GRÁFICO
            // --------------------------------------------------------

            var chart = new Chart
                {
                    Location = new Point(7, 60),
                    Anchor = AnchorStyles.Top |
                            AnchorStyles.Left |
                            AnchorStyles.Right |
                            AnchorStyles.Bottom,
                    MinimumSize = new Size(1, 1)
                };

            conteudo.Controls.Add(chart);

            conteudo.Resize += (s, e) =>
                {
                    int largura = Math.Max(1, conteudo.ClientSize.Width - 14);
                    int altura = Math.Max(1, conteudo.ClientSize.Height - 110);

                    chart.SetBounds(
                        7,
                        60,
                        largura,
                        altura
                    );
                };

            ChartArea area = new ChartArea();

            area.BackColor = Color.White;

            area.AxisX.MajorGrid.Enabled = false;
            area.AxisX.LineColor = Color.LightGray;

            area.AxisX.LabelStyle.Font =
                new Font("Segoe UI", 8F);

            area.AxisX.Interval = 1;
            area.AxisX.IsMarginVisible = true;
            area.AxisX.LabelStyle.IsEndLabelVisible = true;

            area.AxisY.MajorGrid.LineColor =
                Color.Gainsboro;

            area.AxisY.LineColor =
                Color.LightGray;

            area.AxisY.LabelStyle.Font =
                new Font("Segoe UI", 8F);

            area.AxisY.LabelStyle.Format = "N2";

            chart.ChartAreas.Add(area);

            Series serieAmort =
                new Series("Amortização")
                {
                    ChartType =
                        SeriesChartType.Spline,

                    BorderWidth = 3,

                    XValueType =
                        ChartValueType.String,

                    YValueType =
                        ChartValueType.Double,

                    IsValueShownAsLabel = false
                };

            Series serieJuros =
                new Series("Juros")
                {
                    ChartType =
                        SeriesChartType.Spline,

                    BorderWidth = 3,

                    XValueType =
                        ChartValueType.String,

                    YValueType =
                        ChartValueType.Double,

                    IsValueShownAsLabel = false
                };

            serieAmort.Color =
                Color.FromArgb(52, 152, 219);

            serieJuros.Color =
                Color.FromArgb(243, 156, 18);

            Legend legend = new Legend
            {
                Docking = Docking.Bottom,
                Alignment = StringAlignment.Center,
                Font = new Font("Segoe UI", 8F)
            };

            chart.Legends.Add(legend);

            chart.Series.Add(serieAmort);
            chart.Series.Add(serieJuros);

            // --------------------------------------------------------
            // TOTAL
            // --------------------------------------------------------

            Label lblTotais = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 25,
                Font = new Font(
                    "Segoe UI",
                    8F,
                    FontStyle.Bold
                ),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleLeft
            };

            // --------------------------------------------------------
            // CONTEÚDO
            // --------------------------------------------------------

            conteudo.Padding = new Padding(8);

            conteudo.Controls.Add(chart);
            conteudo.Controls.Add(lblTotais);
            conteudo.Controls.Add(topo);

            // --------------------------------------------------------
            // CARREGAR
            // --------------------------------------------------------

            numAno.ValueChanged += (s, e) =>
            {
                CarregarGraficoAmortizacaoJuros(
                    chart,
                    lblTotais,
                    (int)numAno.Value
                );
            };

            CarregarGraficoAmortizacaoJuros(
                chart,
                lblTotais,
                (int)numAno.Value
            );

            return basePanel;
        }

        // ============================================================
        // CARREGAR GRÁFICO AMORTIZAÇÃO / JUROS
        // ============================================================

        private void CarregarGraficoAmortizacaoJuros(
            Chart chart,
            Label lblTotais,
            int ano)
        {
            var repo = new ParcelaRepository();

            var serieAmort =
                chart.Series["Amortização"];

            var serieJuros =
                chart.Series["Juros"];

            serieAmort.Points.Clear();
            serieJuros.Points.Clear();

            double[] totalAmortMes =
                new double[12];

            double[] totalJurosMes =
                new double[12];

            double totalAmortizacao = 0;
            double totalJuros = 0;

            var parcelasAno =
                repo.GetAll()
                    .Where(p =>
                        p.Vencimento.HasValue &&
                        p.Vencimento.Value.Year == ano &&
                        (
                            p.Situacao.Equals(
                                "Paga",
                                StringComparison.OrdinalIgnoreCase
                            )
                            ||
                            p.Situacao.Equals(
                                "Paga em Atraso",
                                StringComparison.OrdinalIgnoreCase
                            )
                            ||
                            p.Situacao.Equals(
                                "Paga Antecipado",
                                StringComparison.OrdinalIgnoreCase
                            )
                        )
                    )
                    .ToList();

            foreach (var parcela in parcelasAno)
            {
                int mes =
                    parcela.Vencimento.Value.Month - 1;

                double amort =
                    parcela.Amortizacao ?? 0;

                double juros =
                    parcela.Juros ?? 0;

                totalAmortMes[mes] += amort;
                totalJurosMes[mes] += juros;

                totalAmortizacao += amort;
                totalJuros += juros;
            }

            string[] meses =
            {
                "Jan",
                "Fev",
                "Mar",
                "Abr",
                "Mai",
                "Jun",
                "Jul",
                "Ago",
                "Set",
                "Out",
                "Nov",
                "Dez"
            };

            for (int mes = 0; mes < 12; mes++)
            {
                serieAmort.Points.AddXY(
                    meses[mes],
                    totalAmortMes[mes]
                );

                serieJuros.Points.AddXY(
                    meses[mes],
                    totalJurosMes[mes]
                );
            }

            double max = new[]
            {
                totalAmortMes.Max(),
                totalJurosMes.Max()
            }.Max();

            chart.ChartAreas[0].AxisY.Maximum =
                max > 0
                    ? max * 1.20
                    : 10;
        

            lblTotais.Text =
                $"Total Amortização: {totalAmortizacao:C2}    |    Total Juros: {totalJuros:C2}";
        }

        // ============================================================
        // PARCELAS A VENCER
        // ============================================================

        private Panel CriarBlocoCaixa()
        {
            Panel basePanel;
            Panel conteudo;

            basePanel = CriarBase(
                "Parcelas A Vencer",
                out conteudo
            );

            // --------------------------------------------------------
            // FILTROS
            // --------------------------------------------------------

            FlowLayoutPanel filtros =
                new FlowLayoutPanel
                {
                    Dock = DockStyle.Top,
                    Height = 35,
                    FlowDirection =
                        FlowDirection.LeftToRight,
                    WrapContents = false,
                    Padding = new Padding(0),
                    Margin = new Padding(0)
                };

            Label lblInicio = new Label
            {
                Text = "Início:",
                AutoSize = true,
                Font = new Font("Segoe UI", 8F),
                Margin = new Padding(0, 7, 5, 0)
            };

            DateTimePicker dtInicio =
                new DateTimePicker
                {
                    Width = 90,
                    Height = 23,
                    Format = DateTimePickerFormat.Short,
                    Font = new Font("Segoe UI", 8F),
                    Margin = new Padding(0, 3, 12, 0)
                };

            Label lblFim = new Label
            {
                Text = "Fim:",
                AutoSize = true,
                Font = new Font("Segoe UI", 8F),
                Margin = new Padding(0, 7, 5, 0)
            };

            DateTimePicker dtFim =
                new DateTimePicker
                {
                    Width = 90,
                    Height = 23,
                    Format = DateTimePickerFormat.Short,
                    Font = new Font("Segoe UI", 8F),
                    Margin = new Padding(0, 3, 12, 0)
                };

            Button btnFiltrar = new Button
            {
                Text = "Pesquisar",
                Width = 75,
                Height = 25,
                Font = new Font("Segoe UI", 8F),
                Margin = new Padding(0, 2, 0, 0)
            };

            filtros.Controls.Add(lblInicio);
            filtros.Controls.Add(dtInicio);
            filtros.Controls.Add(lblFim);
            filtros.Controls.Add(dtFim);
            filtros.Controls.Add(btnFiltrar);

            // --------------------------------------------------------
            // TOTAL
            // --------------------------------------------------------

            Label lblTotal = new Label
            {
                Dock = DockStyle.Top,
                Height = 25,
                Text = "Total A Receber No Período: R$ 0,00",
                Font = new Font(
                    "Segoe UI",
                    8F,
                    FontStyle.Bold
                ),
                ForeColor = Color.FromArgb(45, 45, 45),
                TextAlign = ContentAlignment.MiddleLeft
            };

            // --------------------------------------------------------
            // GRID
            // --------------------------------------------------------

            DataGridView grid =
                new DataGridView
                {
                    Dock = DockStyle.Fill,
                    Font = new Font("Segoe UI", 8F),
                    ReadOnly = true,
                    AllowUserToAddRows = false,
                    AllowUserToDeleteRows = false,
                    AllowUserToResizeRows = false,
                    RowHeadersVisible = false,
                    BorderStyle = BorderStyle.FixedSingle,
                    BackgroundColor = Color.White,
                    AutoSizeColumnsMode =
                        DataGridViewAutoSizeColumnsMode.Fill,
                    SelectionMode =
                        DataGridViewSelectionMode.FullRowSelect,
                    MultiSelect = false,
                    EnableHeadersVisualStyles = false
                };

            grid.ColumnHeadersDefaultCellStyle =
                new DataGridViewCellStyle
                {
                    Font = new Font(
                        "Segoe UI",
                        8F,
                        FontStyle.Bold
                    ),
                    BackColor =
                        Color.FromArgb(245, 247, 249),
                    ForeColor =
                        Color.FromArgb(40, 40, 40)
                };

            grid.RowTemplate.Height = 22;

            grid.Columns.Add(
                "Cliente",
                "Cliente"
            );

            grid.Columns.Add(
                "Contrato",
                "Contrato"
            );

            grid.Columns.Add(
                "Vencimento",
                "Vencimento"
            );

            grid.Columns.Add(
                "Parcela",
                "Parcela"
            );

            grid.Columns.Add(
                "Valor",
                "Valor"
            );

            // --------------------------------------------------------
            // DUPLO CLIQUE
            // --------------------------------------------------------

            grid.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex < 0)
                    return;

                var row = grid.Rows[e.RowIndex];

                if (row.Tag == null)
                    return;

                int emprestimoId =
                    Convert.ToInt32(row.Tag);

                new FormEditarEmprestimo(
                    emprestimoId
                ).ShowDialog();
            };

            // --------------------------------------------------------
            // FILTRAR
            // --------------------------------------------------------

            btnFiltrar.Click += (s, e) =>
            {
                grid.Rows.Clear();

                DateTime inicio =
                    dtInicio.Value.Date;

                DateTime fim =
                    dtFim.Value.Date;

                decimal total = 0;

                var parcelasPeriodo =
                    parcelaRepo
                        .GetAll()
                        .Where(p =>
                            p.Situacao != null &&
                            p.Situacao.Equals(
                                "A Vencer",
                                StringComparison.OrdinalIgnoreCase
                            ) &&
                            p.Vencimento.HasValue &&
                            p.Vencimento.Value.Date >= inicio &&
                            p.Vencimento.Value.Date <= fim
                        )
                        .OrderBy(p => p.Vencimento)
                        .ToList();

                var emprestimoRepo =
                    new EmprestimoRepository();

                var clienteRepo =
                    new ClienteRepository();

                foreach (var parcela in parcelasPeriodo)
                {
                    var emprestimo =
                        emprestimoRepo.GetById(
                            parcela.EmprestimosId
                        );

                    if (emprestimo == null)
                        continue;

                    var cliente =
                        clienteRepo.GetById(
                            emprestimo.ClienteId
                        );

                    total +=
                        (decimal)(
                            parcela.ValorPrestacao ?? 0
                        );

                    int rowIndex =
                        grid.Rows.Add(
                            cliente?.Nome ?? "N/D",
                            emprestimo.Contrato,
                            parcela.Vencimento?
                                .ToString("dd/MM/yyyy"),
                            parcela.NParcelas,
                            parcela.ValorPrestacao?
                                .ToString("C2")
                        );

                    grid.Rows[rowIndex].Tag =
                        emprestimo.Id;
                }

                lblTotal.Text =
                    $"Total A Receber No Período: {total:C2}";
            };

            // --------------------------------------------------------
            // DATAS INICIAIS
            // --------------------------------------------------------

            dtInicio.Value =
                DateTime.Today;

            dtFim.Value =
                DateTime.Today;

            // --------------------------------------------------------
            // ADICIONAR CONTROLES
            // --------------------------------------------------------

            conteudo.Controls.Add(grid);
            conteudo.Controls.Add(lblTotal);
            conteudo.Controls.Add(filtros);

            btnFiltrar.PerformClick();

            return basePanel;
        }

        // ============================================================
        // PARCELAS EM ATRASO
        // ============================================================

        private Panel CriarBlocoVencimentos()
        {
            Panel basePanel;
            Panel conteudo;

            basePanel = CriarBase(
                "Parcelas Em Atraso",
                out conteudo
            );

            var emprestimoRepo =
                new EmprestimoRepository();

            var clienteRepo =
                new ClienteRepository();

            // --------------------------------------------------------
            // GRID
            // --------------------------------------------------------

            DataGridView grid =
                new DataGridView
                {
                    Dock = DockStyle.Fill,
                    Font = new Font("Segoe UI", 8F),
                    ReadOnly = true,
                    AllowUserToAddRows = false,
                    AllowUserToDeleteRows = false,
                    AllowUserToResizeRows = false,
                    RowHeadersVisible = false,
                    BorderStyle = BorderStyle.FixedSingle,
                    BackgroundColor = Color.White,
                    AutoSizeColumnsMode =
                        DataGridViewAutoSizeColumnsMode.Fill,
                    SelectionMode =
                        DataGridViewSelectionMode.FullRowSelect,
                    MultiSelect = false,
                    EnableHeadersVisualStyles = false
                };

            grid.ColumnHeadersDefaultCellStyle =
                new DataGridViewCellStyle
                {
                    Font = new Font(
                        "Segoe UI",
                        8F,
                        FontStyle.Bold
                    ),
                    BackColor =
                        Color.FromArgb(245, 247, 249),
                    ForeColor =
                        Color.FromArgb(40, 40, 40)
                };

            grid.ColumnHeadersHeight = 24;
            grid.RowTemplate.Height = 22;

            grid.Columns.Add(
                "Cliente",
                "Cliente"
            );

            grid.Columns.Add(
                "Contrato",
                "Contrato"
            );

            grid.Columns.Add(
                "Parcela",
                "Parcela"
            );

            grid.Columns.Add(
                "Vencimento",
                "Vencimento"
            );

            grid.Columns.Add(
                "Valor",
                "Valor da Parcela"
            );

            grid.Columns.Add(
                "DiasAtrasados",
                "Dias Atrasados"
            );

            // --------------------------------------------------------
            // DUPLO CLIQUE
            // --------------------------------------------------------

            grid.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex < 0)
                    return;

                if (grid.Rows[e.RowIndex].Tag == null)
                    return;

                int emprestimoId =
                    Convert.ToInt32(
                        grid.Rows[e.RowIndex].Tag
                    );

                new FormEditarEmprestimo(
                    emprestimoId
                ).ShowDialog();
            };

            // --------------------------------------------------------
            // BUSCAR ATRASADAS
            // --------------------------------------------------------

            var parcelasAtrasadas =
                parcelaRepo.GetSomenteAtrasadas();

            foreach (var parcela in parcelasAtrasadas)
            {
                if (!parcela.Vencimento.HasValue)
                    continue;

                var emprestimo =
                    emprestimoRepo.GetById(
                        parcela.EmprestimosId
                    );

                if (emprestimo == null)
                    continue;

                var cliente =
                    clienteRepo.GetById(
                        emprestimo.ClienteId
                    );

                int diasAtraso =
                    (
                        DateTime.Today -
                        parcela.Vencimento.Value.Date
                    ).Days;

                int rowIndex =
                    grid.Rows.Add(
                        cliente?.Nome ?? "N/D",
                        emprestimo.Contrato,
                        parcela.NParcelas,
                        parcela.Vencimento?
                            .ToString("dd/MM/yyyy"),
                        parcela.ValorPrestacao?
                            .ToString("C2"),
                        diasAtraso
                    );

                grid.Rows[rowIndex].Tag =
                    emprestimo.Id;

                // ----------------------------------------------------
                // DESTAQUE DE ATRASOS ACIMA DE 20 DIAS
                // ----------------------------------------------------

                if (diasAtraso > 20)
                {
                    grid.Rows[rowIndex]
                        .DefaultCellStyle
                        .BackColor =
                        Color.MistyRose;

                    grid.Rows[rowIndex]
                        .DefaultCellStyle
                        .ForeColor =
                        Color.DarkRed;

                    grid.Rows[rowIndex]
                        .DefaultCellStyle
                        .Font =
                        new Font(
                            "Segoe UI",
                            8F,
                            FontStyle.Bold
                        );
                }
            }

            conteudo.Controls.Add(grid);

            return basePanel;
        }

        // ============================================================
        // ATUALIZAR DASHBOARD
        // ============================================================

        public void RefreshSeguro()
        {
            this.SuspendLayout();

            try
            {
                this.Controls.Clear();

                InitializeComponent();
            }
            finally
            {
                this.ResumeLayout(true);
            }
        }

        // ============================================================
        // LOGOUT
        // ============================================================

        private void btnLogout_Click(
            object sender,
            EventArgs e)
        {
            Session.Logout();

            Close();

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