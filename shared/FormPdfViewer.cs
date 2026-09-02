using Microsoft.Web.WebView2.WinForms;
using System;
using System.Windows.Forms;
using System.Drawing;

namespace CrudApp.Shared
{
    public class FormPdfViewer : Form
    {
        private WebView2 webView;
        private string filePath;

        public FormPdfViewer(string path)
        {
            filePath = path;
            InitializeComponent();
        }

        private async void InitializeComponent()
        {   
            webView = new WebView2();
            webView.Dock = DockStyle.Fill;
            this.ClientSize = new System.Drawing.Size(900, 1200);
            this.Icon = new Icon("app.ico");

            Controls.Add(webView);
            Text = "Visualizador de PDF";
            //WindowState = FormWindowState.Maximized;

            await webView.EnsureCoreWebView2Async();
            webView.Source = new Uri(filePath);
        }
    }
}
