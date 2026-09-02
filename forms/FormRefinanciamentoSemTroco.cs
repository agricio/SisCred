using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using CrudApp.Models;
using System.Diagnostics;
using CrudApp.Shared;
using System.Drawing;
using CrudApp.Repositories;
using CrudApp.Word;
using CrudApp.Pdf;
using QuestPDF.Fluent;

namespace CrudApp.Forms
{
   public class FormRefinanciamentoSemTroco : Form
    {
        private readonly Emprestimo _emprestimoOriginal;
        private readonly EmprestimoRepository repo = new();
        private readonly ParcelaRepository parcelaRepo = new();
        private readonly ClienteRepository clienteRepo = new();

        private ComboBox cbTipo, cbParcelas, cbCodigo;
        private TextBox txtSaldoDevedor, txtAbatimento, txtSaldoRefinanciado, txtContratoNovo;
        private TextBox txtTaxa, txtValorContrato, txtTotalJuros, txtTotalAmortizacao, txtAgil;
        private DateTimePicker dtVencimento, dtAverbacao;
        private Button btnGerarParcelas, btnSalvar;
        private DataGridView gridParcelas;
        
        private List<Parcela> parcelasGeradas = new();

        public FormRefinanciamentoSemTroco(Emprestimo emprestimo)
    {
        _emprestimoOriginal = emprestimo;
        InitializeComponent();
        PreencherDadosOriginais();
    }

    private void InitializeComponent()
    {
        Text = "Refinanciamento sem Troco";
        Width = 750;
        Height = 900;
        this.Icon = new Icon("app.ico");
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.AutoScaleMode = AutoScaleMode.Dpi;
        this.AutoScaleMode = AutoScaleMode.None;
        this.AutoSize = false;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        int top = 20, leftLabel = 20, leftInput = 170;

        Label L(string t)
        {
            var l = new Label { Text = t, Left = leftLabel, Top = top + 5, Width = 140 };
            Controls.Add(l); return l;
        }

        TextBox T(bool readOnly = false)
        {
            var t = new TextBox { Left = leftInput, Top = top, Width = 200, ReadOnly = readOnly };
            Controls.Add(t); top += 35; return t;
        }

        ComboBox C()
        {
            var c = new ComboBox
            {
                Left = leftInput,
                Top = top,
                Width = 120,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            Controls.Add(c); top += 35; return c;
        }

        DateTimePicker D()
        {
            var d = new DateTimePicker
            {
                Left = leftInput,
                Top = top,
                Width = 120,
                Format = DateTimePickerFormat.Short
            };
            Controls.Add(d); top += 35; return d;
        }

        // ===== CAMPOS =====
        
        L("N Contrato:");
        txtContratoNovo = T();
        
        L("Saldo Devedor:");
        txtSaldoDevedor = T(true);

        L("Abatimento:");
        txtAbatimento = T();

        L("Saldo Refinanciado:");
        txtSaldoRefinanciado = T(true);

        L("Parcelas:");
        cbParcelas = C();
        for (int i = 1; i <= 12; i++) cbParcelas.Items.Add(i.ToString());

        L("Taxa (%):");
        txtTaxa = T();

        L("Tipo:");
        cbTipo = C();
        cbTipo.Items.AddRange(new[] { "Mensal", "Quinzenal", "Semanal" });

        L("Averbação:");
        dtAverbacao = D();

        L("Vencimento:");
        dtVencimento = D();

        L("Valor Contrato:");
        txtValorContrato = T(true);

        L("Total Juros:");
        txtTotalJuros = T(true);

        L("Total Amortização:");
        txtTotalAmortizacao = T(true);

        L("Ágio:");
        txtAgil = T(true);

        btnGerarParcelas = new Button
        {
            Text = "Gerar Parcelas",
            Left = 420,
            Top = 140,
            Width = 120
        };
        btnGerarParcelas.Click += BtnGerarParcelas_Click;
        Controls.Add(btnGerarParcelas);

        btnSalvar = new Button
        {
            Text = "Confirmar Refinanciamento",
            Left = 200,
            Top = 500,
            Width = 220,
            Height = 40
        };
        btnSalvar.Click += BtnSalvar_Click;
        Controls.Add(btnSalvar);

        gridParcelas = new DataGridView
        {
            Left = 20,
            Top = 560,
            Width = 680,
            Height = 260,
            ReadOnly = true,
            AutoGenerateColumns = true
        };

        Controls.Add(gridParcelas);
    }

    private void PreencherDadosOriginais()
    {
        var parcelas = parcelaRepo
            .GetByEmprestimo(_emprestimoOriginal.Id)
            .Where(p =>
                p.Situacao != "Paga" &&
                p.Situacao != "Paga em Atraso" &&
                p.Situacao != "Paga Antecipado")
            .ToList();

        double saldo = 0;

        foreach (var parcela in parcelas)
        {
            double valor = parcela.ValorPrestacao ?? 0;

            // Atualiza somente as parcelas vencidas
            if (parcela.Vencimento.HasValue &&
                parcela.Vencimento.Value.Date < DateTime.Today)
            {
                valor = CalculoAtraso.Calcular(
                    valor,
                    parcela.Vencimento.Value);
            }

            saldo += valor;
        }

        txtSaldoDevedor.Text = saldo.ToString("F2");
    }

    private double ObterSaldoRefinanciado()
    {
        double saldo = double.Parse(txtSaldoDevedor.Text);
        double abat = double.TryParse(txtAbatimento.Text, out var a) ? a : 0;

        double resultado = saldo - abat;

        if (resultado <= 0)
            throw new Exception("Abatimento maior ou igual ao saldo devedor.");

        txtSaldoRefinanciado.Text = resultado.ToString("F2");
        return resultado;
    }

    private void BtnGerarParcelas_Click(object sender, EventArgs e)
    {
        if (cbParcelas.SelectedItem == null)
        {
            MessageBox.Show("Selecione a quantidade de parcelas.");
            return;
        }

        if (cbTipo.SelectedItem == null)
        {
            MessageBox.Show("Selecione o tipo de parcela.");
            return;
        }

        if (!double.TryParse(txtTaxa.Text, out double taxa) || taxa <= 0)
        {
            MessageBox.Show("Informe uma taxa válida.");
            return;
        }

        double valor;
        try
        {
            valor = ObterSaldoRefinanciado();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
            return;
        }

        int qtd = int.Parse(cbParcelas.SelectedItem.ToString());
        string tipo = cbTipo.SelectedItem.ToString();

        parcelasGeradas = GerarParcelasPrice(
            0,
            valor,
            qtd,
            taxa,
            dtAverbacao.Value,
            cbTipo.SelectedItem.ToString()
        );

        if (parcelasGeradas.Any()){ dtVencimento.Value = parcelasGeradas.Max(p => p.Vencimento).Value; }

        gridParcelas.AutoGenerateColumns = false;
        gridParcelas.Columns.Clear();

        gridParcelas.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "NParcelas",
            HeaderText = "Parcela",
            Width = 60
        });

