using System.Globalization;
using myCustomers.Features;
using Quartet.Entities;

namespace myCustomers.Web.Models
{
    public class OrderDetailViewModel
    {
        public HCardViewModel Customer { get; set; }
        public OrderViewModel Order { get; set; }
        public OrderFeatures Features { get; set; }
        public string CurrencyFormat { get; set; }

        public bool CanShipCDS
        {
            get
            {
                var order = this.Order;
                var payments = order.CreditCardPayments;
                var validStatus = order.OrderStatus != OrderStatus.Processed && order.OrderStatus != OrderStatus.ShippedDelivered;

                var validParts = true; // TODO: validation parts

                var validPayment =
                    payments == null ||
                    payments.Length == 0 ||
                    (
                        payments.Length == 1 &&
                        payments[0].Status != PaymentStatus.Approved &&
                        payments[0].Status != PaymentStatus.Declined &&
                        payments[0].Status != PaymentStatus.Error
                    );

                return validStatus && validParts && validPayment;
            }
        }

        public bool ShowCustomerDirections 
        { 
            get 
            {
                return 
                    this.Order.OrderSource == OrderSource.Online && 
                    (
                        !string.IsNullOrWhiteSpace(this.Order.CustomerComment) || 
                        this.Order.IsInterestedInGift || this.Order.IsInterestedInSample || 
                        this.Order.DeliveryPreference.HasValue || this.Order.IsGift
                    );
            } 
        }
    }
}