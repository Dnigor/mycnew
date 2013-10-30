using Quartet.Entities.Views;
using myCustomers.Web.Models;

namespace myCustomers.Web.Services
{
    public class SampleRequestViewModelMapper : IMappingService<SampleRequest, SampleRequestViewModel>
    {
        public SampleRequestViewModel Map(SampleRequest source)
        {
            return new SampleRequestViewModel
            {
                SampleRequestId    = source.SampleRequestId,
                ConsultantKey      = source.ConsultantKey,
                RequestDateUtc     = source.RequestDateUtc,
                DeliveryDateUtc    = source.DeliveryDateUtc,
                IsCompleted        = source.IsCompleted,
                ProductId          = source.ProductId,
                Email              = source.Email,
                FirstName          = source.FirstName,
                LastName           = source.LastName,
                Street             = source.Street,
                City               = source.City,
                StateProvince      = source.StateProvince,
                PostalCode         = source.PostalCode,
                PhoneNumber        = source.PhoneNumber,
                FollowUpTaskId     = source.FollowUpTaskId,
                PartId             = source.PartId,
                DisplayPartId      = source.DisplayPartId,
                ProductName        = source.ProductName,
                ProductDescription = source.ProductDescription,
                ProuctImageUrl     = source.ProuctImageUrl,
                Formula            = source.Formula,
                ShadeName          = source.ShadeName
            };

        }
    }
}