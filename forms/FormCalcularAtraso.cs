using System;
using System.Globalization;
using System.Windows.Forms;
using CrudApp.Models;
using CrudApp.Repositories;
using System.Drawing;

namespace CrudApp.Forms;

public class FormCalculoAtraso : Form
{
    private double valorPrestacao;
    private DateTime vencimento;

    public double NovoValor { get; private set; }
    public double Acrescimo { get; private set; }

    private RichTextBox txtResultado;
    private TextBox txtTaxa;
    private TextBox txtMora;
    private Button btnConfirmar;
    private Button btnCancelar;

    public FormCalculoAtraso(double valorPrestacao, DateTime vencimento)
    {
        this.valorPrestacao = valorPrestacao;
        this.vencimento = vencimento;

        InitializeComponent();
        Recalcular();
    }

   private void InitializeComponent()
    {
        Text = "Cálculo de Multa";
        Width = 360;
        Height = 320;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(5),
            ColumnCount = 2,
            RowCount = 3
        };

        layout.ColumnStyles.Clear();
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));   // Labels
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));  // TextBox menores

        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 120));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

        var lblTaxa = new Label
        {
            Text = "Multa por atraso (%)",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };

        txtTaxa = new TextBox
        {
            Dock = DockStyle.Fill,
            Text = "5"
        };
        txtTaxa.TextChanged += (s, e) => Recalcular();

        var lblMora = new Label
        {
            Text = "Juros de mora (% ao dia)",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };

        txtMora = new TextBox
        {
            Dock = DockStyle.Fill,
            Text = "0,5"
        };
        txtMora.TextChanged += (s, e) => Recalcular();

        txtResultado = new RichTextBox
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = SystemColors.Window,
            Font = new Font("Segoe UI", 9F),
        };

        var panelBotoes = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 50,
            Padding = new Padding(0)
        };

        btnConfirmar = new Button
        {
            Text = "Confirmar",
            Width = 100,
            Height = 35,
            Left = 60,
            Top = 5
        };

        btnConfirmar.Click += (s, e) =>
        {
            DialogResult = DialogResult.OK;
            Close();
        };

        btnCancelar = new Button
        {
            Text = "Cancelar",
            Width = 100,
            Height = 35,
            Left = 180,
            Top = 5
        };

        btnCancelar.Click += (s, e) =>
        {
            DialogResult = DialogResult.Cancel;
            Close();
        };

        panelBotoes.Controls.Add(btnConfirmar);
        panelBotoes.Controls.Add(btnCancelar);

        layout.Controls.Add(lblTaxa, 0, 0);
        layout.Controls.Add(txtTaxa, 1, 0);

        layout.Controls.Add(lblMora, 0, 1);
        layout.Controls.Add(txtMora, 1, 1);

        layout.SetColumnSpan(txtResultado, 2);
        layout.Controls.Add(txtResultado, 0, 2);

       // layout.Controls.Add(btnConfirmar, 0, 4);
        //layout.Controls.Add(btnCancelar, 1, 4);

        Controls.Add(layout);
        Controls.Add(panelBotoes);
    }


    private void Recalcular()
    {
        if (!double.TryParse(txtTaxa.Text, out var taxa) ||
            !double.TryParse(txtMora.Text, out var mora))
        {
            txtResultado.Clear();
            txtResultado.SelectionColor = Color.Red;
            txtResultado.AppendText("Taxa inválida.");
            return;
        }

        // Data atual
        DateTime dataPagamento = DateTime.Today;

        // Calcula os dias de atraso
        int dias = (dataPagamento - vencimento).Days;

        // Se ainda não venceu, considera 0 dias de atraso
        if (dias < 0)
            dias = 0;

        // Converte as porcentagens para decimal
        double taxaMulta = taxa / 100.0;
        double taxaMora = mora / 100.0;

        // MULTA
    
        double multa = valorPrestacao * taxaMulta;

        // JUROS DE MORA
        
        double totalJuros = valorPrestacao * taxaMora * dias;

        // ACRÉSCIMO TOTAL
    
        Acrescimo = multa + totalJuros;

        // NOVO VALOR

        NovoValor = valorPrestacao + Acrescimo;

        // EXIBIÇÃO

        txtResultado.Clear();

        txtResultado.SelectionColor = Color.DarkRed;
        txtResultado.SelectionFont =
            new Font(txtResultado.Font, FontStyle.Bold);

        txtResultado.AppendText(
            $"Dias em atraso: {dias}\n\n");

        txtResultado.SelectionColor = Color.DimGray;
        txtResultado.AppendText(
            $"Valor original: {valorPrestacao:F2}\n");

        txtResultado.SelectionColor = Color.DarkOrange;
        txtResultado.AppendText(
            $"Multa: {multa:F2}\n");

        txtResultado.SelectionColor = Color.Firebrick;
        txtResultado.AppendText(
            $"Total de juros: {totalJuros:F2}\n\n");

        txtResultado.SelectionColor = Color.DarkGreen;
        txtResultado.SelectionFont =
            new Font(txtResultado.Font, FontStyle.Bold);

        txtResultado.AppendText(
            $"Total hoje: {NovoValor:F2}");
    }

}

