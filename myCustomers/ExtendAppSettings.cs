using System.Configuration;
using MaryKay.Configuration;

namespace myCustomers
{
    public static class ExtendAppSettings
    {
        public static string GetValueOrThrow(this IAppSettings settings, string key)
        {
            var value = settings.GetValue(key);

            if (string.IsNullOrEmpty(value))
                throw new ConfigurationErrorsException(string.Format("Missing application setting for key: '{0}'", key));

            return value;
        }
    }
}
