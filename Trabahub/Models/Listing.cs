using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

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
        public int? ESTABPRICE { get; set; }

        [Required]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:hh:mm tt}")]
        public DateTime STARTTIME { get; set; }

        [Required]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:hh:mm tt}")]
        public DateTime ENDTIME { get; set; }


        //<---For Photo Verification--->
        [StringLength(255)]
        public string? ESTABIMAGEPATH { get; set; }

        [NotMapped]
        public IFormFile? ESTABIMG { get; set; }

        //<---For Photo Verificaiton--->
        [StringLength(255)]
        public string? VERIMAGEPATH { get; set; }

        [NotMapped]
        public IFormFile? VERIMG { get; set; }
    }
}
