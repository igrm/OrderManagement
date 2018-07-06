using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OrderManagement.Models.Business
{
    public class OrderLine
    {
        public int OrderLineId { get; set; }
        public int OrderId { get; set; }
        public virtual Order Order { get; set; }
        public int ProductId { get; set; }
        [Required]
        public virtual Product Product { get; set; }
        [Required]
        [Range(0, 10000)]
        public uint Quantity { get; set; }
        [Required]
        [Range(0, 1000000)]
        public decimal UnitCost { get; set; }
        public decimal SubTotal { get { return Quantity * UnitCost; } }
        public int CurrencyId { get; set; }
        [Required]
        public virtual Currency Currency { get; set; }
        public DateTime Timestamp;
    }
}
