using System;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using CrudApp.Repositories;

namespace CrudApp.Forms
{
    public class FormListaDespesas : Form
    {
        DataGridView grid;
        DateTimePicker dtMes;
        Label lblTotal;
        Button btnNovo;
         int despesaSelecionadaId = 0;
        
        private (int ano, int mes) MesSelecionado()
        {
            return (dtMes.Value.Year, dtMes.Value.Month);
        }
        
        DespesaRepository repo = new DespesaRepository();

        public FormListaDespesas()
        {
            InitializeComponent();
            CarregarMesAtual();
           
        }

        private void InitializeComponent()
        {
            this.Icon = new Icon("app.ico");
            Text = "Despesas Mensais";
            Width = 700;
            Height = 420;
            StartPosition = FormStartPosition.CenterScreen;
        
            Label lblMes = new Label
            {
                Text = "Mês:",
                Left = 20,
                Top = 20
            };

            dtMes = new DateTimePicker
            {
                Left = 70,
                Top = 15,
                Width = 120,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "MM/yyyy",
                ShowUpDown = true
            };

            Button btnFiltrar = new Button
            {
                Text = "Filtrar",
                Left = 210,
                Top = 13,
                Width = 90
            };
            btnFiltrar.Click += (s, e) => CarregarPorMes();

            btnNovo = new Button
            {
                Text = "Adicionar",
                Left = 310,
                Top = 13,
                Width = 90
            };
            
            btnNovo.Click += (s, e) =>
            {
                using var form = new FormCadastroDespesa();
                if (form.ShowDialog() == DialogResult.OK)
                    CarregarPorMes(); // recarrega a grid depois de salvar
            };

            grid = new DataGridView
            {
                Left = 20,
                Top = 55,
                Width = 640,
                Height = 260,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,

                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            grid.CellClick += (s, e) =>
            {
                if (e.RowIndex >= 0)
                    despesaSelecionadaId = Convert.ToInt32(
                        grid.Rows[e.RowIndex].Cells["Id"].Value
                    );
            };

            grid.CellDoubleClick += Grid_CellDoubleClick;
            grid.CellContentClick += Grid_CellContentClick;

            lblTotal = new Label
            {
                Left = 20,
                Top = 330,
                Width = 640,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            Controls.Add(dtMes);
            Controls.Add(lblMes);
           
            Controls.Add(btnFiltrar);
            Controls.Add(grid);
            Controls.Add(lblTotal);

            Controls.Add(btnNovo);
        }

        private void CarregarMesAtual()
        {
            var hoje = DateTime.Today;
            dtMes.Value = new DateTime(hoje.Year, hoje.Month, 1);
            CarregarPorMes();
        }
        private void CarregarPorMes()
        {
            var (ano, mes) = MesSelecionado();

            var lista = repo.GetPorMes(ano, mes);

            grid.DataSource = lista.Select(d => new
            {
                d.Id,
                d.Tipo,
                Valor = d.Valor.ToString("C"),
                Vencimento = d.Vencimento.ToString("dd/MM/yyyy"),
                Pagamento = d.DataPagamento?.ToString("dd/MM/yyyy") ?? "-",
                d.Situacao
            }).ToList();

            // Remove coluna de botão se já existir
            if (grid.Columns.Contains("Excluir"))
                grid.Columns.Remove("Excluir");

            // Cria coluna botão
            var colExcluir = new DataGridViewButtonColumn
            {
                Name = "Excluir",
                HeaderText = "",
                Text = " Excluir",
                UseColumnTextForButtonValue = true,
                Width = 70
            };

            grid.Columns.Add(colExcluir);

            // Esconde o ID
            grid.Columns["Id"].Visible = false;

            decimal total = lista.Sum(x => x.Valor);
            lblTotal.Text = $" Total de despesas mês: {total:C}";
        }

        private void Grid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            int id = Convert.ToInt32(
                grid.Rows[e.RowIndex].Cells["Id"].Value
            );

            using var form = new FormCadastroDespesa(id);
            if (form.ShowDialog() == DialogResult.OK)
                CarregarPorMes();
        }

        private void BtnExcluir_Click(object sender, EventArgs e)
        {
            if (despesaSelecionadaId == 0)
            {
                MessageBox.Show("Selecione uma despesa.");
                return;
            }

            if (MessageBox.Show(
                "Deseja excluir esta despesa?",
                "Confirmação",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            repo.Delete(despesaSelecionadaId);
            CarregarPorMes();

            despesaSelecionadaId = 0;
        }

        private void Grid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            if (grid.Columns[e.ColumnIndex].Name != "Excluir")
                return;

            int id = Convert.ToInt32(
                grid.Rows[e.RowIndex].Cells["Id"].Value
            );

            if (MessageBox.Show(
                "Deseja excluir esta despesa?",
                "Confirmação",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            repo.Delete(id);
            CarregarPorMes();
        }

        
    }
}