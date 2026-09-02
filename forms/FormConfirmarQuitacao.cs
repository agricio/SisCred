using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CrudApp.Repositories;
using CrudApp.Models;

namespace CrudApp.Forms
{
   public class FormConfirmarQuitacao : Form
{
    private DataGridView dgv;
    private Label lblTotal;
    private Label lblDetalhes;
    private Button btnConfirmar;
    private Button btnCancelar;
    private TextBox txtDesconto;
    private Button btnAplicarDesconto;
    private readonly ParcelaRepository parcelaRepo = new ParcelaRepository();
    private List<Parcela> parcelasAbertas;
    private int emprestimoId;
    public bool Confirmado { get; private set; }
    public double ValorQuitacaoFinal { get; private set; }

    public FormConfirmarQuitacao(int emprestimoId)
    {
        CriarControles();
        CarregarParcelas(emprestimoId);
    }

        private void CriarControles()
        {
            this.Icon = new Icon("app.ico");
            Text = "Quitação Antecipada";
            Width = 700;
            Height = 570;
            StartPosition = FormStartPosition.CenterParent;
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.AutoScaleMode = AutoScaleMode.None;
            this.AutoSize = false;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            txtDesconto = new TextBox();
            txtDesconto.Width = 100;
            txtDesconto.Location = new Point(20, 10);

            // permitir apenas números
            txtDesconto.KeyPress += (s, e) =>
            {
                if (!char.IsDigit(e.KeyChar) && e.KeyChar != ',' && e.KeyChar != (char)Keys.Back)
                {
                    e.Handled = true;
                }
            };

            dgv = new DataGridView();
            dgv.Dock = DockStyle.Top;
            dgv.Height = 250;
            dgv.AutoGenerateColumns = false;
            dgv.ReadOnly = true;
            Padding = new Padding(20);
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Parcela",
                DataPropertyName = "Parcela"
            });

            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Vencimento",
                DataPropertyName = "Vencimento",
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" }
            });

            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Valor Base",
                DataPropertyName = "ValorPrestacao",
                DefaultCellStyle = new DataGridViewCellStyle { Format = "C" }
            });

            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Multa",
                Name = "Multa",
                DefaultCellStyle = new DataGridViewCellStyle { Format = "C" }
            });

            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Juros",
                Name = "Juros",
                DefaultCellStyle = new DataGridViewCellStyle { Format = "C" }
            });

            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Valor Final",
                Name = "ValorFinal",
                DefaultCellStyle = new DataGridViewCellStyle { Format = "C" }
            });

            lblDetalhes = new Label();
            lblDetalhes.Dock = DockStyle.Top;
            lblDetalhes.Padding = new Padding(10);
            lblDetalhes.Height = 110; // altura fixa suficiente
            lblDetalhes.TextAlign = ContentAlignment.MiddleLeft;

            txtDesconto = new TextBox();
            txtDesconto.Width = 100;
            txtDesconto.Location = new Point(20, 10);
            txtDesconto.PlaceholderText = "0.00";
            txtDesconto.KeyPress += (s, e) =>
            {
                if (!char.IsDigit(e.KeyChar) && e.KeyChar != ',' && e.KeyChar != (char)Keys.Back)
                    e.Handled = true;
            };

            btnAplicarDesconto = new Button();
            btnAplicarDesconto.Text = "Aplicar Desconto";
            btnAplicarDesconto.Width = 130;
            btnAplicarDesconto.Height = 30;
            btnAplicarDesconto.Location = new Point(130, 8);
            btnAplicarDesconto.Click += BtnAplicarDesconto_Click;

            Panel panelDesconto = new Panel();
            panelDesconto.Dock = DockStyle.Top;
            panelDesconto.Height = 40;

            panelDesconto.Controls.Add(txtDesconto);
            panelDesconto.Controls.Add(btnAplicarDesconto);

            lblTotal = new Label();
            lblTotal.Dock = DockStyle.Top;
            lblTotal.Height = 35;
            lblTotal.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lblTotal.TextAlign = ContentAlignment.MiddleCenter;

            Panel panelBotoes = new Panel();
            panelBotoes.Dock = DockStyle.Bottom;
            panelBotoes.Height = 60;

            btnConfirmar = new Button();
            btnConfirmar.Text = "Confirmar";
            btnConfirmar.Width = 120;
            btnConfirmar.Height = 35;
            btnConfirmar.Location = new Point(200, 10);
            btnConfirmar.Click += btnConfirmar_Click;
       
            btnCancelar = new Button();
            btnCancelar.Text = "Cancelar";
            btnCancelar.Width = 120;
            btnCancelar.Height = 35;
            btnCancelar.Location = new Point(350, 10);
            btnCancelar.Click += (s, e) => Close();

            panelBotoes.Controls.Add(btnConfirmar);
            panelBotoes.Controls.Add(btnCancelar);

            Controls.Add(panelBotoes);
            Controls.Add(lblTotal);
            Controls.Add(panelDesconto); // fica logo abaixo de lblDetalhes
            Controls.Add(lblDetalhes);
            
            Controls.Add(dgv);
        }

        private void CarregarParcelas(int emprestimoId)
        {
            var parcelas = parcelaRepo.GetByEmprestimo(emprestimoId);

            parcelasAbertas = parcelas
                .Where(p =>
                    !string.Equals(p.Situacao, "Paga", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(p.Situacao, "Paga Em Atraso", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(p.Situacao, "Paga Antecipado", StringComparison.OrdinalIgnoreCase))
                .OrderBy(p => p.Vencimento)
                .ToList();

            if (!parcelasAbertas.Any())
            {
                MessageBox.Show("Contrato já está quitado.");
                Close();
                return;
            }

            // 🔹 força o grid a criar as linhas
            dgv.Refresh();

            CalcularQuitacao();
        }

    private void CalcularQuitacao()
        {
            double taxa = 5;
            double mora = 0.5;
            double capitalRestante = 0;
            double totalMulta = 0;
            double totalJuros = 0;
            double total = 0;

            dgv.Rows.Clear();

            foreach (var parcela in parcelasAbertas)
            {
                double valor = parcela.ValorPrestacao ?? 0;

                capitalRestante += valor;

                double multa = 0;
                double juros = 0;

                if (parcela.Situacao == "Em Atraso" && parcela.Vencimento.HasValue)
                {
                    int dias = (DateTime.Today - parcela.Vencimento.Value.Date).Days;
                    if (dias < 0) dias = 0;

                    multa = valor * (taxa / 100);
                    juros = valor * (mora / 100) * dias;
                }

                double valorFinal = valor + multa + juros;

                int rowIndex = dgv.Rows.Add(
                    parcela.NParcelas,
                    parcela.Vencimento,
                    valor,
                    multa,
                    juros,
                    valorFinal
                );

                dgv.Rows[rowIndex].Tag = parcela;

                totalMulta += multa;
                totalJuros += juros;
                total += valorFinal;
            }

            double jurosRestantes = parcelasAbertas.Sum(p => p.Juros ?? 0);

            // 🔹 Intervalo total do contrato restante
            DateTime primeiraAberta = parcelasAbertas.Min(p => p.Vencimento ?? DateTime.Today);
            DateTime ultimaAberta = parcelasAbertas.Max(p => p.Vencimento ?? DateTime.Today);

            int diasTotaisRestantes = (ultimaAberta.Date - primeiraAberta.Date).Days;

            if (diasTotaisRestantes <= 0)
                diasTotaisRestantes = 1;

            double jurosPorDia = jurosRestantes / diasTotaisRestantes;

            // 🔹 Dias corridos desde a última paga
            var statusPagos = new[]
            {
                "Paga",
                "Paga Antecipado",
                "Paga em Atraso"
            };

            var ultimaPaga = parcelasAbertas
                .Where(p => statusPagos
                    .Any(s => string.Equals(p.Situacao, s, StringComparison.OrdinalIgnoreCase)))
                .OrderByDescending(p => p.Vencimento)
                .FirstOrDefault();

            int diasCorridos;

            if (ultimaPaga != null && ultimaPaga.Pagamento.HasValue)
                diasCorridos = (DateTime.Today - ultimaPaga.Pagamento.Value.Date).Days;
            else
                diasCorridos = (DateTime.Today - primeiraAberta.Date).Days;

            if (diasCorridos < 0)
                diasCorridos = 0;

            double jurosProporcional = jurosPorDia * diasCorridos;
            double valorQuitacao = capitalRestante + jurosProporcional;

            ValorQuitacaoFinal = total + jurosProporcional;

            lblTotal.Text = $"Valor total para quitação: {ValorQuitacaoFinal:C}";

            lblDetalhes.Text =
                $"Capital restante: {total:C}\n" +
                $"Dias corridos: {diasCorridos}\n"+
                $"Juros por dia: {jurosPorDia:C}\n" +
                $"Juros proporcinal: {jurosProporcional:C}\n" +
                $"Parcelas em aberto: {parcelasAbertas.Count}";
        }

        private void BtnAplicarDesconto_Click(object sender, EventArgs e)
        {
            if (!double.TryParse(txtDesconto.Text, out double desconto))
            {
                MessageBox.Show("Digite um valor de desconto válido.");
                return;
            }

            double valorComDesconto = ValorQuitacaoFinal - desconto;

            if (valorComDesconto < 0)
                valorComDesconto = 0;

            lblTotal.Text = $"Valor total para quitação: {valorComDesconto:C}";

            ValorQuitacaoFinal = valorComDesconto;
        }
    
        private void btnConfirmar_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgv.Rows)
            {
                if (row.IsNewRow)
                    continue;

                if (row.Cells["ValorFinal"].Value == null)
                    continue;

                int parcelaId = Convert.ToInt32(row.Cells[0].Value);

                double multa = row.Cells["Multa"].Value != null 
                    ? Convert.ToDouble(row.Cells["Multa"].Value) 
                    : 0;

                double juros = row.Cells["Juros"].Value != null 
                    ? Convert.ToDouble(row.Cells["Juros"].Value) 
                    : 0;

                double valorFinal = Convert.ToDouble(row.Cells["ValorFinal"].Value);

                var parcela = parcelasAbertas.FirstOrDefault(p => p.Id == parcelaId);

                if (parcela == null)
                    continue;

                parcela.Juros = juros;
                parcela.ValorPrestacao = valorFinal;
                parcela.Situacao = "Paga Antecipado";
                parcela.Pagamento = DateTime.Today;

                parcelaRepo.Update(parcela);
            }

            Confirmado = true;
            DialogResult = DialogResult.OK;

            Close();
        }
    }
}