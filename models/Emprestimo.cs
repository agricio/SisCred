using System;

namespace CrudApp.Models
{
    public class Emprestimo
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }

        public string Contrato { get; set; }
        public double Liberado { get; set; }
        public double ValorContrato { get; set; }
        public double TotalJuros { get; set; }
        public double TotalAmortizacao { get; set; }
        public double Agio { get; set; }
        public int Parcelas { get; set; }
        public int Codigo { get; set; }

        public DateTime? Averbacao { get; set; }
        public DateTime? Vencimento { get; set; }
        public DateTime? Quitacao { get; set; }
        public DateTime? AtualizadoEm { get; set; }

        public string Tipo { get; set; } = "";
        public string Situacao { get; set; } = "";
        public string TipoQuitacao { get; set; } = "";
        public bool Contabilizado { get; set; }
        public double? Abatimento { get; set; }

    }
}
