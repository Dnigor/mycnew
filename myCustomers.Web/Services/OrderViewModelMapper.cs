using System;
using System.Linq;
using System.Web;
using MaryKay.Configuration;
using myCustomers.Features;
using myCustomers.Globalization;
using myCustomers.Services;
using myCustomers.Web.Models;
using Quartet.Entities;

namespace myCustomers.Web.Services
{
    public class OrderViewModelMapper : IMappingService<Order, OrderViewModel>
    {
        readonly IAppSettings                               _appSettings;
        readonly IProductCatalogClientFactory               _productCatalogFactory;
        readonly IMappingService<Address, AddressViewModel> _addressMapping;
        readonly IInventoryService                          _inventoryService;
        readonly ISubsidiaryAccessor                        _subsidiaryAccessor;

        public OrderViewModelMapper
        (
            IAppSettings appSettings,
            IProductCatalogClientFactory productCatalogFactory,
            IMappingService<Address, AddressViewModel> addressMapping,
            IFeaturesConfigService featuresConfigService,
            IInventoryService inventoryService,
            ISubsidiaryAccessor subsidiaryAccessor
        )
        {
            _appSettings           = appSettings;
            _productCatalogFactory = productCatalogFactory;
            _addressMapping        = addressMapping;
            _inventoryService      = inventoryService;
            _subsidiaryAccessor    = subsidiaryAccessor;
        }

