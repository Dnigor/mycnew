using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace myCustomers
{
    public static class ExtendSubscription
    {
        public static MaryKay.IBCDataServices.Entities.SubscriptionAttribute Attribute(this MaryKay.IBCDataServices.Entities.Subscription subscription, Guid attributeKey)
        {
            if (subscription == null) throw new ArgumentNullException("subscription");

            MaryKay.IBCDataServices.Entities.SubscriptionAttribute attribute = (
                from MaryKay.IBCDataServices.Entities.SubscriptionAttribute attr in subscription.SubscriptionAttributes
                where attr.AttributeKey.Equals(attributeKey)
                 && !attr.EntityState.Equals(MaryKay.IBCDataServices.Entities.Enum.EntityState.Deleted)
                select attr
                ).FirstOrDefault();
            return attribute;
        }

        public static bool IsActive(this MaryKay.IBCDataServices.Entities.Subscription subscription)
        {
            bool isActive = (subscription != null)
             && (subscription.EntityState != MaryKay.IBCDataServices.Entities.Enum.EntityState.Deleted)
             && (subscription.Disabled != true)
             && (subscription.RenewalOn > DateTime.UtcNow);
            return isActive;
        }


        public static bool IsSelected(this MaryKay.IBCDataServices.Entities.SubscriptionAttribute attribute)
        {
            bool isSelected = (attribute != null)
             && attribute.Value.Equals("true", StringComparison.InvariantCultureIgnoreCase);
            return isSelected;
        }
    }
}
