using MaryKay.IBCDataServices.Client;

namespace myCustomers
{
    public interface IConsultantDataServiceClientFactory
    {
        IConsultantServiceClient GetConsultantServiceClient();
        IDisclaimerServiceClient GetDisclaimerSeriviceClient();
    }
}
