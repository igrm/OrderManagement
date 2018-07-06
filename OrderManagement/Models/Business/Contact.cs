using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OrderManagement.Models.Business
{
    public class Contact
    {
        public int ContactId { get; set; }
        [Required]
        public ContactType ContactType { get; set; }
        [Required]
        public string Value { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
