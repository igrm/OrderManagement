using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OrderManagement.Models.Business
{
    public class Address
    {
        public int AddressId { get; set; }
        [Required]
        [StringLength(2)]
        public string Country { get; set; }
        [Required]
        [MinLength(2)]
        [MaxLength(255)]
        public string City { get; set; }
        [Required]
        [MinLength(2)]
        [MaxLength(255)]
        public string State { get; set; }
        [Required]
        [MinLength(2)]
        [MaxLength(1000)]
        public string AddressLine { get; set; }
        [Required]
        [MinLength(2)]
        [MaxLength(255)]
        public string Zip { get; set; }
        public DateTime Timestamp;
    }
}
