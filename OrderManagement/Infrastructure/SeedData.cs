using Microsoft.Extensions.DependencyInjection;
using OrderManagement.Models.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderManagement.Infrastructure
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<OrderContext>();
            context.Database.EnsureCreated();

            var currency = new Currency() { CurrencyCode = "EUR", RoundingDecimals = 2, ShortSign = "€" };

            context.Currencies.Add(currency);

            context.Products.Add(new Product() { Code = "PRODUCT-1", Currency = currency, Name = "Product 1", Description = "Test", Price = 100, Timestamp = DateTime.UtcNow });
            context.Products.Add(new Product() { Code = "PRODUCT-2", Currency = currency, Name = "Product 2", Description = "Test", Price = 200, Timestamp = DateTime.UtcNow });

            context.SaveChanges();
        }
    }
}
