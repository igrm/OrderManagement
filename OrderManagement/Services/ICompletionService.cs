using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderManagement.Services
{
    public interface ICompletionService
    {
        void Complete(int orderId);
    }
}
