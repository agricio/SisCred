using System;
using System.Linq;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using CrudApp.Repositories;
using CrudApp.Models;
using CrudApp.Shared;
using System.Diagnostics;
using CrudApp.Pdf;
using CrudApp.Word;
using CrudApp.Services;
using QuestPDF.Fluent;
using DocumentFormat.OpenXml.Packaging;


namespace CrudApp.Forms
{
    public class FormEditarEmprestimo : Form
    {
        private readonly EmprestimoRepository repo = new EmprestimoRepository();
        private readonly ParcelaRepository parcelaRepo = new ParcelaRepository();
        private readonly ClienteRepository clienteRepo = new ClienteRepository();
        private Emprestimo emprestimo;
        private TextBox txtContrato;
        private TextBox txtLiberado;
        private TextBox txtAgil;
        private TextBox txtValorContrato;
        private TextBox txtTotalJuros;
        private TextBox txtTotalAmortizacao;
        private ComboBox cbParcelas;
        private ComboBox cbCodigo;
        private ComboBox cbTipo;
        private ComboBox cbSituacao;
        private ComboBox cbTipoQuitacao;
        private DateTimePicker dtAverbacao;
        private DateTimePicker dtVencimento;
        private DateTimePicker dtQuitacao;
        private DataGridView dgvParcelas;
        private Button btnSalvar;
        private Button btnCancelar;
        private Button btnCalcularLiberado;

        public double AgioCalculado { get; private set; }

        public FormEditarEmprestimo(int emprestimoId)
        {
            emprestimo = repo.GetById(emprestimoId)
                ?? throw new Exception("Contrato não encontrado.");

            InitializeComponent();
            PreencherCampos();
            CarregarParcelas();
        }

