using System;
using System.Web.Http;
using NLog;

namespace myCustomers.Web.Controllers.Api
{
    [ApiAuthorizeConsultant]
    public class LogController : ApiController
    {
        static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public void PostMessage(LogMessage message)
        {
            _logger.Log(LogLevel.FromString(message.level), message.message);
        }

        public class LogMessage
        {
            public string level { get; set; }
            public string message { get; set; }
        }
    }
}
