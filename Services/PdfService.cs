using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.IO;
using CrudApp.Models;

namespace CrudApp.Services
{
    public static class PdfService
    {
        public static string GerarPdfCliente(Cliente c)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            string pasta = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "PDFsClientes"
            );
            Directory.CreateDirectory(pasta);

            string nomeArquivoSeguro = string.Join("_", c.Nome.Split(Path.GetInvalidFileNameChars()));
            string arquivo = Path.Combine(pasta, $"{nomeArquivoSeguro}_Ficha.pdf");

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);

                    // ===== HEADER =====
                    page.Header()
                        .AlignCenter()
                        .Text("Ficha do Cliente")
                        .Bold()
                        .FontSize(20);

                    // ===== CONTEÚDO =====
                    page.Content().Column(col =>
                    {
                        col.Spacing(12);

                        // FOTO
                        if (c.Foto != null && c.Foto.Length > 0)
                        {
                            col.Item().AlignCenter().Container()
                                .Width(140)
                                .Height(180)
                                .Border(1)
                                .Padding(5)
                                .Image(c.Foto)
                                .FitArea();
                        }

                        // DADOS PESSOAIS
                        col.Item().Text($"Nome: {c.Nome}");
                        col.Item().Text($"RG: {c.Rg ?? "-"}");
                        col.Item().Text($"CPF: {c.Cpf ?? "-"}");
                        col.Item().Text($"Email: {c.Email ?? "-"}");
                        col.Item().Text($"Telefone: {c.Telefone ?? "-"}");

                        // ENDEREÇO
                        col.Item().LineHorizontal(1);
                        col.Item().Text("Endereço").Bold();

                        col.Item().Text(
                            $"{c.Rua}, {c.Numero} {c.Complemento}".Trim());
                        col.Item().Text(
                            $"{c.Cidade} - {c.Estado} | CEP: {c.Cep}");

                        // DADOS BANCÁRIOS
                        col.Item().LineHorizontal(1);
                        col.Item().Text("Dados Bancários").Bold();

                        col.Item().Text($"Banco: {c.Banco ?? "-"}");
                        col.Item().Text($"Agência: {c.Agencia ?? "-"}");
                        col.Item().Text($"Conta: {c.Conta ?? "-"}");

                        // SCORE / CONTRATOS
                        col.Item().LineHorizontal(1);
                        col.Item().Text($"Score: {c.ClienteScore ?? "N/D"}");
                        col.Item().Text($"Qtd. Contratos: {c.QtdContratos}");
                    });

                    // ===== FOOTER =====
                    page.Footer()
                        .AlignCenter()
                        .Text($"Gerado em {DateTime.Now:dd/MM/yyyy HH:mm}");
                });
            })
            .GeneratePdf(arquivo);

            return arquivo;
        }
    }
}
