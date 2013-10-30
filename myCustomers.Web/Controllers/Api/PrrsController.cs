using System;
using System.Linq;
using System.Web.Http;
using NLog;
using Quartet.Entities.Commands;

namespace myCustomers.Web.Controllers.Api
{
    [ApiAuthorizeConsultant]
    public class PrrsController : ApiController
    {
        static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        readonly IQuartetClientFactory _clientFactory;

        public PrrsController(IQuartetClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        [AcceptVerbs("GET")]
        public dynamic GetByCustomerId(Guid id)
        {
            var client    = _clientFactory.GetCustomersQueryServiceClient();
            var response  = client.QueryPrrListByDateRange(id, 1, 500, DateTime.UtcNow);

            return new 
            {
                TotalCount = response.TotalCount,
                Items = response.Items.Select
                (
                    r => new PrrModel
                    {
                        ProductId        = r.ProductId,
                        CatalogName      = r.CatalogName,
                        DisplayPartId    = r.DisplayPartId,
                        Name             = r.Name,
                        CustomerId       = r.CustomerId,
                        ReminderDateUtc  = r.ReminderDateUtc
                    }
                )
            };
        }

        [AcceptVerbs("POST")]
        public void ChangeReminderDate(PrrModel model)
        {
            var commandService = _clientFactory.GetCommandServiceClient();

            commandService.Execute(new ChangePrrDate
            {
                ProductId       = model.ProductId,
                CatalogName     = model.CatalogName,
                CustomerId      = model.CustomerId,
                ReminderDateUtc = model.ReminderDateUtc
            });
        }

        [AcceptVerbs("DELETE")]
        public void Remove(PrrModel model)
        {
            var commandService = _clientFactory.GetCommandServiceClient();

            commandService.Execute(new RemovePrr
            {
                ProductId   = model.ProductId,
                CatalogName = model.CatalogName,
                CustomerId  = model.CustomerId
            });
        }

        public class PrrModel
        {
            public string ProductId { get; set; }
            public string CatalogName { get; set; }
            public string DisplayPartId { get; set; }
            public string Name { get; set; }
            public Guid CustomerId { get; set; }
            public DateTime ReminderDateUtc { get; set; }
        }
    }
}
