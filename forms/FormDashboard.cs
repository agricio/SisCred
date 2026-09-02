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
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
        }

        private void InitializeComponent()
        {
            this.Icon = new Icon("app.ico");
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.AutoScaleMode = AutoScaleMode.None;
            this.AutoSize = false;

            Text = $"Speed Cred Sistema de Gerenciamento de Crédito - Dashboard - Usuario: {Session.CurrentUsername}";
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

            ToolStripMenuItem menuEmprestimosAnoAtual = new ToolStripMenuItem("Contratos");
            menuEmprestimosAnoAtual.DropDownItems.Add("Novo Contrato", null, (s, e) => new FormNovoEmprestimo().Show());
            menuEmprestimosAnoAtual.DropDownItems.Add("Contatos deste ano Corrente", null, (s, e) => new FormEmprestimosAnoAtual().Show());
            menuEmprestimosAnoAtual.DropDownItems.Add("Contatos Gerais", null, (s, e) => new FormEmprestimos().Show());
            //menuEmprestimosAnoAtual.Click += (s, e) => new FormEmprestimosAnoAtual().Show();

            ToolStripMenuItem menuCaixa = new ToolStripMenuItem("Caixa");
            menuCaixa.DropDownItems.Add("Adicionar Despesa", null, (s, e) => new FormCadastroDespesa().Show());
            menuCaixa.DropDownItems.Add("Despesas Mensais", null, (s, e) => new FormListaDespesas().Show());
            menuCaixa.DropDownItems.Add(new ToolStripSeparator());
            menuCaixa.DropDownItems.Add("Livro Caixa", null, (s, e) => new FormLivroCaixa().Show());

            ToolStripMenuItem menuFerramentas = new ToolStripMenuItem("Ferramentas");
            menuFerramentas.DropDownItems.Add("Gerenciamento de Usuários", null, (s, e) => new FormUsers().Show());
            menuFerramentas.DropDownItems.Add(new ToolStripSeparator());
            menuFerramentas.DropDownItems.Add("Simulador de Contrato e Parcelas", null, (s, e) => new FormCalcularParcelas().Show());
            menuFerramentas.DropDownItems.Add("Calculadora Valor Liberado", null, (s, e) => new FormFerramentaLiberado().Show());
            menuFerramentas.DropDownItems.Add("Calculadora Parcela Em Atraso", null, (s, e) => new FormFerramentaCalcularAtraso().Show());
            menuFerramentas.DropDownItems.Add(new ToolStripSeparator());
            menuFerramentas.DropDownItems.Add("Restauração e Backup", null, (s, e) => new FormBackup().Show());
            menuFerramentas.DropDownItems.Add("Sincronizar Data Base", null, (s, e) => new FormSyncDB().Show());
            //menuFerramentas.DropDownItems.Add("Importar do Excel", null, (s, e) => new FormImportarExcel().Show());
            menuFerramentas.DropDownItems.Add(new ToolStripSeparator());
            menuFerramentas.DropDownItems.Add("Logout", null, btnLogout_Click);
            menuFerramentas.DropDownItems.Add("Sobre", null, (s, e) => new FormAbout().ShowDialog());

            menu.Items.Add(menuClientes);
            menu.Items.Add(menuServicos);
            menu.Items.Add(menuEmprestimosAnoAtual);
            menu.Items.Add(menuCaixa);
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
                RowCount = 2,
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
            grid.Controls.Add(pParcelasAtraso , 0, 1);
            grid.Controls.Add(pCaixa, 1, 1);
            
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
            Panel basePanel = CriarBase("Entradas por Mês");

            // painel principal interno
            var container = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2
            };
            container.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 75));
            container.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

            // ===== GRÁFICO =====
            Chart chart = new Chart { Dock = DockStyle.Fill }; 
            chart.ChartAreas.Add(new ChartArea());

            Legend legend = new Legend
            {
                Docking = Docking.Bottom,
                Alignment = StringAlignment.Center
            };
            chart.Legends.Add(legend);

            Series sValor = new Series("Parcelas Pagas")
            {
                ChartType = SeriesChartType.Column,
                IsValueShownAsLabel = true,
                LegendText = "Parcelas"
            };

            Series sJuros = new Series("Juros")
            {
                ChartType = SeriesChartType.Column,
                IsValueShownAsLabel = true,
                LegendText = "Juros"
            };

            Series sAmort = new Series("Amortização")
            {
                ChartType = SeriesChartType.Column,
                IsValueShownAsLabel = true,
                LegendText = "Amortização"
            };

            sValor["PointWidth"] = "0.8";
            sJuros["PointWidth"] = "0.8";
            sAmort["PointWidth"] = "0.8";

            sValor.IsValueShownAsLabel = false;
            sJuros.IsValueShownAsLabel = false;
            sAmort.IsValueShownAsLabel = false;

            chart.Series.Add(sValor);
            chart.Series.Add(sJuros);
            chart.Series.Add(sAmort);

            // ===== PAINEL LATERAL =====
            var side = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                Padding = new Padding(10),
                WrapContents = false
            };

            var lblAno = new Label
            {
                Text = "Ano:",
                AutoSize = true,
                Margin = new Padding(0, 180, 0, 6) // empurra para baixo
            };

            var numAno = new NumericUpDown
            {
                Minimum = 2000,
                Maximum = 2100,
                Value = DateTime.Now.Year,
                Width = 100,
                Margin = new Padding(0, 0, 0, 20)
            };

            var lblTotalParcelas = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.Gray,
                Margin = new Padding(0, 0, 0, 2) // empurra mais para baixo
            };

            var lblTotalJuros = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.Gray,
                Margin = new Padding(0, 0, 0, 2)
            };

            var lblTotalAmort = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.Gray,
                Margin = new Padding(0, 0, 0, 2)
            };

            side.Controls.Add(lblAno);
            side.Controls.Add(numAno);
            side.Controls.Add(new Label { Height = 10 });
            side.Controls.Add(lblTotalParcelas);
            side.Controls.Add(lblTotalJuros);
            side.Controls.Add(lblTotalAmort);

            container.Controls.Add(chart, 0, 0);
            container.Controls.Add(side, 1, 0);

            basePanel.Controls.Add(container);

            void Recalcular(int ano)
            {
                sValor.Points.Clear();
                sJuros.Points.Clear();
                sAmort.Points.Clear();

            var parcelas = parcelaRepo.GetAll()
                .Where(p =>
                    p.Pagamento.HasValue &&
                    p.Pagamento.Value.Year == ano &&
                    (
                        p.Situacao.Equals("Paga", StringComparison.OrdinalIgnoreCase) ||
                        p.Situacao.Equals("Paga em Atraso", StringComparison.OrdinalIgnoreCase) ||
                        p.Situacao.Equals("Paga Antecipado", StringComparison.OrdinalIgnoreCase)
                    )
                );

                var lista = parcelas.ToList();

            var porMes = parcelas
                .GroupBy(p => p.Pagamento.Value.Month)
                .Select(g => new
                {
                    Mes = g.Key,
                    Total = g.Sum(x => x.ValorPrestacao ?? 0),
                    Juros = g.Sum(x => x.Juros ?? 0),
                    Amort = g.Sum(x => x.Amortizacao ?? 0)
                })
                .ToDictionary(x => x.Mes);

                string[] meses = { "Jan","Fev","Mar","Abr","Mai","Jun","Jul","Ago","Set","Out","Nov","Dez" };

                double totalParcelas = 0, totalJuros = 0, totalAmort = 0;
                
                for (int mes = 1; mes <= 12; mes++)
                {
                    if (porMes.TryGetValue(mes, out var d))
                    {
                        sValor.Points.AddXY(meses[mes - 1], d.Total);
                        sJuros.Points.AddXY(meses[mes - 1], d.Juros);
                        sAmort.Points.AddXY(meses[mes - 1], d.Amort);

                        totalParcelas += d.Total;
                        totalJuros += d.Juros;
                        totalAmort += d.Amort;
                    }
                    else
                    {
                        sValor.Points.AddXY(meses[mes - 1], 0);
                        sJuros.Points.AddXY(meses[mes - 1], 0);
                        sAmort.Points.AddXY(meses[mes - 1], 0);
                    }
                }

                RemoverLabelsZero(sValor);
                RemoverLabelsZero(sJuros);
                RemoverLabelsZero(sAmort);

                double maxValor = Math.Max(
                    sValor.Points.Max(p => p.YValues[0]),
                    Math.Max(
                        sJuros.Points.Max(p => p.YValues[0]),
                        sAmort.Points.Max(p => p.YValues[0])
                    )
                );

                var area = chart.ChartAreas[0];

                // fundo limpo
                area.BackColor = Color.White;

                // eixo X
                area.AxisX.MajorGrid.Enabled = false;
                area.AxisX.LabelStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);

                // eixo Y
                area.AxisY.MajorGrid.LineColor = Color.Gainsboro;
                area.AxisY.LabelStyle.Font = new Font("Segoe UI", 9);
                area.AxisY.TitleFont = new Font("Segoe UI", 9, FontStyle.Bold);

                // bordas suaves
                area.AxisX.LineColor = Color.LightGray;
                area.AxisY.LineColor = Color.LightGray;


                // verifica se existe ao menos um valor > 0
                double maxValor2 = new[]
                {
                    sValor.Points.Select(p => p.YValues[0]).DefaultIfEmpty(0).Max(),
                    sJuros.Points.Select(p => p.YValues[0]).DefaultIfEmpty(0).Max(),
                    sAmort.Points.Select(p => p.YValues[0]).DefaultIfEmpty(0).Max()
                }.Max();

                var fonteNegrito = new Font("Segoe UI", 9, FontStyle.Bold);

                sValor.Font = fonteNegrito;
                sJuros.Font = fonteNegrito;
                sAmort.Font = fonteNegrito;

                sValor.Color = Color.FromArgb(52, 152, 219);   // azul
                sJuros.Color = Color.FromArgb(231, 76, 60);    // vermelho
                sAmort.Color = Color.FromArgb(46, 204, 113);   // verde

                sValor.BorderWidth = 0;
                sJuros.BorderWidth = 0;
                sAmort.BorderWidth = 0;

                //sValor.IsValueShownAsLabel = false;
                //sJuros.IsValueShownAsLabel = false;
                //sAmort.IsValueShownAsLabel = false;

                if (maxValor2 <= 0)
                {
                    // estado seguro quando não há dados
                    area.AxisY.Minimum = 0;
                    area.AxisY.Maximum = 10;
                }
                else
                {
                    area.AxisY.Minimum = 0;
                    area.AxisY.Maximum = maxValor * 1.3;
                }

                lblTotalParcelas.Text = $"Total Parcelas: {totalParcelas:C2}";
                lblTotalJuros.Text = $"Total Juros: {totalJuros:C2}";
                lblTotalAmort.Text = $"Total Amortização: {totalAmort:C2}";
            }

            numAno.ValueChanged += (s, e) => Recalcular((int)numAno.Value);

            Recalcular(DateTime.Now.Year);

            void RemoverLabelsZero(Series serie)
                {
                    foreach (var p in serie.Points)
                    {
                        if (p.YValues[0] == 0)
                            p.Label = ""; // não mostra nada
                        else
                            p.Label = p.YValues[0].ToString("C2"); // opcional: formato moeda
                    }
                }

            return basePanel;
        }

        // =============== BLOCO AMORTIZAÇÃO E JUROS ===============
        private Panel CriarBlocoAmortizacaoJuros()
        {
            Panel p = CriarBase("Amortização x Juros");

            // ===== LABEL ANO =====
            var lblAno = new Label
            {
                Text = "Ano:",
                Location = new Point(20, 50),
                AutoSize = true
            };

            Label lblTotais = new Label
            {
                Text = "Total Amortização: R$ 0,00    |    Total Juros: R$ 0,00",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.Gray,
                Location = new Point(20, 420),
                AutoSize = true
            };

            // ===== NUMERIC ANO =====
            var numAno = new NumericUpDown
            {
                Minimum = 2000,
                Maximum = 2100,
                Value = DateTime.Now.Year,
                Width = 100,
                Location = new Point(65, 46)
            };

            // ===== GRÁFICO =====
            Chart chart = new Chart
            {
                Location = new Point(10, 85),
                Size = new Size(760, 330)
            };

            chart.ChartAreas.Add(new ChartArea());
            

            Series serieAmort = new Series("Amortização")
            {
                ChartType = SeriesChartType.Spline,
                BorderWidth = 3,
                XValueType = ChartValueType.Int32,
                YValueType = ChartValueType.Double,
                IsValueShownAsLabel = false
            };

            Series serieJuros = new Series("Juros")
            {
                ChartType = SeriesChartType.Spline,
                BorderWidth = 3,
                XValueType = ChartValueType.Int32,
                YValueType = ChartValueType.Double,
                IsValueShownAsLabel = false
            };

            Legend legend = new Legend
            {
                Docking = Docking.Bottom,
                Alignment = StringAlignment.Center,
                Font = new Font("Segoe UI", 9)
            };

            chart.Legends.Add(legend);
            chart.Series.Add(serieAmort);
            chart.Series.Add(serieJuros);

            // ===== EVENTO: MUDOU O ANO =====
            numAno.ValueChanged += (s, e) =>
            {
                int anoSelecionado = (int)numAno.Value;
                CarregarGraficoAmortizacaoJuros(chart, lblTotais, (int)numAno.Value);
            };

            // ===== CARGA INICIAL =====
            CarregarGraficoAmortizacaoJuros(chart, lblTotais, (int)numAno.Value);

            // ===== ADD CONTROLES =====
            p.Controls.Add(lblAno);
            p.Controls.Add(lblTotais);
            p.Controls.Add(numAno);
            p.Controls.Add(chart);
            return p;
        }

        private void CarregarGraficoAmortizacaoJuros(Chart chart, Label lblTotais, int ano)
        {
            var parcelaRepo = new ParcelaRepository();

            var serieAmort = chart.Series["Amortização"];
            var serieJuros = chart.Series["Juros"];

            serieAmort.Points.Clear();
            serieJuros.Points.Clear();

            double[] totalAmortMes = new double[12];
            double[] totalJurosMes = new double[12];

            double totalAmortizacao = 0;
            double totalJuros = 0;

            var parcelasAno = parcelaRepo.GetAll()
            .Where(p =>
                p.Vencimento.HasValue &&
                p.Vencimento.Value.Year == ano &&
                (
                    p.Situacao.Equals("Paga", StringComparison.OrdinalIgnoreCase) ||
                    p.Situacao.Equals("Paga em Atraso", StringComparison.OrdinalIgnoreCase) ||
                    p.Situacao.Equals("Paga Antecipado", StringComparison.OrdinalIgnoreCase)
                )
            )
            .ToList();


            foreach (var parcela in parcelasAno)
            {
                int mes = parcela.Vencimento.Value.Month - 1;

                double amort = parcela.Amortizacao ?? 0;
                double juros = parcela.Juros ?? 0;

                totalAmortMes[mes] += amort;
                totalJurosMes[mes] += juros;

                totalAmortizacao += amort;
                totalJuros += juros;
            }

            for (int mes = 0; mes < 12; mes++)
            {
                string nomeMes = new DateTime(ano, mes + 1, 1)
                    .ToString("MMM");

                serieAmort.Points.AddXY(nomeMes, totalAmortMes[mes]);
                serieJuros.Points.AddXY(nomeMes, totalJurosMes[mes]);
            }

            // ✅ ATUALIZA O LABEL AQUI
            lblTotais.Text =
                $"Total Amortização: {totalAmortizacao:C2}    |    Total Juros: {totalJuros:C2}";
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
                    new FormEditarEmprestimo(emprestimoId).ShowDialog();
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
                new FormEditarEmprestimo(emprestimoId).ShowDialog();
            };

            // ===== BUSCA PARCELAS EM ATRASO =====
            
            var parcelasAtrasadas = parcelaRepo.GetSomenteAtrasadas();
                

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

        public void RefreshSeguro()
        {
            this.SuspendLayout();

            this.Controls.Clear();
            InitializeComponent();

            this.ResumeLayout();
        }

         // ===== FAZER LOGOUT DO SISTEMA =====
        private void btnLogout_Click(object sender, EventArgs e)
                {
            Session.Logout();

            Close(); //ISSO É ESSENCIAL

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
