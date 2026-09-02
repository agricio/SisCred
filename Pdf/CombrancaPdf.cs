using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

using CrudApp.Models;

namespace CrudApp.Pdf
{
    public class CobrancaPdf : IDocument
    {
        private readonly Cliente _cliente;
        private readonly Emprestimo _emprestimo;
        private readonly List<Parcela> _parcelas;

        public CobrancaPdf(
            Cliente cliente,
            Emprestimo emprestimo,
            List<Parcela> parcelas)
        {
            _cliente = cliente;
            _emprestimo = emprestimo;
            _parcelas = parcelas;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);
            });
        }

        // ================= HEADER =================
        private void ComposeHeader(IContainer container)
        {
            container.Column(col =>
                    {
                        col.Item()
                            .AlignCenter()
                            .MaxWidth(190)
                            .Image(Path.Combine(
                                AppDomain.CurrentDomain.BaseDirectory,
                                "Assets",
                                "newlogo.png"))
                            .FitWidth();

                        col.Item().PaddingTop(10)
                            .Background(Colors.Blue.Darken2)
                            .Padding(6)
                            .AlignCenter()
                            .Text("BOLETO DE COBRANÇA DE PARCELAS EM ATRASO")
                            .FontColor(Colors.White)
                            .Bold();
                    });
        }

        // ================= CONTENT =================
        private void ComposeContent(IContainer container)
        {

            string endereco =
                $"{_cliente.Telefone ?? "-"}\nEnd:, {_cliente.Rua}, {_cliente.Numero}" +
                (string.IsNullOrWhiteSpace(_cliente.Bairro) ? "" : $" - {_cliente.Bairro}") +
                (string.IsNullOrWhiteSpace(_cliente.Cidade) ? "" : $" - {_cliente.Cidade}") +
                (string.IsNullOrWhiteSpace(_cliente.Estado) ? "" : $"/{_cliente.Estado}") +
                (string.IsNullOrWhiteSpace(_cliente.Cep) ? "" : $" - CEP: {_cliente.Cep}");
                
            container.Column(col =>
            {
                col.Spacing(8);

                // ===== IDENTIFICAÇÃO =====
                Secao(col, "IDENTIFICAÇÃO DO MUTUÁRIO");

                // 🔹 Linha 1: Nome + Contrato
                CampoDuplo(
                    col,
                    "Nome", _cliente.Nome ?? "-",
                    "Contrato", _emprestimo.Contrato.ToString()
                );

                // 🔹 Linha 2: Telefone
                CampoDuplo(
                    col,
                    "Telefone", _cliente.Telefone?.ToString() ?? "-",
                    "", "" // vazio pra manter alinhamento
                );

                // 🔹 Linha 3: Endereço (linha inteira)
                Campo(
                    col,
                    "Endereço",
                    $"{_cliente.Rua ?? ""}, {_cliente.Numero ?? ""} - {_cliente.Bairro ?? ""}\n" +
                    $"{_cliente.Cidade ?? ""}/{_cliente.Estado ?? ""} - CEP: {_cliente.Cep ?? ""}"
                );

                col.Item().PaddingVertical(6);


                // ===== DATA DE VENCIMENTO DO BOLETO =====
                col.Item().Text(text =>
                {
                    text.Span("Vencimento em: ").Bold();
                    text.Span($"{DateTime.Today.AddDays(90):dd/MM/yyyy}");
                });


                // ===== TABELA DE PARCELAS =====
                TabelaParcelas(col);

                // ===== TOTAL =====
                //col.Item().PaddingTop(15);

                var totalEmAberto = _parcelas.Sum(p => CalcularValorAtualizado(p));
                col.Item()
                    .AlignRight()
                    .Text($"TOTAL DEVIDO: R$ {totalEmAberto:F2}")
                    .Bold()
                    .FontSize(12);

                //col.Item().PaddingTop(10);

                col.Item().Text(text =>
                {
                    text.DefaultTextStyle(x => x.FontSize(9));

                    text.Line("AVISOS:").Bold();
                    text.Line("• Mantenha suas parcelas em dia, evitando assim multas e juros por atraso.");
                    text.Line("• Pagamentos em dia garantem seu Score BOM para possíveis adesões de novos créditos.");
                    text.Line("• Evite atrasos no seu pagamento, para que não ocorra execução de seu contrato, de acordo com o item 7.");
                    text.Line("• Enviar o comprovante de pagamento para o WhatsApp da Loja.");
                });

                col.Item().Text(
                    $"Emitido em {DateTime.Now:dd/MM/yyyy}"
                );
            });
        }

        // ================= TABELA =================
        private void TabelaParcelas(ColumnDescriptor col)
        {
            col.Item().Table(t =>
            {
                t.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(1); // Parcela
                    c.RelativeColumn(2); // Vencimento
                    c.RelativeColumn(1); // Dias
                    c.RelativeColumn(2); // Valor normal
                    c.RelativeColumn(2); // Multa
                    c.RelativeColumn(2); // Juros
                    c.RelativeColumn(2); // Total
                });

                // ===== CABEÇALHO =====
                t.Header(h =>
                {
                    h.Cell().Border(1).Padding(4).Text("Parcela").Bold().FontSize(9);
                    h.Cell().Border(1).Padding(4).Text("Vencimento").Bold().FontSize(9);
                    h.Cell().Border(1).Padding(4).Text("Dias").Bold().FontSize(9);
                    h.Cell().Border(1).Padding(4).Text("Valor Original").Bold().FontSize(9);
                    h.Cell().Border(1).Padding(4).Text("Multa").Bold().FontSize(9);
                    h.Cell().Border(1).Padding(4).Text("Juros").Bold().FontSize(9);
                    h.Cell().Border(1).Padding(4).Text("Total").Bold().FontSize(9);
                });

                foreach (var p in _parcelas)
                {
                    DateTime vencimento = p.Vencimento ?? DateTime.Today;
                    DateTime dataPagamento = DateTime.Today;

                    int dias = (dataPagamento - vencimento).Days;
                    if (dias < 0) dias = 0;

                    double valorPrestacao = p.ValorPrestacao ?? 0.0;
                    double taxa = 2.5;     // multa (%)
                    double mora = 0.4999;  // juros ao dia (%)

                    double taxaDecimal = taxa / 100.0;
                    double jurosDeMora = mora / 100.0;

                    double multa = dias > 0 ? valorPrestacao * taxaDecimal : 0;
                    double totalJuros = dias > 0 ? valorPrestacao * jurosDeMora * dias : 0;

                    double novoValor = valorPrestacao + multa + totalJuros;

                    t.Cell().Border(1).Padding(4).Text(p.NParcelas.ToString()).FontSize(9);
                    t.Cell().Border(1).Padding(4).Text(vencimento.ToString("dd/MM/yyyy")).FontSize(9);
                    t.Cell().Border(1).Padding(4).Text(dias.ToString()).FontSize(9);
                    t.Cell().Border(1).Padding(4).Text(p.ValorPrestacao.ToString()).FontSize(9);

                    t.Cell().Border(1).Padding(4)
                        .Text($"R$ {multa:F2}")
                        .FontSize(9);

                    t.Cell().Border(1).Padding(4)
                        .Text($"R$ {totalJuros:F2}")
                        .FontSize(9);

                    t.Cell().Border(1).Padding(4)
                        .Text($"R$ {novoValor:F2}")
                        .FontSize(9)
                        .Bold();
                }
            });
        }

        // ================= FOOTER =================
        private void ComposeFooter(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().PaddingTop(20).LineHorizontal(1);

                col.Item().Row(row =>
                {
                    row.RelativeItem().AlignLeft().Column(left =>
                    {
                        left.Item().PaddingTop(10).Text("VICTOR HUGO DE OLIVEIRA E SILVA").FontSize(8);
                        left.Item().Text("CPF: 055.537.694-00").FontSize(8);
                        left.Item().Text("CREDOR").FontSize(8);
                    });

                    row.RelativeItem().AlignRight().Text(
                        "Documento gerado automaticamente pelo sistema."
                    ).FontSize(8);
                });
            });
        }

        // ================= HELPERS =================
        private void Secao(ColumnDescriptor col, string titulo)
        {
            col.Item().PaddingTop(10)
                .Text(titulo)
                .Bold();
        }

        private void Campo(ColumnDescriptor col, string label, string valor)
        {
            col.Item().Text(text =>
            {
                text.Span($"{label}: ").SemiBold();
                text.Span(valor);
            });
        }

       private void CampoDuplo(
            ColumnDescriptor col,
            string l1, string v1,
            string l2, string v2)
        {
            col.Item().Row(r =>
            {
                // Campo esquerdo
                r.RelativeItem().Text($"{l1}: {v1}");

                // Campo direito
                if (!string.IsNullOrWhiteSpace(l2) ||
                    !string.IsNullOrWhiteSpace(v2))
                {
                    r.RelativeItem().Text($"{l2}: {v2}");
                }
                else
                {
                    r.RelativeItem().Text("");
                }
            });
        }

        private double CalcularValorAtualizado(Parcela p)
        {
            if (!p.Vencimento.HasValue)
                return 0;

            int dias = (DateTime.Today - p.Vencimento.Value).Days;
            if (dias < 0) dias = 0;

            double valorPrestacao = p.ValorPrestacao ?? 0;

            double taxaMulta = 2.5 / 100.0;
            double jurosDia = 0.4999 / 100.0;

            double multa = dias > 0 ? valorPrestacao * taxaMulta : 0;
            double juros = dias > 0 ? valorPrestacao * jurosDia * dias : 0;

            return valorPrestacao + multa + juros;
        }
    }
}