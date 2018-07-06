using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderManagement.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderManagement.Tests
{
    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<OrderManagement.Startup>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var serviceProvider = new ServiceCollection().AddEntityFrameworkInMemoryDatabase()
                                                             .AddEntityFrameworkProxies()
                                                             .BuildServiceProvider();

                services.AddDbContext<OrderContext>(options =>
                {
                    options.UseInMemoryDatabase("OrderManagement")
                           .UseLazyLoadingProxies();
                    options.UseInternalServiceProvider(serviceProvider);
                });
                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    SeedData.Initialize(scope.ServiceProvider);
                }
            });
        }
    }
}
