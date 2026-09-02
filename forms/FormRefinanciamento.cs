using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CrudApp.Models;

namespace CrudApp.Forms
{
    public class FormRefinanciamento : Form
    {
        // ===== CONTRATO ATUAL =====
        TextBox txtContratoAtual;
        TextBox txtValorContratoAtual;
        TextBox txtParcelasTotal;
        TextBox txtParcelasPagas;
        TextBox txtSaldoDevedor;
        TextBox txtJurosPendentes;
        TextBox txtAgioAtual;

        // ===== TIPO =====
        RadioButton rbSemTroco;
        RadioButton rbComTroco;

        // ===== NOVO CONTRATO =====
        ComboBox cbParcelas;
        ComboBox cbTipo;
        TextBox txtTaxa;
        TextBox txtValorLiberado;
        TextBox txtTroco;
        TextBox txtValorFinal;

        Button btnSimular;
        Button btnConfirmar;
        Button btnCancelar;

        private readonly Emprestimo _emprestimo;

        public enum TipoRefinanciamento
        {
            SemTroco,
            ComTroco
        }

        // 🔹 VALORES DE RETORNO
        public double NovoValorContrato { get; private set; }
        public double TrocoGerado { get; private set; }
        public TipoRefinanciamento TipoSelecionado { get; private set; }

        public FormRefinanciamento(Emprestimo emprestimo)
        {
            _emprestimo = emprestimo;

            InitializeComponent();
            CarregarContratoAtual();
        }

        // ========================= LAYOUT =========================
        private void InitializeComponent()
        {
            Text = "Refinanciamento de Contrato";
            Width = 720;
            Height = 820;
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            int top = 15;

            // ===== CONTRATO ATUAL =====
            GroupBox gbAtual = new GroupBox
            {
                Text = "Contrato Atual",
                Left = 15,
                Top = top,
                Width = 660,
                Height = 210
            };
            Controls.Add(gbAtual);

            int y = 25;

            txtContratoAtual = Campo(gbAtual, "Contrato:", ref y);
            txtValorContratoAtual = Campo(gbAtual, "Valor Contrato:", ref y);
            txtParcelasTotal = Campo(gbAtual, "Parcelas Totais:", ref y);
            txtParcelasPagas = Campo(gbAtual, "Parcelas Pagas:", ref y);
            txtSaldoDevedor = Campo(gbAtual, "Saldo Devedor:", ref y);
            txtJurosPendentes = Campo(gbAtual, "Juros Pendentes:", ref y);
            txtAgioAtual = Campo(gbAtual, "Ágio / Multas:", ref y);

            SetReadOnly(gbAtual);

            // ===== TIPO =====
            GroupBox gbTipo = new GroupBox
            {
                Text = "Tipo de Refinanciamento",
                Left = 15,
                Top = gbAtual.Bottom + 10,
                Width = 660,
                Height = 80
            };
            Controls.Add(gbTipo);

            rbSemTroco = new RadioButton
            {
                Text = "Sem Troco",
                Left = 40,
                Top = 35,
                Checked = true
            };

            rbComTroco = new RadioButton
            {
                Text = "Com Troco",
                Left = 200,
                Top = 35
            };

            rbSemTroco.CheckedChanged += Tipo_CheckedChanged;
            rbComTroco.CheckedChanged += Tipo_CheckedChanged;

            gbTipo.Controls.AddRange(new Control[] { rbSemTroco, rbComTroco });

            // ===== NOVO CONTRATO =====
            GroupBox gbNovo = new GroupBox
            {
                Text = "Novo Contrato",
                Left = 15,
                Top = gbTipo.Bottom + 10,
                Width = 660,
                Height = 260
            };
            Controls.Add(gbNovo);

            y = 25;

            cbParcelas = Combo(gbNovo, "Parcelas:", ref y, 6, 12, 18, 24);
            cbTipo = Combo(gbNovo, "Tipo:", ref y, "Mensal", "Quinzenal", "Semanal");
            txtTaxa = Campo(gbNovo, "Taxa de Juros (%):", ref y);
            txtValorLiberado = Campo(gbNovo, "Valor Liberado:", ref y);
            txtTroco = Campo(gbNovo, "Troco:", ref y);
            txtValorFinal = Campo(gbNovo, "Valor Financiado:", ref y);

            txtValorFinal.ReadOnly = true;

            // ===== BOTÕES =====
            btnSimular = new Button
            {
                Text = "Simular",
                Width = 120,
                Height = 35,
                Left = 160,
                Top = gbNovo.Bottom + 20
            };
            btnSimular.Click += BtnSimular_Click;

            btnConfirmar = new Button
            {
                Text = "Confirmar",
                Width = 120,
                Height = 35,
                Left = 300,
                Top = gbNovo.Bottom + 20
            };
            btnConfirmar.Click += BtnConfirmar_Click;

            btnCancelar = new Button
            {
                Text = "Cancelar",
                Width = 120,
                Height = 35,
                Left = 440,
                Top = gbNovo.Bottom + 20
            };
            btnCancelar.Click += (s, e) => Close();

            Controls.AddRange(new Control[] { btnSimular, btnConfirmar, btnCancelar });

            AjustarLayoutPorTipo();
        }

