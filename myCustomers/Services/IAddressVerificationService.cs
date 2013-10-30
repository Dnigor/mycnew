namespace myCustomers.Services
{
    public enum AddressVerificationState
    {
        NotFound,
        ExactMatch,
        RecommendChange
    }

    public class AddressVerificationResult
    {
        public AddressVerificationState VerificationState { get; set; }
        public Address RecommendedAddress { get; set; }

        public class Address
        {
            public string Street { get; set; }
            public string City { get; set; }
            public string RegionCode { get; set; }
            public string PostalCode { get; set; }
        }
    }

    public class NullAddressVerificationService : IAddressVerificationService
    {
        public AddressVerificationResult VerifyAddress(string street, string city, string regionCode, string postalCode)
        {
            return new AddressVerificationResult
            {
                VerificationState = AddressVerificationState.ExactMatch,
                RecommendedAddress = new AddressVerificationResult.Address
                {
                    Street     = street,
                    City       = city,
                    RegionCode = regionCode,
                    PostalCode = postalCode
                }
            };
        }
    }

    public interface IAddressVerificationService
    {
        AddressVerificationResult VerifyAddress(string street, string city, string regionCode, string postalCode);
    }
}
