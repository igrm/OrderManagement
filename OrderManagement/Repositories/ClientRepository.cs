﻿using OrderManagement.Infrastructure;
using OrderManagement.Models;
using OrderManagement.Models.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderManagement.Repositories
{
    public class ClientRepository: RepositoryBase<Client>, IClientRepository
    {
        public ClientRepository(OrderContext orderContext):base(orderContext)
        {

        }
    }
}
