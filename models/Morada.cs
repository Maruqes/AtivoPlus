using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

//a tabela na db defenicao
namespace AtivoPlus.Models
{
    public class Morada
    {
        [Key]
        public int Id { get; set; }        
        public string Rua { get; set; } = string.Empty;
        public string Piso { get ; set; } = string.Empty;
        public string NumeroPorta { get; set; } = string.Empty;
        public string Concelho { get; set; } = string.Empty;
        public string Distrito { get; set; } = string.Empty;
        public string Localidade { get; set; } = string.Empty;
        public string CodPostal { get; set; } = string.Empty;
    }
}

