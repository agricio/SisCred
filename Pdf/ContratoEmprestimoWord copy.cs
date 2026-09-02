using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

using CrudApp.Models;

namespace CrudApp.Word
{
    public class ContratoEmprestimoWord2
    {
        private readonly Cliente _cliente;
        private readonly Emprestimo _emprestimo;
        private readonly List<Parcela> _parcelas;

        public ContratoEmprestimoWord2(
            Cliente cliente,
            Emprestimo emprestimo,
            List<Parcela> parcelas)
        {
            _cliente = cliente;
            _emprestimo = emprestimo;
            _parcelas = parcelas;
        }

        public void Gerar(string caminho)
        {
            using var doc = WordprocessingDocument.Create(
                caminho,
                WordprocessingDocumentType.Document);

            var mainPart = doc.AddMainDocumentPart();
            mainPart.Document = new Document();
            var body = mainPart.Document.AppendChild(new Body());

            AdicionarCabecalhoDocumento(doc);
            AdicionarRodapeDocumento(doc);

            AdicionarCabecalho(body);
            AdicionarConteudo(body);
            AdicionarQuebraPagina(body);
            AdicionarQuadroResumo(body);
            AdicionarTabelaParcelas(body);
            AdicionarAssinaturas(body);

            mainPart.Document.Save();
        }

        private void AdicionarCabecalho(Body body)
        {
            body.Append(ParagrafoCentralNegrito(
                "CONTRATO DE EMPRÉSTIMO EM DINHEIRO ENTRE PESSOAS FÍSICAS - CRÉDITO PESSOAL",
                24));

            body.Append(ParagrafoDireita(
                $"Número do CCP: {_emprestimo.Contrato}"));

            body.Append(new Paragraph(new Run(new Break())));
        }

        private void AdicionarConteudo(Body body)
        {
            body.Append(ParagrafoTexto(
                $"Por este instrumento particular, de um lado {_cliente.Nome}, inscrito no CPF {_cliente.Cpf}, residente em {_cliente.Cidade}-{_cliente.Estado}, e de outro lado VICTOR HUGO DE OLIVEIRA E SILVA, têm entre si como justo e contratado o que segue:"
            ));

            body.Append(ParagrafoTexto(
                $"1. O valor total da operação é R$ {_emprestimo.ValorContrato:F2}, com vencimento em {_emprestimo.Vencimento:dd/MM/yyyy}."
            ));

            body.Append(ParagrafoTexto(
                "2. O MUTUÁRIO poderá liquidar antecipadamente com abatimento proporcional dos juros."
            ));

            body.Append(new Paragraph(new Run(new Break())));
        }

        private void AdicionarQuadroResumo(Body body)
        {
            body.Append(ParagrafoCentralNegrito("ANEXO - QUADRO RESUMO", 22));

            var table = new Table();

            table.Append(new TableProperties(
                new TableBorders(
                    new TopBorder { Val = BorderValues.Single, Size = 6 },
                    new BottomBorder { Val = BorderValues.Single, Size = 6 },
                    new LeftBorder { Val = BorderValues.Single, Size = 6 },
                    new RightBorder { Val = BorderValues.Single, Size = 6 },
                    new InsideHorizontalBorder { Val = BorderValues.Single, Size = 6 },
                    new InsideVerticalBorder { Val = BorderValues.Single, Size = 6 }
                )));

            AdicionarLinhaTabela(table, "Código", _emprestimo.Codigo.ToString("D3"));
            AdicionarLinhaTabela(table, "Data", _emprestimo.Averbacao?.ToString("dd/MM/yyyy") ?? "");
            AdicionarLinhaTabela(table, "Parcelas", _emprestimo.Parcelas.ToString());
            AdicionarLinhaTabela(table, "Valor Total", $"R$ {_emprestimo.ValorContrato:F2}");

            body.Append(table);
        }

        private void AdicionarTabelaParcelas(Body body)
        {
            body.Append(ParagrafoCentralNegrito("PARCELAS", 22));

            var table = new Table();

            table.Append(new TableProperties(
                new TableBorders(
                    new TopBorder { Val = BorderValues.Single, Size = 6 },
                    new BottomBorder { Val = BorderValues.Single, Size = 6 },
                    new LeftBorder { Val = BorderValues.Single, Size = 6 },
                    new RightBorder { Val = BorderValues.Single, Size = 6 },
                    new InsideHorizontalBorder { Val = BorderValues.Single, Size = 6 },
                    new InsideVerticalBorder { Val = BorderValues.Single, Size = 6 }
                )));

            AdicionarLinhaTabela(table, "Parcela", "Vencimento", "Valor");

            foreach (var p in _parcelas)
            {
                AdicionarLinhaTabela(
                    table,
                    p.NParcelas.ToString(),
                    p.Vencimento?.ToString("dd/MM/yyyy"),
                    $"R$ {p.ValorPrestacao:F2}"
                );
            }

            body.Append(table);
        }

