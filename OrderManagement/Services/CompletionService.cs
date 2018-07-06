using OrderManagement.Exceptions;
using OrderManagement.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderManagement.Services
{
    public class CompletionService : ICompletionService
    {
        private readonly IOrderRepository _orderRepository;

        public CompletionService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public void Complete(int orderId)
        {
            var order = _orderRepository.Get(x => x.OrderId == orderId);
            if (order == null)
                throw new OrderNotFoundException();
            throw new NotImplementedException();
        }
    }
}
