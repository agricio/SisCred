using System;
using System.Globalization;
using System.Windows.Forms;
using CrudApp.Models;
using CrudApp.Repositories;
using System.Drawing;

namespace CrudApp.Forms
{
    public class FormEditarParcela : Form
    {
        private readonly ParcelaRepository repo = new ParcelaRepository();
        private Parcela parcela;

        private TextBox txtNumero;
        private TextBox txtAmortizacao;
        private TextBox txtJuros;
        private TextBox txtValor;
        private TextBox txtSaldo;
        //private TextBox txtAgio;

        private ComboBox cbSituacao;
        private ComboBox cbFormaPagamento;
        private DateTimePicker dtVencimento;
        private DateTimePicker dtPagamento;

        private Button btnSalvar;
        private Button btnCancelar;

        private readonly EmprestimoRepository emprestimoRepo = new EmprestimoRepository();
        private Emprestimo emprestimo;

        public double AgioCalculado { get; private set; }
        

        public FormEditarParcela(int parcelaId)
        {
            parcela = repo.GetById(parcelaId)
                ?? throw new Exception("Parcela não encontrada.");

            emprestimo = emprestimoRepo.GetById(parcela.EmprestimosId)
                ?? throw new Exception("Empréstimo não encontrado.");

            InitializeComponent();
            PreencherCampos();
        }

        private void InitializeComponent()
        {
            this.Icon = new Icon("app.ico");
            Text = "Speed Cred - Editar Parcela";
            Width = 400;
            Height = 480;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            int top = 50;
            int leftLabel = 20;
            int leftInput = 150;

            MenuStrip menu = new MenuStrip();
            menu.Dock = DockStyle.Top;

            // Menu Ferramentas
            ToolStripMenuItem menuFerramentas = new ToolStripMenuItem("Pagamento");
            menuFerramentas.DropDownItems.Add("Antecipação de Parcela", null, (s, e) =>
            {
                BtnCalcularAntecipado_Click(s, e);
            });
            menuFerramentas.DropDownItems.Add("Quitação de Parcela em Atraso", null, (s, e) =>
            {
                BtnCalcularMulta_Click(s, e);
            });

            // Adicionar itens ao menu
            menu.Items.Add(menuFerramentas);

            // Registrar menu no formulário
            this.MainMenuStrip = menu;
            this.Controls.Add(menu);

            Label L(string t)
            {
                var lbl = new Label
                {
                    Text = t,
                    Left = leftLabel,
                    Top = top + 5,
                    Width = 120
                };
                Controls.Add(lbl);
                return lbl;
            }

            TextBox T(bool readOnly = false)
            {
                var txt = new TextBox
                {
                    Left = leftInput,
                    Top = top,
                    Width = 180,
                    ReadOnly = readOnly
                };
                Controls.Add(txt);
                top += 35;
                return txt;
            }

            DateTimePicker D()
            {
                var dt = new DateTimePicker
                {
                    Left = leftInput,
                    Top = top,
                    Width = 180,
                    Format = DateTimePickerFormat.Short,
                    ShowCheckBox = true
                };
                Controls.Add(dt);
                top += 35;
                return dt;
            }

            ComboBox C(params object[] items)
            {
                var cb = new ComboBox
                {
                    Left = leftInput,
                    Top = top,
                    Width = 180,
                    DropDownStyle = ComboBoxStyle.DropDownList
                };
                cb.Items.AddRange(items);
                Controls.Add(cb);
                top += 35;
                return cb;
            }

            // ---------- CAMPOS ----------
            L("Nº Parcela:");
            txtNumero = T(readOnly: true);

            L("Amortização:");
            txtAmortizacao = T();

            L("Juros:");
            txtJuros = T();

            L("Valor Prestação:");
            txtValor = T();

            L("Saldo:");
            txtSaldo = T();

           // L("Ágio:");
           // txtAgio = T();

            L("Vencimento:");
            dtVencimento = D();

            L("Pagamento:");
            dtPagamento = D();

            dtPagamento.ValueChanged += (s, e) =>
            {
                cbFormaPagamento.Enabled = dtPagamento.Checked;

                if (!dtPagamento.Checked)
                    cbFormaPagamento.SelectedIndex = -1;
            };

            L("Situação:");
            cbSituacao = C("A Vencer", "Paga", "Em Atraso", "Paga em Atraso", "Paga Antecipado");

            L("Forma Pagamento:");
            cbFormaPagamento = C( "", "Dinheiro", "PIX", "Débito", "Crédito", "Boleto", "Transferência" );
            cbFormaPagamento.Enabled = false;

            // ---------- BOTÕES ----------

            btnSalvar = new Button
            {
                Text = "Salvar",
                Left = 80,
                Top = top + 15,
                Width = 100,
                Height = 35
            };
            btnSalvar.Click += BtnSalvar_Click;

            btnCancelar = new Button
            {
                Text = "Cancelar",
                Left = 200,
                Top = top + 15,
                Width = 100,
                Height = 35
            };
            btnCancelar.Click += (s, e) => DialogResult = DialogResult.Cancel;

            Controls.Add(btnSalvar);
            Controls.Add(btnCancelar);

            /*

            btnCalcularAntecipado = new Button
            {
                Text = "Calcular Antecipado",
                Left = leftInput,
                Top = top,
                Width = 180,
                Height = 30
            };

            btnCalcularAntecipado.Click += BtnCalcularAntecipado_Click;
            Controls.Add(btnCalcularAntecipado);
            top += 40;
            
            btnCalcularMulta = new Button
            {
                Text = "Calcular Multa",
                Left = leftInput,
                Top = top,
                Width = 180,
                Height = 30
            };

            btnCalcularMulta.Click += BtnCalcularMulta_Click;
            Controls.Add(btnCalcularMulta);
            top += 40;

            */
        }

