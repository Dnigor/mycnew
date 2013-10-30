using myCustomers.Web.Models;
using Quartet.Entities;

namespace myCustomers.Web.Services
{
    public class HyperLinkViewModelMapper : IMappingService<HyperLink, HyperLinkViewModel>
    {
        public HyperLinkViewModel Map(HyperLink source)
        {
            if (source == null)
                return null;

            return new HyperLinkViewModel
                {
                    HyperLinkKey  = source.HyperLinkKey,
                    HyperLinkType = source.HyperLinkType,
                    Url           = source.Url.ToString()
                };
        }

       
    }
}