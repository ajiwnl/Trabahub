using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Trabahub.Models
{
    public class Analytics
    {
        [Key]
        [Required]
        [StringLength(255)]
        public string? DataReference { get; set; }

        [Required]
        public double? TotalIncome { get; set; }
    }
}
