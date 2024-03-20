using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Trabahub.Models
{

    public class ListInteraction
    {
        [Key]
        public int? InteractId { get; set; }

        [StringLength(50)]
        [Required]
        public string? ESTABNAME { get; set; }

        public double? InteractRating { get; set; }

        [Required]
        [StringLength(255)]
        public string? InteractComment { get; set; }
    }
}
