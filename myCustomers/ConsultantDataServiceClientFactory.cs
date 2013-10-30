using System.Configuration;
using MaryKay.Configuration;
using MaryKay.IBCDataServices.Client;
using NLog;

namespace myCustomers
{
    public class ConsultantDataServiceClientFactory : IConsultantDataServiceClientFactory
    {
        static Logger _logger = LogManager.GetCurrentClassLogger();

        string             _clientKey;
        IEnvironmentConfig _environmentConfig; 

        public ConsultantDataServiceClientFactory(IEnvironmentConfig environmentConfig)
        {
            _clientKey         = ConfigurationManager.AppSettings["ClientKey"];
            _environmentConfig = environmentConfig;
        }

        public IConsultantServiceClient GetConsultantServiceClient()
        {
            var region      = _environmentConfig.GetRegion();
            var environment = _environmentConfig.GetEnvironment();

            return new ConsultantService(region, environment);
        }

        public IDisclaimerServiceClient GetDisclaimerSeriviceClient()
        {
            var region = _environmentConfig.GetRegion();
            var environment = _environmentConfig.GetEnvironment();

            return new DisclaimerService(region, environment);
        }
    }
}
