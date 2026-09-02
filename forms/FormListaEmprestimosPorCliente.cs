using System;
using System.Linq;
using System.Windows.Forms;
using CrudApp.Repositories;
using System.Drawing;

namespace CrudApp.Forms
{
    public class FormListaEmprestimosPorCliente : Form
    {
    private readonly EmprestimoRepository repo = new EmprestimoRepository();
    private readonly ParcelaRepository parcelaRepo = new ParcelaRepository();
    private DataGridView grid;
 
   
    // 🔹 PAINÉIS
    private FlowLayoutPanel panelTop;
    private Panel panelInfo;
    private Panel panelMain;

    // 🔹 LABELS
    private Label lblFinalizados;
    private Label lblAvencer;
    private Label lblTotalAgio;
    private Label lblTotalLiberado;
    private Label lblTotalJuros;
    private Label lblScoreCliente;
    private readonly int _clienteId;

    public FormListaEmprestimosPorCliente(int clienteId)
    {
        _clienteId = clienteId;

        InitializeComponent();
        LoadGrid();
    }

       private void InitializeComponent()
        {
            this.Text = "SiS Cred - Historico do Cliente";
            this.Icon = new Icon("app.ico");
            this.ClientSize = new Size(1300, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Padding = new Padding(15);

            // 🔹 LABELS
            lblFinalizados = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.DarkGreen
            };

            lblAvencer = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.DarkOrange
            };

            lblTotalAgio = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.Black
            };

