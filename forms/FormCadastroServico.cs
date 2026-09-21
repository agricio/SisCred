using System;
using System.Globalization;
using System.Windows.Forms;
using CrudApp.Models;
using System.Linq;
using System.Drawing;
using CrudApp.Repositories;

namespace CrudApp.Forms
{
    public class FormCadastroServico : Form
    {
        TextBox txtTipo;
        TextBox txtValor;
        DateTimePicker dtVencimento;
        DateTimePicker dtPagamento;
        CheckBox chkPago;
        ComboBox cbSituacao;
        Button btnSalvar;
        private int servicoId = 0;

        ServicoRepository repo = new ServicoRepository();

        public FormCadastroServico()
        {
            InitializeComponent();
        }

        public FormCadastroServico(int id)
        {
            InitializeComponent();
            CarregarServico(id);
        }

        private void InitializeComponent()
        {
            this.Icon = new Icon("app.ico");
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.AutoScaleMode = AutoScaleMode.None;
            this.AutoSize = false;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            Text = "Adicionar Serviço";
            Width = 420;
            Height = 350;
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            Label lblTipo = new Label { Text = "Tipo:", Left = 30, Top = 30 };
            txtTipo = new TextBox { Left = 150, Top = 25, Width = 200 };

            Label lblValor = new Label { Text = "Valor:", Left = 30, Top = 70 };
            txtValor = new TextBox
            {
                Left = 150,
                Top = 65,
                Width = 120,
                Text = "R$ 0,00"
            };

            // 👉 Máscara de moeda
            txtValor.KeyPress += TxtValor_KeyPress;
            txtValor.TextChanged += TxtValor_TextChanged;

            Label lblVenc = new Label { Text = "Vencimento:", Left = 30, Top = 110 };
            dtVencimento = new DateTimePicker { Left = 150, Top = 105 };

            chkPago = new CheckBox
            {
                Text = "Serviço pago",
                Left = 150,
                Top = 145
            };

            chkPago.CheckedChanged += (s, e) =>
            {
                dtPagamento.Enabled = chkPago.Checked;
                cbSituacao.SelectedItem = chkPago.Checked ? "Paga" : "Pendente";
            };

            Label lblPag = new Label { Text = "Data Pagamento:", Left = 30, Top = 180 };
            dtPagamento = new DateTimePicker
            {
                Left = 150,
                Top = 175,
                Enabled = false
            };

            Label lblSit = new Label { Text = "Situação:", Left = 30, Top = 220 };
            cbSituacao = new ComboBox
            {
                Left = 150,
                Top = 215,
                Width = 120,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cbSituacao.Items.AddRange(new[] { "Pendente", "Paga", "Atrasada" });
            cbSituacao.SelectedIndex = 0;

            btnSalvar = new Button
            {
                Text = "Salvar",
                Left = 150,
                Top = 260,
                Height = 35,
                Width = 120
            };
            btnSalvar.Click += BtnSalvar_Click;

            Controls.AddRange(new Control[]
            {
                lblTipo, txtTipo,
                lblValor, txtValor,
                lblVenc, dtVencimento,
                chkPago,
                lblPag, dtPagamento,
                lblSit, cbSituacao,
                btnSalvar
            });
        }

        // ===================== MÁSCARA DE MOEDA =====================
        private void TxtValor_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
                e.Handled = true;
        }

        private void TxtValor_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtValor.Text)) return;

            string apenasNumeros = new string(txtValor.Text
                .Where(char.IsDigit).ToArray());

            if (string.IsNullOrEmpty(apenasNumeros))
                apenasNumeros = "0";

            decimal valor = decimal.Parse(apenasNumeros) / 100;

            txtValor.TextChanged -= TxtValor_TextChanged;
            txtValor.Text = valor.ToString("C2", CultureInfo.GetCultureInfo("pt-BR"));
            txtValor.SelectionStart = txtValor.Text.Length;
            txtValor.TextChanged += TxtValor_TextChanged;
        }

        // ===================== CARREGAR DESPESA =====================
        private void CarregarServico(int id)
        {
            var d = repo.GetById(id);
            if (d == null) return;

            servicoId = d.Id;
            txtTipo.Text = d.Tipo;
            txtValor.Text = d.Valor.ToString("C2", CultureInfo.GetCultureInfo("pt-BR"));
            dtVencimento.Value = d.Vencimento;

            if (d.DataPagamento.HasValue)
            {
                chkPago.Checked = true;
                dtPagamento.Value = d.DataPagamento.Value;
            }

            cbSituacao.SelectedItem = d.Situacao;
            btnSalvar.Text = "Atualizar";
            Text = "Editar Serviço";
        }

        // ===================== SALVAR =====================
        private void BtnSalvar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTipo.Text))
            {
                MessageBox.Show("Informe o tipo da serviço.");
                return;
            }

            decimal valor = decimal.Parse(
                txtValor.Text.Replace("R$", "").Trim(),
                CultureInfo.GetCultureInfo("pt-BR")
            );

            var servico = new Servico
            {
                Id = servicoId,
                Tipo = txtTipo.Text,
                Valor = valor,
                Vencimento = dtVencimento.Value.Date,
                DataPagamento = chkPago.Checked ? dtPagamento.Value.Date : null,
                Situacao = cbSituacao.Text
            };

            if (servicoId == 0)
                repo.Insert(servico);
            else
                repo.Update(servico);

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}