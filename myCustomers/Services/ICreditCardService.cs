using System;

namespace myCustomers.Services
{
    public interface ICreditCardService
    {
        Guid CreateToken(string ccNumber, DateTime ccExpirationDate);
        AuthorizePaymentResult AuthorizePayment(long account, string invoiceNumber, decimal amount, Guid ccToken, string ccAddress, string ccZip, DateTime ccExpirationDate);
    }

    public sealed class NullCreditCardService : ICreditCardService
    {
        public Guid CreateToken(string ccNumber, DateTime ccExpirationDate)
        {
            return Guid.Empty;
        }

        public AuthorizePaymentResult AuthorizePayment(long account, string invoiceNumber, decimal amount, Guid ccToken, string ccAddress, string ccZip, DateTime ccExpirationDate)
        {
            return null;
        }
    }

    public class AuthorizePaymentResult
    {
        public AuthorizePaymentResult(string authCode, long transactionNumber, int transactionStatus, string avsResponseCode)
        {
            this.AuthCode          = authCode;
            this.TransactionNumber = transactionNumber;
            this.TransactionStatus = transactionStatus;
            this.AVSResponseCode   = avsResponseCode;
        }

        public string AuthCode { get; private set; }
        public long TransactionNumber { get; private set; }
        public int TransactionStatus { get; private set; }
        public string AVSResponseCode { get; private set; }
    }
}
