using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AtivoPlus.Models
{
    [Table("candles")]
    public class Candle
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("symbol", TypeName = "text")]
        public string Symbol { get; set; } = string.Empty;

        [Required]
        [Column("datetime", TypeName = "timestamp")]
        public DateTime DateTime { get; set; }

        [Required]
        [Column("open", TypeName = "numeric(18,8)")]
        public decimal Open { get; set; }

        [Required]
        [Column("high", TypeName = "numeric(18,8)")]
        public decimal High { get; set; }

        [Required]
        [Column("low", TypeName = "numeric(18,8)")]
        public decimal Low { get; set; }

        [Required]
        [Column("close", TypeName = "numeric(18,8)")]
        public decimal Close { get; set; }

        [Required]
        [Column("volume", TypeName = "numeric(18,8)")]
        public decimal Volume { get; set; }
    }
}
