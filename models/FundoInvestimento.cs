using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

//a tabela na db defenicao
namespace AtivoPlus.Models
{
    [Index(nameof(Nome), IsUnique = true)]
    public class FundoInvestimento
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("AtivoFimanceiro")]
        public int AtivoFinaceiroId { get; set; } 
        [ForeignKey("TipoAtivo")]
        public int TipoAtivoId { get; set; } 
        public string Nome { get; set; } = string.Empty;
        public decimal MontanteInvestido { get; set; }
        public float TaxaJuro { get; set; }
        public Boolean TaxaFixa { get; set; }
    }
}