        // ========================= EVENTOS =========================
        private void Tipo_CheckedChanged(object? sender, EventArgs e)
        {
            AjustarLayoutPorTipo();
        }

        private void BtnSimular_Click(object? sender, EventArgs e)
        {
            if (!double.TryParse(txtTaxa.Text, out double taxa) ||
                !double.TryParse(txtValorLiberado.Text, out double liberado))
            {
                MessageBox.Show("Informe taxa e valor liberado.");
                return;
            }

            double saldoAtual =
                _emprestimo.ValorContrato +
                _emprestimo.Agio;

            double troco = 0;

            if (rbComTroco.Checked)
            {
                if (liberado <= saldoAtual)
                {
                    MessageBox.Show("Para gerar troco, o valor liberado deve ser maior que o saldo.");
                    return;
                }
                troco = liberado - saldoAtual;
            }

            txtTroco.Text = troco.ToString("F2");

            double valorFinanciado = saldoAtual + troco;
            txtValorFinal.Text = valorFinanciado.ToString("F2");
        }

        private void BtnConfirmar_Click(object? sender, EventArgs e)
        {
            if (!double.TryParse(txtValorFinal.Text, out double valorFinal))
            {
                MessageBox.Show("Simule antes de confirmar.");
                return;
            }

            NovoValorContrato = valorFinal;
            TrocoGerado = double.Parse(txtTroco.Text);
            TipoSelecionado = rbComTroco.Checked
                ? TipoRefinanciamento.ComTroco
                : TipoRefinanciamento.SemTroco;

            DialogResult = DialogResult.OK;
            Close();
        }

        // ========================= DADOS =========================
        private void CarregarContratoAtual()
        {
            txtContratoAtual.Text = _emprestimo.Contrato.ToString();
            txtValorContratoAtual.Text = _emprestimo.ValorContrato.ToString("F2");
            txtParcelasTotal.Text = _emprestimo.Parcelas.ToString();
            txtParcelasPagas.Text = "0"; // ajuste se tiver controle
            txtSaldoDevedor.Text = _emprestimo.ValorContrato.ToString("F2");
            txtJurosPendentes.Text = _emprestimo.TotalJuros.ToString("F2");
            txtAgioAtual.Text = _emprestimo.Agio.ToString("F2");
        }

        private void AjustarLayoutPorTipo()
        {
            txtTroco.ReadOnly = rbSemTroco.Checked;
            if (rbSemTroco.Checked)
                txtTroco.Text = "0,00";
        }

        // ========================= HELPERS =========================
        TextBox Campo(Control pai, string label, ref int y)
        {
            var lbl = new Label { Text = label, Left = 20, Top = y + 5, Width = 150 };
            var txt = new TextBox { Left = 180, Top = y, Width = 420 };
            pai.Controls.Add(lbl);
            pai.Controls.Add(txt);
            y += 30;
            return txt;
        }

        ComboBox Combo(Control pai, string label, ref int y, params object[] items)
        {
            var lbl = new Label { Text = label, Left = 20, Top = y + 5, Width = 150 };
            var cb = new ComboBox
            {
                Left = 180,
                Top = y,
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cb.Items.AddRange(items);
            cb.SelectedIndex = 0;
            pai.Controls.Add(lbl);
            pai.Controls.Add(cb);
            y += 30;
            return cb;
        }

        void SetReadOnly(Control pai)
        {
            foreach (Control c in pai.Controls)
                if (c is TextBox t)
                    t.ReadOnly = true;
        }
    }
}
