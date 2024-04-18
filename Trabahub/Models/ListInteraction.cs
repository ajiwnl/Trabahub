using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore;

namespace Trabahub.Models
{
    [PrimaryKey(nameof(InteractId))]

    public class ListInteraction
    {
        [Column(Order = 0)]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int? InteractId { get; set; }

        public string? Username { get; set; }

		public string? OwnerUsername { get; set; }

		[StringLength(50)]
        [Required]
        public string? ESTABNAME { get; set; }

        public double? InteractRating { get; set; }
        [StringLength(255)]
        public string? InteractComment { get; set; }
    }
}
