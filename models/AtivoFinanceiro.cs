using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

//a tabela na db defenicao
namespace AtivoPlus.Models
{
    public class AtivoFinanceiro
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("User")]
        public int UserId { get; set; } 
        [ForeignKey("EntidadeAtivo")]
        public int EntidadeAtivoId {get; set;}
        [ForeignKey("Carteira")]
        public int CarteiraId { get; set; }
        public DateTime DataInicio { get; set; } 
        public int DuracaoMeses { get; set; }
        public float TaxaImposto { get; set; }
    }
}