        private void AdicionarAssinaturas(Body body)
        {
            body.Append(new Paragraph(new Run(new Break())));
            body.Append(new Paragraph(new Run(new Break())));

            body.Append(ParagrafoTexto("______________________________________"));
            body.Append(ParagrafoTexto(_cliente.Nome));
            body.Append(ParagrafoTexto("MUTUÁRIO"));

            body.Append(new Paragraph(new Run(new Break())));

            body.Append(ParagrafoTexto("______________________________________"));
            body.Append(ParagrafoTexto("VICTOR HUGO DE OLIVEIRA E SILVA"));
            body.Append(ParagrafoTexto("MUTUANTE"));
        }

        private void AdicionarQuebraPagina(Body body)
        {
            body.Append(new Paragraph(new Run(new Break() { Type = BreakValues.Page })));
        }

        private void AdicionarLinhaTabela(Table table, params string[] valores)
        {
            var tr = new TableRow();

            foreach (var v in valores)
            {
                tr.Append(new TableCell(
                    new Paragraph(new Run(new Text(v ?? "")))));
            }

            table.Append(tr);
        }

        private Paragraph ParagrafoTexto(string texto)
        {
            return new Paragraph(new Run(new Text(texto)));
        }

        private Paragraph ParagrafoCentralNegrito(string texto, int size)
        {
            return new Paragraph(
                new Run(
                    new RunProperties(
                        new Bold(),
                        new FontSize { Val = (size * 2).ToString() }),
                    new Text(texto)))
            {
                ParagraphProperties = new ParagraphProperties(
                    new Justification { Val = JustificationValues.Center })
            };
        }

        private Paragraph ParagrafoDireita(string texto)
        {
            return new Paragraph(
                new Run(new Text(texto)))
            {
                ParagraphProperties = new ParagraphProperties(
                    new Justification { Val = JustificationValues.Right })
            };
        }

        //CABEÇALHO

        private void AdicionarCabecalhoDocumento(WordprocessingDocument doc)
        {
            var mainPart = doc.MainDocumentPart;

            var headerPart = mainPart.AddNewPart<HeaderPart>();
            string headerId = mainPart.GetIdOfPart(headerPart);

            var header = new Header();

            var paragrafo = new Paragraph(
                new Run(
                    new Text("SPEED CRED - CRÉDITO PESSOAL")));

            paragrafo.ParagraphProperties = new ParagraphProperties(
                new Justification { Val = JustificationValues.Center });

            header.Append(paragrafo);

            headerPart.Header = header;

            var sectionProps = doc.MainDocumentPart.Document.Body
                .Elements<SectionProperties>()
                .FirstOrDefault();

            if (sectionProps == null)
            {
                sectionProps = new SectionProperties();
                doc.MainDocumentPart.Document.Body.Append(sectionProps);
            }

            sectionProps.RemoveAllChildren<HeaderReference>();

            sectionProps.Append(
                new HeaderReference
                {
                    Id = headerId,
                    Type = HeaderFooterValues.Default
                });
        }

        //RODAPE
        private void AdicionarRodapeDocumento(WordprocessingDocument doc)
        {
            var mainPart = doc.MainDocumentPart;

            var footerPart = mainPart.AddNewPart<FooterPart>();
            string footerId = mainPart.GetIdOfPart(footerPart);

            var footer = new Footer();

            var paragrafo = new Paragraph(
                new Run(
                    new Text("Página "),
                    new FieldChar { FieldCharType = FieldCharValues.Begin },
                    new FieldCode(" PAGE "),
                    new FieldChar { FieldCharType = FieldCharValues.End }
                ));

            paragrafo.ParagraphProperties = new ParagraphProperties(
                new Justification { Val = JustificationValues.Center });

            footer.Append(paragrafo);

            footerPart.Footer = footer;

            var sectionProps = doc.MainDocumentPart.Document.Body
                .Elements<SectionProperties>()
                .FirstOrDefault();

            if (sectionProps == null)
            {
                sectionProps = new SectionProperties();
                doc.MainDocumentPart.Document.Body.Append(sectionProps);
            }

            sectionProps.RemoveAllChildren<FooterReference>();

            sectionProps.Append(
                new FooterReference
                {
                    Id = footerId,
                    Type = HeaderFooterValues.Default
                });
        }
    }
}