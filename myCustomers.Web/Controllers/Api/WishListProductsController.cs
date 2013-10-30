using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using MaryKay.Configuration;
using myCustomers.Contexts;
using NLog;
using Quartet.Entities.Commands;
using Quartet.Entities.Products;

namespace myCustomers.Web.Controllers.Api
{
    [ApiAuthorizeConsultant]
    public class WishListProductsController : ApiController
    {
        IQuartetClientFactory _clientFactory;
        IConsultantContext _consultantContext;
        IAppSettings _appSettings;

        static Logger _logger = LogManager.GetCurrentClassLogger();

        public WishListProductsController
        (
            IQuartetClientFactory clientFactory,
            IConsultantContext consultantContext,
            IAppSettings appSettings
        )
        {
            _clientFactory     = clientFactory;
            _consultantContext = consultantContext;
            _appSettings       = appSettings;
        }

        // GET ~/api/customers/{custid}/wishlist
        [AcceptVerbs("GET")]
        public IEnumerable<dynamic> GetProducts(Guid custid)
        {
            var client = _clientFactory.GetCustomersQueryServiceClient();

            var favorites = client.GetFavoriteProducts(custid) ?? Enumerable.Empty<ProductListItem>();
            var wishList = client.GetWishlist(custid) ?? Enumerable.Empty<ProductListItem>();
            var result = wishList.Concat(favorites).GroupBy(p => p.ProductId).Select(g => MapProduct(g.First()));

            return result;
        }

        public class AddWishListProductModel
        {
            public string ProductId { get; set; }
        }

        [AcceptVerbs("POST")]
        public bool AddToWishList(Guid custid, AddWishListProductModel model)
        {
            if (model != null)
            {
                if (!ModelState.IsValid)
                    throw ApiHelpers.ServerError(ModelState);

                var queryClient = _clientFactory.GetCustomersQueryServiceClient();

                //REVIEW:Per Kevin: we persist to Favorites for wishlist and not wishlist.
                //But we must defensively check both lists before adding...
                var favorites = queryClient.GetFavoriteProducts(custid) ?? new ProductListItem[0];
                var wishList  = queryClient.GetWishlist(custid) ?? new ProductListItem[0];
                var result    = wishList.Concat(favorites).GroupBy(p => p.ProductId).Select(g => MapProduct(g.First()));

                if (!result.Any(p => p.id.Equals(model.ProductId)))
                {
                    var commandClient = _clientFactory.GetCommandServiceClient();
                    try
                    {
                        commandClient.Execute(new AddProductIdToFavorites 
                        { 
                            CustomerId = custid, 
                            ProductId = model.ProductId 
                        });
                        return true;
                    }
                    catch (CommandException cex)
                    {
                        throw ApiHelpers.ServerError(cex.Message, string.Join("\r\n", cex.Errors));
                    }
                }
            }
            return false;
        }

        [AcceptVerbs("DELETE")]
        public bool RemoveFromWishList(Guid custid, AddWishListProductModel model)
        {
            if (model != null)
            {
                if (!ModelState.IsValid)
                    throw ApiHelpers.ServerError(ModelState);

               var commandClient = _clientFactory.GetCommandServiceClient();
                    try
                    {
                        commandClient.Execute(new RemoveProductFromFavorites
                        {
                            CustomerId = custid,
                            ProductId = model.ProductId
                        });
                        return true;
                    }
                    catch (CommandException cex)
                    {
                        throw ApiHelpers.ServerError(cex.Message, string.Join("\r\n", cex.Errors));
                    }
                
            }
            return false;
        }

        dynamic MapProduct(ProductListItem p)
        {
            return new
            {
                id          = p.ProductId,
                partId      = p.Product.PartId,
                dispPartId  = p.Product.DisplayPartId,
                name        = p.Product.DisplayName,
                desc        = p.Product.Description,
                shade       = p.Product.ShadeName,
                price       = p.Product.ListPrice,
                cdsFree     = p.Product.CDSFree,
                isGift      = p.Product.IsGift,
                isSample    = p.Product.IsSample,
                isLimited   = p.Product.IsLimitedEdition,
                useupRate   = 0,  // p.UseUpRateInDays, // TODO: add useupRate when supported from endeca
                listImage   = MapImageUrl(p.Product.HeroListImagePath)
            };
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
    }
}
