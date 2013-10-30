using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Quartet.Entities;

namespace myCustomers.Web.Models
{
    public class CreateSalesTicketModel
    {
        public Guid CustomerId { get; set; }
        public DateTime OrderDateUtc { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public bool ShipCDS { get; set; }

        // products
        public LineItem[] Items { get; set; }

        // delivery
        public DateTime? DeliveryDateUtc { get; set; }
        public Address DeliveryAddress { get; set; }
        public bool IsGift { get; set; }
        public string GiftMessage { get; set; }

        // payment
        public OrderPaymentStatus PaymentStatus { get; set; }
        public CashPayment[] CashPayments { get; set; }
        public CheckPayment[] CheckPayments { get; set; }

        // notes
        public string Notes { get; set; }

        // followups
        public FollowUpItem[] Followups { get; set; }

        public bool AddDeliveryAddressToCustomer { get; set; }

        public class LineItem
        {
            public string Id { get; set; }
            public int Qty { get; set; }
            public int? UseupRate { get; set; }
            public decimal Price { get; set; }
        }

        public class CashPayment
        {
            public decimal Amount { get; set; }
            public DateTime PaymentDateUtc { get; set; }
        }

        public class CheckPayment
        {
            public string CheckNumber { get; set; }
            public decimal Amount { get; set; }
            public DateTime PaymentDateUtc { get; set; }
        }
    }

    public class SaveOrderModel
    {
        public DateTime OrderDateUtc { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public bool ShipCDS { get; set; }

        // products
        public LineItem[] Items { get; set; }

        // delivery
        public DateTime? DeliveryDateUtc { get; set; }
        public Address DeliveryAddress { get; set; }
        public bool IsGift { get; set; }
        public string GiftMessage { get; set; }

        // payment
        public OrderPaymentStatus PaymentStatus { get; set; }
        public CashPayment[] CashPayments { get; set; }
        public CheckPayment[] CheckPayments { get; set; }
        public HashSet<Guid> DeletedCCPaymentIds { get; set; }

        // notes
        public string Notes { get; set; }

        // followups
        public FollowUpItem[] Followups { get; set; }

        public bool AddDeliveryAddressToCustomer { get; set; }

        public class LineItem
        {
            public string Id { get; set; }
            public int Qty { get; set; }
            public int? UseupRate { get; set; }
            public decimal Price { get; set; }
        }

        public class CashPayment
        {
            public Guid PaymentId { get; set; }
            public decimal Amount { get; set; }
            public DateTime PaymentDateUtc { get; set; }
        }

        public class CheckPayment
        {
            public Guid PaymentId { get; set; }
            public string CheckNumber { get; set; }
            public decimal Amount { get; set; }
            public DateTime PaymentDateUtc { get; set; }
        }
    }

    public class OrderViewModel
    {
        // Order Header Info
        public Guid? OrderId { get; set; }
        public Guid CustomerId { get; set; }
        public DateTime? OrderDateUtc { get; set; }
        public OrderSource OrderSource { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsArchived { get; set; }
        public string ConfirmationNumber { get; set; }

        public string CustomerComment { get; set; }
        public string ConsultantComment { get; set; }
        public bool IsInterestedInGift { get; set; }
        public bool IsInterestedInSample { get; set; }
        public string OrderConfirmationPreference { get; set; }

        // products
        public LineItem[] Items { get; set; }
        public decimal EstimatedRetailTotal { get; set; }

        // delivery
        public CustomerDeliveryPreference? DeliveryPreference { get; set; }
        public string DeliveryPreferenceDetailIfOther { get; set; }
        public AddressViewModel DeliveryAddress { get; set; }
        public bool IsGift { get; set; }
        public string GiftMessage { get; set; }
        public DateTime? DeliveryDateUtc { get; set; }
        public bool ShipCDS { get; set; }

        // payment
        public OrderPaymentStatus PaymentStatus { get; set; }
        public CashPayment[] CashPayments { get; set; }
        public CheckPayment[] CheckPayments { get; set; }
        public CreditCardPayment[] CreditCardPayments { get; set; }

        public string Notes { get; set; }
        public FollowUpItem[] Followups { get; set; }

        public class LineItem
        {
            public string Id { get; set; }
            public string PartId { get; set; }
            public string DispPartId { get; set; }
            public string Name { get; set; }
            public string Shade { get; set; }
            public string Desc { get; set; }
            public int Qty { get; set; }
            public int? UseupRate { get; set; }
            public decimal Price { get; set; }
            public decimal Total { get; set; }
            public string ListImage { get; set; }
            public bool CDSFree { get; set; }
            public bool CDSPaid { get; set; }
            public bool AvailableForCDS { get; set; }
        }

        public class CashPayment
        {
            public Guid PaymentId { get; set; }
            public decimal Amount { get; set; }
            public DateTime PaymentDateUtc { get; set; }
        }

        public class CheckPayment
        {
            public Guid PaymentId { get; set; }
            public string CheckNumber { get; set; }
            public decimal Amount { get; set; }
            public DateTime PaymentDateUtc { get; set; }
        }

        public class CreditCardPayment
        {
            public Guid PaymentId { get; set; }
            public string InvoiceId { get; set; }
            public string TransactionId { get; set; }
            public CreditCardType Type { get; set; }
            public PaymentStatus Status { get; set; }
            public string CardHolderName { get; set; }
            public string BillingAddress { get; set; }
            public string BillingZip { get; set; }
            public string Last4Digits { get; set; }
            public int ExpMonth { get; set; }
            public int ExpYear { get; set; }
            public decimal Amount { get; set; }
            public decimal? PostAuthAmount { get; set; }
            public DateTime? PostAuthDateUtc { get; set; }
            public decimal? PreAuthAmount { get; set; }
            public DateTime? PreAuthDateUtc { get; set; }
            public DateTime? PaymentSettleDateUtc { get; set; }
            public DateTime? PaymentDateUtc { get; set; }
            public string ApprovalCode { get; set; }
            public string AVSResponseCode { get; set; }
            public string TransactionStatus { get; set; }
            public string ProPayLink { get; set; }

            public bool HasToken { get { return !string.IsNullOrWhiteSpace(this.Token); } }

            // Don't return the following properties to the client
            [JsonIgnore]
            public string ExpDate
            {
                get
                {
                    try
                    {
                        return new DateTime(this.ExpYear, this.ExpMonth, 1).ToString("MMM yyyy");
                    }
                    catch
                    {
                        return null;
                    }
                }
            }

            [JsonIgnore]
            public string Token { get; set; }
        }

        public DateTime MapFollowupDate(FollowUpPeriodUnit unit, int value)
        {
            var orderDate = this.OrderDateUtc ?? DateTime.UtcNow;

            switch (unit)
            {
                case FollowUpPeriodUnit.Days:
                    return orderDate.AddDays(value);
                case FollowUpPeriodUnit.Weeks:
                    return orderDate.AddDays(value * 7);
                case FollowUpPeriodUnit.Months:
                    return orderDate.AddMonths(value);
                case FollowUpPeriodUnit.Years:
                    return orderDate.AddYears(value);
                default:
                    return orderDate;
            }
        }
    }
}