using System;

namespace CrudApp.Models
{
    public class Despesa
    {
        public int Id { get; set; }
        public string Tipo { get; set; }
        public decimal Valor { get; set; }
        public DateTime Vencimento { get; set; }
        public DateTime? DataPagamento { get; set; }
        public string Situacao { get; set; }
        public DateTime CriadoEm { get; set; }
    }
}