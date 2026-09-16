using System;
using System.Collections.Generic;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Elements.Table;
using CrudApp.Models;
using System.IO;

namespace CrudApp.Pdf
{
    public class PdfLivroCaixa
    {
        public void GeneratePdf(
            List<dynamic> dados,
            List<Despesa> despesas,
            List<Servico> servicos,
            decimal totalPrevisto,
            decimal totalRecebido,
            decimal totalEmAtraso,
            decimal totalServicos,
            decimal totalDespesas,
            int totalFinalizados,
            int totalAtrasados,
            int parcelasReceber,
            int parcelasPagasNoMes,
            string caminho,
            DateTime mesReferencia,
            bool anoCompleto = false)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var cultura = new System.Globalization.CultureInfo("pt-BR");
            var mesFormatado = anoCompleto ? $"ANO {mesReferencia.Year}" : mesReferencia.ToString("MMMM/yyyy", cultura).ToUpper();

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(9));

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
                            .Text($"RELATÓRIO - LIVRO CAIXA ({mesFormatado})")
                            .FontColor(Colors.White)
                            .Bold();
                    });

                    // ================= CONTENT =================
                    page.Content().PaddingTop(15).Column(col =>
                    {
                        col.Spacing(15);

                        // 🔹 RESUMO
                        Bloco(col, "RESUMO DE MOVIMENTAÇÕES", () =>
                        {
                            col.Item().Column(c =>
                            {
                                Linha(c, "Contratos Finalizados:", totalFinalizados.ToString());
                                Linha(c, "Parcelas Atrasdas:", totalAtrasados.ToString());
                                Linha(c, "Parcelas A Receber:", parcelasReceber.ToString());
                                Linha(c, "Parcelas Pagas:", parcelasPagasNoMes.ToString());
                            });
                        });

                        // 🔹 PARCELAS
                       Bloco(col, "PARCELAS", () =>
                        {
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(); // Cliente
                                    columns.RelativeColumn(); // Contrato
                                    columns.RelativeColumn(); // Parcela
                                    columns.RelativeColumn(); // Produto
                                    columns.RelativeColumn(); // Vencimento
                                    columns.RelativeColumn(); // Pagamento
                                    columns.RelativeColumn(); // Forma Pgto
                                    columns.RelativeColumn(); // Valor
                                    columns.RelativeColumn(); // Juros
                                    columns.RelativeColumn(); // Status
                                });

                                table.Header(header =>
                                {
                                    HeaderCell(header.Cell(), "Cliente");
                                    HeaderCell(header.Cell(), "Contrato");
                                    HeaderCell(header.Cell(), "Parcela");
                                    HeaderCell(header.Cell(), "Produto");
                                    HeaderCell(header.Cell(), "Vencimento");
                                    HeaderCell(header.Cell(), "Pagamento");
                                    HeaderCell(header.Cell(), "Forma Pgto");
                                    HeaderCell(header.Cell(), "Valor");
                                    HeaderCell(header.Cell(), "Juros");
                                    HeaderCell(header.Cell(), "Status");
                                });

                                foreach (var p in dados)
                                {
                                    var status = p.Status?.ToString() ?? "";

                                    BodyCell(table.Cell(), p.Cliente, status);
                                    BodyCell(table.Cell(), p.Contrato?.ToString(), status);
                                    BodyCell(table.Cell(), p.NumeroParcela, status);
                                    BodyCell(table.Cell(), p.Produto, status);
                                    BodyCell(table.Cell(), p.Vencimento, status);

                                    BodyCell(table.Cell(),
                                        p.Ultimo_PGTO != null
                                            ? Convert.ToDateTime(p.Ultimo_PGTO).ToString("dd/MM/yyyy")
                                            : "-", status);

                                    BodyCell(table.Cell(), p.Forma_de_PGTO, status);

                                    BodyCell(table.Cell(),
                                        p.Bruto_no_Mes != null
                                            ? Convert.ToDecimal(p.Bruto_no_Mes).ToString("C")
                                            : "R$ 0,00", status);

                                    BodyCell(table.Cell(),
                                        p.Agio_Ativo != null
                                            ? Convert.ToDecimal(p.Agio_Ativo).ToString("C")
                                            : "R$ 0,00", status);

                                    BodyCell(table.Cell(), status, status);
                                }
                            });
                        });

                        // 🔹 DESPESAS
                        Bloco(col, "DESPESAS", () =>
                        {
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(c =>
                                {
                                    c.RelativeColumn();
                                    c.RelativeColumn();
                                    c.RelativeColumn();
                                    c.RelativeColumn();
                                    c.RelativeColumn();
                                });

                                table.Header(header =>
                                {
                                    HeaderCell(header.Cell(), "Tipo");
                                    HeaderCell(header.Cell(), "Situação");
                                    HeaderCell(header.Cell(), "Vencimento");
                                    HeaderCell(header.Cell(), "Pagamento");
                                    HeaderCell(header.Cell(), "Valor");
                                });

                                foreach (var d in despesas)
                                {
                                    BodyCell(table.Cell(), d.Tipo);
                                    BodyCell(table.Cell(), d.Situacao);
                                    BodyCell(table.Cell(), d.Vencimento.ToString("dd/MM/yyyy"));
                                    BodyCell(table.Cell(), d.DataPagamento?.ToString("dd/MM/yyyy") ?? "-");
                                    BodyCell(table.Cell(), d.Valor.ToString("C"));
                                }
                            });
                        });

                        // 🔹 SERVIÇOS
                         Bloco(col, "SERVIÇOS", () =>
                        {
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(c =>
                                {
                                    c.RelativeColumn();
                                    c.RelativeColumn();
                                    c.RelativeColumn();
                                    c.RelativeColumn();
                                    c.RelativeColumn();
                                });

                                table.Header(header =>
                                {
                                    HeaderCell(header.Cell(), "Tipo");
                                    HeaderCell(header.Cell(), "Situação");
                                    HeaderCell(header.Cell(), "Vencimento");
                                    HeaderCell(header.Cell(), "Pagamento");
                                    HeaderCell(header.Cell(), "Valor");
                                });

                                foreach (var s in servicos)
                                {
                                    BodyCell(table.Cell(), s.Tipo);
                                    BodyCell(table.Cell(), s.Situacao);
                                    BodyCell(table.Cell(), s.Vencimento.ToString("dd/MM/yyyy"));
                                    BodyCell(table.Cell(), s.DataPagamento?.ToString("dd/MM/yyyy") ?? "-");
                                    BodyCell(table.Cell(), s.Valor.ToString("C"));
                                }
                            });
                        });

                        Bloco(col, "RESUMO FINANCEIRO", () =>
                            {
                                col.Item().Column(c =>
                                {
                                    Linha(c, "Total Previsto:", totalPrevisto.ToString("C"), Colors.Grey.Darken1);

                                    Linha(c, "Total Recebido:", totalRecebido.ToString("C"), Colors.Green.Darken2);

                                    Linha(c, "Total em Atraso:", totalEmAtraso.ToString("C"), Colors.Red.Darken2);

                                    Linha(c, "Total Serviços:", totalServicos.ToString("C"), Colors.Blue.Darken2);

                                    Linha(c, "Total Despesas:", totalDespesas.ToString("C"), Colors.Orange.Darken2);

                                    decimal saldo = totalRecebido + totalServicos - totalDespesas;

                                    // cor dinâmica
                                    var corSaldo = saldo >= 0
                                        ? Colors.Green.Darken3
                                        : Colors.Red.Darken3;

                                    Linha(c, "Saldo Final:", saldo.ToString("C"), corSaldo);
                                    
                                });

                                
                            });

                    });

                    // ================= FOOTER =================
                    page.Footer().AlignCenter()
                        .Text($"SiS Cred Sistema de Credito - {DateTime.Now:dd/MM/yyyy}");
                });
            })
            .GeneratePdf(caminho);
        }

        // 🔧 HELPERS (REAPROVEITADOS DO SEU TEMPLATE)

            private void Linha(
                ColumnDescriptor col,
                string label,
                string valor,
                string corValor = null)
            {
                col.Item().PaddingVertical(2).Row(row =>
                {
                    row.ConstantColumn(180).Text(label).SemiBold();

                    row.RelativeColumn().Text(txt =>
                    {
                        if (!string.IsNullOrEmpty(corValor))
                            txt.Span(valor).FontColor(corValor); // ✅ CORRETO
                        else
                            txt.Span(valor);
                    });
                });
            }

        private void Bloco(ColumnDescriptor col, string titulo, Action conteudo)
        {
            col.Item().Padding(10).Column(c =>
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
                    .Padding(5)
                    .AlignCenter()
                    .Text(texto)
                    .FontColor(Colors.White)
                    .Bold();
            });
        }

        private void BodyCell(ITableCellContainer cell, string texto, string status = null)
        {
            cell.Element(container =>
            {
                var background = Colors.White;
                var fontColor = Colors.Black;

                if (!string.IsNullOrEmpty(status))
                {
                    var s = status.ToLower();

                   switch (s)
                        {
                            case "paga":
                            case "paga em atraso":
                            case "paga antecipado":
                                background = Colors.Green.Lighten4;
                                fontColor = Colors.Green.Darken3;
                                break;

                            case "em atraso":
                                background = Colors.Red.Lighten4;
                                fontColor = Colors.Red.Darken3;
                                break;

                            case "a vencer":
                                background = Colors.Yellow.Lighten4;
                                fontColor = Colors.Yellow.Darken4;
                                break;
                        }
                }

                container
                    .Background(background)
                    .BorderBottom(1)
                    .BorderColor(Colors.Grey.Lighten2)
                    .Padding(3)
                    .AlignCenter()
                    .Text(texto ?? "-")
                    .FontSize(9) // 👈 AQUI você controla
                    .FontColor(fontColor);
            });
        }
    }
}