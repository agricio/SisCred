using System;
using System.Linq;
using System.Windows.Forms;
using CrudApp.Repositories;
using System.Drawing;
using CrudApp.Pdf;
using CrudApp.Shared;
using System.IO;
using System.Collections.Generic;


namespace CrudApp.Forms
{
    public class FormLivroCaixa : Form
    {
    private readonly EmprestimoRepository repo = new EmprestimoRepository();
    private readonly ParcelaRepository parcelaRepo = new ParcelaRepository();
    private readonly ServicoRepository servicoRepo = new ServicoRepository();
    private readonly DespesaRepository despesaRepo = new DespesaRepository ();
    private DataGridView grid;
    private DataGridView gridDespesas;
    private DataGridView gridServicos;
    private ComboBox cbAno;
    private ComboBox cbMes;
    private TextBox txtBusca;

    // 🔹 PAINÉIS
    private FlowLayoutPanel panelTop;
    private Panel panelInfo;
    private Panel panelMain;

    // 🔹 LABELS
    private Label lblFinalizados;
    private Label lblContratosAtrasados;
    private Label lblParcelasReceber;
    private Label lblTotalPrevisto;
    private Label lblparcelasPagasNoMes;
    private Label lblTotalRecebido;
    private Label lblTotalServicos;
    private Label lblServicos;
    private Label lblDespesas; 
    private Label lblTotalAtraso;
    private Label lblSaldoFinal;
    private Label lblTotalDespesas;

    private decimal totalServicos = 0;
    private decimal totalDespesas = 0;
    private decimal totalContratos = 0;

    private decimal totalPrevisto = 0;
    private decimal totalRecebido = 0;
    private decimal totalEmAtraso = 0;
    private int totalFinalizados;
    private int totalAtrasados;
    private int parcelasReceber;
    private int parcelasPagasNoMes;

        public FormLivroCaixa()
        {
            InitializeComponent();
            LoadAnos();
            LoadMeses();
            LoadGrid();
        }

       private void InitializeComponent()
        {
            this.Text = "SiS Cred - Livro Caixa";
            this.Icon = new Icon("app.ico");
            this.ClientSize = new Size(1300, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Padding = new Padding(15);
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.AutoScaleMode = AutoScaleMode.None;
            //this.AutoSize = false;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            //MaximizeBox = false;

            // Painel superior (busca)
            panelTop = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 45,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(5)
            };

            txtBusca = new TextBox
            {
                PlaceholderText = "Pesquisar por nome, vencimento ou contrato...",
                Width = 350,
                Height = 28
            };
            txtBusca.TextChanged += (s, e) => Buscar(txtBusca.Text);

            cbAno = new ComboBox
            {
                Width = 100,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            cbAno.SelectedIndexChanged += (s, e) => LoadGrid();

            //panelTop.Controls.Add(txtBusca);
            panelTop.Controls.Add(cbAno);
            
            cbMes = new ComboBox
            {
                Width = 120,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            cbMes.SelectedIndexChanged += (s, e) => LoadGrid();
            panelTop.Controls.Add(cbMes);

            // 🔹 LABELS
            lblFinalizados = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.DarkBlue
            };

            lblparcelasPagasNoMes = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.DarkRed
            };

            lblContratosAtrasados = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.Gray
            };

            lblParcelasReceber = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.Black
            };

