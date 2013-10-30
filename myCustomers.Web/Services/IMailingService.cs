using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using myCustomers.Web.Models;
using myCustomers.Web.Controllers.Api;

namespace myCustomers.Web.Services
{
	public interface IMailingService
	{
		void SendConfirmationEmail(ConfirmationEmailViewModel model);
		void SendInviteToRegister(CustomerModel model);
		void SendOptInMail(CustomerModel model);
	}
}