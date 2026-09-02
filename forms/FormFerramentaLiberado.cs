using System;
using System.Drawing;
using System.Windows.Forms;

namespace CrudApp.Forms
{
    public class FormFerramentaLiberado : Form
    {
        private TextBox txtTaxa;
        private TextBox txtParcela;
        private TextBox txtRenda;
        private TextBox txtLiberado;
        private ComboBox cbParcelas;

        private Button btnCalcular;
        
        private RadioButton rbContrato1;
        private RadioButton rbContrato2;
        private RadioButton rbContrato3;
        private double valorLiberadoBase; // valor original (100%)


        public double ValorLiberado { get; private set; }

        public FormFerramentaLiberado()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Icon = new Icon("app.ico");
            Text = "Speed Cred - Calcular Valor Liberado";
            Width = 380;
            Height = 390;
            


            Label L(string t, int top) =>
                new Label { Text = t, Left = 20, Top = top + 4, Width = 150 };

                txtRenda = new TextBox { Left = 180, Top = 20, Width = 160 };
                txtTaxa = new TextBox { Left = 180, Top = 55, Width = 160 };
                cbParcelas = new ComboBox
                {
                    Left = 180,
                    Top = 90,
                    Width = 160,
                    DropDownStyle = ComboBoxStyle.DropDownList
                };

            // valores de 1 a 12
            for (int i = 1; i <= 12; i++)
                cbParcelas.Items.Add(i);

            // padrão (opcional)
            cbParcelas.SelectedIndex = 0;

            txtParcela = new TextBox { Left = 180, Top = 125, Width = 160 };

            btnCalcular = new Button
            {
                Text = "Calcular",
                Left = 20,
                Top = 290,
                Width = 120,
                Height = 35
            };
            btnCalcular.Click += BtnCalcular_Click;

            Controls.AddRange(new Control[]
            {
                L("Renda Líquida:", 20),
                L("Taxa (%):", 55),
                L("Qtd Parcelas:", 90),
                L("Valor Parcela:", 125),
                L("Valor Liberado:", 155),

                txtRenda,
                txtTaxa,
                cbParcelas,
                txtParcela,
                txtLiberado,

                btnCalcular,
            });

            txtRenda.TextChanged += TxtRenda_TextChanged;

            txtLiberado = new TextBox
            {
                Left = 180,
                Top = 155,
                Width = 160,
                ReadOnly = true
            };
            Controls.Add(new Label
            
            {
                Text = "Valor Liberado:",
                Left = 20,
                Top = 155 + 4,
                Width = 150
            });
            Controls.Add(txtLiberado);

            Label lblContrato = new Label
            {
                Text = "Qtd. Contratos:",
                Left = 20,
                Top = 200,
                Width = 120
            };
            Controls.Add(lblContrato);

            rbContrato1 = new RadioButton
            {
                Text = "1 Contrato (50%)",
                Left = 150,
                Top = 200,
                Width = 150,
                Checked = true // padrão
            };

            rbContrato2 = new RadioButton
            {
                Text = "2 Contratos (75%)",
                Left = 150,
                Top = 225,
                Width = 150
            };

            rbContrato3 = new RadioButton
            {
                Text = "3 Contratos (100%)",
                Left = 150,
                Top = 250,
                Width = 150,
            };

            Controls.Add(rbContrato1);
            Controls.Add(rbContrato2);
            Controls.Add(rbContrato3);

            rbContrato1.CheckedChanged += Contrato_CheckedChanged;
            rbContrato2.CheckedChanged += Contrato_CheckedChanged;
            rbContrato3.CheckedChanged += Contrato_CheckedChanged;

            rbContrato1.Enabled =
            rbContrato2.Enabled =
            rbContrato3.Enabled = valorLiberadoBase > 0;

        }

        private void BtnCalcular_Click(object sender, EventArgs e)
        {
            if (!double.TryParse(txtRenda.Text, out double renda) ||
                !double.TryParse(txtTaxa.Text, out double taxaPerc) ||
                !int.TryParse(cbParcelas.Text, out int parcelas) ||
                !double.TryParse(txtParcela.Text, out double parcela))
            {
                MessageBox.Show("Preencha todos os campos corretamente.");
                return;
            }

            // 🔒 Regra de segurança: parcela máx = 30% da renda
            double parcelaMaxima = renda * 0.30;

            if (parcela > parcelaMaxima)
            {
                MessageBox.Show(
                    $"Parcela excede 30% da renda líquida.\n" +
                    $"Máximo permitido: R$ {parcelaMaxima:F2}",
                    "Atenção",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            double taxa = taxaPerc / 100.0;

            ValorLiberado = VP(taxa, parcelas, parcela);

            // guarda o valor base (100%)
            valorLiberadoBase = ValorLiberado;

            // atualiza conforme contrato selecionado
            AtualizarValorLiberado();

            rbContrato1.Enabled = true;
            rbContrato2.Enabled = true;
            rbContrato3.Enabled = true;

            //MessageBox.Show($"Valor Liberado calculado:\nR$ {ValorLiberado:F2}");
            
        }

        // 🔹 FUNÇÃO VP AGORA COM ASSINATURA CLARA
        private static double VP(
            double taxa,
            int nper,
            double pgto,
            double vf = 0,
            int tipo = 0)
        {
            double vp;

            if (taxa == 0)
            {
                vp = -(pgto * nper + vf);
            }
            else
            {
                double fator = Math.Pow(1 + taxa, nper);
                vp = -(pgto * (1 + taxa * tipo) * (fator - 1) / (taxa * fator) + vf / fator);
            }

            // 🔹 RETORNA SEMPRE POSITIVO
            return Math.Abs(vp);
        }

        

        private void TxtRenda_TextChanged(object? sender, EventArgs e)
        {
            if (double.TryParse(txtRenda.Text, out double renda))
            {
                double parcela = CalcularParcelaAutomatica(renda);
                txtParcela.Text = parcela.ToString("F2");
            }
            else
            {
                txtParcela.Clear();
            }
        }

        private double CalcularParcelaAutomatica(double rendaLiquida)
        {
            return (rendaLiquida / 3.0) * 0.27;
        }

        private void Contrato_CheckedChanged(object? sender, EventArgs e)
        {
            AtualizarValorLiberado();
        }

        private void AtualizarValorLiberado()
        {
            if (valorLiberadoBase <= 0)
                return;

            double fator = 1.0;

            if (rbContrato1.Checked)
                fator = 0.50;
            else if (rbContrato2.Checked)
                fator = 0.75;
            else if (rbContrato3.Checked)
                fator = 1.00;

            double novoValor = valorLiberadoBase * fator;
            txtLiberado.Text = novoValor.ToString("F2");
            ValorLiberado = novoValor;

            cbParcelas.SelectedIndexChanged += (s, e) =>
            {
                if (valorLiberadoBase > 0)
                    BtnCalcular_Click(s, e);
            };
        }

    }
}
