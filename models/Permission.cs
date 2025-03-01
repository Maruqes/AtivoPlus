using System.ComponentModel.DataAnnotations;
//a tabela na db defenicao
namespace AtivoPlus.Models
{
    public class Permission
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}

