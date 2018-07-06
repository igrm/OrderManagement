using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderManagement.Infrastructure
{
    public interface IUnitOfWork
    {
        void Commit();
    }
}
