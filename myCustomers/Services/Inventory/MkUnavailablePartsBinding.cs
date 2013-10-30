using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace myCustomers.Services.Inventory
{
    public class MkUnavailablePartsBinding : BasicHttpBinding
    {
        public MkUnavailablePartsBinding() : base(BasicHttpSecurityMode.None)
        {
            OpenTimeout            = TimeSpan.FromMinutes(1);
            CloseTimeout           = TimeSpan.FromMinutes(1);
            SendTimeout            = TimeSpan.FromMinutes(1);
            ReceiveTimeout         = TimeSpan.FromMinutes(10);
            AllowCookies           = false;
            BypassProxyOnLocal     = false;
            HostNameComparisonMode = HostNameComparisonMode.StrongWildcard;
            MaxBufferSize          = 1048576;
            MaxBufferPoolSize      = 1048576;
            MaxReceivedMessageSize = 1048576;
            MessageEncoding        = WSMessageEncoding.Text;
            TextEncoding           = Encoding.UTF8;
            UseDefaultWebProxy     = true;
        }
    }
}
