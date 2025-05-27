using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

//a tabela na db defenicao
namespace AtivoPlus.Models
{
    //crypto/etf/stock
    [Index(nameof(Nome))]
    public class FundoInvestimento
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("AtivoFinanceiro")]
        public int AtivoFinaceiroId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public decimal MontanteInvestido { get; set; }
        public string AtivoSigla { get; set; } = string.Empty;

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    }
}

