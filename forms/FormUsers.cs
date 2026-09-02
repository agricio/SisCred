using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CrudApp.Database;

using CrudApp;

namespace CrudApp.Forms
{
    public class FormUsers : Form
    {
        private DataGridView grid;
        private Button btnAdd;
        private Panel topPanel;
        private Panel panelGrid;

        public FormUsers()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void InitializeComponent()
        {
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Icon = new Icon("app.ico");

            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.AutoScaleMode = AutoScaleMode.None;
            this.AutoSize = false;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            
            // Painel do topo (onde fica o botão)
            topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                Padding = new Padding(10)
            };

            // Botão
            btnAdd = new Button
            {
                Text = "Criar Usuário",
                Height = 30,
                Width = 100,
                Left = 10,
                Top = 10
            };
            btnAdd.Click += BtnAdd_Click;

            topPanel.Controls.Add(btnAdd);

            // Painel da tabela
            panelGrid = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            // Grid
            grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false
            };

          
            grid.CellDoubleClick += Grid_CellDoubleClick;
            grid.CellClick += Grid_CellClick;
            panelGrid.Controls.Add(grid);

            // Adiciona tudo no Form
            this.Controls.Add(panelGrid);
            this.Controls.Add(topPanel);

            this.ClientSize = new Size(650, 450);
            this.Text = "Speed Cred - Gerenciamento de Usuários";
        }

    private void LoadUsers()
    {
        grid.Columns.Clear(); // Remove colunas duplicadas depois da atualização

        var list = Auth.GetAllUsers();
        grid.DataSource = list.Select(u => new
        {
            ID = u.id,
            Usuário = u.username,
            Permissão = u.role
        }).ToList();

        // --- ADICIONA O BOTÃO "EXCLUIR" NO FINAL ---
        var colExcluir = new DataGridViewButtonColumn
        {
            HeaderText = "Excluir",
            Text = "Excluir",
            UseColumnTextForButtonValue = true,
            Width = 60,
            Name = "btnExcluir"
        };

        grid.Columns.Add(colExcluir); // <-- AGORA É REALMENTE A ÚLTIMA COLUNA
    }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            using var dlg = new FormNovoUsuario(); // modo criar
            if (dlg.ShowDialog() == DialogResult.OK)
                LoadUsers();
        }

        // Abrir tela de edição com duplo clique
        private void Grid_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            int id = Convert.ToInt32(grid.Rows[e.RowIndex].Cells["ID"].Value);

            using var dlg = new FormNovoUsuario(id); // modo editar
            if (dlg.ShowDialog() == DialogResult.OK)
                LoadUsers();
        }

        private void Grid_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (grid.Columns[e.ColumnIndex].Name == "btnExcluir")
            {
                int id = Convert.ToInt32(grid.Rows[e.RowIndex].Cells["ID"].Value);

                if (MessageBox.Show("Deseja excluir este usuário?",
                    "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    Auth.DeleteUser(id);
                    LoadUsers();
                }
            }
        }
    }
}
