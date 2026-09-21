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
        private Panel panelConteudo;

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
            this.ClientSize = new Size(570, 670);
            this.Text = "SiS Cred - Adicionar Cliente";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.AutoScaleMode = AutoScaleMode.None;
            this.AutoSize = false;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            //rolagem vertical do form

            panelConteudo = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };

            this.Controls.Add(panelConteudo);
            
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
                var lbl = new Label
                {
                    Text = text,
                    Left = leftLabel,
                    Top = t + 3,
                    Width = labelW
                };

                panelConteudo.Controls.Add(lbl);

                return lbl;
            }

            // NOME
            txtNome = new TextBox { Left = leftInput, Top = top, Width = inputW };
            L("Nome:", top);
            panelConteudo.Controls.Add(txtNome);
            top += vGap;

            // RG
            txtRg = new MaskedTextBox
            { Left = leftInput, Top = top, Width = 90, Mask = "00.000.000-0" };
            panelConteudo.Controls.Add(txtRg);
            L("RG:", top);
            top += vGap;

            // CPF
            txtCpf = new MaskedTextBox { Left = leftInput, Top = top, Width = 90, Mask = "000.000.000-00" };
            panelConteudo.Controls.Add(txtCpf);
            L("CPF:", top);
            top += vGap;

            // Ocupação
            txtOcupacao = new TextBox { Left = leftInput, Top = top, Width = inputW };
            L("Ocupação:", top);
            panelConteudo.Controls.Add(txtOcupacao);
            top += vGap;

            // Nacionalidade
            txtNacionalidade = new TextBox { Left = leftInput, Top = top, Width = inputW };
            L("Nacionalidade:", top);
            panelConteudo.Controls.Add(txtNacionalidade);
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
            panelConteudo.Controls.Add(cbEstado_civil);
            top += vGap;

            // Estado
            txtEstado = new TextBox { Left = leftInput, Top = top, Width = 90 };
            L("Estado:", top);
            panelConteudo.Controls.Add(txtEstado);
            top += vGap;

            // Cidade
            txtCidade = new TextBox { Left = leftInput, Top = top, Width = inputW };
            L("Cidade:", top);
            panelConteudo.Controls.Add(txtCidade);
            top += vGap;

            // Bairro
            txtBairro = new TextBox { Left = leftInput, Top = top, Width = inputW };
            L("Bairro:", top);
            panelConteudo.Controls.Add(txtBairro);
            top += vGap;

            // Rua
            txtRua = new TextBox { Left = leftInput, Top = top, Width = inputW };
            L("Rua:", top);
            panelConteudo.Controls.Add(txtRua);
            top += vGap;

            // Número
            txtNumero = new TextBox { Left = leftInput, Top = top, Width = 60 };
            L("Número:", top);
            panelConteudo.Controls.Add(txtNumero);
            top += vGap;

            // Complemento
            txtComplemento = new TextBox { Left = leftInput, Top = top, Width = inputW };
            L("Compl.:", top);
            panelConteudo.Controls.Add(txtComplemento);
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
            panelConteudo.Controls.Add(txtCep);
            top += vGap;

            // Email
            txtEmail = new TextBox { Left = leftInput, Top = top, Width = inputW };
            L("Email:", top);
            panelConteudo.Controls.Add(txtEmail);
            top += vGap;

            // Telefone
            txtTelefone = new MaskedTextBox { Left = leftInput, Top = top, Width = 100, Mask = "(00) 00000-0000" };
            panelConteudo.Controls.Add(txtTelefone);
            L("Telefone:", top);
            top += vGap;

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
            panelConteudo.Controls.Add(txtBanco);
            top += vGap;

            // Agencia
            txtAgencia = new MaskedTextBox { Left = leftInput, Top = top, Width = 60, Mask = "0000-0" };
            L("Agencia:", top);
            panelConteudo.Controls.Add(txtAgencia);
            top += vGap;

            // Conta
            txtConta = new MaskedTextBox { Left = leftInput, Top = top, Width = 90, Mask = "00000000-0" };
            L("Conta:", top);
            panelConteudo.Controls.Add(txtConta);
            top += vGap;

            // Pix Telefone

            txtCel_pix = new TextBox { Left = leftInput, Top = top, Width = 140, };
            panelConteudo.Controls.Add(txtCel_pix);
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

            btnTirarFoto = new Button { 
                Left = 400, 
                Top = 230, 
                Width = 120,
                Height = 35,  
                Text = "Carregar Foto" 
            };

            btnAbrirCamera = new Button { 
                Left = 400, 
                Top = 270, 
                Width = 120,
                Height = 35,  
                Text = "Abrir Câmera" 
            };
            
            btnSalvar = new Button { 
                Left = 20, 
                Top = 680,  
                Width = 120,
                Height = 35, 
                Text = "Salvar" 
            };

            btnCancelar = new Button { 
                Left = 160, 
                Top = 680, 
                Width = 120,
                Height = 35,  
                Text = "Cancelar" 
            };

            // EVENTOS
            btnTirarFoto.Click += BtnTirarFoto_Click;
            btnSalvar.Click += BtnSalvar_Click;
            btnCancelar.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            btnAbrirCamera.Click += BtnAbrirCamera_Click;

            // ADD CONTROLS
            panelConteudo.Controls.Add(txtNome);
            panelConteudo.Controls.Add(txtEmail);
            panelConteudo.Controls.Add(txtTelefone);
            panelConteudo.Controls.Add(picFoto);
            panelConteudo.Controls.Add(btnTirarFoto);
            panelConteudo.Controls.Add(btnSalvar);
            panelConteudo.Controls.Add(btnCancelar);
            panelConteudo.Controls.Add(btnAbrirCamera);

            // espaço vertical para a rolagem
            panelConteudo.AutoScrollMinSize = new Size(0, top + 80);

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
