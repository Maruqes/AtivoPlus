using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
//a tabela na db defenicao
namespace AtivoPlus.Models
{
    public class ImovelArrendado
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("AtivoFimanceiro")]
        public int AtivoFinaceiroId { get; set; } 
        [ForeignKey("Morada")]
        public int MoradaId { get; set; }
        public string Designacao { get; set; } = string.Empty;
        public string Localizacao { get; set; } = string.Empty;
        public Decimal ValorImovel { get; set; }
        public Decimal ValorRenda { get; set; }
        public Decimal ValorMensalCondominio { get; set; }
        public Decimal ValorAnualDespesasEstimadas { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.Now;
    }
}

