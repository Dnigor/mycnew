using System.Web.Mvc;
using System.Web.Mvc.Html;
using MaryKay.Configuration;
using Microsoft.Practices.ServiceLocation;

namespace myCustomers
{
    public static class ExtendHtmlHelper
    {
        public static MvcHtmlString AppSettingsPartial(this HtmlHelper htmlHelper, string appSettingsPartialViewKey, object model = null, ViewDataDictionary viewData = null)
        {
            var appSettings = ServiceLocator.Current.GetInstance<IAppSettings>();

            var partialViewName = appSettings.GetValue(appSettingsPartialViewKey);

            return htmlHelper.Partial(partialViewName, model, viewData);
        }
    }
}