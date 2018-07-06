using OrderManagement.Models.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderManagement.Models.Requests
{
    public class InitializeSameAddressRequest: InitializeRequestBase
    {
        public Address Address { get; set; }
    }
}
