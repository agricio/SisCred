using System;
using System.Linq;
using System.Windows.Forms;
using CrudApp.Repositories;
using System.Drawing;

namespace CrudApp.Forms
{
    public class FormMain : Form
    {
        private readonly ClienteRepository repo = new ClienteRepository();
        private readonly EmprestimoRepository emprestimoRepo = new EmprestimoRepository();
        private readonly ParcelaRepository parcelaRepo = new ParcelaRepository();
        private DataGridView grid;
        private TextBox txtPesquisa;

        public FormMain()
        {
            InitializeComponent();
            LoadGrid();
        }

        private void InitializeComponent()
        {
            this.Text = "Speed Cred - Clientes";
            this.ClientSize = new System.Drawing.Size(1072, 600);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Icon = new Icon("app.ico");

            // ===== PAINEL SUPERIOR =====
            var panelTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                Padding = new Padding(10),
                BackColor = System.Drawing.Color.WhiteSmoke
            };

            // 🔍 CAMPO DE PESQUISA
            txtPesquisa = new TextBox
            {
                Width = 300,
                Top = 20,
                Left = 20,
                PlaceholderText = "Pesquisar (Nome / RG / CPF)"
            };
            txtPesquisa.TextChanged += TxtPesquisa_TextChanged;

            // ➕ BOTÃO ADICIONAR
            var btnAdd = new Button
            {
                Text = "Adicionar Cliente",
                Left = 330,
                Top = 20,
                Width = 120
            };
            btnAdd.Click += BtnAdd_Click;

            panelTop.Controls.Add(txtPesquisa);
            panelTop.Controls.Add(btnAdd);

            // ===== PAINEL CENTRAL =====
            var panelCenter = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10) // <-- Afasta a tabela das bordas
            };

            // ===== GRID =====
            grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            // COLUNAS
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "ID", DataPropertyName = "Id", Name = "Id", Width = 40 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Nome", DataPropertyName = "Nome", Name = "Nome", Width = 150 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "CPF", DataPropertyName = "Cpf", Name = "Cpf", Width = 100 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "RG", DataPropertyName = "Rg", Name = "Rg", Width = 120 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Telefone", DataPropertyName = "Telefone", Name = "Telefone", Width = 120 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Cidade", DataPropertyName = "Cidade", Name = "Cidade", Width = 120 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Banco", DataPropertyName = "Banco", Name = "Banco", Width = 120 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Contratos", DataPropertyName = "QtdContratos", Name = "Contratos", Width = 80 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Score", DataPropertyName = "ClienteScore", Name = "Score", Width = 80 });
            grid.Columns.Add(new DataGridViewButtonColumn  { HeaderText = "Excluir", Text = "Excluir", UseColumnTextForButtonValue = true, Width = 80 });

            // EVENTO: Botão excluir
            grid.CellClick += Grid_CellClick;

            // ⭐ EVENTO: Duplo clique para editar
            grid.CellDoubleClick += Grid_CellDoubleClick;

            panelCenter.Controls.Add(grid);

            this.Controls.Add(panelCenter);
            this.Controls.Add(panelTop);
        }

        // ============== BOTÃO ADICIONAR ==============
        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            using var f = new FormNovoCliente();
            if (f.ShowDialog() == DialogResult.OK)
                LoadGrid();
        }

        // ============== CARREGAR GRID ==============
        private void LoadGrid()
        {
            AtualizarScoreClientes();

            var lista = repo.GetAll();
            
            grid.DataSource = lista.Select(c => new
            {
                c.Id,
                c.Nome,
                c.Cpf,
                c.Rg,
                c.Telefone,
                c.Cidade,
                c.Banco, 
                c.QtdContratos,
                c.ClienteScore
            }).ToList();
        }

        private void AtualizarScoreClientes()
        {
            var clientes = repo.GetAll();
            var todosEmprestimos = emprestimoRepo.GetAll();

            foreach (var cliente in clientes)
            {
                var emprestimosCliente = todosEmprestimos
                    .Where(e => e.ClienteId == cliente.Id)
                    .ToList();

                if (!emprestimosCliente.Any())
                {
                    if (cliente.ClienteScore != "Neutro")
                    {
                        cliente.ClienteScore = "Neutro";
                        repo.Update(cliente);
                    }
                    continue;
                }

                var parcelas = emprestimosCliente
                    .SelectMany(e => parcelaRepo.GetByEmprestimo(e.Id))
                    .ToList();

                // 🔹 Considerar apenas parcelas vencidas
                var parcelasVencidas = parcelas
                    .Where(p => p.Vencimento.HasValue &&
                                p.Vencimento.Value.Date <= DateTime.Today)
                    .ToList();

                if (parcelasVencidas.Count == 0)
                {
                    if (cliente.ClienteScore != "Neutro")
                    {
                        cliente.ClienteScore = "Neutro";
                        repo.Update(cliente);
                    }
                    continue;
                }

                var statusPagos = new[]
                {
                    "Paga",
                    "Paga Antecipado",
                    "Paga Em Atraso"
                };

                int totalParcelas = parcelasVencidas.Count;

                int parcelasPagas = parcelasVencidas.Count(p =>
                    p.Situacao != null &&
                    statusPagos.Any(s =>
                        string.Equals(p.Situacao, s, StringComparison.OrdinalIgnoreCase))
                );

                double percentual = (parcelasPagas / (double)totalParcelas) * 100.0;

                string novoScore =
                    percentual == 100 ? "Excelente" :
                    percentual >= 80 ? "Bom" :
                    percentual >= 50 ? "Médio" :
                    "Péssimo";

                if (!string.Equals(cliente.ClienteScore, novoScore, StringComparison.OrdinalIgnoreCase))
                {
                    cliente.ClienteScore = novoScore;
                    repo.Update(cliente);
                }
            }
        }



        // ============== PESQUISA COMBINADA ==============
        private void TxtPesquisa_TextChanged(object? sender, EventArgs e)
        {
            string txt = txtPesquisa.Text.ToLower().Trim();
            var palavras = txt.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var lista = repo.GetAll();

            var filtrado = lista.Where(c =>
                palavras.All(p =>
                    (c.Nome?.ToLower().Contains(p) ?? false) ||
                    (c.Cpf?.ToLower().Contains(p) ?? false) ||
                    (c.Rg?.ToLower().Contains(p) ?? false)
                )
            )
            .Select(c => new
            {
                c.Id,
                c.Nome,
                c.Cpf,
                c.Rg,
                c.Telefone
            })
            .ToList();

            grid.DataSource = filtrado;
        }

        // ============== DUAS VEZES CLIQUE PARA EDITAR ==============
        private void Grid_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            int id = Convert.ToInt32(grid.Rows[e.RowIndex].Cells["Id"].Value);

            using var f = new FormEditarCliente(id);
            if (f.ShowDialog() == DialogResult.OK)
                LoadGrid();
        }

        // ============== BOTÃO EXCLUIR ==============
        private void Grid_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            string coluna = grid.Columns[e.ColumnIndex].HeaderText;

            if (coluna == "Excluir")
            {
                int id = Convert.ToInt32(grid.Rows[e.RowIndex].Cells["Id"].Value);

                if (MessageBox.Show("Excluir cliente?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    repo.Delete(id);
                    LoadGrid();
                }
            }
        }
    }
}
