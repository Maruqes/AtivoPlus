using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

//a tabela na db defenicao
namespace AtivoPlus.Models
{
    [Index(nameof(Username), IsUnique = true)]
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Hash { get; set; } = string.Empty;
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    }
}

