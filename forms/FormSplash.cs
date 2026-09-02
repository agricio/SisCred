using System;
using System.Drawing;
using System.Windows.Forms;

public class FormSplash : Form
{
    public FormSplash()
    {
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.CenterScreen;
        Width = 680;
        Height = 369;

        var img = new PictureBox
        {
            Dock = DockStyle.Fill,
            Image = Image.FromFile("assets/splash.png"), // coloque o png na pasta do exe
            SizeMode = PictureBoxSizeMode.StretchImage
        };

        Controls.Add(img);
    }
}