using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Trabahub.Models
{
    public class Listing
    {
        [Key]
        [Required]
        [StringLength(50)]
        public string? ESTABNAME { get; set; }
        
        [Required]
        [StringLength(255)]
        public string? ESTABDESC { get; set; }
       
        [Required]
        [StringLength(50)]
        public string? ESTABADD { get; set; }
        
        [Required]
        [StringLength(10)]
        public string? ESTABTIME { get; set; }

        //<---For Photo--->
        [StringLength(255)]
        public string? ESTABIMAGEPATH { get; set; }

        [NotMapped]
        public IFormFile? ESTABIMG { get; set; }
    }
}
