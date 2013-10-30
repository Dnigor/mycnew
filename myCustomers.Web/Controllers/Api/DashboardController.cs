using System;
using System.Web.Http;
using MaryKay.Configuration;
using myCustomers.Web.Models;
using myCustomers.Web.Services;
using Quartet.Entities.Views;
using Quartet.Services.Contracts;

namespace myCustomers.Web.Controllers.Api
{
    [ApiAuthorizeConsultant]
    public class DashboardController : ApiController
    {
        IQuartetClientFactory _clientFactory;
        IAppSettings _appSettings;
        IMappingService<NotificationCounts, DashboardViewModel> _dashboardMapping;

        public DashboardController
        (
            IQuartetClientFactory clientFactory,
            IAppSettings appSettings,
            IMappingService<NotificationCounts, DashboardViewModel> dashboardMapping
        )
        {
            _clientFactory    = clientFactory;
            _appSettings      = appSettings;
            _dashboardMapping = dashboardMapping;

        }

        // GET ~/api/dash/counts
        [AcceptVerbs("GET")]
        public DashboardViewModel Counts()
        {
            var client = _clientFactory.GetNotificationQueryServiceClient();

            //TODO: need to use UTC DateTime
            var parameters = new NotificationCountsQueryParameters
            {
               StartDateUtc = DateTime.Today.AddMonths(_appSettings.GetValue<int>("MinDueDateMonths")),
               EndDateUtc   = DateTime.Today.AddDays(_appSettings.GetValue<int>("MaxDueDateDays"))
            };

            var counts = client.GetNotificationCountsForConsultantKey(parameters);

            var model = _dashboardMapping.Map(counts);
            return model;
        }
    }
}