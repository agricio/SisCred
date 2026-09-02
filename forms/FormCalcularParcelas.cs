using System;
using System.Linq;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using CrudApp.Repositories;
using CrudApp.Models;
using System.Diagnostics;

using CrudApp.Pdf;
using CrudApp.Shared;
using QuestPDF.Fluent;

namespace CrudApp.Forms
{
    public class FormCalcularParcelas : Form
    {
        private readonly EmprestimoRepository repo = new EmprestimoRepository();
        private readonly ClienteRepository crepo = new ClienteRepository();

        private ComboBox cbCliente, cbTipo, cbParcelas, cbCodigo;        
        private TextBox txtTaxa;
        private TextBox txtLiberado;
        private TextBox txtAgio;
        private TextBox txtValorContrato;
        private TextBox txtTotalJuros;
        private TextBox txtTotalAmortizacao;
        private DateTimePicker dtAverbacao;
        private DateTimePicker dtVencimento;
        private Button btnSalvar;
        private Button btnCancelar;
        private Button btnLimpar;
        private DataGridView gridParcelas;
        private List<Parcela> parcelasGeradas = new();
        public FormCalcularParcelas()
        {
            InitializeComponent();
            //LoadClientes();
        }

        private void InitializeComponent()
        {
            this.Icon = new Icon("app.ico");
            this.Text = "SiS Cred - Simualdor de Contrato e Parcelas";
            this.Width = 750;
            this.Height = 710;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.AutoScaleMode = AutoScaleMode.None;
            this.AutoSize = false;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            int top = 20;
            int leftLabel = 20;
            int leftInput = 150;

            var btnGerarParcelas = new Button
            {
                Text = "Gerar Parcelas",
                Left = 420,
                Top = top + 110,
                Width = 100,
                Height = 26
            };

            btnGerarParcelas.Click += BtnGerarParcelas_Click;
            this.Controls.Add(btnGerarParcelas);
            

            Button btnVP = new Button
            {
                Text = "Calcular Liberado",
                Left = 420,
                Top = 18,
                Width = 120,
                Height = 26
            };

            btnVP.Click += BtnVP_Click;
            Controls.Add(btnVP);

            Label L(string text)
            {
                var lbl = new Label
                {
                    Text = text,
                    Left = leftLabel,
                    Top = top + 5,
                    Width = 120
                };
                this.Controls.Add(lbl);
                return lbl;
            }

            TextBox T(int width = 200)
            {
                var txt = new TextBox
                {
                    Left = leftInput,
                    Top = top,
                    Width = width
                };
                this.Controls.Add(txt);
                top += 35;
                return txt;
            }

            DateTimePicker D()
            {
                var dt = new DateTimePicker
                {
                    Left = leftInput,
                    Top = top,
                    Width = 120,
                    Format = DateTimePickerFormat.Short
                };
                this.Controls.Add(dt);
                top += 35;
                return dt;
            }

            ComboBox C()
            {
                var cb = new ComboBox
                {
                    Left = leftInput,
                    Top = top,
                    Width = 250,
                    DropDownStyle = ComboBoxStyle.DropDownList
                };
                this.Controls.Add(cb);
                top += 35;
                return cb;
            }

            ComboBox C2()
            {
                var cb = new ComboBox
                {
                    Left = leftInput,
                    Top = top,
                    Width = 90,
                    DropDownStyle = ComboBoxStyle.DropDownList
                };
                cb.Items.AddRange(new object[]
                {
                    "Mensal",
                    "Quinzenal",
                    "Semanal",
                    "Diario"
                });

                this.Controls.Add(cb);
                top += 35;
                return cb;
            }

            ComboBox C3()
            {
                var cb = new ComboBox
                {
                    Left = leftInput,
                    Top = top,
                    Width = 80,
                    DropDownStyle = ComboBoxStyle.DropDownList
                };

                for (int i = 1; i <= 31; i++)
                    cb.Items.Add(i.ToString());

                this.Controls.Add(cb);
                top += 35;
                return cb;
            }

            
            ComboBox C4()
            {
                var cb = new ComboBox
                {
                    Left = leftInput,
                    Top = top,
                    Width = 80,
                    DropDownStyle = ComboBoxStyle.DropDownList
                };
                cb.Items.AddRange(new object[]
                {
                    "001",
                    "002",
                    "003",
                    "004",
                    "005",
                    "006",
                    "007",
                    "008",
                    "009",
                    "010",
                    "011",
                    "012",
                    "013",
                    "014",
                    "015"
                });

                this.Controls.Add(cb);
                top += 35;
                return cb;
            }

            ComboBox C5()
            {
                var cb = new ComboBox
                {
                    Left = leftInput,
                    Top = top,
                    Width = 90,
                    DropDownStyle = ComboBoxStyle.DropDownList
                };
                cb.Items.AddRange(new object[]
                {
                    "Quitado",
                    "A Vencer",
                    "Vencido",
                });

                this.Controls.Add(cb);
                top += 35;
                return cb;
            }

            ComboBox C6()
            {
                var cb = new ComboBox
                {
                    Left = leftInput,
                    Top = top,
                    Width = 90,
                    DropDownStyle = ComboBoxStyle.DropDownList
                };
                cb.Items.AddRange(new object[]
                {
                    "Normal",
                    "Antecipada",
                    "Em Curso",
                    "Atrasada",
                    "Refinanciada"
                });

                this.Controls.Add(cb);
                top += 35;
                return cb;
            }


            // ---------- CAMPOS ----------

            //L("Cliente:");
            //cbCliente = C();

           // L("Contrato:");
           // txtContrato = T();

            L("Valor Liberado:");
            txtLiberado = T();

            L("Parcelas:");
            cbParcelas = C3();

            L("Taxa (%):");
            txtTaxa = T();

            L("Código:");
            cbCodigo = C4();

            L("Tipo:");
            cbTipo = C2();

            L("Averbação:");
            dtAverbacao = D();

            L("Vencimento:");
            dtVencimento = D();

            L("Agio:");
            txtAgio = T();

            L("Valor do Contato:");
            txtValorContrato = T();

            L("Total de Juros:");
            txtTotalJuros = T();

            L("Total Amortização:");
            txtTotalAmortizacao = T();

            // ---------- BOTÕES ----------
            btnSalvar = new Button
            {
                Text = "Salvar",
                Left = 150,
                Top = top + 240,
                Width = 100,
                Height = 35
            };
            btnSalvar.Click += BtnSalvar_Click;
            //this.Controls.Add(btnSalvar);

            btnCancelar = new Button
            {
                Text = "Cancelar",
                Left = 280,
                Top = top + 240,
                Width = 100,
                Height = 35
            };
            btnCancelar.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
           // this.Controls.Add(btnCancelar);

           btnLimpar = new Button
            {
                Text = "Limpar",
                Left = 580,
                Top = 360,
                Width = 100,
                Height = 35
            };
            btnLimpar.Click += BtnLimpar_Click;
            Controls.Add(btnLimpar);

            Button btnPdf = new Button
            {
                Text = "Gerar Proposta PDF",
                Left = 420,
                Top = 360,
                Width = 140,
                Height = 35
            };

            btnPdf.Click += BtnGerarPdf_Click;
            Controls.Add(btnPdf);

            gridParcelas = new DataGridView
            {
                Left = 20,
                Top = 430,
                Width = 660,
                Height = 220,
                ReadOnly = true,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false
            };

            gridParcelas.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Parcela",
                DataPropertyName = "NParcelas",
                Width = 60,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            gridParcelas.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Vencimento",
                DataPropertyName = "Vencimento",
                Width = 100,
                DefaultCellStyle = { Format = "dd/MM/yyyy" }
            });

