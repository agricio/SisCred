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
using QuestPDF.Fluent;
using CrudApp.Services;

namespace CrudApp.Forms
{
    public class FormEditarEmprestimoUserAtrasados : Form
    {
        private readonly EmprestimoRepository repo = new EmprestimoRepository();
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
        private readonly ParcelaRepository parcelaRepo = new ParcelaRepository();

        private Button btnSalvar;
        private Button btnCancelar;
        private Button btnCalcularLiberado;

        public double AgioCalculado { get; private set; }

        public FormEditarEmprestimoUserAtrasados(int emprestimoId)
        {
            emprestimo = repo.GetById(emprestimoId)
                ?? throw new Exception("Contrato não encontrado.");

            InitializeComponent();
            PreencherCampos();
            CarregarParcelas();
            OcultarCamposEdicaoEmprestimo();
        }

        private void InitializeComponent()
        {
            this.Icon = new Icon("app.ico");
            Text = "Avaliar/Editar Contrato";
            Width = 750;
            Height = 500;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;

            int top = 40;
            int leftLabel = 20;
            int leftInput = 150;

            MenuStrip menu = new MenuStrip();
            menu.Dock = DockStyle.Top;

            // Menu Ferramentas
            //ToolStripMenuItem menuFerramentas = new ToolStripMenuItem("Operações");

            //menuFerramentas.DropDownItems.Add("Gerar Contrato Atual", null, (s, e) => BtnGerarContrato_Click(s, e));
        
            // Adicionar itens ao menu
           // menu.Items.Add(menuFerramentas);

            // Registrar menu no formulário
            //this.MainMenuStrip = menu;
            //this.Controls.Add(menu);

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

            L("Tipo Quitação:"); cbTipoQuitacao = C("Normal", "Antecipada", "Em Curso", "Atrazada", "Refinanciada", "Refinanciada Com Troco");

            
            // ---------- TABELA DE PARCELAS ----------
            Label lblParcelas = new Label
            {
                Text = "Parcelas do Empréstimo",
                Left = 20,
                Top = 50,
                Width = 200
            };
            Controls.Add(lblParcelas);

            dgvParcelas = new DataGridView
            {
                Left = 20,
                Top = 80,
                Width = 682,
                Height = 250,
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
                Width = 90
            });

            dgvParcelas.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Pagamento",
                DataPropertyName = "Pagamento",
                Width = 90
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
                Left = 290,
                Top = 350,
                Width = 120,
                Height = 40
            };
            btnSalvar.Click += BtnSalvar_Click;

            btnCancelar = new Button
            {
                Text = "Cancelar",
                Left = 430,
                Top = 350,
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
                Top = 200,
                Width = 120,
                Height = 26
            };

            btnCalcularLiberado.Click += BtnCalcularLiberado_Click;
            Controls.Add(btnCalcularLiberado);

            var btnCobranca = new Button
            {
                Text = "Gerar Cobrança",
                Left = 150,
                Top = 350,
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
            cbSituacao.SelectedItem = emprestimo.Situacao;
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

            emprestimo.Tipo = cbTipo.SelectedItem?.ToString() ?? "";
            emprestimo.Situacao = cbSituacao.SelectedItem?.ToString() ?? "";
            emprestimo.TipoQuitacao = cbTipoQuitacao.SelectedItem?.ToString() ?? "";

            emprestimo.Parcelas = int.Parse(cbParcelas.SelectedItem.ToString());
            emprestimo.Tipo = cbTipo.SelectedItem?.ToString() ?? "";

            emprestimo.ValorContrato = double.Parse(txtValorContrato.Text);
            //emprestimo.TotalJuros = double.Parse(txtTotalJuros.Text);
            //emprestimo.TotalAmortizacao = double.Parse(txtTotalAmortizacao.Text);

            RecalcularTotaisDoEmprestimo();
            repo.Update(emprestimo);

            MessageBox.Show("Empréstimo atualizado com sucesso!");
            DialogResult = DialogResult.OK;
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

             // FILTRO: SOMENTE PARCELAS EM ATRASO
            var parcelasEmAtraso = parcelas
                .Where(p =>
                    p.Vencimento.HasValue &&
                    p.Vencimento.Value.Date < DateTime.Today && // já venceu
                    p.Situacao.Trim().Equals("Em Atraso", StringComparison.OrdinalIgnoreCase)
                )
                .OrderBy(p => p.Vencimento)
                .ToList();

                    dgvParcelas.DataSource = null;
                    dgvParcelas.DataSource = parcelasEmAtraso;
                }


