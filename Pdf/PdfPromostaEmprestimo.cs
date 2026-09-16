using System;
using System.Collections.Generic;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Elements.Table;
using CrudApp.Models;
using System.IO;

namespace CrudApp.Pdf
{
    public class PdfPropostaEmprestimo
    {
        public void GeneratePdf(
            Emprestimo emp,
            List<Parcela> parcelas,
            string caminho)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    // ================= HEADER =================
                    page.Header().Column(col =>
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
                            .Background(Colors.Green.Darken2)
                            .Padding(6)
                            .AlignCenter()
                            .Text("SIMULAÇÃO DE PROPOSTA DE EMPRÉSTIMO")
                            .FontColor(Colors.White)
                            .Bold();
                    });

                    // ================= CONTENT =================
                    page.Content().PaddingTop(15).Column(col =>
                    {
                        col.Spacing(15);

                        // ===== DADOS DO CONTRATO =====
                        Bloco(col, "DADOS DO CONTRATO", () =>
                        {
                            col.Item().Column(c =>
                            {
                                c.Spacing(0); // 🔥 remove espaço entre Linha()

                                Linha(c, "Código:", emp.Codigo.ToString("D3"));
                                Linha(c, "Parcelas:", emp.Parcelas.ToString());
                                Linha(c, "Taxa:", "1% ao mês");
                                Linha(c, "Vencimento:", emp.Vencimento?.ToString("dd/MM/yyyy") ?? "-");
                                Linha(c, "Tipo:", emp.Tipo);
                                Linha(c, "Valor Liberado:", emp.Liberado.ToString("C"));
                                Linha(c, "Total de Juros:", emp.TotalJuros.ToString("C"));
                                Linha(c, "Total Amortização:", emp.TotalAmortizacao.ToString("C"));
                                Linha(c, "Valor do Contrato:", emp.ValorContrato.ToString("C"));
                            });
                        });


                        // ===== TABELA =====
                        Bloco(col, "CRONOGRAMA DE PAGAMENTOS", () =>
                        {
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(40);
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                // HEADER
                                table.Header(header =>
                                {
                                    HeaderCell(header.Cell(), "Nº");
                                    HeaderCell(header.Cell(), "Vencimento");
                                    HeaderCell(header.Cell(), "Amortização");
                                    HeaderCell(header.Cell(), "Juros");
                                    HeaderCell(header.Cell(), "Prestação");
                                });

                                // DADOS
                                foreach (var p in parcelas)
                                {
                                    BodyCell(table.Cell(), p.NParcelas.ToString());
                                    BodyCell(table.Cell(), p.Vencimento?.ToString("dd/MM/yyyy") ?? "-");
                                    BodyCell(table.Cell(), p.Amortizacao?.ToString("C"));
                                    BodyCell(table.Cell(), p.Juros?.ToString("C"));
                                    BodyCell(table.Cell(), p.ValorPrestacao?.ToString("C"));
                                }
                            });
                        });

                        col.Spacing(15);

                        col.Item().AlignCenter().Column(c =>
                            {
                                c.Item().Text("Assinatura do Cliente: ").SemiBold();

                                c.Item()
                                    .PaddingTop(0)
                                    .PaddingLeft(110)
                                    .Width(200)
                                    .LineHorizontal(1);
                            });
                    });

                    // ================= FOOTER =================
                    page.Footer().AlignCenter()
                        .Text($"SiS Cred Sistema Financeiro - {DateTime.Now:dd/MM/yyyy}");
                });
            })
            .GeneratePdf(caminho);
        }

        // ================= HELPERS =================

        private void Linha(ColumnDescriptor col, string label, string valor)
        {
            col.Item().PaddingVertical(1).Row(row =>
            {
                row.ConstantColumn(160)
                    .Text(label)
                    .SemiBold();

                row.RelativeColumn()
                    .Text(valor);
            });
        }

        private void Bloco(ColumnDescriptor col, string titulo, Action conteudo)
        {
            col.Item()
                //.Border(1)
                //.BorderColor(Colors.Grey.Lighten2)
                .Padding(10)
                .Column(c =>
                {
                    c.Item()
                        .Background(Colors.Green.Lighten4)
                        .Padding(6)
                        .Text(titulo)
                        .Bold()
                        .FontColor(Colors.Green.Darken2);

                    conteudo();
                });
        }

        private void HeaderCell(ITableCellContainer cell, string texto)
        {
            cell.Element(container =>
            {
                container
                    .Background(Colors.Green.Darken2)
                    .PaddingVertical(6)
                    .PaddingHorizontal(4)
                    .AlignCenter()
                    .Text(texto)
                    .FontColor(Colors.White)
                    .Bold();
            });
        }

        private void BodyCell(ITableCellContainer cell, string texto)
        {
            cell.Element(container =>
            {
                container
                    .BorderBottom(1)
                    .BorderColor(Colors.Grey.Lighten2)
                    .Padding(4)
                    .AlignCenter()
                    .Text(texto);
            });
        }
    }
}
