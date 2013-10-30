using System;
using MaryKay.Configuration;
using Quartet.Entities;
using Quartet.Entities.Views;
using myCustomers.Globalization;
using myCustomers.Web.Models;

namespace myCustomers.Web.Services
{
	public class TaskViewModelMapper : IMappingService<TaskListItem, TaskViewModel>
	{
		static IQuartetClientFactory _clientFactory;
		static IAppSettings _appSettings;

		public TaskViewModelMapper(IQuartetClientFactory clientFactory, IAppSettings appSettings)
		{
			_clientFactory = clientFactory;
			_appSettings   = appSettings;
		}

        //TODO: should be fixed in the Quartet to return CustomerFirstName and CustomerLastName for the tasks to not call GetCustomer for each task
        static string MapCustomerName(TaskListItem task)
		{
			if (task.CustomerId == null)
			{
			    if (!String.IsNullOrEmpty(task.CustomerLastName) && !String.IsNullOrEmpty(task.CustomerFirstName))
			    {
			        return string.Format(_appSettings.GetValue("TaskNameFormat"), task.CustomerFirstName, task.CustomerLastName);
			    }
			    return String.Empty;
			}

			var customerService = _clientFactory.GetCustomersQueryServiceClient();
			var customer        = customerService.GetCustomer((Guid) task.CustomerId);
			if (customer == null)
			{
                return Resources.GetString("REMINDERLIST_INCORRECT_CUSTOMERID");
			}
            return string.Format(_appSettings.GetValue("TaskNameFormat"), customer.ContactInformation.FirstName, customer.ContactInformation.LastName);
		}

		static string MapTargetUrl(TaskListItem task)
		{
		    string targetUrl = null;
			TaskType taskType;
			var hasValue = Enum.TryParse(task.TaskType, out taskType);

			if (hasValue)
			{
				switch (taskType)
				{
				   case TaskType.OrderFollowUp:
                   case TaskType.Order:
                   case TaskType.OrderDelivery:
						if (task.OrderId.HasValue)
						{
							targetUrl = String.Format(
                                _appSettings.GetValue("TaskItemOpenOrder_TargetUrl"), 
                                task.OrderId.Value);
						}
						break;

					case TaskType.ProductSampleRequestFollowUp:
                    case TaskType.ProductSampleDelivery:
						if (task.SampleRequestId.HasValue)
						{
							targetUrl = String.Format(
								_appSettings.GetValue("TaskItemSampleRequest_TargetUrl"),
								task.SampleRequestId.Value);
						}
						break;
				}
			}
			return targetUrl;
		}

        static string MapUrlText(TaskListItem task)
        {
            string urlText = null;
            TaskType taskType;
            var hasValue = Enum.TryParse(task.TaskType, out taskType);

            if (hasValue)
            {
                switch (taskType)
                {
                    case TaskType.OrderFollowUp:
                    case TaskType.Order:
                    case TaskType.OrderDelivery:
                        if (task.OrderId.HasValue)
                        {
                            urlText = Resources.GetString("REMINDERLIST_URLTEXT_ORDER");
                        }
                        break;

                    case TaskType.ProductSampleRequestFollowUp:
                    case TaskType.ProductSampleDelivery:
                        if (task.SampleRequestId.HasValue)
                        {
                            urlText = Resources.GetString("REMINDERLIST_URLTEXT_PRODUCTSAMPLE");
                        }
                        break;
                }
            }
            return urlText;
        }

		static string MapProfiletUrl(Guid? customerId)
		{
			string profiletUrl = null;
			if (customerId.HasValue)
			{
				profiletUrl = _appSettings.GetValue("TaskItemProfile_TargetUrl") + customerId.Value; 
			}
			return profiletUrl;
		}

        private static string MapPrefixText(TaskListItem task)
        {
            string prefix = null;
            TaskType taskType;
            var hasValue = Enum.TryParse(task.TaskType, out taskType);

            if (hasValue)
            {
                switch (taskType)
                {
                    case TaskType.NewCustomerRegistrationFollowUp:
                    case TaskType.ProfileUpdatedByCustomerFollowUp:
                    case TaskType.InterestedInMkOpportunityFollowUp:
                    case TaskType.HostAPartyFollowup:
                        prefix = Resources.GetString("REMINDERLIST_PREFIX_FROM");
                        break;
                   
                    case TaskType.ProductReorderReminder:
                    case TaskType.OrderFollowUp:
                    case TaskType.Order:
                    case TaskType.ProductSampleRequestFollowUp:
                    case TaskType.OrderDelivery:
                    case TaskType.BirthdayFollowUp:
                    case TaskType.AnniversaryFollowUp:
                    case TaskType.ProductSampleDelivery:
                    case TaskType.Consultant:
                    case TaskType.Customer:
                        prefix = String.Empty;
                        if (task.CustomerId != null)
                        {
                            prefix = Resources.GetString("REMINDERLIST_PREFIX_FOR");
                        }
                        break;

                    case TaskType.HostessEventFollowUp:
                        prefix = Resources.GetString("REMINDERLIST_PREFIX_WITH");
                        break;
                }
            }
            return prefix;
        }

	    public TaskViewModel Map(TaskListItem task)
		{
			if (task == null)
				return null;

			return new TaskViewModel
				{
					CustomerId      = task.CustomerId,
					Description     = task.Description,
					DueDateUtc      = task.DueDateUtc,
					IsComplete      = task.IsComplete,
					ProfileUrl      = MapProfiletUrl(task.CustomerId),
                    OrderId         = task.OrderId,
                    SampleRequestId = task.SampleRequestId,
					TargetUrl       = MapTargetUrl(task),
					TaskId          = task.TaskId,
					TaskType        = task.TaskType,
					Title           = task.MapTaskTitle(),
					Name            = MapCustomerName(task),
                    UrlText         = MapUrlText(task),
                    PrefixText      = MapPrefixText(task),
                    EndText         = task.TaskType == TaskType.HostessEventFollowUp.ToString()? Resources.GetString("REMINDERLIST_END_GUEST") : ""
				};
		}
	}
}