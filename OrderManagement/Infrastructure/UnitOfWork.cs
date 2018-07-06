using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderManagement.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly OrderContext _dbContext;

        public OrderContext DbContext
        {
            get { return _dbContext; }
        }

        public UnitOfWork(OrderContext orderContext)
        {
            _dbContext = orderContext;
        }

        public void Commit()
        {
            DbContext.Commit();
        }
    }
}
