using MaryKay.Configuration;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace myCustomers.Data
{
    public class AddressFormatter
    {
        public static string Format(string city, string region, string postalCode)
        {
            var appSettings = ServiceLocator.Current.GetInstance<IAppSettings>();
            string formattedLocale;
            if (!string.IsNullOrWhiteSpace(city))
                if (!string.IsNullOrWhiteSpace(region))
                    if (!string.IsNullOrWhiteSpace(postalCode))
                        formattedLocale = string.Format(appSettings.GetValue("FormattedLocale_CityRegionPostalCode"), city, region, postalCode);
                    else
                        formattedLocale = string.Format(appSettings.GetValue("FormattedLocale_CityRegion"), city, region);
                else
                    if (!string.IsNullOrWhiteSpace(postalCode))
                        formattedLocale = string.Format(appSettings.GetValue("FormattedLocale_CityPostalCode"), city, postalCode);
                    else
                        formattedLocale = city;
            else
                if (!string.IsNullOrWhiteSpace(region))
                    if (!string.IsNullOrWhiteSpace(postalCode))
                        formattedLocale = string.Format(appSettings.GetValue("FormattedLocale_RegionPostalCode"), region, postalCode);
                    else
                        formattedLocale = region;
                else
                    if (!string.IsNullOrWhiteSpace(postalCode))
                        formattedLocale = postalCode;
                    else
                        formattedLocale = string.Empty;
            return formattedLocale;
        }
    }
}
