using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using myCustomers.Data;
using myCustomers.Web.Models;
using Quartet.Entities;

namespace myCustomers.Web.Services
{
    public class ImportedCustomerViewModelMapper: IMappingService<UploadedCustomer, ImportedCustomer>
    {
        IQuartetClientFactory _clientFactory;

        public ImportedCustomerViewModelMapper(IQuartetClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public ImportedCustomer Map(UploadedCustomer uploadedCustomer)
        {
            return new ImportedCustomer
            {
                CustomerId = uploadedCustomer.CustomerId,
                ExistingCustomerId = uploadedCustomer.ExistingCustomerId,
                FirstName = uploadedCustomer.FirstName,
                LastName    = uploadedCustomer.LastName,
                ImportAction    = uploadedCustomer.Action.ToString(),
                IsFaulted     = uploadedCustomer.IsFaulted.ToString(),
                Subscriptions = GetSubscriptions(uploadedCustomer.CustomerId)
            };
        }

        private Subscription[] GetSubscriptions(Guid customerId)
        {
            var availableSubscriptions = new List<Subscription>();
            List<Subscription> customerSubscriptions;

            foreach (SubscriptionType subscriptionType in Enum.GetValues(typeof(SubscriptionType)))
            {
                availableSubscriptions.Add(new Subscription { CustomerId = customerId, SubscriptionType = subscriptionType, SubscriptionStatus = SubscriptionStatus.OptedOutByConsultant });
            }

            var client = _clientFactory.GetCustomersQueryServiceClient();
            var customer = client.GetCustomer(customerId);
            if (customer != null)
                customerSubscriptions = customer.Subscriptions.Select(s => s.Value).ToList();
            else return availableSubscriptions.ToArray();


            var result = from s in availableSubscriptions
                         join cs in customerSubscriptions on s.SubscriptionType equals cs.SubscriptionType into inners
                         from inner in inners.DefaultIfEmpty(new Subscription {CustomerId = customerId, SubscriptionType = s.SubscriptionType, SubscriptionStatus = SubscriptionStatus.OptedOutByConsultant })
                         select new Subscription
                         {
                             CustomerId = inner.CustomerId,
                             SubscriptionType = s.SubscriptionType,
                             SubscriptionStatus = inner.SubscriptionStatus                             
                         };

            return result.ToArray(); 

        }
    }
}