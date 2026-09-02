
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
    public class ContratoEmprestimoPdf : IDocument
    {
        private readonly Cliente _cliente;
        private readonly Emprestimo _emprestimo;
        private readonly List<Parcela> _parcelas;

        public ContratoEmprestimoPdf(Cliente cliente, Emprestimo emprestimo, List<Parcela> parcelas)
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

       private void ComposeHeader(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().Row(row =>
                {
                    // 🖼 LOGO (ESQUERDA)
                    row.ConstantItem(100)
                    .Width(98)
                    .Image(Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        "Assets", "doc_logo.jpg"));

                    // CENTRO
                    row.RelativeItem(5).AlignCenter().Text("CONTRATO DE EMPRÉSTIMO EM DINHEIRO ENTRE PESSOAS FÍSICAS - CRÉDITO PESSOAL")
                        .Bold().FontSize(12);

                    // DIREITA
                    row.RelativeItem(1).AlignRight().Text(t =>
                    {            
                        t.Line($"Número do CCP: {_emprestimo.Contrato}").Bold();
                    });
                });

                col.Item().PaddingTop(5).LineHorizontal(1);
            });
        }

    private void ComposeContent(IContainer container)
    {
        container.Column(col =>
        {
            col.Spacing(0);

            col.Item().PaddingVertical(5);

            col.Item().Text(text =>
            {
                text.Span("   Por este instrumento particular, de um lado ");

                text.Span(_cliente.Nome).Bold();

                text.Span($", nacionalidade {_cliente.Nacionalidade}, profissão {_cliente.Ocupacao}, {_cliente.Estado_civil} portador do RG nº {_cliente.Rg}, inscrito no CPF sob o nº {_cliente.Cpf}, residente e domiciliado à {_cliente.Rua}, Bairro {_cliente.Bairro}, na cidade de {_cliente.Cidade}, Estado de(a) {_cliente.Estado}, cep {_cliente.Cep}, nº {_cliente.Numero}, de ora em diante denominado simplesmente MUTUARIO e de outro lado, ");

                text.Span("VICTOR HUGO DE OLIVEIRA E SILVA").Bold();

                text.Span(", inscrito no CPF nº 055.537.694-00, com endereço na cidade de Itabaiana, estado da Paraíba, localizado a Praça Marieta Medeiros, nº 23, centro, CEP: 58.360-000, de ora em diante denominado simplesmente MUTUANTE, têm entre si como justo e contratado o que segue:");
            });

            col.Item().PaddingVertical(5);

            // ===== CLÁUSULA 1 =====
            col.Item().Text(
                $"1. O MUTUANTE celebre neste ato junto ao MUTUÁRIO o presente contrato de empréstimo sendo a quantia total da operação no valor de R$ {_emprestimo.ValorContrato:F2}, " +
                $"pelo prazo equivalente até a data de: {_emprestimo.Vencimento:dd/MM/yyyy}, sendo discriminada toda operação no QUADRO RESUMO em anexo nesse contrato."
            );

            col.Item().PaddingVertical(5);

            // ===== CLÁUSULA 2 =====
            col.Item().Text(
                "2. A quantia acima será aplicada juros calculados como prevê a Lei nº 10.406/2002 em seus artigos 591 e 406, de acordo com a quantidade de parcelas determinadas pelo MUTUÁRIO, bem como, o valor do empréstimo será entregue pelo MUTUANTE, ao MUTUÁRIO, em moeda corrente oficial, em espécie, no ato da assinatura do presente contrato valendo esse, também, como RECIBO DO VALOR TOMADO COMO EMPRÉSTIMO."
            );

            col.Item().PaddingVertical(5);

            // ===== CLÁUSULA 3 =====
            col.Item().Text(
                "3. O MUTUÁRIO poderá amortizar ou liquidar a dívida ora contraída, antes do vencimento, com abatimento proporcional dos juros, calculado pelo MUTUANTE."
            );

            col.Item().PaddingVertical(5);

            // ===== CLÁUSULA 4 =====
            col.Item().Text(
                "4. O ATRASO NO PAGAMENTO de quaisquer valores devidos, vencidos e não pagos na época em que forem exigíveis por força do disposto neste contrato, configurará a situação de atraso, ficando a dívida sujeita do vencimento ao efetivo pagamento, dos seguintes encargos:"
            );

            col.Item().PaddingVertical(5);

            col.Item().Text(text =>
            {
                text.Span("●  MULTA MORATÓRIA").Bold(); // Parte em negrito
                text.Span(" que incidirá sobre o valor da parcela em atraso."); // Parte normal
            });

            col.Item().PaddingVertical(5);

            col.Item().Text(text =>
            {
                text.Span("●  JUROS MORATÓRIOS").Bold(); // Parte em negrito
                text.Span(" que incidirá sobre o valor da parcela em atraso diariamente;"); // Parte normal
            });

            col.Item().PaddingVertical(5);

            col.Item().Text(
                "4.1 Esses atrasos serão fixados de acordo com a taxa referencial do Sistema Especial de Liquidação e Custódia (Selic), essa taxa será utilizada para ATUALIZAÇÃO de dívidas das parcelas em atrasado, e será pré-definida no momento da celebração do presente contrato, conforme prevê o art. 406 do CCB."
            );

            col.Item().PaddingVertical(5);

            col.Item().Text(
                "4.2 Ocorrendo o previsto no item 4 o MUTUANTE poderá executar o presente contrato, através da liquidação definitiva do valor devido pelo MUTUÁRIO, quando existirem até 02 (DUAS) PARCELAS ATRASADAS."
            );

            col.Item().PaddingVertical(5);

            // ===== CLÁUSULA 6 =====
            col.Item().Text(
                "6. EM CASO DE INADIMPLEMENTO: O Contratante Mutuário terá seu nome inserido no cadastro de inadimplentes, recorrendo o MUTUANTE aos serviços do SPC e Serasa para negativar, se caso for, o Mutuário devedor, bem como cobrar em juízo ao MUTUÁRIO por perdas e danos, tudo conforme art. 389 do Código Civil, outrossim, declaram as partes que este instrumento tem validade de título executivo extrajudicial na forma do inciso III do artigo 784 do Código de Processo Civil."
            ).Bold();

            col.Item().PaddingVertical(5);

            // ===== CLÁUSULA 7 =====
            col.Item().Text(
                "7. Declaro que li e compreendi o sentido e o alcance de todas as disposições acima e da íntegra das Condições Gerais do presente contrato, cuja cópia foi entregue para mim neste ato, bem como a operação contratada está adequada a minha necessidade, interesses e objetivos. Além do mais após verificar a condição de pagamento, esta contratação se mostrou adequada a minha atual situação financeira, não implicando em excessivo endividamento, nem prejudicando a minha subsistência."
            );

            col.Item().PaddingVertical(5);

            // ===== CLÁUSULA 8 =====
            col.Item().Text(
                "8. O presente contrato será assinado de forma digital pelo MUTUÁRIO, conforme prevê a Lei nº 14.063, de 23 de setembro de 2020, que reconhece a validade jurídica da assinatura eletrônica."
            );

            col.Item().PaddingVertical(5);

            // ===== CLÁUSULA 9 =====
            col.Item().Text(
                "9. Fica eleito o foro desta cidade de Itabaiana, estado da Paraíba, para dirimir eventuais litígios decorrentes do ora contrato."
            );

            col.Item().PaddingVertical(12);

            col.Item().Text("E, por estarem assim justas e contratadas, as partes contratantes assinam o presente em 02 (duas) vias de igual teor.");

            // ===== DATA =====
            col.Item().Text($"Itabaiana – PB, {DateTime.Now:dd/MM/yyyy}");

            //col.Item().PaddingVertical(15);

            // ===== QUEBRA DE PAGINA =====
            col.Item().PageBreak();

            // ===== QUADRO RESUMO =====
            col.Item().PaddingTop(15).Text("ANEXO")
                .FontSize(13).Bold().AlignCenter();

            col.Item().PaddingTop(5).Text("QUADRO RESUMO")
                .FontSize(11).Bold().AlignCenter();

            col.Item().PaddingVertical(6);

            col.Item().Text("IDENTIFICAÇÃO DO MUTUANTE").FontSize(12).Bold();
            col.Item().PaddingVertical(3);

            col.Item().Text(text =>
            {
                text.Span("NOME:").FontSize(10).Bold(); // Parte em negrito
                text.Span(" VICTOR HUGO DE OLIVEIRA E SILVA").FontSize(10); // Parte normal
            });

            col.Item().PaddingVertical(0).Text(text =>
            {
                text.Span("CPF:").FontSize(10).Bold(); // Parte em negrito
                text.Span(" 055.537.694-00").FontSize(10); // Parte normal
            });

            col.Item().PaddingVertical(0).Text(text =>
            {
                text.Span("ENDEREÇO:").Bold().FontSize(10); // Parte em negrito
                text.Span(" Rua Marieta Medeiros, nº 23, centro, Itabaiana, Paraíba").FontSize(10); // Parte normal
            });

            col.Item().PaddingVertical(5);

            col.Item().Text("IDENTIFICAÇÃO DO MUTUÁRIO").FontSize(12).Bold();

            col.Item().PaddingVertical(3);

            col.Item().Text(text =>
            {
                text.Span("Mutuário: ").FontSize(10).Bold(); // Parte em negrito
                text.Span($" {_cliente.Nome}").FontSize(10); // Parte normal
            });

            col.Item().Text(text =>
            {
                text.Span("CPF: ").FontSize(10).Bold(); // Parte em negrito
                text.Span($" {_cliente.Cpf}").FontSize(10); // Parte normal
            });

            col.Item().Text(text =>
            {
                text.Span("ENDEREÇO: ").FontSize(10).Bold(); // Parte em negrito
                text.Span($"{_cliente.Rua}, nº {_cliente.Numero}, {_cliente.Bairro}, {_cliente.Cidade}, {_cliente.Estado}").FontSize(10); // Parte normal
            });

            col.Item().PaddingVertical(5);

            col.Item().Text("DADOS DA OPERAÇÃO:").FontSize(12).Bold();

            col.Item().PaddingVertical(3);

            col.Item().Table(t =>
            {
                t.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(1); // Coluna 1: título
                    c.RelativeColumn(2); // Coluna 2: valor
                });

                // Cada linha: título na 1ª coluna, valor na 2ª coluna
                t.Cell().Text("CODIGO DA OPERAÇÃO:").FontSize(10).Bold();
                t.Cell().Text(_emprestimo.Codigo.ToString("D3")).FontSize(10);

                t.Cell().Text("DATA DO CONTRATO:").FontSize(10).Bold();
                t.Cell().Text($"{_emprestimo.Averbacao:dd/MM/yyyy}").FontSize(10);

                t.Cell().Text("QUANTIDADE DE PARCELAS:").FontSize(10).Bold();
                t.Cell().Text($"{_emprestimo.Parcelas}").FontSize(10);

                t.Cell().Text("VALOR DA PARCELA:").FontSize(10).Bold();
                t.Cell().Text($"{_parcelas.First().ValorPrestacao:F2}").FontSize(10);

                t.Cell().Text("VALOR TOTAL DA OPERAÇÃO:").FontSize(10).Bold();
                t.Cell().Text($"{_emprestimo.ValorContrato:F2}").FontSize(10);

                t.Cell().Text("VENCIMENTO:").FontSize(10).Bold();
                t.Cell().Text($"{_emprestimo.Vencimento:dd/MM/yyyy}").FontSize(10);

                t.Cell().Text("JUROS MORATORIOS:").FontSize(10).Bold();
                t.Cell().Text("0,4999% a.d. (ao dia) (SELIC)").FontSize(10);

                t.Cell().Text("MULTA MORATÓRIA:").FontSize(10).Bold();
                t.Cell().Text("2,5% (dois e meio por cento)").FontSize(10);

                t.Cell().Text("GRARANTIAS ACESSÓRIAS:").FontSize(10).Bold();
                t.Cell().Text("Não tem garantias acessórias.").FontSize(10);
            });

            col.Item().PaddingVertical(10);

            // ===== TABELA DE PARCELAS =====
            col.Item().PaddingTop(15).Text("Parcelas, Vencimento e Valor:")
                .FontSize(13).Bold().AlignCenter();
            
            col.Item().PaddingVertical(6);

            TabelaParcelas(col);

        });
    }



    private void Secao(ColumnDescriptor col, string titulo)
    {
        col.Item().PaddingTop(10)
            .Text(titulo).Bold();
    }

    private void Campo(ColumnDescriptor col, string label, string valor)
    {
        col.Item().Text($"{label}: {valor}");
    }

    private void CampoDuplo(
        ColumnDescriptor col,
        string l1, string v1,
        string l2, string v2)
    {
        col.Item().Row(r =>
        {
            r.RelativeItem().Text($"{l1}: {v1}");
            r.RelativeItem().Text($"{l2}: {v2}");
        });
    }

    private void TabelaParcelas(ColumnDescriptor col)
    {
        col.Item().Table(t =>
        {
            t.ColumnsDefinition(c =>
            {
                c.ConstantColumn(60);
                c.RelativeColumn(100);
                c.ConstantColumn(80);
            });

            // Cabeçalho
            t.Header(h =>
            {
                h.Cell().Border(1).Padding(5).Text("Parcelas").Bold();
                h.Cell().Border(1).Padding(5).Text("Vencimento").Bold();
                h.Cell().Border(1).Padding(5).Text("Valor").Bold();
            });

            // Linhas da tabela
            foreach (var p in _parcelas)
            {
                t.Cell().Border(1).Padding(5).AlignCenter().Text(p.NParcelas.ToString());
                t.Cell().Border(1).Padding(5).AlignCenter().Text(p.Vencimento?.ToString("dd/MM/yyyy"));
                t.Cell().Border(1).Padding(5).Text($"R$ {p.ValorPrestacao:F2}");
            }
        });
    }


        

    private void ComposeFooter(IContainer container)
        {
            container.Column(col =>
            {
                
                col.Item().Row(row =>
                
                {
                    // ===== COLUNA ESQUERDA (CLIENTE) =====
                    row.RelativeItem().AlignLeft().Column(col =>
                    {
                        // Linha da assinatura
                        col.Item()
                            .PaddingTop(25)
                            .Width(180)
                            .LineHorizontal(1);

                        col.Item().PaddingTop(12).Text(_cliente.Nome).FontSize(8);
                        col.Item().Text("CPF: "+ _cliente.Cpf).FontSize(8);
                        col.Item().Text("MUTUÁRIO").FontSize(8);
                    });
                    // ===== COLUNA DIREITA (LOGO + MUTUANTE) =====
                    row.RelativeItem().AlignRight().Column(right =>
                    {
                        right.Item()
                            .AlignRight()
                            .Height(40)
                            .Image(Path.Combine(
                                AppDomain.CurrentDomain.BaseDirectory,
                                "Assets",
                                "ass_img.jpg"));

                        right.Item().PaddingTop(0).Text(t =>
                        {
                            //t.Line("__________________________________");
                            t.Line("VICTOR HUGO DE OLIVEIRA E SILVA").FontSize(8);
                            t.Line("CPF: 055.537.694-00").FontSize(8);
                            t.Line("MUTUANTE").FontSize(8);
                        });
                    });
                });
            });
        }

    }
}
