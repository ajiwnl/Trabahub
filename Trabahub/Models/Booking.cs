using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Trabahub.Models
{

    public class Booking
    {
        [Column(Order = 0)]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        [Required]
        public string? ESTABNAME { get; set; }

        [Required]
        public string? Username { get; set; }

        [Required]
        public string? PriceRate { get; set; }

        [Required]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:hh:mm tt}")]
        public DateTime STARTTIME { get; set; }

        [Required]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:hh:mm tt}")]
        public DateTime ENDTIME { get; set; }

        [Required]
        public string? SelectedOption { get; set; }

        [Required]
        public DateTime DynamicDate { get; set; }

        [Required]
		public string? Status { get; set; }

	}
}
