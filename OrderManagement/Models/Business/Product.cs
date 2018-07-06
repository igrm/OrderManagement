using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OrderManagement.Models.Business
{
    public class Product
    {
        public int ProductId { get; set; }
        [Required]
        [MaxLength(255)]
        public string Code { get; set; }
        [Required]
        [MaxLength(100)]
        [MinLength(2)]
        public string Name { get; set; }
        [Required]
        [MaxLength(1000)]
        [MinLength(2)]
        public string Description { get; set; }
        [Required]
        [Range(0, 1000000)]
        public decimal Price { get; set; }
        public int CurrencyId { get; set; }
        [Required]
        public virtual Currency Currency { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
