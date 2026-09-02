using System;
using System.IO;
using System.Collections.Generic;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using CrudApp.Models;
using System.Linq;

namespace CrudApp.Word
{
    public class ContratoEmprestimoWord
    {
        private readonly Cliente _cliente;
        private readonly Emprestimo _emprestimo;
        private readonly List<Parcela> _parcelas;

        public ContratoEmprestimoWord(
            Cliente cliente,
            Emprestimo emprestimo,
            List<Parcela> parcelas)
        {
            _cliente = cliente;
            _emprestimo = emprestimo;
            _parcelas = parcelas;
        }

        public void Gerar(string caminhoDestino)
        {
            string modelo = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "assets",
                "ContratoModelo.docx");

            if (!File.Exists(modelo))
                throw new Exception("Modelo do contrato não encontrado.");

            File.Copy(modelo, caminhoDestino, true);

            using (var doc = WordprocessingDocument.Open(caminhoDestino, true))
            {
                var campos = MontarCampos();
                SubstituirCampos(doc, campos);
                PreencherTabelaParcelas(doc, _parcelas);
                doc.MainDocumentPart.Document.Save();
            }
        }

        private Dictionary<string, string> MontarCampos()
        {
            string endereco =
                $"{_cliente.Rua}, {_cliente.Numero}" +
                (string.IsNullOrWhiteSpace(_cliente.Bairro) ? "" : $" - {_cliente.Bairro}") +
                (string.IsNullOrWhiteSpace(_cliente.Cidade) ? "" : $" - {_cliente.Cidade}") +
                (string.IsNullOrWhiteSpace(_cliente.Estado) ? "" : $"/{_cliente.Estado}") +
                (string.IsNullOrWhiteSpace(_cliente.Cep) ? "" : $" - CEP: {_cliente.Cep}");

            return new Dictionary<string, string>
            {
                { "{{CONTRATO}}", _emprestimo.Contrato.ToString("D3") },
                { "{{NOME}}", _cliente.Nome ?? "" },
                { "{{NACI}}", _cliente.Nacionalidade ?? "" },
                { "{{PROFISSAO}}", _cliente.Ocupacao ?? "" },
                { "{{ESTADO_CIVIL}}", _cliente.Estado_civil ?? "" },
                { "{{RG}}", _cliente.Rg ?? "" },
                { "{{CPF}}", _cliente.Cpf ?? "" },
                { "{{RUA}}", _cliente.Rua ?? "" },
                { "{{NUMERO}}", _cliente.Numero ?? "" },
                { "{{BAIRRO}}", _cliente.Bairro ?? "" },
                { "{{CIDADE}}", _cliente.Cidade ?? "" },
                { "{{ESTADO}}", _cliente.Estado ?? "" },
                { "{{ENDERECO}}", endereco },

                { "{{VALOR_CONTRATO}}", _emprestimo.ValorContrato.ToString("N2") },
                { "{{VENCIMENTO}}", _emprestimo.Vencimento?.ToString("dd/MM/yyyy") ?? "" },
                { "{{DATA_ATUAL}}", DateTime.Now.ToString("dd/MM/yyyy") },

                { "{{OPERACAO}}", _emprestimo.Codigo.ToString("D3") },
                { "{{AVERBACAO}}", _emprestimo.Averbacao?.ToString("dd/MM/yyyy") ?? "" },
                { "{{PARCELAS}}", _emprestimo.Parcelas.ToString() },
                { "{{VALOR_PRESTACAO}}",
                (_parcelas.Count > 0
                    ? _parcelas[0].ValorPrestacao?.ToString("N2") ?? "0,00"
                    : "0,00")}
            };
        }

        private void SubstituirCampos(
            WordprocessingDocument doc,
            Dictionary<string, string> campos)
        {
            ProcessarParte(doc.MainDocumentPart, campos);

            foreach (var header in doc.MainDocumentPart.HeaderParts)
                ProcessarParte(header, campos);

            foreach (var footer in doc.MainDocumentPart.FooterParts)
                ProcessarParte(footer, campos);
        }

        private void ProcessarParte(OpenXmlPart part, Dictionary<string, string> campos)
        {
            var paragrafos = part.RootElement
                                .Descendants<Paragraph>()
                                .ToList();

            foreach (var p in paragrafos)
            {
                var runs = p.Elements<Run>().ToList();

                if (!runs.Any())
                    continue;

                // Junta texto do parágrafo
                string textoParagrafo = string.Concat(
                    runs.Select(r => r.GetFirstChild<Text>()?.Text ?? "")
                );

                bool alterou = false;

                foreach (var campo in campos)
                {
                    if (textoParagrafo.Contains(campo.Key))
                    {
                        textoParagrafo = textoParagrafo.Replace(
                            campo.Key,
                            campo.Value ?? ""
                        );
                        alterou = true;
                    }
                }

                if (!alterou)
                    continue;

                // Remove apenas os Text dos runs
                foreach (var run in runs)
                {
                    var text = run.GetFirstChild<Text>();
                    if (text != null)
                        text.Text = "";
                }

                // Coloca o texto novo no primeiro run
                var primeiroText = runs[0].GetFirstChild<Text>();
                if (primeiroText != null)
                    primeiroText.Text = textoParagrafo;
            }
        }

        private void PreencherTabelaParcelas(
            WordprocessingDocument doc,
            List<Parcela> parcelas)
        {
            var tables = doc.MainDocumentPart.Document
                            .Descendants<Table>()
                            .ToList();

            if (!tables.Any())
                return;

            // Ajuste se você tiver mais de uma tabela
            var tabela = tables.Last(); 

            var linhas = tabela.Elements<TableRow>().ToList();

            if (linhas.Count < 2)
                return;

            // Linha modelo (segunda linha)
            var linhaModelo = linhas[1];

            // Remove todas as linhas exceto cabeçalho
            for (int i = linhas.Count - 1; i >= 1; i--)
                linhas[i].Remove();

            foreach (var p in parcelas)
            {
                var novaLinha = (TableRow)linhaModelo.CloneNode(true);

                var textos = novaLinha.Descendants<Text>().ToList();

                foreach (var t in textos)
                {
                    if (t.Text.Contains("{{NUM}}"))
                        t.Text = p.NParcelas.ToString();

                    if (t.Text.Contains("{{VDATA}}"))
                        t.Text = p.Vencimento?.ToString("dd/MM/yyyy") ?? "";

                    if (t.Text.Contains("{{VALOR}}"))
                        t.Text = p.ValorPrestacao?.ToString("N2") ?? "0,00";
                }

                tabela.Append(novaLinha);
            }
        }

        private void SubstituirEmParte(
            OpenXmlPart part,
            Dictionary<string, string> campos)
        {
            using (var stream = part.GetStream())
            using (var reader = new StreamReader(stream))
            {
                string xml = reader.ReadToEnd();

                foreach (var campo in campos)
                    xml = xml.Replace(campo.Key, campo.Value ?? "");

                stream.SetLength(0);

                using (var writer = new StreamWriter(stream))
                {
                    writer.Write(xml);
                }
            }
        }
    }
}