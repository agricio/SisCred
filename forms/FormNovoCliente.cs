using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using CrudApp.Models;
using CrudApp.Repositories;
using CrudApp.Services;

namespace CrudApp.Forms
{
    public class FormNovoCliente : Form
    {
        private readonly ClienteRepository repo = new ClienteRepository();
        private Cliente? editing;

        private TextBox txtNome, txtEmail, txtEstado, txtCidade, txtRua, txtNumero, txtBairro, txtNacionalidade, txtComplemento, txtBanco, txtOcupacao, txtCel_pix;
        private ComboBox cbScore, cbEstado_civil;
        private PictureBox picFoto;
        private Button btnTirarFoto, btnSalvar, btnCancelar, btnAbrirCamera, btnGerarPdf;
        private NumericUpDown numQtdContratos;
        private byte[]? fotoBytes;

        private MaskedTextBox txtRg, txtCpf, txtTelefone , txtAgencia, txtConta, txtCep;

        public FormNovoCliente(int? id = null)
        {
            InitializeComponent();

            if (id.HasValue)
            {
                editing = repo.GetById(id.Value);
                if (editing != null) LoadEditing();
            }
        }

        private void InitializeComponent()
        {
            this.Icon = new Icon("app.ico");
            this.ClientSize = new Size(570, 730);
            this.Text = "Speed Cred - Adicionar Cliente";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.AutoScaleMode = AutoScaleMode.None;
            this.AutoSize = false;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            
            // Labels + inputs (arranjo simples)
            int leftLabel = 20;
            int leftInput = 120;
            int top = 20;
            int vGap = 32;
            int labelW = 90;
            int inputW = 220;
            
            // --- CAMPOS ---

            Label L(string text, int t)
            {
                var lbl = new Label { Text = text, Left = leftLabel, Top = t + 3, Width = labelW };
                this.Controls.Add(lbl);
                return lbl;
            }

            // NOME
            txtNome = new TextBox { Left = leftInput, Top = top, Width = inputW };
            L("Nome:", top);
            this.Controls.Add(txtNome);
            top += vGap;

            // RG
            txtRg = new MaskedTextBox
            { Left = leftInput, Top = top, Width = 90, Mask = "00.000.000-0" };
            this.Controls.Add(txtRg);
            L("RG:", top);
            top += vGap;

            // CPF
            txtCpf = new MaskedTextBox { Left = leftInput, Top = top, Width = 90, Mask = "000.000.000-00" };
            this.Controls.Add(txtCpf);
            L("CPF:", top);
            top += vGap;

            // Ocupação
            txtOcupacao = new TextBox { Left = leftInput, Top = top, Width = inputW };
            L("Ocupação:", top);
            Controls.Add(txtOcupacao);
            top += vGap;

            // Nacionalidade
            txtNacionalidade = new TextBox { Left = leftInput, Top = top, Width = inputW };
            L("Nacionalidade:", top);
            Controls.Add(txtNacionalidade);
            top += vGap;

            // Estado_civil
            cbEstado_civil = new ComboBox
            {
                Left = leftInput,
                Top = top,
                Width = 120,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            cbEstado_civil.Items.AddRange(new object[]
            {
                "Solteiro(a)",
                "Casado(a)",
                "Divorciado(a)"
            });

            cbEstado_civil.SelectedIndex = 0;

            L("Estado CIvil:", top);
            Controls.Add(cbEstado_civil);
            top += vGap;

            // Estado
            txtEstado = new TextBox { Left = leftInput, Top = top, Width = 90 };
            L("Estado:", top);
            Controls.Add(txtEstado);
            top += vGap;

            // Cidade
            txtCidade = new TextBox { Left = leftInput, Top = top, Width = inputW };
            L("Cidade:", top);
            Controls.Add(txtCidade);
            top += vGap;

            // Bairro
            txtBairro = new TextBox { Left = leftInput, Top = top, Width = inputW };
            L("Bairro:", top);
            Controls.Add(txtBairro);
            top += vGap;

            // Rua
            txtRua = new TextBox { Left = leftInput, Top = top, Width = inputW };
            L("Rua:", top);
            Controls.Add(txtRua);
            top += vGap;

            // Número
            txtNumero = new TextBox { Left = leftInput, Top = top, Width = 60 };
            L("Número:", top);
            Controls.Add(txtNumero);
            top += vGap;

            // Complemento
            txtComplemento = new TextBox { Left = leftInput, Top = top, Width = inputW };
            L("Compl.:", top);
            Controls.Add(txtComplemento);
            top += vGap;

            // CEP
            txtCep = new MaskedTextBox
            {
                Left = leftInput,
                Top = top,
                Width = 80,
                Mask = "00000-000"
            };
            L("CEP:", top);
            Controls.Add(txtCep);
            top += vGap;

            // Email
            txtEmail = new TextBox { Left = leftInput, Top = top, Width = inputW };
            L("Email:", top);
            this.Controls.Add(txtEmail);
            top += vGap;

            // Telefone
            txtTelefone = new MaskedTextBox { Left = leftInput, Top = top, Width = 100, Mask = "(00) 00000-0000" };
            this.Controls.Add(txtTelefone);
            L("Telefone:", top);
            top += vGap;

            // Qtd Contratos


            // Score
            cbScore = new ComboBox
            {
                Left = leftInput,
                Top = top,
                Width = 120,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            cbScore.Items.AddRange(new object[]
            {
                "Baixo",
                "Médio",
                "Alto"
            });

            cbScore.SelectedIndex = 0;

            L("Score:", top);
            Controls.Add(cbScore);
            top += vGap;

            // Banco
            txtBanco = new TextBox { Left = leftInput, Top = top, Width = 120 };
            L("Banco:", top);
            this.Controls.Add(txtBanco);
            top += vGap;

            // Agencia
            txtAgencia = new MaskedTextBox { Left = leftInput, Top = top, Width = 60, Mask = "0000-0" };
            L("Agencia:", top);
            this.Controls.Add(txtAgencia);
            top += vGap;

            // Conta
            txtConta = new MaskedTextBox { Left = leftInput, Top = top, Width = 90, Mask = "00000000-0" };
            L("Conta:", top);
            this.Controls.Add(txtConta);
            top += vGap;

            // Pix Telefone

            txtCel_pix = new TextBox { Left = leftInput, Top = top, Width = 140, };
            this.Controls.Add(txtCel_pix);
            L("Chave PIX:", top);
            top += vGap;


                   
            picFoto = new PictureBox
            {
                Left = 390,
                Top = 20,
                Width = 150,
                Height = 200,
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom
            };

            btnTirarFoto = new Button { Left = 400, Top = 230, Width = 120, Text = "Carregar Foto" };
            btnAbrirCamera = new Button { Left = 400, Top = 260, Width = 120, Text = "Abrir Câmera" };
            
            btnSalvar = new Button { Left = 20, Top = 670, Width = 120, Text = "Salvar" };
            btnCancelar = new Button { Left = 160, Top = 670, Width = 120, Text = "Cancelar" };

            // EVENTOS
            btnTirarFoto.Click += BtnTirarFoto_Click;
            btnSalvar.Click += BtnSalvar_Click;
            btnCancelar.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            btnAbrirCamera.Click += BtnAbrirCamera_Click;

            // ADD CONTROLS
            this.Controls.Add(txtNome);
            this.Controls.Add(txtEmail);
            this.Controls.Add(txtTelefone);
            this.Controls.Add(picFoto);
            this.Controls.Add(btnTirarFoto);
            this.Controls.Add(btnSalvar);
            this.Controls.Add(btnCancelar);
            this.Controls.Add(btnAbrirCamera);

        }

        // FOTO DO ARQUIVO
        private void BtnTirarFoto_Click(object? sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog();
            ofd.Filter = "Imagens|*.jpg;*.jpeg;*.png";
            if (ofd.ShowDialog() != DialogResult.OK) return;

            fotoBytes = File.ReadAllBytes(ofd.FileName);

            using var ms = new MemoryStream(fotoBytes);
            picFoto.Image = Image.FromStream(ms);
        }

        // ABRIR FORM DE CÂMERA SEPARADO
        private void BtnAbrirCamera_Click(object? sender, EventArgs e)
        {
            using var formCam = new FormCamera();

            if (formCam.ShowDialog() == DialogResult.OK && formCam.FotoCapturada != null)
            {
                fotoBytes = formCam.FotoCapturada;

                using var ms = new MemoryStream(fotoBytes);
                picFoto.Image = Image.FromStream(ms);
            }
        }

        // CARREGAR DADOS

        private void LoadEditing()
        {
            txtNome.Text = editing!.Nome;
            txtRg.Text = editing.Rg ?? "";
            txtCpf.Text = editing.Cpf ?? "";
            txtEmail.Text = editing.Email ?? "";
            txtOcupacao.Text = editing.Ocupacao ?? "";
            txtTelefone.Text = editing.Telefone ?? "";
            txtCel_pix.Text = editing.Cel_pix ?? "";
            cbEstado_civil.Text = editing.Estado_civil ?? "";
            txtNacionalidade.Text = editing.Nacionalidade ?? "";

            txtEstado.Text = editing.Estado ?? "";
            txtBairro.Text = editing.Bairro ?? "";
            txtCidade.Text = editing.Cidade ?? "";
            txtRua.Text = editing.Rua ?? "";
            txtNumero.Text = editing.Numero ?? "";
            txtComplemento.Text = editing.Complemento ?? "";
            txtCep.Text = editing.Cep ?? "";

            txtBanco.Text = editing.Banco ?? "";
            txtAgencia.Text = editing.Agencia ?? "";
            txtConta.Text = editing.Conta ?? "";

            numQtdContratos.Value = editing.QtdContratos;
            cbScore.SelectedItem = editing.ClienteScore ?? "Baixo";

            if (editing.Foto != null)
            {
                fotoBytes = editing.Foto;
                using var ms = new MemoryStream(editing.Foto);
                picFoto.Image = Image.FromStream(ms);
            }
        }

        // SALVAR
        private void BtnSalvar_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNome.Text))
            {
                MessageBox.Show("Nome obrigatório");
                return;
            }

