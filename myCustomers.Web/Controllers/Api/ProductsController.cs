using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using System.Web.Http;
using MaryKay.Configuration;
using myCustomers.Services;
using Quartet.Client.Products;
using Quartet.Entities.Products;

namespace myCustomers.Web.Controllers.Api
{
    [ApiAuthorizeConsultant]
    public class ProductsController : ApiController
    {
        readonly IAppSettings _appSettings;
        readonly IProductCatalogClientFactory _clientFactory;
        readonly ISubsidiaryAccessor _subsidiaryAccessor;
        readonly IInventoryService _inventoryService;
        readonly HashSet<string> _unavailableParts;

        public ProductsController
        (
            IAppSettings appSettings, 
            IProductCatalogClientFactory clientFactory,
            ISubsidiaryAccessor subsidiaryAccessor,
            IInventoryService inventoryService
        )
        {
            _appSettings        = appSettings;
            _clientFactory      = clientFactory;
            _subsidiaryAccessor = subsidiaryAccessor;
            _inventoryService   = inventoryService;

            var subsidiaryCode = _subsidiaryAccessor.GetSubsidiaryCode();
            _unavailableParts  = _inventoryService.GetUnavailableParts(subsidiaryCode);
        }

        // GET ~/api/products/{guid}
        [AcceptVerbs("GET")]
        public dynamic GetById(string id)
        {
            var client = _clientFactory.GetProductCatalogClient();

            var query = new ProductQuery
            {
                SearchTerms = id,
                SearchField = ProductQuery.SearchFields.ProductId,
                Page = 1,
                PageSize = 1
            };
            
            var result = client.Search(query);

            if (result.TotalCount > 0)
                return MapProduct(result.Products[0]);

            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        // GET ~/api/products?q=&cat=&i=&s=&cdsf=&cdsp=
        [AcceptVerbs("GET")]
        public dynamic Search
        (
            string q   = null, // search terms. product name, partid or display partid
            string cat = null, // category name. NOTE: this is an english value
            int i      = 1,    // page number
            int s      = 5,    // page size
            bool? cdsf = null, // cds free filter
            bool? cdsp = null  // cds paid filter
        )
        {
            var today = DateTime.Now.Date;
            var client = _clientFactory.GetProductCatalogClient();

            var query = new ProductQuery
            {
                SearchTerms               = q,
                CategoryName              = cat,
                SearchField               = ProductQuery.SearchFields.LimitedFields,
                IsCDSFree                 = cdsf,
                IsCDSPaid                 = cdsp,
                MaxConsultantStartDateUtc = today,
                MinExpirationDateUtc      = today.AddYears(_appSettings.GetValue<int>("ProductCatalog.MinExpirationDateUtc.YearOffset")),
                Page                      = i,
                PageSize                  = s
            };

            var currentCulture = Thread.CurrentThread.CurrentUICulture;
            try
            {
                var result = client.Search(query);

                return new
                {
                    totalCount = result.TotalCount,
                    products   = result.Products.Select(MapProduct).ToArray()
                };
            }
            finally
            {
                Thread.CurrentThread.CurrentUICulture = currentCulture;
            }
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

        dynamic MapProduct(Product p)
        {
            var today     = DateTime.Now.Date;
            var available = !_unavailableParts.Contains(p.PartId);
            var expired   = p.ExpirationDate.HasValue && p.ExpirationDate.Value <= today;

            return new
            {
                id              = p.ProductId,
                partId          = p.PartId,
                dispPartId      = p.DisplayPartId,
                name            = p.DisplayName,
                desc            = p.Description,
                shade           = p.ShadeName,
                price           = p.ListPrice,
                cdsFree         = p.CDSFree,
                isGift          = p.IsGift,
                isSample        = p.IsSample,
                isLimited       = p.IsLimitedEdition,
                useupRate       = p.UseUpRateInDays,
                availableForCDS = available && !expired,
                listImage       = MapImageUrl(p.HeroListImagePath)
            };
        }
    }
}
