using OrderManagement.Models;
using OrderManagement.Models.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderManagement.Services
{
    public interface IOrderService
    {
        int Initialize(Client client, Address shippingAddress, Address billingAddress, PaymentMethod paymentMethod, string currencyCode, decimal discountRate);
        int Initialize(Client client, Address address, PaymentMethod paymentMethod, string currencyCode, decimal discountRate);
        void Add(int orderId, string productCode, uint quantity);
        void Remove(int orderId, string productCode);
        void SetQuantity(int orderId, string productCode, uint quantity);
        void Complete(int orderId);
        void ClearOut(int orderId);
        Order GetOrder(int orderId);
        IEnumerable<Order> GetOrders();
        
    }
}
