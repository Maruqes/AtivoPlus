using System.ComponentModel.DataAnnotations;

//a tabela na db defenicao
namespace AtivoPlus.Models
{
    public class Produto
    {
        [Key]
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public decimal Preco { get; set; }
    }
}

