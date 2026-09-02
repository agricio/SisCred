namespace CrudApp.Models
{
    public class Cliente
    {
        public int Id { get; set; }

        public byte[]? Foto { get; set; }

        public string Nome { get; set; } = string.Empty;
        public string? Rg { get; set; }
        public string? Ocupacao { get; set; }
        public string? Cpf { get; set; }
        public string? Email { get; set; }
        public string? Telefone { get; set; }
        public string? Bairro { get; set; }
        public string? Nacionalidade { get; set; }
        public string? Estado_civil { get; set; }
        public string? Cel_pix { get; set; }

        // Endereço
        public string? Estado { get; set; }
        public string? Cidade { get; set; }
        public string? Rua { get; set; }
        public string? Numero { get; set; }
        public string? Complemento { get; set; }
        public string? Cep { get; set; }

        // Dados bancários
        public string? Banco { get; set; }
        public string? Agencia { get; set; }
        public string? Conta { get; set; }

        public int QtdContratos { get; set; }
        public string? ClienteScore { get; set; }
    }
}