        private void DgvParcelas_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var parcela = (Parcela)dgvParcelas.Rows[e.RowIndex].DataBoundItem;

            using var frm = new FormEditarParcela(parcela.Id);

            if (frm.ShowDialog() == DialogResult.OK)
            {
                // pega o valor atual do textbox
                double agioAtual = 0;
                double.TryParse(txtAgil.Text, out agioAtual);

                // SOMA (não sobrescreve)
                agioAtual += frm.AgioCalculado;

                // reflete na tela
                txtAgil.Text = agioAtual.ToString("F2");

                // opcional: mantém no objeto em memória
                emprestimo.Agio = agioAtual;

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
                string.Equals(parcela.Situacao, "Paga Antecipado", StringComparison.OrdinalIgnoreCase);

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
            double totalAgio = parcelas.Sum(p => p.Agio ?? 0);

            emprestimo.TotalJuros = totalJuros;
            emprestimo.TotalAmortizacao = totalAmortizacao;
            emprestimo.ValorContrato = totalPrestacoes;
            emprestimo.Agio = totalAgio;

            // 🔹 atualiza tela
            txtTotalJuros.Text = totalJuros.ToString("F2");
            txtTotalAmortizacao.Text = totalAmortizacao.ToString("F2");
            txtValorContrato.Text = totalPrestacoes.ToString("F2");
            txtAgil.Text = totalAgio.ToString("F2");
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
                $"Cobranca_{emprestimo.Contrato}_{cliente.Nome}_{DateTime.Now:ddMMyyyyHHmm}_{Session.CurrentUsername}.pdf";

            string caminho = Path.Combine(pasta, nomeArquivo);

            var pdf = new CobrancaPdf(cliente, emprestimo, parcelas);
            pdf.GeneratePdf(caminho);

            var viewer = new FormPdfViewer(caminho);
            viewer.ShowDialog();
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

                string nomeArquivo =
                    $"Contrato_{emprestimo.Contrato}_{cliente.Nome}_{emprestimo.Averbacao:ddMMyyyy}.pdf";

                nomeArquivo = string.Concat(
                    nomeArquivo.Split(Path.GetInvalidFileNameChars())
                );

                string caminhoPdf = Path.Combine(pasta, nomeArquivo);

                var documento = new ContratoEmprestimoPdf(
                    cliente,
                    emprestimo,
                    parcelas
                );

                documento.GeneratePdf(caminhoPdf);
                        var viewer = new FormPdfViewer(caminhoPdf);
                            viewer.ShowDialog(); // ou Show()

               // System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
               // {
                 //   FileName = caminhoPdf,
                 //   UseShellExecute = true
               // });
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

        private void OcultarCamposEdicaoEmprestimo()
        {
            // 🔹 TextBox
            txtContrato.Visible = false;
            txtLiberado.Visible = false;
            txtAgil.Visible = false;
            txtValorContrato.Visible = false;
            txtTotalJuros.Visible = false;
            txtTotalAmortizacao.Visible = false;

            // 🔹 ComboBox
            cbParcelas.Visible = false;
            cbCodigo.Visible = false;
            cbTipo.Visible = false;
            cbSituacao.Visible = false;
            cbTipoQuitacao.Visible = false;

            // 🔹 DateTimePicker
            dtAverbacao.Visible = false;
            dtVencimento.Visible = false;
            dtQuitacao.Visible = false;

            // 🔹 Botão
            btnCalcularLiberado.Visible = false;

            // 🔹 OCULTAR LABELS PELO TEXTO
            string[] labelsParaOcultar =
            {
                "Contrato:",
                "Valor Liberado:",
                "Ágio:",
                "Parcelas:",
                "Código:",
                "Averbação:",
                "Vencimento:",
                "Valor do Contrato:",
                "Total de Juros:",
                "Total Amortização:",
                "Tipo:",
                "Situação:",
                "Quitação:",
                "Tipo Quitação:"
            };

            foreach (var lbl in Controls.OfType<Label>())
            {
                if (labelsParaOcultar.Contains(lbl.Text))
                    lbl.Visible = false;
            }
        }


        private static DateTime? GetDate(DateTimePicker dt)
            => dt.Checked ? dt.Value.Date : null;
    }
}
