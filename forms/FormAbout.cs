using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

public class FormAbout : Form
{
    public FormAbout()
{
    Text = "Sobre o Programa";
    StartPosition = FormStartPosition.CenterScreen;
    FormBorderStyle = FormBorderStyle.FixedDialog;
    MaximizeBox = false;
    Width = 420;
    Height = 360;

    var layout = new TableLayoutPanel
    {
        Dock = DockStyle.Fill,
        Padding = new Padding(10),
        ColumnCount = 1,
        RowCount = 4
    };

    // Proporções mais naturais
    layout.RowStyles.Add(new RowStyle(SizeType.Percent, 45)); // imagem
    layout.RowStyles.Add(new RowStyle(SizeType.Percent, 25)); // descrição
    layout.RowStyles.Add(new RowStyle(SizeType.Percent, 15)); // links
    layout.RowStyles.Add(new RowStyle(SizeType.Percent, 15)); // botão

    var img = new PictureBox
    {
        Dock = DockStyle.Fill,
        SizeMode = PictureBoxSizeMode.Zoom,
        Image = Image.FromFile("assets/newlogo.png"),
        Margin = new Padding(0, 0, 0, 8)
    };

    var lblDesc = new Label
    {
        Text = "Sistema de controle de empréstimos\nVersão 0.2.0 (beta)\nDesenvolvido por: Agricio Neto",
        Dock = DockStyle.Fill,
        TextAlign = ContentAlignment.MiddleCenter,
        Font = new Font("Segoe UI", 10),
        Margin = new Padding(0, 4, 0, 4)
    };

    var panelLinks = new FlowLayoutPanel
    {
        Dock = DockStyle.Fill,
        FlowDirection = FlowDirection.LeftToRight,
        Anchor = AnchorStyles.None,
        AutoSize = true
    };

    var linkSite = new LinkLabel { Text = "Site", Margin = new Padding(10, 5, 10, 5) };
    linkSite.Click += (s, e) => Abrir("https://seusite.com");

    var linkWhats = new LinkLabel { Text = "WhatsApp", Margin = new Padding(10, 5, 10, 5) };
    linkWhats.Click += (s, e) => Abrir("https://wa.me/55999999999");

    var linkGit = new LinkLabel { Text = "GitHub", Margin = new Padding(10, 5, 10, 5) };
    linkGit.Click += (s, e) => Abrir("https://github.com/seuusuario");

    panelLinks.Controls.Add(linkSite);
    panelLinks.Controls.Add(linkWhats);
    panelLinks.Controls.Add(linkGit);

    var btnFechar = new Button
    {
        Text = "Fechar",
        Width = 120,
        Height = 32,
        Anchor = AnchorStyles.None
    };
    btnFechar.Click += (s, e) => Close();

    layout.Controls.Add(img, 0, 0);
    layout.Controls.Add(lblDesc, 0, 1);
    layout.Controls.Add(panelLinks, 0, 2);
    layout.Controls.Add(btnFechar, 0, 3);

    Controls.Add(layout);
}


    private void Abrir(string url)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
    }
}
