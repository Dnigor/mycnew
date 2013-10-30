using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using MaryKay.Configuration;
using myCustomers.Contexts;
using Quartet.Client.Products;
using Quartet.Entities.Products;
using Quartet.Entities.Views;

namespace myCustomers.Web.Controllers.Api
{
    [ApiAuthorizeConsultant]
    public class RecentlyOrderedProductsController : ApiController
    {
        IAppSettings _appSettings;
        IProductCatalogClientFactory _productCatalogClientFactory;
        IQuartetClientFactory _clientFactory;
        IConsultantContext _consultantContext;

        public RecentlyOrderedProductsController
        (
            IAppSettings appSettings,
            IProductCatalogClientFactory productCatalogClientFactory,
            IQuartetClientFactory clientFactory,
            IConsultantContext consultantContext
        )
        {
            _appSettings                 = appSettings;
            _productCatalogClientFactory = productCatalogClientFactory;
            _clientFactory               = clientFactory;
            _consultantContext           = consultantContext;
        }

        // GET ~/api/customers/{custid}/recentlyorderedproducts
        public IEnumerable<dynamic> GetProducts(Guid custid)
        {
            var productsClient = _productCatalogClientFactory.GetProductCatalogClient();
            var client = _clientFactory.GetCustomersQueryServiceClient();
            var orders = client.GetCustomerOrders(custid, false, false) ?? Enumerable.Empty<OrderListItem>();
            
            var products =
                (
                    from o in orders
                    from i in o.ProductsOrdered
                    select new { i.ProductId, o.OrderDateUtc }
                )
                .ToLookup(p => p.ProductId);

            var details = GetProductDetails(productsClient, products.Select(g => g.Key));

            return details.Select(p => MapProduct(p, products[p.ProductId].Max(od => od.OrderDateUtc))).OrderByDescending(p => p.lastOrderDateUtc);
        }

		Product[] GetProductDetails(IProductCatalog client, IEnumerable<string> productIds)
		{
			if (productIds == null)
				throw new ArgumentNullException("productIds");

            if (!productIds.Any())
                return new Product[] { };
				
            var products = productIds
                .Chunk(10)
                .SelectMany
                (
                    chunk =>
                    {
                        var query = new ProductQuery
                        {
                            Page        = 1,
                            PageSize    = int.MaxValue,
                            SearchTerms = string.Join(" ", chunk.ToArray()),
                            SearchField = ProductQuery.SearchFields.ProductId
                        };

                        return client.Search(query).Products;
                    }
                )
                .ToArray();

            return products;
		}

        string MapImageUrl(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return VirtualPathUtility.ToAbsolute(_appSettings.GetValue("ProductCatalog.DefaultImageUrl"));

            try
            {
                var baseUrl = new Uri(Request.RequestUri, _appSettings.GetValue("ProductCatalog.BaseImageUrl"));
                var url = new Uri(baseUrl, imageUrl);
                return url.ToString();
            }
            catch
            {
                return null;
            }
        }

        dynamic MapProduct(Product p, DateTime? orderDateUtc)
        {
            return new
            {
                id               = p.ProductId,
                partId           = p.PartId,
                dispPartId       = p.DisplayPartId,
                name             = p.DisplayName,
                desc             = p.Description,
                shade            = p.ShadeName,
                price            = p.ListPrice,
                cdsFree          = p.CDSFree,
                isGift           = p.IsGift,
                isSample         = p.IsSample,
                isLimited        = p.IsLimitedEdition,
                useupRate        = 0,  // p.UseUpRateInDays, // TODO: add useupRate when supported from endeca
                listImage        = MapImageUrl(p.HeroListImagePath),
                lastOrderDateUtc = orderDateUtc
            };
        }
    }
}
