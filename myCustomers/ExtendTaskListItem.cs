using System;
using myCustomers.Globalization;
using Quartet.Entities;
using Quartet.Entities.Views;

namespace myCustomers
{
    public static class ExtendTaskListItem
    {
        public static string MapTaskTitle(this TaskListItem task)
        {
            string title = null;
            TaskType taskType;
            var hasValue = Enum.TryParse(task.TaskType, out taskType);

            if (hasValue)
            {
                switch (taskType)
                {
                    case TaskType.NewCustomerRegistrationFollowUp:
                        title = Resources.GetString("REMINDERLIST_REMTITLE_NEWWEBSITEREGISTRATION");
                        break;
                    case TaskType.ProfileUpdatedByCustomerFollowUp:
                        title = Resources.GetString("REMINDERLIST_REMTITLE_UPDATEWEBSITEREGISTRATION");
                        break;

                    case TaskType.InterestedInMkOpportunityFollowUp:
                        title = Resources.GetString("REMINDERLIST_REMTITLE_BUSINESS");
                        break;

                    case TaskType.OrderFollowUp:
                    case TaskType.Order:
                    case TaskType.ProductSampleRequestFollowUp:
                    case TaskType.HostessEventFollowUp:
                        title = Resources.GetString("REMINDERLIST_REMTITLE_FOLLOWUP");
                        break;

                    case TaskType.OrderDelivery:
                    case TaskType.ProductSampleDelivery:
                        title = Resources.GetString("REMINDERLIST_REMTITLE_DELIVERYREMINDER");
                        break;

                    case TaskType.BirthdayFollowUp:
                        title = Resources.GetString("REMINDERLIST_REMTITLE_BIRTHDAY");
                        break;

                    case TaskType.AnniversaryFollowUp:
                        title = Resources.GetString("REMINDERLIST_REMTITLE_ANNIVERSARY");
                        break;

                    case TaskType.HostAPartyFollowup:
                        title = Resources.GetString("REMINDERLIST_REMTITLE_HOST");
                        break;

                    case TaskType.EarnHostessRewardsFollowUp:
                        title = Resources.GetString("REMINDERLIST_REMTITLE_EARNHOSTESSREWARDS");
                        break;

                    case TaskType.ProductReorderReminder:
                        title = Resources.GetString("REMINDERLIST_REMTITLE_PRR");
                        break;

                    case TaskType.Consultant:
                    case TaskType.Customer:
                        title = task.Title;
                        break;
                }
            }
            return title;
        }
    }
}
