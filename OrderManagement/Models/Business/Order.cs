using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OrderManagement.Models.Business
{
    public class Order
    {
        public int OrderId { get; set; }
        public int ClientId { get; set; }
        [Required]
        public virtual Client Client { get; set; }
        public int ShippingInfoId { get; set; }
        [Required]
        public virtual ShippingInfo ShippingInfo { get; set; }
        public int BillingInfoId { get; set; }
        [Required]
        public virtual BillingInfo BillingInfo { get; set; }
        [Required]
        public virtual ICollection<OrderLine> OrderLines { get; set; }
        public int CurrencyId { get; set; }
        [Required]
        public virtual Currency Currency { get; set; }
        public decimal OrderAmount { get { return OrderLines.Sum(line => line.SubTotal); } }
        public decimal VatRate { get; set; }
        public decimal Vat { get { return Math.Round((OrderAmount- Discount) * VatRate, Currency.RoundingDecimals); } }
        [Required]
        public decimal DiscountRate { get; set; }
        public decimal Discount { get { return Math.Round(OrderAmount * DiscountRate, Currency.RoundingDecimals); } }
        public decimal Total { get { return (OrderAmount - Discount) + Vat; } }
        [Required]
        public OrderStatus OrderStatus { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
