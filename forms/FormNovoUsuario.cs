using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Data.SQLite;
using CrudApp.Database;
using CrudApp;

namespace CrudApp.Forms
{
    public class FormNovoUsuario : Form
    {
        private TextBox txtUser;
        private TextBox txtPass;
        private ComboBox cbRole;
        private Button btnSalvar;
        private Button btnExcluir;

        private readonly int? userId = null;

        public FormNovoUsuario(int? id = null)
        {
            userId = id;

            InitializeComponent();

            if (userId != null)
                LoadData();
        }

        private void InitializeComponent()
        {
            this.Icon = new Icon("app.ico");
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            txtUser = new TextBox
            {
                Left = 20,
                Top = 20,
                Width = 220,
                PlaceholderText = "Username"
            };

            txtPass = new TextBox
            {
                Left = 20,
                Top = 60,
                Width = 220,
                PlaceholderText = "Nova senha (opcional)",
                UseSystemPasswordChar = true
            };

            cbRole = new ComboBox
            {
                Left = 20,
                Top = 100,
                Width = 220,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            cbRole.Items.AddRange(new string[]
            {
                "admin",
                "user"
            });

            cbRole.SelectedIndex = 1;

            btnSalvar = new Button
            {
                Left = 20,
                Top = 150,
                Width = 100,
                Text = "Salvar"
            };

            btnSalvar.Click += BtnSalvar_Click;

            btnExcluir = new Button
            {
                Left = 140,
                Top = 150,
                Width = 100,
                Text = "Excluir"
            };

            btnExcluir.Click += BtnExcluir_Click;

            if (userId == null)
                btnExcluir.Enabled = false;

            this.Controls.AddRange(new Control[]
            {
                txtUser,
                txtPass,
                cbRole,
                btnSalvar,
                btnExcluir
            });

            this.ClientSize = new Size(280, 220);
            this.Text = userId == null
                ? "Novo Usuário"
                : "Editar Usuário";
        }

        private void LoadData()
        {
            using var conn = Database.Database.GetConnection();
            conn.Open();

            string sql = @"
                SELECT id, username, role
                FROM usuarios
                WHERE id = @id
                LIMIT 1";

            using var cmd = new SQLiteCommand(sql, conn);

            cmd.Parameters.AddWithValue("@id", userId.Value);

            using var reader = cmd.ExecuteReader();

            if (!reader.Read())
                return;

            txtUser.Text = reader["username"]?.ToString() ?? "";

            string role = reader["role"]?.ToString() ?? "user";

            cbRole.SelectedItem = role;

            if (cbRole.SelectedIndex == -1)
                cbRole.SelectedIndex = 1;
        }

        private void BtnSalvar_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUser.Text))
            {
                MessageBox.Show(
                    "Informe o username.",
                    "Aviso",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            string username = txtUser.Text.Trim();
            string role = cbRole.SelectedItem?.ToString() ?? "user";

            // ==========================================
            // MODO CRIAR
            // ==========================================
            if (userId == null)
            {
                if (string.IsNullOrWhiteSpace(txtPass.Text))
                {
                    MessageBox.Show(
                        "Informe a senha para criar o usuário.",
                        "Aviso",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );

                    return;
                }

                try
                {
                    Auth.CreateUser(
                        username,
                        txtPass.Text,
                        role
                    );

                    MessageBox.Show(
                        "Usuário criado com sucesso!",
                        "Sucesso",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Erro ao criar usuário:\n{ex.Message}",
                        "Erro",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );

                    return;
                }
            }
            // ==========================================
            // MODO EDITAR
            // ==========================================
            else
            {
                try
                {
                    Auth.UpdateUsername(
                        userId.Value,
                        username
                    );

                    Auth.UpdateUserRole(
                        userId.Value,
                        role
                    );

                    if (!string.IsNullOrWhiteSpace(txtPass.Text))
                    {
                        Auth.ChangePassword(
                            userId.Value,
                            txtPass.Text
                        );
                    }

                    MessageBox.Show(
                        "Usuário atualizado!",
                        "Sucesso",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Erro ao atualizar usuário:\n{ex.Message}",
                        "Erro",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );

                    return;
                }
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void BtnExcluir_Click(object? sender, EventArgs e)
        {
            if (userId == null)
                return;

            DialogResult resultado = MessageBox.Show(
                "Deseja excluir este usuário?",
                "Confirmar exclusão",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (resultado != DialogResult.Yes)
                return;

            try
            {
                Auth.DeleteUser(userId.Value);

                MessageBox.Show(
                    "Usuário excluído!",
                    "Sucesso",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Erro ao excluir usuário:\n{ex.Message}",
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}

