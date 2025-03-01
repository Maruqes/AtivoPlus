using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
//a tabela na db defenicao
namespace AtivoPlus.Models
{
    public class DepositoPrazo
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("AtivoFinanceiro")]
        public int AtivoFinaceiroId { get; set; } 
        [ForeignKey("TipoAtivo")]
        public int TipoAtivoId { get; set; } 
        [ForeignKey("Banco")]
        public int BancoId { get; set; }
        [ForeignKey("User")]
        public int TitularId { get; set; }
        public int NumeroConta { get; set; }
        public float TaxaJuroAnual { get; set; }
        public Decimal ValorAtual { get; set; }
        public Decimal ValorInvestido { get; set; }
        public Decimal ValorAnualDespesasEstimadas { get; set; }
    }
}

