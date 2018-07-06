using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OrderManagement.Models.Business
{
    public class ShippingInfo
    {
        public int ShippingInfoId { get; set; }
        public int AddressId { get; set; }
        [Required]
        public virtual Address Address { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
