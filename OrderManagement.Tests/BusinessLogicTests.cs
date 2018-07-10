using OrderManagement.Services;
using OrderManagement.Repositories;
using OrderManagement.Infrastructure;
using System;
using Xunit;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Models;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Linq;
using OrderManagement.Models.Business;

namespace OrderManagement.Tests
{
    public class BusinessLogicTests
    {
        IOrderService _orderService;

        public BusinessLogicTests()
        {
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            var config = configurationBuilder.SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
            var optionsBuilder = new DbContextOptionsBuilder<OrderContext>();
            optionsBuilder.UseInMemoryDatabase("OrderManagement").UseLazyLoadingProxies();
            var context = new OrderContext(optionsBuilder.Options);
            SeedTestData(context);
            _orderService = new OrderService(
                                 new OrderRepository(context),
                                 new OrderLineRepository(context),
                                 new ClientRepository(context),
                                 new AddressRepository(context),
                                 new CurrencyRepository(context),
                                 new UnitOfWork(context),
                                 new ConfigurationService(config),
                                 new CompletionService(new OrderRepository(context)),
                                 new ProductRepository(context));
        }

        private void SeedTestData(OrderContext orderContext)
        {
            var currency = new Currency() { CurrencyCode = "EUR", RoundingDecimals = 2, ShortSign = "€", Timestamp = DateTime.UtcNow };
            orderContext.Currencies.Add(currency);
            orderContext.Products.Add(new Product() { Code = "PRODUCT-1", Currency = currency, Name = "Product 1", Description = "Test", Price = 100, Timestamp = DateTime.UtcNow });
            orderContext.Products.Add(new Product() { Code = "PRODUCT-2", Currency = currency, Name = "Product 2", Description = "Test", Price = 200, Timestamp = DateTime.UtcNow });
            orderContext.SaveChanges();
        }

        [Fact]
        public void Ininitalize_Test()
        {
            var id = _orderService.Initialize(new Client() { ClientCode = "CLIENT-1" }, new Address() { Country = "EE" }, PaymentMethod.WireTransfer, "EUR", 0.10m);
            Assert.NotEqual(0, id);
        }

        [Fact]
        public void Add_Test()
        {
            var id = _orderService.Initialize(new Client() { ClientCode = "CLIENT-1" }, new Address() { Country = "EE" }, PaymentMethod.WireTransfer, "EUR", 0.10m);
            _orderService.Add(id, "PRODUCT-1", 10);
            var order = _orderService.GetOrder(id);
            Assert.Equal(1, order.OrderLines.Count);
        }

        [Fact]
        public void Remove_Test()
        {
            var id = _orderService.Initialize(new Client() { ClientCode = "CLIENT-1" }, new Address() { Country = "EE" }, PaymentMethod.WireTransfer, "EUR", 0.10m);
            _orderService.Add(id, "PRODUCT-1", 10);
            _orderService.Remove(id, "PRODUCT-1");
            var order = _orderService.GetOrder(id);
            Assert.Equal(0, order.OrderLines.Count);
        }

        [Fact]
        public void SetQuantity_Test()
        {
            var id = _orderService.Initialize(new Client() { ClientCode = "CLIENT-1" }, new Address() { Country = "EE" }, PaymentMethod.WireTransfer, "EUR", 0.10m);
            _orderService.Add(id, "PRODUCT-1", 10);
            _orderService.SetQuantity(id, "PRODUCT-1", 100);
            var order = _orderService.GetOrder(id);
            Assert.Equal(100, order.OrderLines.Sum(x => x.Quantity));
        }

        [Fact]
        public void ClearOut_Test()
        {
            var id = _orderService.Initialize(new Client() { ClientCode = "CLIENT-1" }, new Address() { Country = "EE" }, PaymentMethod.WireTransfer, "EUR", 0.10m);
            _orderService.Add(id, "PRODUCT-1", 10);
            _orderService.SetQuantity(id, "PRODUCT-1", 100);
            _orderService.ClearOut(id);
            var order = _orderService.GetOrder(id);
            Assert.Empty(order.OrderLines);
        }
    }
}
