using myCustomers.Web.Models;
using Quartet.Entities;

namespace myCustomers.Web.Services
{
    public class PhoneNumberViewModelMapper : IMappingService<PhoneNumber, PhoneNumberViewModel>
    {
        public PhoneNumberViewModel Map(PhoneNumber source)
        {
            if (source == null)
                return null;

            return new PhoneNumberViewModel
            {
                PhoneNumberKey = source.PhoneNumberKey,
                PhoneNumberType      = source.PhoneNumberType.ToString(),
                Number         = source.Number,
                Extension      = source.Extension,
                IsPrimary      = source.IsPrimary
            };
        }
    }
}