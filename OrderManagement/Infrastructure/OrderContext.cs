using Microsoft.EntityFrameworkCore;
using OrderManagement.Models;
using OrderManagement.Models.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderManagement.Infrastructure
{
    public class OrderContext : DbContext
    {
        public OrderContext(DbContextOptions<OrderContext> options)
            : base(options)
        {

        }
       
        public DbSet<Address> Addresses { get; set; }
        public DbSet<BillingInfo> BillingInfos { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderLine> OrderLines { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ShippingInfo> ShippingInfos { get; set; }

        public virtual void Commit()
        {
            base.SaveChanges();
        }
    }
}
