using System.Security.Cryptography;
using System.Text;
using System;
using System.Security.Cryptography;
using System.Text;


namespace CrudApp.Security
{
    public static class HashHelper
    {
        public static string GerarHash(string texto)
        {
            using var sha = SHA256.Create();
            byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(texto));
            return Convert.ToHexString(bytes);
        }
    }
}
