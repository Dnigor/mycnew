using System;
using System.Linq;
using Quartet.Client.Products;
using Quartet.Entities.Products;

namespace myCustomers
{
    public static class ExtendIProductCatalog
    {
        public static SearchResult GetByProductIds(this IProductCatalog client, params string[] productIds)
        {
            if (productIds == null)
                throw new ArgumentNullException("productIds");

            if (productIds.Length == 0)
                return new SearchResult
                {
                    TotalCount = 0,
                    Products = new Product[] { }
                };

            var products = productIds
                .Chunk(10)
                .SelectMany
                (
                    chunk =>
                    {
                        var query = new ProductQuery
                        {
                            Page = 1,
                            PageSize = int.MaxValue,
                            SearchTerms = string.Join(" ", chunk.ToArray()),
                            SearchField = ProductQuery.SearchFields.ProductId
                        };

                        return client.Search(query).Products;
                    }
                )
                .ToArray();

            return new SearchResult
            {
                TotalCount = products.Length,
                Products = products
            };
        }
    }
}
