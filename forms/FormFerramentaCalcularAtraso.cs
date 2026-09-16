using System;
using System.Globalization;
using System.Windows.Forms;
using System.Drawing;

namespace CrudApp.Forms;

public class FormFerramentaCalcularAtraso : Form
{
    public double NovoValor { get; private set; }
    private TextBox txtValorPrestacao;
    private DateTimePicker dtVencimento;
    private DateTimePicker dtPagamento;
    private RichTextBox txtResultado;
    private TextBox txtTaxa;
    private TextBox txtMora;
    private Button btnCancelar;

    public FormFerramentaCalcularAtraso()
    {
        InitializeComponent();

        //txtValorPrestacao.Text = valorPrestacao.ToString("F2");
        //dtVencimento.Value = vencimento;
        dtPagamento.Value = DateTime.Today;

        Recalcular();
    }

    private void InitializeComponent()
    {
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Icon = new Icon("app.ico");
        Text = "SiS Cred - Cálculo de Parcela Atrasada";
        Width = 400;
        Height = 420;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(8),
            ColumnCount = 2,
            RowCount = 7
        };

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));

        for (int i = 0; i < 6; i++)
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));

        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        // Valor da prestação
        layout.Controls.Add(new Label { Text = "Valor da prestação", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
        txtValorPrestacao = new TextBox { Dock = DockStyle.Fill };
        txtValorPrestacao.TextChanged += (s, e) => Recalcular();
        layout.Controls.Add(txtValorPrestacao, 1, 0);

        // Data da parcela
        layout.Controls.Add(new Label { Text = "Data de vencimento da parcela", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 1);
        dtVencimento = new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Short };
        dtVencimento.ValueChanged += (s, e) => Recalcular();
        layout.Controls.Add(dtVencimento, 1, 1);

        // Data de pagamento
        layout.Controls.Add(new Label { Text = "Data de pagamento", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 2);
        dtPagamento = new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Short };
        dtPagamento.ValueChanged += (s, e) => Recalcular();
        layout.Controls.Add(dtPagamento, 1, 2);

        // Multa
        layout.Controls.Add(new Label { Text = "Multa por atraso (%)", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 3);
        txtTaxa = new TextBox { Dock = DockStyle.Fill, Text = "5" };
        txtTaxa.TextChanged += (s, e) => Recalcular();
        layout.Controls.Add(txtTaxa, 1, 3);

        // Mora
        layout.Controls.Add(new Label { Text = "Juros de mora (% ao dia)", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 4);
        txtMora = new TextBox { Dock = DockStyle.Fill, Text = "0,5" };
        txtMora.TextChanged += (s, e) => Recalcular();
        layout.Controls.Add(txtMora, 1, 4);

        // Resultado
        txtResultado = new RichTextBox
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = SystemColors.Window,
            Font = new Font("Segoe UI", 9F),
        };

        layout.SetColumnSpan(txtResultado, 2);
        layout.Controls.Add(txtResultado, 0, 6);

        var panelBotoes = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 50
        };

        btnCancelar = new Button { 
            Text = "Fechar", 
            Width = 90, 
            Height = 27, 
            Left = 140, 
            Top = 4 
        };

        btnCancelar.Click += (s, e) =>
        {
            DialogResult = DialogResult.Cancel;
            Close();
        };

        panelBotoes.Controls.Add(btnCancelar);

        Controls.Add(layout);
        Controls.Add(panelBotoes);
    }

    private void Recalcular()
    {
        if (!double.TryParse(txtValorPrestacao.Text, NumberStyles.Any, CultureInfo.CurrentCulture, out var valorPrestacao) ||
            !double.TryParse(txtTaxa.Text, out var taxa) ||
            !double.TryParse(txtMora.Text, out var mora))
        {
            txtResultado.Clear();
            txtResultado.SelectionColor = Color.Red;
            txtResultado.AppendText("Valores inválidos.");
            return;
        }

        DateTime vencimento = dtVencimento.Value.Date;
        DateTime pagamento = dtPagamento.Value.Date;

        int dias = (pagamento - vencimento).Days;
        //if (dias < 0) dias = 0;

        double taxaDecimal = taxa / 100.0;
        double jurosDeMora = mora / 100.0;

        double valorMulta = valorPrestacao * taxaDecimal;
        double totalJuros = valorPrestacao * jurosDeMora * dias;

        NovoValor = valorPrestacao + valorMulta + totalJuros;

        txtResultado.Clear();

        txtResultado.SelectionColor = Color.DarkRed;
        txtResultado.SelectionFont = new Font(txtResultado.Font, FontStyle.Bold);
        txtResultado.AppendText($"Dias em atraso: {dias}\n\n");
        
        txtResultado.SelectionColor = Color.DimGray;
        txtResultado.AppendText($"Valor original: {valorPrestacao:F2}\n");

        txtResultado.SelectionColor = Color.DarkOrange;
        txtResultado.AppendText($"Multa: {valorMulta:F2}\n");

        txtResultado.SelectionColor = Color.Firebrick;
        txtResultado.AppendText($"Juros: {totalJuros:F2}\n\n");

        txtResultado.SelectionColor = Color.DarkGreen;
        txtResultado.SelectionFont = new Font(txtResultado.Font, FontStyle.Bold);
        txtResultado.AppendText($"Total hoje: {NovoValor:F2}");
    }
}
