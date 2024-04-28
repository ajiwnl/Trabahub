using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Trabahub.Models
{
    public class Credentials
    {
        [Key, Column(Order = 0)]
        [Required]
        [StringLength(255)]
        public string? Email { get; set; }

        [Required]
        [StringLength(15)]
        public string? Username { get; set; }

        [Required]
        [StringLength(255)]
        public string? Password { get; set; }


		[StringLength(255)]
		public string? VerificationCode { get; set; }

		[StringLength(255)]
		public string? UserType { get; set; }


        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime CreationDate { get; set; }


    }
}