        public OrderViewModel Map(Order order)
        {
            if (order == null)
                return null;

            if (order.Items == null)
                order.Items = new OrderItem[] { };

            if (order.Payments == null)
                order.Payments = new Payment[] { };

            var orderItems = order.Items
                .Select
                (
                    i => new OrderViewModel.LineItem 
                    { 
                        Id           = i.ProductId ?? string.Empty, 
                        Qty          = i.Quantity, 
                        Price        = i.Price, 
                        Total        = i.LineTotal,
                        CDSFree      = i.CDSFree,
                        CDSPaid      = i.CDSPaid,
                        UseupRate    = i.UseUpRateInDays
                    }
                )
                .ToDictionary(i => i.Id);

            if (orderItems.Count > 0)
            {
                var today            = DateTime.Now.Date;
                var subsidiaryCode   = _subsidiaryAccessor.GetSubsidiaryCode();
                var unavailableParts = _inventoryService.GetUnavailableParts(subsidiaryCode);
                var productCatalog   = _productCatalogFactory.GetProductCatalogClient();

                var res = productCatalog.GetByProductIds(orderItems.Values.Select(i => i.Id).ToArray());
                foreach (var product in res.Products)
                {
                    var available = !unavailableParts.Contains(product.PartId);
                    var expired   = product.ExpirationDate.HasValue && product.ExpirationDate.Value <= today;

                    OrderViewModel.LineItem item = null;
                    if (orderItems.TryGetValue(product.ProductId, out item))
                    {
                        item.PartId          = product.PartId;
                        item.DispPartId      = product.DisplayPartId;
                        item.Name            = product.DisplayName;
                        item.Desc            = product.Description;
                        item.Shade           = product.ShadeName;
                        item.ListImage       = MapProductImageUrl(product.HeroListImagePath);
                        item.AvailableForCDS = available && !expired;
                    }
                }
            }
            
            var deliveryAddress = order.ShippingAddress != null ? new AddressViewModel
            {
                AddressKey  = order.ShippingAddress.AddressKey,
                Street      = order.ShippingAddress.Street,
                UnitNumber  = order.ShippingAddress.UnitNumber,
                City        = order.ShippingAddress.City,
                RegionCode  = order.ShippingAddress.RegionCode,
                PostalCode  = order.ShippingAddress.PostalCode,
                CountryCode = order.ShippingAddress.CountryCode
            } : null;

            var cashPayments = order
                .Payments
                .Where(p => p.PaymentType == PaymentType.Cash)
                .Select
                (
                    p => new OrderViewModel.CashPayment
                    {
                        PaymentId      = p.PaymentId,
                        PaymentDateUtc = p.PaymentDateUtc ?? DateTime.UtcNow,
                        Amount         = p.AmountPaid ?? 0.0m
                    }
                )
                .ToArray();

            var checkPayments = order
                .Payments
                .Where(p => p.PaymentType == PaymentType.PersonalCheck)
                .Select
                (
                    p => new OrderViewModel.CheckPayment
                    {
                        PaymentId      = p.PaymentId,
                        PaymentDateUtc = p.PaymentDateUtc ?? DateTime.UtcNow,
                        CheckNumber    = p.CheckNumber,
                        Amount         = p.AmountPaid ?? 0.0m
                    }
                )
                .ToArray();
            
            var ccPayments = order
                .Payments
                .Where(p => p.PaymentType == PaymentType.CreditCard)
                .OrderBy(p => p.TransactionId)
                .Select
                (
                    p => new OrderViewModel.CreditCardPayment
                    {
                        PaymentId            = p.PaymentId,
                        Status               = p.PaymentStatus,
                        PaymentDateUtc       = p.PaymentDateUtc ?? DateTime.UtcNow,
                        Type                 = p.CreditCard != null ? p.CreditCard.Type : CreditCardType.Unknown,
                        CardHolderName       = p.CreditCard != null ? p.CreditCard.CardHolderName : null,
                        BillingAddress       = p.BillingAddress != null ? p.BillingAddress.Street : null,
                        BillingZip           = p.BillingAddress != null ? p.BillingAddress.PostalCode : null,
                        Last4Digits          = p.CreditCard != null ? p.CreditCard.Last4Digits : null,
                        ExpMonth             = p.CreditCard != null ? p.CreditCard.ExpMonth : 0,
                        ExpYear              = p.CreditCard != null ? p.CreditCard.ExpYear : 0,
                        Token                = p.CreditCard != null ? p.CreditCard.Token : null,
                        ApprovalCode         = p.ApprovalCode,
                        AVSResponseCode      = p.AVSResponseCode,
                        TransactionId        = p.TransactionId,
                        TransactionStatus    = p.TransactionStatus,
                        InvoiceId            = p.InvoiceId ?? order.OrderId.ToString().ToUpper().Substring(1,10), // NOTE: payments entered in the previous version of the app did not store an invoice id and used this format.
                        PreAuthAmount        = p.PreAuthAmount,
                        PreAuthDateUtc       = p.PreAuthDateUtc,
                        PostAuthAmount       = p.PostAuthAmount,
                        PostAuthDateUtc      = p.PostAuthDateUtc,
                        PaymentSettleDateUtc = p.PaymentSettleDateUtc,
                        ProPayLink           = string.Format(_appSettings.GetValue("ProPay.TransactionDetailsLink"), p.TransactionId),
                        Amount               = p.AmountPaid ?? 0.0m
                    }
                )
                .ToArray();

            return new OrderViewModel
            {
                OrderId                         = order.OrderId,
                CustomerId                      = order.CustomerId,
                OrderDateUtc                    = order.OrderDateUtc,
                OrderSource                     = order.OrderSource,
                OrderStatus                     = order.OrderStatus,
                IsDeleted                       = order.IsDeleted,
                IsArchived                      = order.IsArchived,             
                PaymentStatus                   = order.OrderPaymentStatus,
                ConfirmationNumber              = order.ConfirmationNumber.ToUpper(),
                CustomerComment                 = order.CustomerComment,
                ConsultantComment               = order.ConsultantComment,
                IsInterestedInGift              = order.InterstedInGWP ?? false,
                IsInterestedInSample            = order.InterestedInFreeSample ?? false,
                Items                           = orderItems.Values.ToArray(),
                EstimatedRetailTotal            = order.EstimatedOrderAmount,
                DeliveryPreference              = order.CustomerDeliveryPreference,
                DeliveryPreferenceDetailIfOther = string.IsNullOrWhiteSpace(order.CustomerDeliveryPreferenceDetailIfOther) ? Resources.GetString("DELIVERYPREFERENCE_OTHER") : order.CustomerDeliveryPreferenceDetailIfOther,
                DeliveryAddress                 = _addressMapping.Map(order.ShippingAddress),
                IsGift                          = order.GiftMessage != null ? order.GiftMessage.IsGift : false,
                GiftMessage                     = order.GiftMessage != null ? order.GiftMessage.Message : null,
                DeliveryDateUtc                 = order.DeliveryDateUtc,
                CashPayments                    = cashPayments,
                CheckPayments                   = checkPayments,
                CreditCardPayments              = ccPayments,
                Notes                           = order.PrivateOrderNote,
                Followups                       = order.FollowUps ?? new FollowUpItem[] {},
                ShipCDS                         = order.ShipCDS ?? false
            };
        }

        string MapProductImageUrl(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return VirtualPathUtility.ToAbsolute(_appSettings.GetValue("ProductCatalog.DefaultImageUrl"));

            try
            {
                var baseUrl = new Uri(HttpContext.Current.Request.Url, _appSettings.GetValue("ProductCatalog.BaseImageUrl"));
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