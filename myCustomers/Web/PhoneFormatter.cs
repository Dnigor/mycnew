using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using PhoneNumbers;

namespace myCustomers.Web
{
    public static class PhoneFormatter
    {
        static Regex _rxClean = new Regex(@"[^\d]", RegexOptions.Compiled);
        static PhoneNumberUtil _util;

        static PhoneFormatter()
        {
            _util = PhoneNumberUtil.GetInstance();
        }

        public static string GetSampleNumber(string regionCode)
        {
            var number = _util.GetExampleNumber(regionCode);
            return _util.Format(number, PhoneNumberFormat.NATIONAL);
        }

        public static string Clean(string number)
        {
            if (null == number)
                number = string.Empty;

            if (number.StartsWith("+"))
                number = "+" + _rxClean.Replace(number, "");
            else
                number = _rxClean.Replace(number, "");

            return number;
        }

        public static string FormatInternational(string number, string regionCode)
        {
            number = Clean(number);
            var formatter = _util.GetAsYouTypeFormatter(regionCode);
            var output = number; ;
            for (var i = 0; i < number.Length; i++)
                output = formatter.InputDigit(number[i]);
            return output;
        }

        public static string FormatLocal(string number, string regionCode)
        {
            number = Clean(number);
            try
            {
                var countryCode = _util.GetCountryCodeForRegion(regionCode);
                var phone = _util.ParseAndKeepRawInput(number, regionCode);
                if (_util.IsValidNumberForRegion(phone, regionCode))
                {
                    return _util.Format(phone, PhoneNumberFormat.NATIONAL);
                }
                else if (_util.IsValidNumber(phone))
                    return _util.Format(phone, PhoneNumberFormat.INTERNATIONAL);
                else
                    return _util.FormatInOriginalFormat(phone, regionCode);
            }
            catch
            {
                return number;
            }
        }
    }
}