        gridParcelas.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "Vencimento",
            HeaderText = "Vencimento",
            Width = 90,
            DefaultCellStyle = { Format = "dd/MM/yyyy" }
        });

        gridParcelas.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "ValorPrestacao",
            HeaderText = "Valor Parcela",
            Width = 90,
            DefaultCellStyle = { Format = "N2" }
        });

        gridParcelas.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "Juros",
            HeaderText = "Juros",
            Width = 80,
            DefaultCellStyle = { Format = "N2" }
        });

        gridParcelas.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "Amortizacao",
            HeaderText = "Amortização",
            Width = 90,
            DefaultCellStyle = { Format = "N2" }
        });

        gridParcelas.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "Saldo",
            HeaderText = "Saldo",
            Width = 90,
            DefaultCellStyle = { Format = "N2" }
        });


        gridParcelas.DataSource = null;
        gridParcelas.DataSource = parcelasGeradas;

        double valorContrato = parcelasGeradas.Sum(p => p.ValorPrestacao ?? 0);
        double totalJuros = parcelasGeradas.Sum(p => p.Juros ?? 0);
        double totalAmortizacao = parcelasGeradas.Sum(p => p.Amortizacao ?? 0);
        double agio = (valorContrato - valor) / qtd;

        txtValorContrato.Text = valorContrato.ToString("F2");
        txtTotalJuros.Text = totalJuros.ToString("F2");
        txtTotalAmortizacao.Text = totalAmortizacao.ToString("F2");
        txtAgil.Text = agio.ToString("F2");
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
                Vencimento = CalcularVencimento(
                    primeiroVencimento,
                    n,
                    tipoParcela
                )
            });
        }

        return parcelas;
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


        private void BtnSalvar_Click(object sender, EventArgs e)
        {
            if (parcelasGeradas.Count == 0)
            {
                MessageBox.Show("Gere as parcelas.");
                return;
            }

            if (!int.TryParse(txtContratoNovo.Text, out int contratoNovo))
            {
                MessageBox.Show("Contrato inválido.");
                return;
            }

            bool existe = repo.GetByCliente(_emprestimoOriginal.ClienteId)
                .Any(e => e.Contrato == contratoNovo);

            if (existe)
            {
                MessageBox.Show("Já existe um empréstimo com esse contrato.");
                return;
            }

            // 🔒 contrato antigo
            _emprestimoOriginal.Situacao = "Quitado";
            _emprestimoOriginal.TipoQuitacao = "Refinanciada";
            _emprestimoOriginal.Quitacao = DateTime.Today;
            AtualizarParcelasRefinanciadas();
            repo.Update(_emprestimoOriginal);

            // 🆕 novo contrato
            var novo = new Emprestimo
            {
                ClienteId = _emprestimoOriginal.ClienteId,
                Contrato = contratoNovo, // vem da UI
                ValorContrato = double.Parse(txtValorContrato.Text),
                Liberado = double.Parse(txtSaldoRefinanciado.Text),
                TotalJuros = double.Parse(txtTotalJuros.Text),
                Agio = double.Parse(txtAgil.Text),
                TotalAmortizacao = double.Parse(txtTotalAmortizacao.Text),
                Parcelas = parcelasGeradas.Count,
                Tipo = cbTipo.Text,
                Situacao = "A Vencer",
                TipoQuitacao = "Refinanciada",
                Averbacao = dtAverbacao.Value,
                Vencimento = parcelasGeradas.Last().Vencimento
            };


            int novoId = repo.AddAndReturnId(novo);

            foreach (var p in parcelasGeradas)
            {
                p.EmprestimosId = novoId;
                parcelaRepo.Insert(p);
            }

            MessageBox.Show("Refinanciamento realizado com sucesso!");
            BtnGerarContrato_Click(this, EventArgs.Empty);
            DialogResult = DialogResult.OK;
            Close();
        }

     private void BtnGerarContrato_Click(object sender, EventArgs e)
        {
            try
            {
                if (parcelasGeradas == null || parcelasGeradas.Count == 0)
                {
                    MessageBox.Show("Gere as parcelas antes de gerar o contrato.");
                    return;
                }

                // 🔎 Cliente vem do empréstimo original
                var cliente = clienteRepo.GetById(_emprestimoOriginal.ClienteId);

                if (cliente == null)
                {
                    MessageBox.Show("Cliente não encontrado.");
                    return;
                }

                var emprestimo = ObterEmprestimoDaTela();

                // =====================
                // PASTA CONTRATOS
                // =====================
                string pasta = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "Contratos");

                Directory.CreateDirectory(pasta);

                // =====================
                // NOME BASE
                // =====================
                string nomeBase =
                    $"Contrato_{emprestimo.Contrato}_{cliente.Nome}_{emprestimo.Averbacao:dd-MM-yyyy}";

                nomeBase = string.Concat(
                    nomeBase.Split(Path.GetInvalidFileNameChars()));

                // =====================
                // PDF
                // =====================
                /* string caminhoPdf = Path.Combine(
                    pasta,
                    nomeBase + ".pdf");

                var documentoPdf = new ContratoEmprestimoPdf(
                    cliente,
                    emprestimo,
                    parcelasGeradas
                );

                documentoPdf.GeneratePdf(caminhoPdf);
                */

                // =====================
                // WORD
                // =====================
                string caminhoWord = Path.Combine(
                    pasta,
                    nomeBase + ".docx");

                var documentoWord = new ContratoEmprestimoWord(
                    cliente,
                    emprestimo,
                    parcelasGeradas
                );

                documentoWord.Gerar(caminhoWord);

                // =====================
                // MENSAGEM
                // =====================
                MessageBox.Show("Empréstimo e parcelas salvos com sucesso!");

                // =====================
                // ABRIR WORD
                // =====================
                Process.Start(new ProcessStartInfo
                {
                    FileName = caminhoWord,
                    UseShellExecute = true
                });

                // =====================
                // VISUALIZAR PDF
                // =====================
                //var viewer = new FormPdfViewer(caminhoPdf);
                //viewer.ShowDialog();

                //DialogResult = DialogResult.OK;
                //Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Erro ao gerar contrato:\n\n" + ex.Message,
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private Emprestimo ObterEmprestimoDaTela()
        {
            if (parcelasGeradas == null || parcelasGeradas.Count == 0)
                throw new InvalidOperationException("Gere as parcelas antes de gerar o contrato.");

            if (!int.TryParse(txtContratoNovo.Text, out var contrato))
                throw new InvalidOperationException("Contrato inválido.");

            if (!double.TryParse(txtSaldoRefinanciado.Text, out var liberado))
                throw new InvalidOperationException("Valor liberado inválido.");

            if (!double.TryParse(txtValorContrato.Text, out var valorContrato))
                throw new InvalidOperationException("Valor do contrato inválido.");

            double.TryParse(txtAgil.Text, out var agio);
            double.TryParse(txtTotalJuros.Text, out var totalJuros);
            double.TryParse(txtTotalAmortizacao.Text, out var totalAmortizacao);

            return new Emprestimo
            {
                ClienteId = _emprestimoOriginal.ClienteId,
                Contrato = contrato,
                Liberado = liberado,
                ValorContrato = valorContrato,
                Agio = agio,
                TotalJuros = totalJuros,
                TotalAmortizacao = totalAmortizacao,
                Parcelas = parcelasGeradas.Count,

                Tipo = cbTipo.SelectedItem?.ToString() ?? "Mensal",
                Situacao = "A Vencer",
                TipoQuitacao = "Refinanciada",

                Averbacao = dtAverbacao.Value,
                Vencimento = parcelasGeradas.Last().Vencimento
            };
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

        private void AtualizarParcelasRefinanciadas()
        {
            var parcelas = parcelaRepo
                .GetByEmprestimo(_emprestimoOriginal.Id)
                .Where(p =>
                    p.Situacao != "Paga" &&
                    p.Situacao != "Paga em Atraso" &&
                    p.Situacao != "Paga Antecipado")
                .ToList();

            foreach (var parcela in parcelas)
            {
                parcela.Situacao = "Refinanciada";
                parcela.Pagamento = DateTime.Today;

                parcelaRepo.Update(parcela);
            }
        }

        
  }
    
}
