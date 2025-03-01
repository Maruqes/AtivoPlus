using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

//a tabela na db defenicao
namespace AtivoPlus.Models
{
    public class UserPermission
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        [ForeignKey("Permission")]
        public int PermissionId { get; set; }
    }
}

