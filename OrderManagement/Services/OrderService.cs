using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using OrderManagement.Exceptions;
using OrderManagement.Infrastructure;
using OrderManagement.Models;
using OrderManagement.Models.Business;
using OrderManagement.Repositories;

namespace OrderManagement.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderLineRepository _orderLineRepository;
        private readonly IClientRepository _clientRepository;
        private readonly ICurrencyRepository _currencyRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfigurationService _configurationService;
        private readonly ICompletionService _completionService;
        private readonly IProductRepository _productRepository;

        public OrderService(IOrderRepository orderRepository, IOrderLineRepository orderLineRepository, IClientRepository clientRepository, IAddressRepository addressRepository, ICurrencyRepository currencyRepository, IUnitOfWork unitOfWork, IConfigurationService configurationService, ICompletionService completionService, IProductRepository productRepository)
        {
            _orderRepository = orderRepository;
            _orderLineRepository = orderLineRepository;
            _clientRepository = clientRepository;
            _unitOfWork = unitOfWork;
            _currencyRepository = currencyRepository;
            _configurationService = configurationService;
            _completionService = completionService;
            _productRepository = productRepository;
        }

        public void Add(int orderId, string productCode, uint quantity)
        {
            using (var transaction = new TransactionScope())
            {
                var order = _orderRepository.Get(x => x.OrderId == orderId);
                if (order!=null)
                {
                    var orderLines = order.OrderLines ?? new List<OrderLine>();
                    var item =_productRepository.Get(x => x.Code == productCode);
                    if (item == null) { throw new ProductNotFoundException(); }
                    var productExists = orderLines.Where(x => x.Product.Code == productCode).Count() > 0;
                    if (productExists)
                    {
                        throw new ProductAlreadyExistsException();
                    }
                    else
                    {
                        orderLines.Add(new OrderLine() { Currency = order.Currency, Product = item, Quantity = quantity, UnitCost = item.Price, Timestamp = DateTime.UtcNow });
                        order.OrderLines = orderLines;
                    }
                }
                else
                {
                    throw new OrderNotFoundException();
                }

                order.Timestamp = DateTime.UtcNow;
                _orderRepository.Update(order);
                _unitOfWork.Commit();
                
                transaction.Complete();
            }
        }

        public void Complete(int orderId)
        {
            _completionService.Complete(orderId);
        }

        public int Initialize(Client client, Address shippingAddress, Address billingAddress, PaymentMethod paymentMethod, string currencyCode, decimal discountRate)
        {
            int result = 0;
            using (var transaction = new TransactionScope())
            {
                var existingClient = _clientRepository.Get(x => x.ClientCode == client.ClientCode);
                var currency = _currencyRepository.Get(x => x.CurrencyCode == currencyCode);
                if (currency == null) { throw new CurrencyNotFoundException(); }
                Order order = new Order()
                {
                    Client = existingClient ?? client,
                    BillingInfo = new BillingInfo() { Address = billingAddress, PaymentMethod = paymentMethod, Timestamp = DateTime.UtcNow},
                    Currency = currency,
                    DiscountRate = discountRate,
                    OrderStatus = OrderStatus.Initialized,
                    ShippingInfo = new ShippingInfo() { Address = shippingAddress, Timestamp = DateTime.UtcNow },
                    VatRate = _configurationService.GetVatRate(),
                    Timestamp = DateTime.UtcNow
                };

                _orderRepository.Add(order);
                _unitOfWork.Commit();
                result = order.OrderId;
                transaction.Complete();
            }
            return result;
        }

        public int Initialize(Client client, Address address, PaymentMethod paymentMethod, string currencyCode, decimal discountRate)
        {
            return Initialize(client, address, address, paymentMethod, currencyCode, discountRate);
        }

        public void Remove(int orderId, string productCode)
        {
            using (var transaction = new TransactionScope())
            {
                var order = _orderRepository.Get(x => x.OrderId == orderId);

                if (order == null)
                    throw new OrderNotFoundException();
                if (order.OrderLines == null)
                    throw new ProductNotFoundException();

                var productExists = order.OrderLines.Where(x => x.Product.Code == productCode).Count() > 0;

                if (productExists)
                {
                    order.OrderLines.Remove(order.OrderLines.Where(x=>x.Product.Code == productCode).Single());
                }
                else
                {
                    throw new ProductNotFoundException();
                }

                order.Timestamp = DateTime.UtcNow;
                _orderRepository.Update(order);
                _unitOfWork.Commit();

                transaction.Complete();
            }
        }

        public void SetQuantity(int orderId, string productCode, uint quantity)
        {
            if (quantity <= 0)
                throw new QuantityException();

            using (var transaction = new TransactionScope())
            {
                var order = _orderRepository.Get(x => x.OrderId == orderId);
                if (order != null && order.OrderLines!=null)
                {
                    var orderLine = order.OrderLines
                                         .Where(x => x.Product.Code == productCode)
                                         .SingleOrDefault();
                    if (orderLine == null) { throw new ProductNotFoundException(); }
                    orderLine.Quantity = quantity;
                }
                else
                {
                    throw new OrderNotFoundException();
                }
                _orderRepository.Update(order);
                _unitOfWork.Commit();

                transaction.Complete();
            }
        }

        public void ClearOut(int orderId)
        {
            using (var transaction = new TransactionScope())
            {
                var order = _orderRepository.Get(x => x.OrderId == orderId);
                if (order == null)
                    throw new OrderNotFoundException();
                if (order.OrderLines == null)
                    throw new ProductNotFoundException();

                foreach(var orderLine in order.OrderLines)
                {
                    _orderLineRepository.Delete(orderLine);
                }

                order.OrderLines.Clear();
                _orderRepository.Update(order);

                _unitOfWork.Commit();

                transaction.Complete();
            }
        }

        public Order GetOrder(int orderId)
        {
            return _orderRepository.Get(x => x.OrderId == orderId);
        }

        public IEnumerable<Order> GetOrders()
        {
            return _orderRepository.GetAll();
        }
    }
}