            if (editing == null)
            {
                var c = new Cliente
                {
                    Nome = txtNome.Text,
                    Rg = MaskToNull(txtRg),
                    Cpf = MaskToNull(txtCpf),
                    Email = EmptyToNull(txtEmail),
                    Ocupacao = EmptyToNull(txtOcupacao),
                    Nacionalidade = EmptyToNull(txtNacionalidade),
                    Telefone = MaskToNull(txtTelefone),
                    Cel_pix = EmptyToNull(txtCel_pix),

                    Estado = EmptyToNull(txtEstado),
                    Cidade = EmptyToNull(txtCidade),
                    Rua = EmptyToNull(txtRua),
                    Bairro = EmptyToNull(txtBairro),
                    Numero = EmptyToNull(txtNumero),
                    Complemento = EmptyToNull(txtComplemento),
                    Cep = MaskToNull((MaskedTextBox)txtCep),

                    Banco = EmptyToNull(txtBanco),
                    Agencia = MaskToNull(txtAgencia),
                    Conta = MaskToNull(txtConta),

                    QtdContratos = 0,
                    ClienteScore = cbScore.SelectedItem?.ToString(),
                    Estado_civil = cbEstado_civil.SelectedItem?.ToString(),

                    Foto = fotoBytes
                };

                repo.Add(c);
            }
            else
            {
                editing.Nome = txtNome.Text;
                editing.Rg = MaskToNull(txtRg);
                editing.Cpf = MaskToNull(txtCpf);
                editing.Email = EmptyToNull(txtEmail);
                editing.Ocupacao = EmptyToNull(txtOcupacao);
                editing.Nacionalidade = EmptyToNull(txtNacionalidade);
                editing.Telefone = MaskToNull(txtTelefone);
                editing.Cel_pix = EmptyToNull(txtCel_pix);

                editing.Estado = EmptyToNull(txtEstado);
                editing.Cidade = EmptyToNull(txtCidade);
                editing.Bairro = EmptyToNull(txtBairro);
                editing.Rua = EmptyToNull(txtRua);
                editing.Numero = EmptyToNull(txtNumero);
                editing.Complemento = EmptyToNull(txtComplemento);
                editing.Cep = MaskToNull((MaskedTextBox)txtCep);

                editing.Banco = EmptyToNull(txtBanco);
                editing.Agencia = MaskToNull(txtAgencia);
                editing.Conta = MaskToNull(txtConta);

                editing.QtdContratos = 0;
                editing.ClienteScore = cbScore.SelectedItem?.ToString();
                editing.Estado_civil = cbEstado_civil.SelectedItem?.ToString();

                editing.Foto = fotoBytes;

                repo.Update(editing);
            }


            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private string? EmptyToNull(TextBox t)
    =>  string.IsNullOrWhiteSpace(t.Text) ? null : t.Text;

        private string? MaskToNull(MaskedTextBox m)
            => m.MaskCompleted ? m.Text : null;


    }
}
