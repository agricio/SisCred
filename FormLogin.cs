using System;
using System.Windows.Forms;
using System.Drawing;
using CrudApp.Database;
using CrudApp.Services;
namespace CrudApp
{
    public partial class FormLogin : Form
    {
        public int LoggedUserId { get; private set; }
        public string LoggedUsername { get; private set; }
        public string LoggedUserRole { get; private set; }

        public FormLogin()
        {
            this.Icon = new Icon("app.ico");
            InitializeComponent(); 
        }

        private void btnEntrar_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (username == "" || password == "")
            {
                MessageBox.Show("Preencha usuário e senha!");
                return;
            }

            var result = Auth.ValidateUser(username, password);

            if (!result.ok)
            {
                MessageBox.Show("Usuário ou senha incorretos!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            LoggedUserId = result.userId;
            LoggedUsername = username;
            LoggedUserRole = result.role;

            Session.Login(result.userId, username, result.role);

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