        private void InitializeComponent()
        {
            this.Icon = new Icon("app.ico");
            Text = "Avaliar/Editar Contrato";
            Width = 650;
            Height = 860;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.AutoScaleMode = AutoScaleMode.None;
            this.AutoSize = false;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            int top = 40;
            int leftLabel = 20;
            int leftInput = 150;

            MenuStrip menu = new MenuStrip();
            menu.Dock = DockStyle.Top;

            // Menu Ferramentas
            ToolStripMenuItem menuFerramentas = new ToolStripMenuItem("Operações");

            menuFerramentas.DropDownItems.Add("Refinanciamento sem Troco", null, (s, e) =>
            {   
                var tiposFinalizados = new[]
                {
                    "Antecipada",
                    "Normal",
                    "Refinanciada"
                };

                if (emprestimo.TipoQuitacao != null &&
                    tiposFinalizados.Any(t =>
                        string.Equals(emprestimo.TipoQuitacao, t, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show("Contrato já foi quitado.");
                    return;
                }

                using var frm = new FormRefinanciamentoSemTroco(emprestimo);

                if (frm.ShowDialog() == DialogResult.OK)
                {
                    MessageBox.Show("Refinanciamento realizado com sucesso!");
                    DialogResult = DialogResult.OK;
                }
            });

            menuFerramentas.DropDownItems.Add("Quitação Antecipada de Contrato", null, (s, e) =>
            {
                RealizarQuitacaoAntecipada();
            });

            menuFerramentas.DropDownItems.Add(new ToolStripSeparator());

            menuFerramentas.DropDownItems.Add("Gerar Contrato Atual", null, (s, e) => BtnGerarContrato_Click(s, e));
        
            // Adicionar itens ao menu
            menu.Items.Add(menuFerramentas);

            // Registrar menu no formulário
            this.MainMenuStrip = menu;
            this.Controls.Add(menu);

            Label L(string t)
            {
                var lbl = new Label { Text = t, Left = leftLabel, Top = top + 5, Width = 120 };
                Controls.Add(lbl);
                return lbl;
            }

            TextBox T()
            {
                var txt = new TextBox { Left = leftInput, Top = top, Width = 200 };
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
                    Width = 200,
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
                    Width = 200,
                    DropDownStyle = ComboBoxStyle.DropDownList
                };
                cb.Items.AddRange(items);
                Controls.Add(cb);
                top += 35;
                return cb;
            }

            // ---------- CAMPOS ----------
            L("Contrato:"); txtContrato = T();

            L("Valor Liberado:"); txtLiberado = T();

            L("Ágio:"); txtAgil = T();

            L("Parcelas:");
                cbParcelas = new ComboBox
                {
                    Left = leftInput,
                    Top = top,
                    Width = 200,
                    DropDownStyle = ComboBoxStyle.DropDownList
                };

                for (int i = 1; i <= 12; i++)
                    cbParcelas.Items.Add(i.ToString());

                Controls.Add(cbParcelas);
                top += 35;

            L("Código:"); cbCodigo = C(
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
                );

            L("Averbação:"); dtAverbacao = D();

            L("Vencimento:"); dtVencimento = D();

            L("Valor do Contrato:"); txtValorContrato = T();
            txtValorContrato.ReadOnly = true;

            L("Total de Juros:"); txtTotalJuros = T();
            txtTotalJuros.ReadOnly = true;

            L("Total Amortização:"); txtTotalAmortizacao = T();
            txtTotalAmortizacao.ReadOnly = true;

            L("Tipo:");
                cbTipo = new ComboBox
                {
                    Left = leftInput,
                    Top = top,
                    Width = 200,
                    DropDownStyle = ComboBoxStyle.DropDownList
                };

                cbTipo.Items.AddRange(new object[]
                {
                    "Mensal",
                    "Quinzenal",
                    "Semanal",
                    "Diario"
                });

                Controls.Add(cbTipo);
                top += 35;


            L("Situação:"); cbSituacao = C("Quitado", "A Vencer", "Em Atraso");

            L("Quitação:"); dtQuitacao = D();

            L("Tipo Quitação:"); cbTipoQuitacao = C("","Normal", "Antecipada", "Em Curso", "Atrazada", "Refinanciada", "Refinanciada Com Troco");

            
            // ---------- TABELA DE PARCELAS ----------
            Label lblParcelas = new Label
            {
                Text = "Parcelas do Empréstimo",
                Left = 20,
                Top = top + 10,
                Width = 200
            };
            Controls.Add(lblParcelas);

            dgvParcelas = new DataGridView
            {
                Left = 20,
                Top = top + 35,
                Width = 590,
                Height = 180,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = true,
                AutoGenerateColumns = false
            };
            

            dgvParcelas.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Parcela",
                DataPropertyName = "NParcelas",
                Width = 70
            });

