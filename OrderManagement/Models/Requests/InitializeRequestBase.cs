using OrderManagement.Models.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderManagement.Models.Requests
{
    public abstract class InitializeRequestBase
    {
        public Client Client { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string CurrencyCode { get; set; }
        public decimal DiscountRate { get; set; }
    }
}
