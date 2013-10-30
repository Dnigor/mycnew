using System;
using myCustomers.Features;
using myCustomers.Web.Models;

namespace myCustomers.Web.Services
{
    public class GroupDetailViewModelMapper : IMappingService<Group, GroupDetailViewModel>
    {
        private readonly IFeaturesConfigService _features;

        public GroupDetailViewModelMapper(IFeaturesConfigService features)
        {
            _features = features;
        }

        public GroupDetailViewModel Map(Group source)
        {
            return new GroupDetailViewModel
            {
                GroupId = source.GroupId,
                GroupName = source.GroupName,           
                Features = _features.GetGroupDetailFeatures()
            };
        }
    }
}