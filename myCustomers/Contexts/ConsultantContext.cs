using System;
using System.Security.Cryptography;
using System.Web;
using System.Web.Caching;
using System.Web.Security;
using MaryKay.Configuration;
using MaryKay.IBCDataServices.Entities;
using NLog;

namespace myCustomers.Contexts
{
    public class ConsultantContext : IConsultantContext
    {
        static Logger _logger = LogManager.GetCurrentClassLogger();

        readonly IConsultantDataServiceClientFactory _clientFactory;
        readonly ISubsidiaryAccessor _subsidiaryAccessor;
        
        public ConsultantContext(IConsultantDataServiceClientFactory clientFactory, ISubsidiaryAccessor subsidiaryAccessor)
        {
            _clientFactory      = clientFactory;
            _subsidiaryAccessor = subsidiaryAccessor;
        }

        public virtual Consultant Consultant
        {
            get 
            {
                if (!HttpContext.Current.Request.IsAuthenticated)
                    return null;

                try
                {
                    var subsidiaryCode = _subsidiaryAccessor.GetSubsidiaryCode();
                    var contactId      = long.Parse(HttpContext.Current.User.Identity.Name);
                    var cacheKey       = string.Format("__Consultant:{0}:{1}", subsidiaryCode, contactId);
                    var consultant     = HttpRuntime.Cache.Get(cacheKey) as Consultant;

                    if (consultant == null || consultant.ID != contactId || consultant.SubsidiaryCode != subsidiaryCode)
                    {
                        var service = _clientFactory.GetConsultantServiceClient();
                        consultant  = service.GetConsultant(contactId, subsidiaryCode);
                        HttpRuntime.Cache.Add(cacheKey, consultant, null, Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(30), CacheItemPriority.High, null);
                    }

                    return consultant;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }

                return null;
            }
        }

        public void Clear()
        {
            if (!HttpContext.Current.Request.IsAuthenticated)
                return;

            var subsidiaryCode = _subsidiaryAccessor.GetSubsidiaryCode();
            var contactId      = long.Parse(HttpContext.Current.User.Identity.Name);
            var cacheKey       = string.Format("__Consultant:{0}:{1}", subsidiaryCode, contactId);

            HttpRuntime.Cache.Remove(cacheKey);
        }

        public FormsAuthenticationTicket GetAuthTicket()
        {
            var cookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];

            if (cookie != null && !string.IsNullOrEmpty(cookie.Value))
            {
                try
                {
                    return FormsAuthentication.Decrypt(cookie.Value);
                }
                catch (CryptographicException) { }
            }

            return null;
        }
    }
}