using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using myCustomers.Contexts;
using Quartet.Entities;
using System.Web;
using MaryKay.Configuration;

namespace myCustomers.Web.Controllers.Api
{
    [ApiAuthorizeConsultant]
    public class RecommendedProductsController : ApiController
    {
        IQuartetClientFactory _clientFactory;
        IConsultantContext _consultantContext;
        IAppSettings _appSettings;

        public RecommendedProductsController
        (
            IQuartetClientFactory clientFactory,
            IConsultantContext consultantContext,
            IAppSettings appSettings
        )
        {
            _clientFactory     = clientFactory;
            _consultantContext = consultantContext;
            _appSettings = appSettings;
        }

        // GET ~/api/customers/{custid}/recommendedproducts
        public IEnumerable<dynamic> GetProducts(Guid custid)
        {
            var client = _clientFactory.GetCustomersQueryServiceClient();

            var recommendations = client.QueryRecommendationsByCustomerId(custid);

            if (recommendations == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            var result =
                from r in recommendations
                select new
                {
                    cat = r.CategoryName,
                    products = r.Products.Select(p => MapProduct(p)).ToArray()
                };

            return result;
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

        dynamic MapProduct(ProductRecommendation p)
        {
            return new
            {
                id          = p.ProductId,
                partId      = p.PartId,
                dispPartId  = p.DisplayPartId,
                name        = p.ProductName,
                desc        = p.ProductDescription,
                shade       = p.ShadeName,
                price       = p.ListPrice,
                useupRate   = 0,  // p.UseUpRateInDays, // TODO: add useupRate when supported from endeca
                listImage   = MapImageUrl(p.ProductImageUrl)
            };
        }
    }
}