            lblScoreCliente = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.DarkBlue
            };

            lblTotalLiberado = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.DarkGray
            };

            lblTotalJuros = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.Gray
            };

            // 🔹 PAINEL ESQUERDO
            var panelLeft = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true,
                WrapContents = false,
                //Margin = new Padding(0, 0, 550, 0)
            };
            
            panelLeft.Controls.Add(lblScoreCliente);
            panelLeft.Controls.Add(lblFinalizados);
            panelLeft.Controls.Add(lblAvencer);
            //panelLeft.Controls.Add(lblTotalJuros);

            // 🔹 PAINEL DIREITO
            var panelRight = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true
            };
            //panelRight.Controls.Add(lblFinalizados);
            //panelRight.Controls.Add(lblAvencer);

            var panelSpacer = new Panel
            {
                Dock = DockStyle.Fill,
                MinimumSize = new Size(850,0)
            };
    
            // 🔹 PAINEL INFO (CONTÊM LEFT + RIGHT)
            panelInfo = new Panel
            {
                Dock = DockStyle.Top,
                Height = 90,
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
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                BorderStyle = BorderStyle.FixedSingle
            };

            grid.CellDoubleClick += Grid_CellDoubleClick;
            grid.RowPrePaint += Grid_RowPrePaint;

            panelMain = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(5)
            };
            panelMain.Controls.Add(grid);

            // ⚠️ ORDEM FINAL
            this.Controls.Add(panelMain);
            this.Controls.Add(panelInfo);
            this.Controls.Add(panelTop);
        }


        private void LoadGrid()
        {
            
            grid.Columns.Clear();


            int anoAtual = DateTime.Now.Year;

            var data = repo.GetAll()
    
                .Where(x => x.ClienteId == _clienteId)
                .ToList();

            double totalAgio = data.Sum(x => x.Agio);
            double totalLiberado = data.Sum(x => x.Liberado);
            double totalJuros = data.Sum(x => x.TotalJuros);
            
            var clienteRepo = new ClienteRepository();
            var cliente = clienteRepo.GetById(_clienteId);

            var score = cliente?.ClienteScore;

            lblScoreCliente.Text = $"Score: {score}";

            switch (score)
            {
                case "Bom":
                    lblScoreCliente.ForeColor = Color.Green;
                    break;
                case "Médio":
                    lblScoreCliente.ForeColor = Color.Orange;
                    break;
                case "Péssimo":
                    lblScoreCliente.ForeColor = Color.Red;
                    break;
            }
 
            // Verificador de de parcela em atraso

            foreach (var emp in data)
            {
                string novaSituacao = parcelaRepo.ObterSituacaoDoEmprestimo(emp.Id);

                if (!string.Equals(emp.Situacao, novaSituacao, StringComparison.OrdinalIgnoreCase))
                {
                    emp.Situacao = novaSituacao;
                    repo.Update(emp);
                }

            }

            // Contador de contratos quitados

            var contratosFinalizados = data
                .Where(x => x.Situacao != null &&
                            x.Situacao.Equals("Quitado", StringComparison.OrdinalIgnoreCase))
                .ToList();

            int totalFinalizados = contratosFinalizados.Count;
            //double valorTotalFinalizados = contratosFinalizados.Sum(x => x.ValorContrato);

            // Contador de contratos a vencer

            var contratosAvencer = data
                .Where(x => x.Situacao != null &&
                            x.Situacao.Equals("A Vencer", StringComparison.OrdinalIgnoreCase))
                .ToList();

            int totalAvencer = contratosAvencer.Count;
            //double valorTotalAvencer = contratosAvencer.Sum(x => x.ValorContrato);

            lblFinalizados.Text = $"Contratos Finalizados: {totalFinalizados}";
            lblAvencer.Text = $"Contratos A Vencer: {totalAvencer}";

            var lista = data.Select(x =>
            {
                var cliente = clienteRepo.GetById(x.ClienteId);
                bool atraso = parcelaRepo.TemParcelaEmAtraso(x.Id);

                return new
                {
                    x.Id,
                    x.Contrato,
                    Cliente = cliente?.Nome ?? "N/D",
                    Cpf = cliente?.Cpf ?? "N/D",
                    Contatos = cliente?.QtdContratos,
                    Cliente_Score = cliente?.ClienteScore,
                    x.Liberado,
                    Agio = x.Agio,
                    Juros = x.TotalJuros,
                    x.Parcelas,
                    Codigo = x.Codigo.ToString("D3"),
                    x.Averbacao,
                    x.Situacao,
                    Vencimento = x.Vencimento?.ToString("dd/MM/yyyy") ?? "",
                    Quitacao = x.Quitacao?.ToString("dd/MM/yyyy") ?? "",
                    x.TipoQuitacao,
                    EmAtraso = x.Situacao == "Em Atraso"  // CAMPO OCULTO
                };
            }).ToList();
            

            grid.DataSource = lista;


            // esconder a coluna auxiliar
            grid.Columns["EmAtraso"].Visible = false;
            
            var colExcluir = new DataGridViewButtonColumn
            {
                Text = "Excluir",
                Name = "Delete",
                UseColumnTextForButtonValue = true,
                Width = 80
            };

            grid.Columns.Add(colExcluir);

            grid.CellContentClick -= Grid_CellContentClick;
            grid.CellContentClick += Grid_CellContentClick;

            lblTotalAgio.Text = $"Total Agio: {totalAgio:C2}";
            lblTotalLiberado.Text = $"Total Liberado: {totalLiberado:C2}";
            lblTotalJuros.Text = $"Total Juros: {totalJuros:C2}";
            
        }

        private void Grid_RowPrePaint(object? sender, DataGridViewRowPrePaintEventArgs e)
        {
            var row = grid.Rows[e.RowIndex];

            bool emAtraso = false;

            if (row.Cells["EmAtraso"] != null &&
                row.Cells["EmAtraso"].Value != DBNull.Value)
            {
                emAtraso = Convert.ToBoolean(row.Cells["EmAtraso"].Value);
            }

            if (emAtraso)
            {
                row.DefaultCellStyle.BackColor = Color.MistyRose;
                row.DefaultCellStyle.ForeColor = Color.DarkRed;
                row.DefaultCellStyle.Font = new Font(grid.Font, FontStyle.Bold);

                row.DefaultCellStyle.SelectionBackColor = Color.IndianRed;
                row.DefaultCellStyle.SelectionForeColor = Color.White;
            }
            else
            {
                // limpa estilo quando não está em atraso
                row.DefaultCellStyle.BackColor = Color.White;
                row.DefaultCellStyle.ForeColor = Color.Black;
                row.DefaultCellStyle.Font = grid.Font;
            }
        }


        // ⭐⭐ EDITAR via duplo clique ⭐⭐
        private void Grid_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            int id = Convert.ToInt32(grid.Rows[e.RowIndex].Cells["Id"].Value);

            using var f = new FormEditarEmprestimo(id);
            if (f.ShowDialog() == DialogResult.OK)
                LoadGrid();
        }

        // EXCLUIR
        private void Grid_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (grid.Columns[e.ColumnIndex].Name == "Delete")
            {
                int id = Convert.ToInt32(grid.Rows[e.RowIndex].Cells["Id"].Value);

                if (MessageBox.Show("Excluir empréstimo?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    repo.Delete(id);
                    LoadGrid();
                }
            }
        }
    }
}
