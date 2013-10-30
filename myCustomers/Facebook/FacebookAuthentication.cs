using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Web;
using System.Net;
using System.IO;
using MaryKay.Configuration;

namespace myCustomers.Facebook
{
    public enum Method { GET, POST };

    public interface IFacebookAuthentication
    {       
        void AccessTokenGet(string authToken, string callBackUrl);
        string Token { get; set; }
        string FQLWebRequest(Method method, string query, string postData);
        string AuthorizationLinkGet(string callBackUrl);
    }

    public class FacebookAuthentication : IFacebookAuthentication
	{
        private IAppSettings _appSettings;

        public FacebookAuthentication(IAppSettings appSettings)
        {
            _appSettings = appSettings;
        }

		public string ApplicationID
		{
			get
			{
				if (_applicationID.Length == 0)
				{
					_applicationID = _appSettings.GetValue("FacebookInTouchApplicationID");
				}
				return _applicationID;
			}
			set { _applicationID = value; }
		}

		public string ApplicationSecret
		{
			get
			{
				if (_applicationSecret.Length == 0)
				{
                    _applicationSecret = _appSettings.GetValue("FacebookInTouchApplicationSecret");
				}
				return _applicationSecret;
			}
			set { _applicationSecret = value; }
		}

		public string Token { get; set; }

		public string AuthorizationLinkGet(string callBackUrl)
		{
			if (callBackUrl.Substring(0, 1) == "/") callBackUrl = callBackUrl.Substring(1);
			return string.Format(AUTHORIZE_URL, ApplicationID, HttpContext.Current.Server.UrlEncode(callBackUrl));
		}

		public void AccessTokenGet(string authToken, string callBackUrl)
		{
			Token = authToken;
			var accessTokenUrl = string.Format(ACCESS_TOKEN_URL, ApplicationID, callBackUrl, ApplicationSecret, authToken);
			var response = WebRequest(Method.GET, accessTokenUrl, string.Empty);

			if (response.Length > 0)
			{
				var qs = HttpUtility.ParseQueryString(response);

				if (qs["access_token"] != null)
				{
					Token = qs["access_token"];
				}
			}
		}

		public string FQLWebRequest(Method method, string query, string postData)
		{
			string url = string.Format(FQL_URL, query, Token);
			return WebRequest(method, url, postData);
		}

		public string WebRequest(Method method, string url, string postData)
		{
			HttpWebRequest webRequest = null;
			StreamWriter requestWriter = null;
			var responseData = "";

			webRequest = System.Net.WebRequest.Create(url) as HttpWebRequest;
			webRequest.Method = method.ToString();
			webRequest.ServicePoint.Expect100Continue = false;
			webRequest.Timeout = 200000;

			if (method == Method.POST)
			{
				webRequest.ContentType = "application/x-www-form-urlencoded";
				requestWriter = new StreamWriter(webRequest.GetRequestStream());

				try
				{
					requestWriter.Write(postData);
				}
				finally
				{
					requestWriter.Close();
					requestWriter = null;
				}
			}

			responseData = WebResponseGet(webRequest);
			webRequest = null;
			return responseData;
		}

		public string WebResponseGet(HttpWebRequest webRequest)
		{
			StreamReader responseReader = null;
			WebResponse webResponse = null;
			string responseData = "";
			
			try
			{
				webResponse = webRequest.GetResponse();
				if (webResponse != null)
				{
					responseReader = new StreamReader(webResponse.GetResponseStream());
					responseData = responseReader.ReadToEnd();
				}
			}
			finally
			{
				if (webResponse != null) webRequest.GetResponse().GetResponseStream().Close();
				if (responseReader != null) responseReader.Close();
				responseReader = null;
				webResponse = null;
			}

			return responseData;
		}

		private string BaseUrl
		{
			get
			{
				var strReturn = "";

				if (HttpContext.Current != null)
				{
					Uri uri = HttpContext.Current.Request.Url;
					strReturn = string.Format("{0}://{1}/", uri.Scheme, uri.Authority);
				}

				return strReturn;
			}
		}

		private const string AUTHORIZE_URL = "https://graph.facebook.com/oauth/authorize?client_id={0}&redirect_uri={1}";
		private const string ACCESS_TOKEN_URL = "https://graph.facebook.com/oauth/access_token?client_id={0}&redirect_uri={1}&client_secret={2}&code={3}&";
		private const string FQL_URL = "https://graph.facebook.com/fql?q={0}&access_token={1}";
		private string _applicationID = "";
		private string _applicationSecret = "";
		
	}
}
