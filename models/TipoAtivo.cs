using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
//a tabela na db defenicao
namespace AtivoPlus.Models
{
    [Index(nameof(Nome), IsUnique = true)]
    public class TipoAtivo
    {
        [Key]
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
    }
}
