using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Trabahub.Models
{
    public class Analytics
    {
        [Column(Order = 0)]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string? DataReference { get; set; }

        [Required]
        public double? TotalIncome { get; set; }
    }
}
