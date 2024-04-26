using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore;

namespace Trabahub.Models
{
    public class DailyAnalytics
    {
        [Column(Order = 0)]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int? Id { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int? TotalUsers { get; set; }
    }
}
