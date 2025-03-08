using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

//a tabela na db defenicao
namespace AtivoPlus.Models
{
    [Index(nameof(Nome), IsUnique = true)]
    public class Carteira
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("User")]
        public int UserId { get; set; } 
        public string Nome { get; set; } = string.Empty;
        public DateTime DataCriacao { get; set; }
    }
}