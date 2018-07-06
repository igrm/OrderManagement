using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OrderManagement.Models.Business
{
    public class Client
    {
        public int ClientId { get; set; }
        [Required]
        [MaxLength(255)]
        public string ClientCode { get; set; }
        [Required]
        [MaxLength(100)]
        [MinLength(2)]
        public string FirstName { get; set; }
        [Required]
        [MaxLength(100)]
        [MinLength(2)]
        public string LastName { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }
        [Required]
        public Gender Gender { get; set; }
        [Required]
        public virtual ICollection<Contact> Contacts { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
