using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

//a tabela na db defenicao
namespace AtivoPlus.Models
{
    public class UserInfo
    {
        [Key, ForeignKey("User")]
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public string Morada { get; set; } = string.Empty;
        public string NIF { get; set; } = string.Empty;
        public DateTime DateTime { get; set; } = DateTime.Now;
        public string IBAN { get; set; } = string.Empty;
    }
}

