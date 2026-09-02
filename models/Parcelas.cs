using System;

namespace CrudApp.Models
{
    public class Parcela
    {
        public int Id { get; set; }

        public int EmprestimosId { get; set; }
        public int? NParcelas { get; set; }
        public double? Amortizacao { get; set; }
        public double? Juros { get; set; }
        public double? ValorPrestacao { get; set; }
        public double? Saldo { get; set; }
        public double? Agio { get; set; }
        public string Situacao { get; set; }
        public string FormaPagamento { get; set; }

        public DateTime? Vencimento { get; set; }
        public DateTime? Pagamento { get; set; }
    }
}
