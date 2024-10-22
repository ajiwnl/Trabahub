﻿using System.ComponentModel.DataAnnotations.Schema;
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
        [StringLength(255)]
        public string? ESTABADD { get; set; }

        [Required]
        public int? ACCOMODATION { get; set; }

        public int? ESTABHRPRICE { get; set; }

        public int? ESTABDAYPRICE { get; set; }

        public int? ESTABWKPRICE { get; set; }

        public int? ESTABMONPRICE { get; set; }

        public string OwnerUsername { get; set; }

        public bool ListingStatus { get; set; }


        [Required]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:hh:mm tt}")]
        public DateTime STARTTIME { get; set; }

        [Required]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:hh:mm tt}")]
        public DateTime ENDTIME { get; set; }

        [Required]
        public double ESTABRATING { get; set; }

        //<---For Photo Verification--->
        [StringLength(255)]
        public string? ESTABIMAGEPATH { get; set; }

        [NotMapped]
        public IFormFile? ESTABIMG { get; set; }

        //<---For Photo Verificaiton--->
        [StringLength(255)]
        public string? VERIMAGEPATH1 { get; set; }

        [NotMapped]
        public IFormFile? VERIMG1 { get; set; }

        [StringLength(255)]
        public string? VERIMAGEPATH2 { get; set; }

        [NotMapped]
        public IFormFile? VERIMG2 { get; set; }

        [StringLength(255)]
        public string? VERIMAGEPATH3 { get; set; }

        [NotMapped]
        public IFormFile? VERIMG3 { get; set; }

        [StringLength(255)]
        public string? VERIMAGEPATH4 { get; set; }

        [NotMapped]
        public IFormFile? VERIMG4 { get; set; }

        [StringLength(255)]
        public string? VERIMAGEPATH5 { get; set; }

        [NotMapped]
        public IFormFile? VERIMG5 { get; set; }


    }

    public class ListingDetails
    {
        public Listing? Listing { get; set; }
        public List<ListInteraction>? Interactions { get; set; }
    }
}