            gridParcelas.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Amortização",
                DataPropertyName = "Amortizacao",
                Width = 120
            });

            gridParcelas.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Juros",
                DataPropertyName = "Juros",
                Width = 100
            });

            gridParcelas.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Prestação",
                DataPropertyName = "ValorPrestacao",
                Width = 120
            });

            gridParcelas.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Saldo",
                DataPropertyName = "Saldo",
                Width = 120
            });

            this.Controls.Add(gridParcelas);

        }


        private void BtnSalvar_Click(object? sender, EventArgs e)
        {
            if (parcelasGeradas.Count == 0)
            {
                MessageBox.Show("Gere as parcelas antes de salvar.");
                return;
            }

            var emp = new Emprestimo
            {
                ClienteId = (int)cbCliente.SelectedValue,
                Liberado = double.Parse(txtLiberado.Text),
                Agio = double.Parse(txtAgio.Text),
                ValorContrato = double.Parse(txtValorContrato.Text),
                TotalJuros = double.Parse(txtTotalJuros.Text),
                TotalAmortizacao = double.Parse(txtTotalAmortizacao.Text),
                Parcelas = parcelasGeradas.Count,
                Codigo = int.TryParse(cbCodigo.SelectedItem?.ToString(), out var cod) ? cod : 0,
                Averbacao = dtAverbacao.Value,
                Vencimento = dtVencimento.Value,
                //Quitacao = dtQuitacao.Value,
                Tipo = cbTipo.SelectedItem?.ToString() ?? "",
                //Situacao = cbSituacao.SelectedItem?.ToString() ?? "",
                //TipoQuitacao = cbTipoQuitacao.SelectedItem?.ToString() ?? ""
            };

            //  salva empréstimo UMA VEZ
            int emprestimoId = repo.AddAndReturnId(emp);

            var parcelaRepo = new ParcelaRepository();

            foreach (var p in parcelasGeradas)
            {
                p.EmprestimosId = emprestimoId;
                parcelaRepo.Insert(p);
            }

            MessageBox.Show("Empréstimo e parcelas salvos com sucesso!");
            DialogResult = DialogResult.OK;
            Close();
        }

        private List<Parcela> GerarParcelasPrice(
            int emprestimoId,
            double valor,
            int qtdParcelas,
            double taxaPercentual,
            DateTime primeiroVencimento,
            string tipoParcela)
        {
            var parcelas = new List<Parcela>();

            double i = taxaPercentual / 100.0;
            double parcelaFixa = valor * (i / (1 - Math.Pow(1 + i, -qtdParcelas)));

            double saldo = valor;

            for (int n = 1; n <= qtdParcelas; n++)
            {
                double juros = saldo * i;
                double amortizacao = parcelaFixa - juros;
                saldo -= amortizacao;

                parcelas.Add(new Parcela
                {
                    EmprestimosId = emprestimoId,
                    NParcelas = n,
                    Juros = Math.Round(juros, 2),
                    Amortizacao = Math.Round(amortizacao, 2),
                    ValorPrestacao = Math.Round(parcelaFixa, 2),
                    Saldo = Math.Round(saldo < 0 ? 0 : saldo, 2),
                    Situacao = "A Vencer",
                    Vencimento = CalcularVencimento( primeiroVencimento, n, tipoParcela)
                });
            }

            return parcelas;
        }

        private void BtnGerarParcelas_Click(object? sender, EventArgs e)
        {
            if (!int.TryParse(cbParcelas.SelectedItem?.ToString(), out int qtd))
            {
                MessageBox.Show("Selecione a quantidade de parcelas.");
                return;
            }

            if (!double.TryParse(txtLiberado.Text, out double valor))
            {
                MessageBox.Show("Informe o valor liberado.");
                return;
            }

            if (!double.TryParse(txtTaxa.Text, out double taxa))
            {
                MessageBox.Show("Informe a taxa.");
                return;
            }

            parcelasGeradas = GerarParcelasPrice(
                emprestimoId: 0,
                valor: valor,
                qtdParcelas: qtd,
                taxaPercentual: taxa,
                primeiroVencimento: dtAverbacao.Value, // averbação
                tipoParcela: cbTipo.SelectedItem?.ToString() ?? "Mensal"
            );

            // ✅ vencimento final automático
            dtVencimento.Value = CalcularVencimento(
                dtAverbacao.Value,
                qtd,
                cbTipo.SelectedItem?.ToString() ?? "Mensal"
            );

            gridParcelas.DataSource = null;
            gridParcelas.DataSource = parcelasGeradas;

            CalcularValorContrato();
            CalcularAgio();
            CalcularJurosContrato();
            CalcularAmortizacaoContrato();
        }

        private DateTime CalcularVencimento(
            DateTime dataAverbacao,
            int qtdParcelas,
            string tipo)
        {
            return tipo switch
            {
                "Mensal"     => dataAverbacao.AddMonths(qtdParcelas),
                "Semanal"    => dataAverbacao.AddDays(7 * qtdParcelas),
                "Quinzenal"  => dataAverbacao.AddDays(15 * qtdParcelas),
                "Diario"    => CalcularDiaUtil(dataAverbacao, qtdParcelas),

        _ => dataAverbacao
            };
        }

        private void BtnVP_Click(object? sender, EventArgs e)
        {
            using var frm = new FormCalcularLiberado();

            if (frm.ShowDialog() == DialogResult.OK)
            {
                txtLiberado.Text = frm.ValorLiberado.ToString("F2");
            }
        }

        private void CalcularValorContrato()
        {
            if (parcelasGeradas == null || parcelasGeradas.Count == 0)
            {
                txtValorContrato.Text = "0,00";
                return;
            }

            double total = parcelasGeradas.Sum(p => p.ValorPrestacao ?? 0);

            txtValorContrato.Text = total.ToString("F2");
        }

        private void CalcularJurosContrato()
        {
            if (parcelasGeradas == null || parcelasGeradas.Count == 0)
            {
                txtTotalJuros.Text = "0,00";
                return;
            }

            double total = parcelasGeradas.Sum(p => p.Juros ?? 0);

            txtTotalJuros.Text = total.ToString("F2");
        }

        private void CalcularAmortizacaoContrato()
        {
            if (parcelasGeradas == null || parcelasGeradas.Count == 0)
            {
                txtTotalAmortizacao.Text = "0,00";
                return;
            }

            double total = parcelasGeradas.Sum(p => p.Amortizacao ?? 0);

            txtTotalAmortizacao.Text = total.ToString("F2");
        }

        private void CalcularAgio()
            {
                if (!double.TryParse(txtValorContrato.Text, out var valorContrato))
                     return;

                if (!double.TryParse(txtLiberado.Text, out var valorLiberado))
                        return;

                if (!int.TryParse(cbParcelas.SelectedItem?.ToString(), out var qtdParcelas))
                        return;

                if (qtdParcelas <= 0)
                        return;

                double agio = (valorContrato - valorLiberado) / qtdParcelas;

                if (agio < 0)
                        agio = 0;

                    txtAgio.Text = agio.ToString("F2");
        }

        private DateTime CalcularDiaUtil(DateTime dataInicial, int diasUteis)
        {
            DateTime data = dataInicial;

            int adicionados = 0;

            while (adicionados < diasUteis)
            {
                data = data.AddDays(1);

                // Ignora sábado e domingo
                if (data.DayOfWeek != DayOfWeek.Saturday &&
                    data.DayOfWeek != DayOfWeek.Sunday)
                {
                    adicionados++;
                }
            }

            return data;
        }

        private void BtnLimpar_Click(object? sender, EventArgs e)
        {
            // TextBox
            txtLiberado.Clear();
            txtTaxa.Clear();
            txtAgio.Clear();
            txtValorContrato.Clear();
            txtTotalJuros.Clear();
            txtTotalAmortizacao.Clear();

            // ComboBox
            cbParcelas.SelectedIndex = -1;
            cbCodigo.SelectedIndex = -1;
            cbTipo.SelectedIndex = -1;

            // Datas
            dtAverbacao.Value = DateTime.Today;
            dtVencimento.Value = DateTime.Today;

            // Grid
            parcelasGeradas.Clear();

            gridParcelas.DataSource = null;
            gridParcelas.Rows.Clear();

            // Foco inicial
            txtLiberado.Focus();
        }

       private void BtnGerarPdf_Click(object sender, EventArgs e)
        {
            if (parcelasGeradas == null || parcelasGeradas.Count == 0)
            {
                MessageBox.Show("Gere as parcelas antes de gerar o PDF.");
                return;
            }

            var emp = new Emprestimo
            {
                Liberado = double.Parse(txtLiberado.Text),
                Parcelas = int.Parse(cbParcelas.SelectedItem.ToString()),
                TotalJuros = double.Parse(txtTotalJuros.Text),
                TotalAmortizacao = double.Parse(txtTotalAmortizacao.Text),
                ValorContrato = double.Parse(txtValorContrato.Text),
                Codigo = int.Parse(cbCodigo.SelectedItem.ToString()),
                Vencimento = dtVencimento.Value,
                Tipo = cbTipo.SelectedItem?.ToString() ?? ""
            };

           // string caminho = Path.Combine(
            //    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
             //   $"Proposta_Emprestimo_{DateTime.Now:yyyyMMddHHmmss}.pdf"
           // );

            string pasta = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "Propostas");

            Directory.CreateDirectory(pasta);

            string caminho = Path.Combine(
                pasta,
                $"Proposta_Emprestimo_{DateTime.Now:dd-MM-yyyy_HH-mm}.pdf"
                .Replace(" ", "_"));

            var pdf = new PdfPropostaEmprestimo();
            pdf.GeneratePdf(emp, parcelasGeradas, caminho);

            //MessageBox.Show("📄 Proposta gerada com sucesso!");

            // ✅ ABRIR NO SEU FORM DE VISUALIZAÇÃO
            var viewer = new FormPdfViewer(caminho);
            viewer.ShowDialog(); // ou Show()
        }

    }
}
