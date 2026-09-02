using System;
using System.Drawing;
using System.Windows.Forms;

namespace CrudApp.Forms
{
    public class FormSimulacaoQuitacao : Form
    {
        Label lblCapital;
        Label lblJurosRestantes;
        Label lblDiasCorridos;
        Label lblJurosProporcional;
        Label lblValorFinal;
        Button btnConfirmar;
        Button btnCancelar;

        public bool Confirmado { get; private set; }

        public FormSimulacaoQuitacao(
            double capitalRestante,
            double jurosRestantes,
            int diasCorridos,
            double jurosProporcional,
            double valorFinal)
        {
            InitializeComponent();

            lblCapital.Text = $"Capital restante: {capitalRestante:C}";
            lblJurosRestantes.Text = $"Juros restantes: {jurosRestantes:C}";
            lblDiasCorridos.Text = $"Dias corridos: {diasCorridos}";
            lblJurosProporcional.Text = $"Juros proporcional: {jurosProporcional:C}";
            lblValorFinal.Text = $"VALOR FINAL: {valorFinal:C}";
        }

        private void InitializeComponent()
        {
            Text = "Simulação de Quitação";
            Width = 420;
            Height = 350;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            int left = 30;
            int top = 30;
            int espacamento = 35;

            lblCapital = new Label
            {
                Left = left,
                Top = top,
                Width = 350
            };

            lblJurosRestantes = new Label
            {
                Left = left,
                Top = top += espacamento,
                Width = 350
            };

            lblDiasCorridos = new Label
            {
                Left = left,
                Top = top += espacamento,
                Width = 350
            };

            lblJurosProporcional = new Label
            {
                Left = left,
                Top = top += espacamento,
                Width = 350
            };

            lblValorFinal = new Label
            {
                Left = left,
                Top = top += espacamento,
                Width = 350,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.DarkGreen
            };

            btnConfirmar = new Button
            {
                Text = "Confirmar",
                Width = 120,
                Left = 70,
                Top = top + 50
            };

            btnCancelar = new Button
            {
                Text = "Cancelar",
                Width = 120,
                Left = 210,
                Top = top + 50
            };

            btnConfirmar.Click += (s, e) =>
            {
                Confirmado = true;
                Close();
            };

            btnCancelar.Click += (s, e) =>
            {
                Confirmado = false;
                Close();
            };

            Controls.Add(lblCapital);
            Controls.Add(lblJurosRestantes);
            Controls.Add(lblDiasCorridos);
            Controls.Add(lblJurosProporcional);
            Controls.Add(lblValorFinal);
            Controls.Add(btnConfirmar);
            Controls.Add(btnCancelar);
        }
    }
}