            lblTotalPrevisto = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.DarkOrange
            };

            lblTotalRecebido = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.DarkBlue
            };

            lblTotalAtraso = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.DarkRed
            };

            lblSaldoFinal = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.Blue
            };

            var btnPdf = new Button
            {
                Text = "Gerar PDF",
                Width = 120,
                Height = 25,
                //BackColor = Color.DarkBlue,
                //ForeColor = Color.White
            };

            btnPdf.Click += (s, e) => GerarPdfLivroCaixa();

            panelTop.Controls.Add(btnPdf);

            // 🔹 PAINEL ESQUERDO
            var panelLeft = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true,
                WrapContents = false,
                Margin = new Padding(0, 0, 550, 0)
            };
            panelLeft.Controls.Add(lblTotalPrevisto);
            panelLeft.Controls.Add(lblTotalRecebido);
            panelLeft.Controls.Add(lblTotalAtraso); 
            panelLeft.Controls.Add(lblSaldoFinal);

            // 🔹 PAINEL DIREITO
            var panelRight = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true
            };
            panelRight.Controls.Add(lblFinalizados);
            panelRight.Controls.Add(lblContratosAtrasados);
            panelRight.Controls.Add(lblParcelasReceber);
            panelRight.Controls.Add(lblparcelasPagasNoMes);
            
            // 🔹 PAINEL INFO (CONTÊM LEFT + RIGHT)
            var panelSpacer = new Panel
            {
                Dock = DockStyle.Fill,
                MinimumSize = new Size(850,0)
            };
    
            // 🔹 PAINEL INFO (CONTÊM LEFT + RIGHT)
            panelInfo = new Panel
            {
                Dock = DockStyle.Top,
                Height = 130,
                Padding = new Padding(10)
            };

            panelLeft.Dock = DockStyle.Left;
            panelRight.Dock = DockStyle.Right;
            panelSpacer.Dock = DockStyle.Fill;

            panelInfo.Controls.Add(panelSpacer);
            panelInfo.Controls.Add(panelRight);
            panelInfo.Controls.Add(panelLeft);

            // 📋 GRID
           grid = new DataGridView
            {
                Left = 0,
                Top = 0,
                //Width = 1250,
                Height = 250,
                ReadOnly = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                BorderStyle = BorderStyle.FixedSingle
            };

            grid.CellDoubleClick += Grid_CellDoubleClick;
            grid.RowPrePaint += Grid_RowPrePaint;

            // 📋 GRID 2
            gridDespesas = new DataGridView
            {
                Left = 0,
                Top = 290, // 👈 abaixo da primeira
                Width = 1250,
                Height = 200,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            // 📋 GRID 3
            gridServicos = new DataGridView
            {
                Left = 0,
                Top = 560,
                Width = 1250,
                Height = 200,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

                    
            lblTotalServicos = new Label
            {
                Left = 0,
                Top = gridServicos.Bottom + 10,
                Width = 1250,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
            };

            lblTotalDespesas = new Label
            {
                Left = 0,
                Top = gridDespesas.Bottom + 10,
                Width = 1250,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
            };


            lblDespesas = new Label
            {
                Left = 0,
                Top = gridDespesas.Top - 25,
                Width = 1250,
                Text = "Despesas",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
            };

            lblServicos = new Label
            {
                Left = 0,
                Top = gridServicos.Top - 25,
                Width = 1250,
                Text = "Serviços",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
            };


            panelMain = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true, // 👈 ISSO ATIVA A ROLAGEM
                Padding = new Padding(5)
            };

            panelMain.Controls.Add(grid);

            panelMain.Controls.Add(lblDespesas);
            panelMain.Controls.Add(gridDespesas);
            panelMain.Controls.Add(lblTotalDespesas);

            panelMain.Controls.Add(lblServicos);
            panelMain.Controls.Add(gridServicos);
            panelMain.Controls.Add(lblTotalServicos);

            // ⚠️ ORDEM FINAL
            this.Controls.Add(panelMain);
            this.Controls.Add(panelInfo);
            this.Controls.Add(panelTop);
        }


        private void LoadGrid()
        {
            grid.Columns.Clear();

            var emprestimos = repo.GetAll();
            var clienteRepo = new ClienteRepository();

            int? anoSelecionado = null;
            int? mesSelecionado = null;

            // Ano
            if (cbAno.SelectedItem != null &&
                cbAno.SelectedItem.ToString() != "Todos os anos")
            {
                anoSelecionado = Convert.ToInt32(cbAno.SelectedItem);
            }

            // Mês
            if (cbMes.SelectedItem != null &&
                cbMes.SelectedItem.ToString() != "Todos os meses")
            {
                mesSelecionado = Convert.ToInt32(cbMes.SelectedItem.ToString().Substring(0, 2));
            }

            // 🔥 PARCELAS FILTRADAS PELO VENCIMENTO
            var todasParcelas = parcelaRepo.GetAll();

            var parcelas = todasParcelas
                .Where(p =>
                {
                    if (p.Situacao == null)
                        return false;

                    var status = p.Situacao.ToLower();

                    bool isPaga =
                        status == "paga" ||
                        status == "paga em atraso" ||
                        status == "paga antecipado";

                    // 🔹 PAGAS → filtrar por data de pagamento
                    if (isPaga)
                    {
                        if (!p.Pagamento.HasValue)
                            return false;

                        return
                            (!anoSelecionado.HasValue || p.Pagamento.Value.Year == anoSelecionado) &&
                            (!mesSelecionado.HasValue || p.Pagamento.Value.Month == mesSelecionado);
                    }

                    // 🔹 NÃO PAGAS → filtrar por vencimento
                    if (p.Vencimento.HasValue)
                    {
                        return
                            (
                                (!anoSelecionado.HasValue || p.Vencimento.Value.Year == anoSelecionado) &&
                                (!mesSelecionado.HasValue || p.Vencimento.Value.Month == mesSelecionado)
                            )
                            // 🔥 mantém atrasadas sempre visíveis
                            || status == "em atraso";
                    }

                    return false;
                })
                .ToList();

            totalPrevisto = parcelas.Sum(p => (decimal)(p.ValorPrestacao ?? 0));

            totalRecebido = parcelas
                .Where(p =>
                    p.Situacao != null &&
                    (
                        p.Situacao.Equals("Paga", StringComparison.OrdinalIgnoreCase) ||
                        p.Situacao.Equals("Paga em Atraso", StringComparison.OrdinalIgnoreCase) ||
                        p.Situacao.Equals("Paga Antecipado", StringComparison.OrdinalIgnoreCase)
                    )
                )
                .Sum(p => (decimal)(p.ValorPrestacao ?? 0));

            totalEmAtraso = parcelas
                .Where(p => p.Situacao != null && p.Situacao.Equals("Em Atraso", StringComparison.OrdinalIgnoreCase))
                .Sum(p => (decimal)(p.ValorPrestacao ?? 0));

            // 🔥 JOIN PARCELA + EMPRESTIMO
            var lista = (from p in parcelas
                        join e in emprestimos on p.EmprestimosId equals e.Id
                        let cliente = clienteRepo.GetById(e.ClienteId)
                        select new
                        {
                            // 🔹 PARCELA
                            e.Contrato,
                            Cliente = cliente?.Nome ?? "N/D",
                            NumeroParcela = $"{p.NParcelas}/{e.Parcelas}",
                            Agio_Ativo = p.Juros,
                            Status = p.Situacao,
                            Produto = e.Codigo.ToString("D3"),
                            Agio_Pago = p.Juros,
                            Vencimento = p.Vencimento?.ToString("dd/MM/yyyy") ?? "",
                            Forma_de_PGTO = p.FormaPagamento,
                            Ultimo_PGTO = p.Pagamento,
                            Bruto_no_Mes = p.ValorPrestacao,
                            EmprestimoId = e.Id,
 
                            // 🔹 VISUAL
                            EmAtraso = p.Situacao == "Em Atraso"
                        })
                        .OrderBy(x => x.Vencimento)
                        .ToList();

            grid.DataSource = lista;

            grid.Columns["EmprestimoId"].Visible = false;

            // esconder auxiliares
            if (grid.Columns.Contains("DataVencimento"))
                grid.Columns["DataVencimento"].Visible = false;

            if (grid.Columns.Contains("EmAtraso"))
                grid.Columns["EmAtraso"].Visible = false;

            // botão excluir
            var colExcluir = new DataGridViewButtonColumn
            {
                Text = "Excluir",
                Name = "Delete",
                UseColumnTextForButtonValue = true,
                Width = 80
            };


            // 🔥 TOTAIS (mantém do empréstimo - NÃO usar lista nova)
            double totalAgio = emprestimos.Sum(x => x.Agio);
            double totalLiberado = emprestimos.Sum(x => x.Liberado);
            double totalJuros = emprestimos.Sum(x => x.TotalJuros);

            //lblTotalAgio.Text = $"Total Agio: {totalAgio:C2}";
            
            lblTotalPrevisto.Text = $"Total Previsto: R$ {totalPrevisto}";
            lblTotalRecebido.Text = $"Total Recebido: R$ {totalRecebido}";
            lblTotalAtraso.Text = $"Total Em Atraso: R$ {totalEmAtraso }";

            totalContratos = (decimal)totalAgio;

            CarregarDespesasPorMes();
            CarregarServicosPorMes();
            AtualizarIndicadores();
            AtualizarSaldoFinal();
        }

        private void AtualizarIndicadores()
        {
            var (anoSelecionado, mesSelecionado) = MesSelecionado();

            var emprestimos = repo.GetAll();
            var listaParcelas = parcelaRepo.GetAll();

            // 🔥 FILTRAR PARCELAS PELO MÊS
            var parcelasFiltradas = listaParcelas
                .Where(p =>
                    p.Vencimento.HasValue &&
                    (!anoSelecionado.HasValue || p.Vencimento.Value.Year == anoSelecionado) &&
                    (!mesSelecionado.HasValue || p.Vencimento.Value.Month == mesSelecionado)
                )
                .ToList();

            // 🔥 PEGAR IDs DOS EMPRÉSTIMOS ENVOLVIDOS NO MÊS
            var idsEmprestimos = parcelasFiltradas
                .Select(p => p.EmprestimosId)
                .Distinct()
                .ToList();

            var emprestimosFiltrados = emprestimos
                .Where(e => idsEmprestimos.Contains(e.Id))
                .ToList();

            // 🔹 EMPRÉSTIMOS
             totalFinalizados = emprestimosFiltrados
                .Count(e => e.Situacao != null &&
                            e.Situacao.Equals("Quitado", StringComparison.OrdinalIgnoreCase));

            totalAtrasados = emprestimosFiltrados
                .Count(e => e.Situacao != null &&
                            e.Situacao.Equals("Em Atraso", StringComparison.OrdinalIgnoreCase));

            // 🔹 PARCELAS
           int parcelasAtrasadas = parcelasFiltradas
                .Count(p => p.Vencimento.Value < DateTime.Today &&
                p.Situacao != "Paga");

            parcelasReceber = parcelasFiltradas
                .Count(p => p.Vencimento.Value >= DateTime.Today &&
                            p.Situacao != "Paga");
            
            parcelasPagasNoMes = listaParcelas
                .Count(p =>
                    p.Pagamento.HasValue &&
                    (!anoSelecionado.HasValue || p.Pagamento.Value.Year == anoSelecionado) &&
                    (!mesSelecionado.HasValue || p.Pagamento.Value.Month == mesSelecionado) &&
                    p.Situacao != null &&
                    (
                        p.Situacao == "Paga" ||
                        p.Situacao == "Paga em Atraso" ||
                        p.Situacao == "Paga Antecipado"
                    )
                );
          

            // 🔥 EXIBIR
            lblFinalizados.Text = $"Contratos Finalizados: {totalFinalizados}";
            lblContratosAtrasados.Text = $"Contratos em Atraso: {totalAtrasados}";
            lblParcelasReceber.Text = $"Parcelas a Receber: {parcelasReceber}";
            lblparcelasPagasNoMes.Text = $"Parcelas Em Dia: {parcelasPagasNoMes}";
        }

        private void LoadMeses()
        {
            cbMes.Items.Clear();

            cbMes.Items.Add("Todos os meses");

            var meses = Enumerable.Range(1, 12)
                .Select(m => new
                {
                    Numero = m,
                    Nome = new DateTime(2000, m, 1).ToString("MMMM")
                })
                .ToList();

            foreach (var mes in meses)
                cbMes.Items.Add($"{mes.Numero:D2} - {mes.Nome}");

            // 🔥 selecionar mês atual
            int mesAtual = DateTime.Now.Month;

            // +1 por causa do "Todos os meses" na posição 0
            cbMes.SelectedIndex = mesAtual;
        }

        private void LoadAnos()
        {
            var anos = repo.GetAll()
                .Where(x => x.Averbacao.HasValue)
                .Select(x => x.Averbacao!.Value.Year)
                .Distinct()
                .OrderByDescending(x => x)
                .ToList();

            cbAno.Items.Clear();

            // 🔹 opção global
            cbAno.Items.Add("Todos os anos");

            foreach (var ano in anos)
                cbAno.Items.Add(ano);

            // seleciona ano atual se existir, senão "Todos"
            int anoAtual = DateTime.Now.Year;

            if (cbAno.Items.Contains(anoAtual))
                cbAno.SelectedItem = anoAtual;
            else
                cbAno.SelectedIndex = 0; // Todos os anos
        }

        private (int? ano, int? mes) MesSelecionado()
        {
            int? ano = null;
            int? mes = null;

            if (cbAno.SelectedItem != null &&
                cbAno.SelectedItem.ToString() != "Todos os anos")
            {
                ano = Convert.ToInt32(cbAno.SelectedItem);
            }

            if (cbMes.SelectedItem != null &&
                cbMes.SelectedItem.ToString() != "Todos os meses")
            {
                mes = Convert.ToInt32(cbMes.SelectedItem.ToString().Substring(0, 2));
            }

            return (ano, mes);
        }

        private void CarregarDespesasPorMes()
        {
            var (anoSelecionado, mesSelecionado) = MesSelecionado();

            var lista = despesaRepo.GetAll()
                .Where(x =>
                    (!anoSelecionado.HasValue || x.Vencimento.Year == anoSelecionado) &&
                    (!mesSelecionado.HasValue || x.Vencimento.Month == mesSelecionado)
                )
                .ToList();

            gridDespesas.DataSource = lista.Select(x => new
            {
                x.Id,
                x.Tipo,
                Valor = x.Valor.ToString("C"),
                Vencimento = x.Vencimento.ToString("dd/MM/yyyy"),
                Pagamento = x.DataPagamento?.ToString("dd/MM/yyyy") ?? "-",
                x.Situacao
            }).ToList();

            foreach (DataGridViewRow row in gridDespesas.Rows)
            {
                var situacao = row.Cells["Situacao"]?.Value?.ToString();

                if (situacao == "Atrasada")
                {
                    row.DefaultCellStyle.BackColor = Color.MistyRose;
                    row.DefaultCellStyle.ForeColor = Color.DarkRed;
                }
                else if (situacao == "Paga")
                {
                    row.DefaultCellStyle.BackColor = Color.Honeydew;
                    row.DefaultCellStyle.ForeColor = Color.DarkGreen;
                }
            }

            if (gridDespesas.Columns.Contains("Id"))
                gridDespesas.Columns["Id"].Visible = false;

            totalDespesas = lista.Sum(x => x.Valor);
            lblTotalDespesas.Text = $"Total Despesas: {totalDespesas:C}";
        }

        private void CarregarServicosPorMes()
        {
            var (anoSelecionado, mesSelecionado) = MesSelecionado();

            var lista = servicoRepo.GetAll()
                .Where(x =>
                    (!anoSelecionado.HasValue || x.Vencimento.Year == anoSelecionado) &&
                    (!mesSelecionado.HasValue || x.Vencimento.Month == mesSelecionado)
                )
                .ToList();

            gridServicos.DataSource = lista.Select(x => new
            {
                x.Id,
                x.Tipo,
                Valor = x.Valor.ToString("C"),
                Vencimento = x.Vencimento.ToString("dd/MM/yyyy"),
                Pagamento = x.DataPagamento?.ToString("dd/MM/yyyy") ?? "-",
                x.Situacao
            }).ToList();

            foreach (DataGridViewRow row in gridServicos.Rows)
            {
                var situacao = row.Cells["Situacao"]?.Value?.ToString();

                if (situacao == "Atrasada")
                {
                    row.DefaultCellStyle.BackColor = Color.MistyRose;
                    row.DefaultCellStyle.ForeColor = Color.DarkRed;
                }
                else if (situacao == "Paga")
                {
                    row.DefaultCellStyle.BackColor = Color.Honeydew;
                    row.DefaultCellStyle.ForeColor = Color.DarkGreen;
                }
            }

            if (gridServicos.Columns.Contains("Id"))
                gridServicos.Columns["Id"].Visible = false;

            totalServicos = lista.Sum(x => x.Valor);
            lblTotalServicos.Text = $"Total Serviços: {totalServicos:C}";
        }

       private void Buscar(string termo)
        {

            int? anoSelecionado = null;
            int? mesSelecionado = null;

            if (cbAno.SelectedItem != null &&
                cbAno.SelectedItem.ToString() != "Todos os anos")
            {
                anoSelecionado = Convert.ToInt32(cbAno.SelectedItem);
            }

            if (cbMes.SelectedItem != null &&
                cbMes.SelectedItem.ToString() != "Todos os meses")
            {
                mesSelecionado = Convert.ToInt32(cbMes.SelectedItem.ToString().Substring(0, 2));
            }

            if (string.IsNullOrWhiteSpace(termo))
            {
                LoadGrid();
                return;
            }

            termo = termo.ToLower();

            var clienteRepo = new ClienteRepository();

            var lista = repo.GetAll()
                .Where(x =>
                    x.Averbacao.HasValue &&
                    (!anoSelecionado.HasValue || x.Averbacao.Value.Year == anoSelecionado) &&
                    (!mesSelecionado.HasValue || x.Averbacao.Value.Month == mesSelecionado)
                )
                .Where(x =>
                {
                    if (anoSelecionado.HasValue)
                    {
                        if (!x.Averbacao.HasValue ||
                            x.Averbacao.Value.Year != anoSelecionado)
                            return false;
                    }

                    var nomeCliente = clienteRepo.GetById(x.ClienteId)?.Nome?.ToLower() ?? "";
                    var venc = x.Vencimento.HasValue
                        ? x.Vencimento.Value.ToString("dd/MM/yyyy").ToLower()
                        : "";
                    var contrato = x.Contrato.ToString().ToLower();

                    return nomeCliente.Contains(termo)
                        || venc.Contains(termo)
                        || contrato.Contains(termo);
                })
                .Select(x => new
                {
                    x.Id,
                    Cliente = clienteRepo.GetById(x.ClienteId)?.Nome ?? "N/D",
                    Cpf = clienteRepo.GetById(x.ClienteId)?.Cpf ?? "N/D",
                    x.Contrato,
                    x.Liberado,
                    x.Parcelas,
                    x.Vencimento,
                    x.Situacao
                })
                .ToList();

            grid.DataSource = lista;

            foreach (DataGridViewRow row in grid.Rows)
            {
                if (row.Cells["Situacao"].Value?.ToString() == "Em Atraso")
                {
                    row.DefaultCellStyle.BackColor = Color.MistyRose;
                    row.DefaultCellStyle.ForeColor = Color.DarkRed;
                    row.DefaultCellStyle.Font = new Font(grid.Font, FontStyle.Bold);
                }
            }

        }

        private void Grid_RowPrePaint(object? sender, DataGridViewRowPrePaintEventArgs e)
        {
            var row = grid.Rows[e.RowIndex];

            var status = row.Cells["Status"]?.Value?.ToString();

            if (string.IsNullOrEmpty(status))
                return;

            // 🔴 ATRASADO
            if (status.Equals("Em Atraso", StringComparison.OrdinalIgnoreCase))
            {
                row.DefaultCellStyle.BackColor = Color.MistyRose;
                row.DefaultCellStyle.ForeColor = Color.DarkRed;
                row.DefaultCellStyle.Font = new Font(grid.Font, FontStyle.Bold);

                row.DefaultCellStyle.SelectionBackColor = Color.IndianRed;
                row.DefaultCellStyle.SelectionForeColor = Color.White;
            }
            // 🟢 PAGO
            else if (status.Equals("Paga", StringComparison.OrdinalIgnoreCase))
            {
                row.DefaultCellStyle.BackColor = Color.Honeydew;
                row.DefaultCellStyle.ForeColor = Color.DarkGreen;

                row.DefaultCellStyle.SelectionBackColor = Color.Green;
                row.DefaultCellStyle.SelectionForeColor = Color.White;
            }

            else if (status.Equals("Paga Em Atraso", StringComparison.OrdinalIgnoreCase))
            {
                row.DefaultCellStyle.BackColor = Color.Honeydew;
                row.DefaultCellStyle.ForeColor = Color.DarkGreen;

                row.DefaultCellStyle.SelectionBackColor = Color.Green;
                row.DefaultCellStyle.SelectionForeColor = Color.White;
            }

            else if (status.Equals("Paga Antecipado", StringComparison.OrdinalIgnoreCase))
            {
                row.DefaultCellStyle.BackColor = Color.Honeydew;
                row.DefaultCellStyle.ForeColor = Color.DarkGreen;

                row.DefaultCellStyle.SelectionBackColor = Color.Green;
                row.DefaultCellStyle.SelectionForeColor = Color.White;
            }
            // 🟡 A VENCER
            else if (status.Equals("A Vencer", StringComparison.OrdinalIgnoreCase))
            {
                row.DefaultCellStyle.BackColor = Color.LemonChiffon;
                row.DefaultCellStyle.ForeColor = Color.DarkGoldenrod;

                row.DefaultCellStyle.SelectionBackColor = Color.Goldenrod;
                row.DefaultCellStyle.SelectionForeColor = Color.White;
            }
            // 🔵 PADRÃO
            else
            {
                row.DefaultCellStyle.BackColor = Color.White;
                row.DefaultCellStyle.ForeColor = Color.Black;
                row.DefaultCellStyle.Font = grid.Font;
            }
        }

        private void AtualizarSaldoFinal()
        {
            decimal saldo = totalRecebido + totalServicos - totalDespesas;

            lblSaldoFinal.Text = $"Saldo Final: {saldo:C}";

            // cor dinâmica
            if (saldo < 0)
                lblSaldoFinal.ForeColor = Color.Red;
            else
                lblSaldoFinal.ForeColor = Color.DarkGreen;
        }


        // ⭐⭐ EDITAR via duplo clique ⭐⭐
        private void Grid_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            int id = Convert.ToInt32(grid.Rows[e.RowIndex].Cells["EmprestimoId"].Value);

            using var f = new FormEditarEmprestimo(id);
            if (f.ShowDialog() == DialogResult.OK)
                LoadGrid();
        }

       private void GerarPdfLivroCaixa()
        {
            var pdf = new PdfLivroCaixa();

            var (ano, mes) = MesSelecionado();

            DateTime mesReferencia;

            bool anoCompleto = false;

            if (ano.HasValue && mes.HasValue)
            {
                // mês específico
                mesReferencia = new DateTime(ano.Value, mes.Value, 1);
            }
            else if (ano.HasValue && !mes.HasValue)
            {
                // 🔥 ANO TODO
                mesReferencia = new DateTime(ano.Value, 1, 1);
                anoCompleto = true;
            }
            else
            {
                // fallback (tudo vazio)
                mesReferencia = DateTime.Now;
            }
            string pasta = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "Livro Caixa");

            Directory.CreateDirectory(pasta);

            // 🔥 corrigir data (SEM /)
            string caminho = Path.Combine(
                pasta,
                $"Caixa_{DateTime.Now:dd-MM-yyyy}.pdf");

            // 🔥 CORREÇÃO DO ERRO DE CAST
            var dados = ((System.Collections.IEnumerable)grid.DataSource)
                .Cast<dynamic>()
                .ToList();

            pdf.GeneratePdf(
                dados,
                despesaRepo.GetAll(),
                servicoRepo.GetAll(),
                totalPrevisto,
                totalRecebido,
                totalEmAtraso,
                totalServicos,
                totalDespesas,
                totalFinalizados,
                totalAtrasados,
                parcelasReceber,
                parcelasPagasNoMes,
                caminho,
                mesReferencia,
                anoCompleto
            );

            new FormPdfViewer(caminho).ShowDialog();
        }

    }
}