        private void PreencherCampos()
        {
            txtNumero.Text = parcela.NParcelas.ToString();

            txtAmortizacao.Text = parcela.Amortizacao?.ToString("F2") ?? "0,00";
            txtJuros.Text       = parcela.Juros?.ToString("F2") ?? "0,00";
            txtValor.Text       = parcela.ValorPrestacao?.ToString("F2") ?? "0,00";
            txtSaldo.Text       = parcela.Saldo?.ToString("F2") ?? "0,00";

            cbSituacao.SelectedItem = parcela.Situacao;

            if (parcela.Vencimento.HasValue)
            {
                dtVencimento.Value = parcela.Vencimento.Value;
                dtVencimento.Checked = true;
            }
            else
            {
                dtVencimento.Checked = false;
            }

            if (parcela.Pagamento.HasValue)
            {
                dtPagamento.Value = parcela.Pagamento.Value;
                dtPagamento.Checked = true;
                cbFormaPagamento.Enabled = true;
            }
            else
            {
                dtPagamento.Checked = false;
                cbFormaPagamento.Enabled = false;
            }

            if (!string.IsNullOrEmpty(parcela.FormaPagamento))
                cbFormaPagamento.SelectedItem = parcela.FormaPagamento;
        }

        private void BtnSalvar_Click(object? sender, EventArgs e)
        {
            if (!double.TryParse(txtAmortizacao.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var amort) ||
                !double.TryParse(txtJuros.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var juros) ||
                !double.TryParse(txtValor.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var valor) ||
                !double.TryParse(txtSaldo.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var saldo))
            {
                MessageBox.Show("Valores numéricos inválidos.");
                return;
            }

            if (dtPagamento.Checked && cbFormaPagamento.SelectedIndex == -1)
            {
                MessageBox.Show("Informe a forma de pagamento.");
                return;
            }

            parcela.Amortizacao = amort;
            parcela.Juros = juros;
            parcela.ValorPrestacao = valor;
            parcela.Saldo = saldo;
            parcela.Situacao = cbSituacao.SelectedItem?.ToString() ?? "";

            parcela.Vencimento = dtVencimento.Checked
                ? dtVencimento.Value.Date
                : null;

            parcela.Pagamento = dtPagamento.Checked
                ? dtPagamento.Value.Date
                : null;

            parcela.FormaPagamento = dtPagamento.Checked
                ? cbFormaPagamento.SelectedItem?.ToString()
                : null;

            repo.Update(parcela);

            MessageBox.Show("Parcela atualizada com sucesso!");
            DialogResult = DialogResult.OK;
        }

        private void BtnCalcularAntecipado_Click(object? sender, EventArgs e)
        {
            if (!dtVencimento.Checked)
            {
                MessageBox.Show("Informe a data de vencimento.");
                return;
            }

            if (!double.TryParse(txtValor.Text, NumberStyles.Any, CultureInfo.CurrentCulture, out var valorPrestacao))
            {
                MessageBox.Show("Valor da prestação inválido.");
                return;
            }

            DateTime dataVencimento = dtVencimento.Value.Date;

            if (DateTime.Today >= dataVencimento)
            {
                MessageBox.Show("Pagamento não é antecipado.");
                return;
            }

            using (var form = new FormCalculoAntecipado(valorPrestacao, dataVencimento))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    txtValor.Text = form.NovoValor.ToString("F2");
                    txtSituacaoAuto("Paga Antecipado");
                }
            }
        }

        private void BtnCalcularMulta_Click(object? sender, EventArgs e)
        {
            if (!dtVencimento.Checked)
            {
                MessageBox.Show("Informe a data de vencimento.");
                return;
            }

            if (!double.TryParse(txtValor.Text, NumberStyles.Any, CultureInfo.CurrentCulture, out var valorPrestacao))
            {
                MessageBox.Show("Valor da prestação inválido.");
                return;
            }

            DateTime dataVencimento = dtVencimento.Value.Date;

            if (DateTime.Today <= dataVencimento)
            {
                MessageBox.Show("Prestação ainda está a vencer.");
                return;
            }

            if ((parcela.Agio ?? 0) > 0)
            {
                MessageBox.Show("Essa parcela já possui multa aplicada.");
                return;
            }

            using (var form = new FormCalculoAtraso(valorPrestacao, dataVencimento))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    AplicarMulta(form.NovoValor); // atualiza parcela e AgioCalculado
                }
            }
        }


        private void AplicarMulta(double novoValor)
        {
            double valorOriginal = parcela.ValorPrestacao ?? 0.0;
            double acrescimo = novoValor - valorOriginal;

            parcela.Agio = acrescimo;
            AgioCalculado = acrescimo;

            parcela.Situacao = "Paga em Atraso";

            txtValor.Text = novoValor.ToString("F2");
            txtSituacaoAuto("Paga em Atraso");
        }


        private void txtSituacaoAuto(string situacao)
            {
                if (cbSituacao.Items.Contains(situacao))
                     cbSituacao.SelectedItem = situacao;
            }

     }
}
