using System;
using System.Globalization;
using System.Windows.Forms;
using CrudApp.Models;
using CrudApp.Repositories;
using System.Drawing;

namespace CrudApp.Forms;

public class FormCalculoAntecipado : Form
{
    private double valorPrestacao;
    private DateTime vencimento;
    public double NovoValor { get; private set; }
    private Label lblResultado;
    private TextBox txtTaxa;
    private Button btnConfirmar;
    private Button btnCancelar;

    public FormCalculoAntecipado(double valorPrestacao, DateTime vencimento)
    {
        this.valorPrestacao = valorPrestacao;
        this.vencimento = vencimento;

        InitializeComponent();
        Recalcular();
    }

    private void InitializeComponent()
    {
        Text = "Cálculo Antecipado";
        Width = 360;
        Height = 260;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        this.AutoScaleMode = AutoScaleMode.Dpi;
        this.AutoScaleMode = AutoScaleMode.None;
        this.AutoSize = false;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        var lblTaxa = new Label
        {
            Text = "Taxa de desconto (% ao mês):",
            Left = 20,
            Top = 20,
            Width = 250
        };

        txtTaxa = new TextBox
        {
            Left = 20,
            Top = 45,
            Width = 120,
            Text = "5"
        };
        txtTaxa.TextChanged += (s, e) => Recalcular();

        lblResultado = new Label
        {
            Left = 20,
            Top = 80,
            Width = 300,
            Height = 90
        };

        btnConfirmar = new Button
        {
            Text = "Confirmar",
            Left = 60,
            Top = 180,
            Width = 100
        };
        btnConfirmar.Click += (s, e) =>
        {
            DialogResult = DialogResult.OK;
            Close();
        };

        btnCancelar = new Button
        {
            Text = "Cancelar",
            Left = 180,
            Top = 180,
            Width = 100
        };
        btnCancelar.Click += (s, e) => DialogResult = DialogResult.Cancel;

        Controls.Add(lblTaxa);
        Controls.Add(txtTaxa);
        Controls.Add(lblResultado);
        Controls.Add(btnConfirmar);
        Controls.Add(btnCancelar);
    }

    private void Recalcular()
    {
        if (!double.TryParse(txtTaxa.Text, out var taxa))
        {
            lblResultado.Text = "Taxa inválida.";
            return;
        }

        DateTime dataPagamento = DateTime.Today;
        int dias = (vencimento - dataPagamento).Days;

        if (dias <= 0)
        {
            lblResultado.Text = "Pagamento não é antecipado.";
            return;
        }

        // taxa em porcentagem → converter para decimal
        double taxaDecimal = taxa / 100.0;

        double descontoDiario = taxaDecimal / 30.0;
        double descontoTotal = descontoDiario * dias;

        NovoValor = valorPrestacao - (descontoTotal * valorPrestacao);
        if (NovoValor < 0)
            NovoValor = 0;

        lblResultado.Text =
            $"Dias antecipados: {dias}\n" +
            $"Desconto aplicado: {(descontoTotal * 100):F2}%\n\n" +
            $"Valor original: {valorPrestacao:F2}\n" +
            $"Novo valor: {NovoValor:F2}";
    }
}

