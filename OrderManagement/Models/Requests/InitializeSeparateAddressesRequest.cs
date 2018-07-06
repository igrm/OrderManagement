using OrderManagement.Models.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderManagement.Models.Requests
{
    public class InitializeSeparateAddressesRequest: InitializeRequestBase
    {
       public Address ShippingAddress { get; set; }
       public Address BillingAddress { get; set; }
    }
}
