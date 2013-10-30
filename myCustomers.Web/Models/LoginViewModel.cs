using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myCustomers.Web.Models
{
    public class LoginViewModel
    {
        public string SubsidiaryCode { get; set; }
        public string Culture { get; set; }
        public string UICulture { get; set; }
        public string ConsultantID { get; set; }
        public string Password { get; set; }
    }
}