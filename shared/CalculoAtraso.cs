using System;


namespace CrudApp.Shared
{
    public static class CalculoAtraso
    {
        public static double Calcular(
            double valorPrestacao,
            DateTime vencimento,
            double multaPercentual = 5,
            double moraDiaPercentual = 0.5)
        {
            if (DateTime.Today <= vencimento)
                return valorPrestacao;

            int dias = (DateTime.Today - vencimento).Days;

            double multa = valorPrestacao * (multaPercentual / 100.0);
            double juros = valorPrestacao * (moraDiaPercentual / 100.0) * dias;

            return valorPrestacao + multa + juros;
        }
    }
}