            dgvParcelas.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Vencimento",
                DataPropertyName = "Vencimento",
                Width = 90,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" }
            });

            dgvParcelas.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Pagamento",
                DataPropertyName = "Pagamento",
                Width = 90,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" }
            });

            dgvParcelas.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Valor",
                DataPropertyName = "ValorPrestacao",
                Width = 90,
                DefaultCellStyle = { Format = "N2" }
            });

            dgvParcelas.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Juros",
                DataPropertyName = "Juros",
                Width = 90,
                DefaultCellStyle = { Format = "N2" }
            });
            
            dgvParcelas.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Amortizacao",
                DataPropertyName = "Amortizacao",
                Width = 90,
                DefaultCellStyle = { Format = "N2" }
            });

            dgvParcelas.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Situação",
                DataPropertyName = "Situacao",
                Width = 120
            });

            dgvParcelas.CellDoubleClick += DgvParcelas_CellDoubleClick;
            dgvParcelas.RowPrePaint += DgvParcelas_RowPrePaint;

            Controls.Add(dgvParcelas);


            // ---------- BOTÕES ----------
            btnSalvar = new Button
            {
                Text = "Salvar",
                Left = 250,
                Top = top + 240,
                Width = 120,
                Height = 40
            };
            btnSalvar.Click += BtnSalvar_Click;

            btnCancelar = new Button
            {
                Text = "Cancelar",
                Left = 390,
                Top = top + 240,
                Width = 120,
                Height = 40
            };
            btnCancelar.Click += (s, e) => DialogResult = DialogResult.Cancel;

            Controls.Add(btnSalvar);
            Controls.Add(btnCancelar);

            btnCalcularLiberado = new Button
            {
                Text = "Calcular Liberado",
                Left = leftInput + 210,
                Top = txtLiberado.Top,
                Width = 120,
                Height = 26
            };

            btnCalcularLiberado.Click += BtnCalcularLiberado_Click;
            Controls.Add(btnCalcularLiberado);

             var btnCobranca = new Button
            {
                Text = "Gerar Cobrança",
                Left = 110,
                Top = top + 240,
                Width = 120,
                Height = 40
            };

            btnCobranca.Click += BtnGerarCobranca_Click;
            Controls.Add(btnCobranca);

        }

        private void PreencherCampos()
        {
            txtContrato.Text = emprestimo.Contrato.ToString();
            txtLiberado.Text = emprestimo.Liberado.ToString("F2");
            txtAgil.Text = emprestimo.Agio.ToString("F2");

            txtValorContrato.Text = emprestimo.ValorContrato.ToString("F2");
            txtTotalJuros.Text = emprestimo.TotalJuros.ToString("F2");
            txtTotalAmortizacao.Text = emprestimo.TotalAmortizacao.ToString("F2");

            cbCodigo.SelectedItem = emprestimo.Codigo.ToString("D3");
            cbParcelas.SelectedItem = emprestimo.Parcelas.ToString();
            cbTipo.SelectedItem = emprestimo.Tipo;
            cbSituacao.Text = emprestimo.Situacao;
            cbTipoQuitacao.SelectedItem = emprestimo.TipoQuitacao;

            SetDate(dtAverbacao, emprestimo.Averbacao);
            SetDate(dtVencimento, emprestimo.Vencimento);
            SetDate(dtQuitacao, emprestimo.Quitacao);
        }


        private void BtnSalvar_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtContrato.Text, out var contrato) ||
                !double.TryParse(txtLiberado.Text, out var liberado) ||
                !double.TryParse(txtAgil.Text, out var agil) ||
                !int.TryParse(cbParcelas.SelectedItem?.ToString(), out var parcelas) ||
                !int.TryParse(cbCodigo.Text, out var codigo))
            {
                MessageBox.Show("Verifique os valores numéricos.");
                return;
            }

            emprestimo.Contrato = contrato;
            emprestimo.Liberado = liberado;
            emprestimo.Agio = agil;
            emprestimo.Parcelas = parcelas;
            emprestimo.Codigo = codigo;

            emprestimo.Averbacao = GetDate(dtAverbacao);
            emprestimo.Vencimento = GetDate(dtVencimento);
            emprestimo.Quitacao = GetDate(dtQuitacao);

            emprestimo.Tipo = cbTipo.Text;
            emprestimo.Situacao = cbSituacao.Text;
            emprestimo.TipoQuitacao = cbTipoQuitacao.Text;

            RecalcularTotaisDoEmprestimo();

            emprestimo.ValorContrato = double.Parse(txtValorContrato.Text);

            repo.Update(emprestimo);

            
            MessageBox.Show("Empréstimo atualizado com sucesso!");
            DialogResult = DialogResult.OK;
            var dashboard = Application.OpenForms.OfType<FormDashboard>().FirstOrDefault();dashboard?.RefreshSeguro();
        }

        // 🔧 Helpers
        private static void SetDate(DateTimePicker dt, DateTime? value)
        {
            if (value.HasValue)
            {
                dt.Value = value.Value;
                dt.Checked = true;
            }
            else
            {
                dt.Checked = false;
            }
        }

        private void CarregarParcelas()
        {
            var parcelas = parcelaRepo.GetByEmprestimo(emprestimo.Id);

            foreach (var parcela in parcelas)
            {
                if (!parcela.Vencimento.HasValue)
                    continue;

                var novaSituacao = parcela.Situacao;

                if (parcela.Vencimento.Value.Date < DateTime.Today)
                {
                    if (parcela.Situacao == "A Vencer")
                        novaSituacao = "Em Atraso";
                }

                if (novaSituacao != parcela.Situacao)
                {
                    parcela.Situacao = novaSituacao;
                    parcelaRepo.Update(parcela);
                }
            }

            dgvParcelas.DataSource = null;
            dgvParcelas.DataSource = parcelas;
        }


        private void DgvParcelas_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var parcela = (Parcela)dgvParcelas.Rows[e.RowIndex].DataBoundItem;

            using var frm = new FormEditarParcela(parcela.Id);

            if (frm.ShowDialog() == DialogResult.OK)
            {
                double agioAtual = 0;
                double.TryParse(txtAgil.Text, out agioAtual);

                agioAtual += frm.AgioCalculado;

                txtAgil.Text = agioAtual.ToString("F2");

                // 🔥 Atualiza no objeto
                emprestimo.Agio = agioAtual;

                // 🔥 SALVA NO BANCO ANTES de recarregar
                //emprestimoRepo.Update(emprestimo);

                CarregarParcelas();
                RecalcularTotaisDoEmprestimo();
            }
        }

        private void BtnCalcularLiberado_Click(object? sender, EventArgs e)
        {
            using var frm = new FormCalcularLiberado();

            if (frm.ShowDialog() == DialogResult.OK)
            {
                txtLiberado.Text = frm.ValorLiberado.ToString("F2");
            }
        }

        private void DgvParcelas_RowPrePaint(object? sender, DataGridViewRowPrePaintEventArgs e)
        {
            var row = dgvParcelas.Rows[e.RowIndex];

            if (row.DataBoundItem is not Parcela parcela)
                return;

            bool paga =
                string.Equals(parcela.Situacao, "Paga", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(parcela.Situacao, "Paga em Atraso", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(parcela.Situacao, "Paga Antecipado", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(parcela.Situacao, "Refinanciada", StringComparison.OrdinalIgnoreCase);

            bool emAtraso =
                parcela.Vencimento.HasValue &&
                parcela.Vencimento.Value.Date < DateTime.Today &&
                !paga;


            if (emAtraso)
            {
                row.DefaultCellStyle.BackColor = Color.MistyRose;
                row.DefaultCellStyle.ForeColor = Color.DarkRed;
                row.DefaultCellStyle.SelectionBackColor = Color.IndianRed;
                row.DefaultCellStyle.SelectionForeColor = Color.White;
            }
            else
            {
                // reset total
                row.DefaultCellStyle = new DataGridViewCellStyle(dgvParcelas.DefaultCellStyle);
            }
        }

        private void RecalcularTotaisDoEmprestimo()
        {
            var parcelas = parcelaRepo.GetByEmprestimo(emprestimo.Id);

            double totalJuros = parcelas.Sum(p => p.Juros ?? 0);
            double totalAmortizacao = parcelas.Sum(p => p.Amortizacao ?? 0);
            double totalPrestacoes = parcelas.Sum(p => p.ValorPrestacao ?? 0);

            emprestimo.TotalJuros = totalJuros;
            emprestimo.TotalAmortizacao = totalAmortizacao;
            emprestimo.ValorContrato = totalPrestacoes;

            txtTotalJuros.Text = totalJuros.ToString("F2");
            txtTotalAmortizacao.Text = totalAmortizacao.ToString("F2");
            txtValorContrato.Text = totalPrestacoes.ToString("F2");

            // 🔥 Mantém o Ágio já existente no contrato
            txtAgil.Text = emprestimo.Agio.ToString("F2");
        }

        private void RecalcularTotaisSemValorContrato()
        {
            var parcelas = parcelaRepo.GetByEmprestimo(emprestimo.Id);

            double totalJuros = parcelas.Sum(p => p.Juros ?? 0);
            double totalAmortizacao = parcelas.Sum(p => p.Amortizacao ?? 0);
            double totalPrestacoes = parcelas.Sum(p => p.ValorPrestacao ?? 0);

            emprestimo.TotalJuros = totalJuros;
            emprestimo.TotalAmortizacao = totalAmortizacao;
            //emprestimo.ValorContrato = totalPrestacoes;

            txtTotalJuros.Text = totalJuros.ToString("F2");
            txtTotalAmortizacao.Text = totalAmortizacao.ToString("F2");
            //txtValorContrato.Text = ValorContrato.ToString("F2");

            // 🔥 Mantém o Ágio já existente no contrato
            txtAgil.Text = emprestimo.Agio.ToString("F2");
        }

       private void BtnGerarContrato_Click(object sender, EventArgs e)
        {
            try
            {
                if (emprestimo == null)
                {
                    MessageBox.Show("Contrato inválido.");
                    return;
                }

                var clienteRepo = new ClienteRepository();
                var cliente = clienteRepo.GetById(emprestimo.ClienteId);

                if (cliente == null)
                {
                    MessageBox.Show("Cliente não encontrado.");
                    return;
                }

                var parcelas = parcelaRepo.GetByEmprestimo(emprestimo.Id);

                if (parcelas.Count == 0)
                {
                    MessageBox.Show("Este contrato não possui parcelas.");
                    return;
                }

                string pasta = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "Contratos");

                Directory.CreateDirectory(pasta);

                string nomeBase =
                    $"Contrato_{emprestimo.Contrato}_{cliente.Nome}_{Session.CurrentUsername}_{emprestimo.Averbacao:dd-MM-yyyy_HH-mm-ss}";

                nomeBase = string.Concat(
                    nomeBase.Split(Path.GetInvalidFileNameChars())
                );

                // =====================
                // PDF
                // =====================
               /* 
               string caminhoPdf = Path.Combine(pasta, nomeBase + ".pdf");

                var documentoPdf = new ContratoEmprestimoPdf(
                    cliente,
                    emprestimo,
                    parcelas
                );

                documentoPdf.GeneratePdf(caminhoPdf);
                */

                // =====================
                // WORD
                // =====================
                string caminhoWord = Path.Combine(pasta, nomeBase + ".docx");

                var documentoWord = new ContratoEmprestimoWord(
                    cliente,
                    emprestimo,
                    parcelas
                );

                documentoWord.Gerar(caminhoWord);

                // =====================
                // VISUALIZA PDF
                // =====================
                //var viewer = new FormPdfViewer(caminhoPdf);
                //viewer.ShowDialog();

                MessageBox.Show(
                    "Contrato gerado com sucesso!\n\nWord Criado Com Sucesso.",
                    "Sucesso",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                // =====================
                // VISUALIZA WORD
                // =====================

                Process.Start(new ProcessStartInfo { FileName = caminhoWord, UseShellExecute = true});
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

        private void SubstituirCampos(
            WordprocessingDocument doc,
            Dictionary<string, string> campos)
        {
            var mainPart = doc.MainDocumentPart;

            // Documento principal
            SubstituirEmOpenXmlPart(mainPart, campos);

            // Cabeçalhos
            foreach (var header in mainPart.HeaderParts)
                SubstituirEmOpenXmlPart(header, campos);

            // Rodapés
            foreach (var footer in mainPart.FooterParts)
                SubstituirEmOpenXmlPart(footer, campos);
        }

        private void SubstituirEmOpenXmlPart(
            OpenXmlPart part,
            Dictionary<string, string> campos)
        {
            using (var stream = part.GetStream())
            using (var reader = new StreamReader(stream))
            {
                string xml = reader.ReadToEnd();

                foreach (var campo in campos)
                {
                    xml = xml.Replace(campo.Key, campo.Value);
                }

                stream.SetLength(0);

                using (var writer = new StreamWriter(stream))
                {
                    writer.Write(xml);
                }
            }
        }

        private void BtnGerarCobranca_Click(object sender, EventArgs e)
        {
            if (dgvParcelas.SelectedRows.Count == 0)
            {
                MessageBox.Show("Selecione ao menos uma parcela.");
                return;
            }

            var parcelasSelecionadas = dgvParcelas.SelectedRows
                .Cast<DataGridViewRow>()
                .Select(r => (Parcela)r.DataBoundItem)
                .Where(p =>
                    p.Vencimento.HasValue &&
                    p.Vencimento.Value.Date < DateTime.Today &&
                    !string.Equals(p.Situacao, "Paga", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(p.Situacao, "Paga em Atraso", StringComparison.OrdinalIgnoreCase)
                )
                .ToList();

            if (parcelasSelecionadas.Count == 0)
            {
                MessageBox.Show("Nenhuma parcela em atraso selecionada.");
                return;
            }

            GerarPdfCobranca(parcelasSelecionadas);
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

            txtAgil.Text = agio.ToString("F2");
        }

        private void GerarPdfCobranca(List<Parcela> parcelas)
        {
            var clienteRepo = new ClienteRepository();
            var cliente = clienteRepo.GetById(emprestimo.ClienteId);

            if (cliente == null)
            {
                MessageBox.Show("Cliente não encontrado.");
                return;
            }

            string pasta = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "Cobrancas"
            );

            Directory.CreateDirectory(pasta);

            string nomeArquivo =
                $"Cobranca_{emprestimo.Contrato}_{cliente.Nome}_{Environment.MachineName}_{Session.CurrentUsername}_{emprestimo.Averbacao:dd-MM-yyyy_HH-mm-ss}.pdf";

            string caminho = Path.Combine(pasta, nomeArquivo);

            var pdf = new CobrancaPdf(cliente, emprestimo, parcelas);
            pdf.GeneratePdf(caminho);

            var viewer = new FormPdfViewer(caminho);
            viewer.ShowDialog();
        }

        private static DateTime? GetDate(DateTimePicker dt)
            => dt.Checked ? dt.Value.Date : null;

       private void RealizarQuitacaoAntecipada()
        {
            var parcelas = parcelaRepo.GetByEmprestimo(emprestimo.Id);

            var parcelasAbertas = parcelas
                .Where(p =>
                    !string.Equals(p.Situacao, "Paga", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(p.Situacao, "Paga Em Atraso", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(p.Situacao, "Paga Antecipado", StringComparison.OrdinalIgnoreCase)
                )
                .OrderBy(p => p.Vencimento)
                .ToList();

            if (!parcelasAbertas.Any())
            {
                MessageBox.Show("Contrato já está quitado.");
                return;
            }

            // verifica se existe apenas uma parcela recomenda para outra função e bloqueia a operação
            if (parcelasAbertas.Count == 1)
            {
                MessageBox.Show(
                    "A quitação antecipada só pode ser realizada quando existir mais de uma parcela em aberto.\nUtilize a quitação antecipada nas opções de parcelas, o sistema executara quitação o contrato altomaticamente!",
                    "Quitação não permitida",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            // 🔹 abre tela que já faz os cálculos
            var frm = new FormConfirmarQuitacao(emprestimo.Id);

            if (frm.ShowDialog() != DialogResult.OK || !frm.Confirmado)
                return;

            double valorQuitacao = frm.ValorQuitacaoFinal;
            MessageBox.Show($"Valor da quitação: {valorQuitacao:C}");
            MessageBox.Show($"frm Valor: {frm.ValorQuitacaoFinal:C}");

            // 🔹 atualizar contrato
            emprestimo.Situacao = "Quitado";
            emprestimo.TipoQuitacao = "Antecipada";
            emprestimo.Quitacao = DateTime.Today;
            emprestimo.ValorContrato = valorQuitacao;

            repo.Update(emprestimo);

            var cliente = clienteRepo.GetById(emprestimo.ClienteId);

            if (cliente != null && !emprestimo.Contabilizado)
            {
                cliente.QtdContratos += 1;
                clienteRepo.Update(cliente);
                emprestimo.Contabilizado = true;
                repo.Update(emprestimo);
            }

            CarregarParcelas();
            RecalcularTotaisSemValorContrato();
            txtValorContrato.Text = valorQuitacao.ToString("F2");
            cbSituacao.SelectedItem = emprestimo.Situacao;
            cbTipoQuitacao.SelectedItem = emprestimo.TipoQuitacao;
            SetDate(dtQuitacao, emprestimo.Quitacao);
            CalcularAgio();

            MessageBox.Show("Contrato quitado com sucesso!");
        }
    }
}
