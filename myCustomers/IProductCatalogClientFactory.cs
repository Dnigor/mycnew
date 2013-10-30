using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quartet.Client.Products;

namespace myCustomers
{
    public interface IProductCatalogClientFactory
    {
        IProductCatalog GetProductCatalogClient();
    }
}
