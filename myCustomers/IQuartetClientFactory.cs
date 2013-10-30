using Quartet.Client;
using Quartet.Client.Customers;
using Quartet.Client.Ordering;
using Quartet.Client.Search;
using Quartet.Client.Tasks;
using Quartet.Client.Notifications;

namespace myCustomers
{
    public interface IQuartetClientFactory
    {
        ICommandServiceClient GetCommandServiceClient();
        IGlobalQueryServiceClient GetGlobalQueryServiceClient();
        ICustomersQueryServiceClient GetCustomersQueryServiceClient();
        ICustomerPictureServiceClient GetCustomerPictureServiceClient();
        IBasketQueryServiceClient GetBasketQueryServiceClient();
        ITaskQueryServiceClient GetTaskQueryServiceClient();
        ICustomerSearch GetCustomerSearchClient();
        IOrderSearch GetOrderSearchClient();
        ICustomerNoteSearch GetCustomerNotesSearchClient();
        INotificationQueryServiceClient GetNotificationQueryServiceClient();
        IPromotionQueryServiceClient GetPromotionQueryServiceClient();
    }
}
