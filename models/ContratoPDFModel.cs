using System;
using System.Collections.Generic;

namespace CrudApp.Models;
public class ContratoPdfModel
{
    public string NumeroContrato { get; set; }
    public string NomeCliente { get; set; }
    public string CPF { get; set; }
    public string Profissao { get; set; }
    public string EstadoCivil { get; set; }
    public string Endereco { get; set; }

    public decimal ValorContrato { get; set; }
    public int QuantidadeParcelas { get; set; }
    public decimal ValorParcela { get; set; }

    public DateTime DataContrato { get; set; }
    public DateTime VencimentoContrato { get; set; }

    public List<Parcela> Parcelas { get; set; } = new();
}