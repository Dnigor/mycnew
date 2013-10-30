using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.SessionState;

namespace myCustomers.Web
{
    public class ImportCustomersHttpModule: IHttpModule
    {

        public void Init(HttpApplication context)
        {
            context.BeginRequest += new EventHandler(context_BeginRequest);
        }

        void context_BeginRequest(object sender, EventArgs e)
        {
            HttpContext currentContext = (sender as HttpApplication).Context;

            //Need to enable session when importing customers, webapi is stateless by default
            if (IsImportCustomersRequest(currentContext))
            {
                currentContext.SetSessionStateBehavior(SessionStateBehavior.Required);
            }
            
        }

        private bool IsImportCustomersRequest(HttpContext currentContext)
        {
            return currentContext.Request.AppRelativeCurrentExecutionFilePath.StartsWith("~/api/import/importcustomers");
        }

        public void Dispose()
        {

        }
    }
}