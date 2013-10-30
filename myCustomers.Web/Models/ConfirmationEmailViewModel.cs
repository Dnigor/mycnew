using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myCustomers.Web.Models
{
    public class ConfirmationEmailViewModel
    {        

        public bool ContactMeAboutDeliverySelected { get; set; }
        public bool ContactMeAboutPaymentSelected { get; set; }
        public bool ContactMeAboutProcessingSelected { get; set; }
        public bool ContactMeAsDesiredSelected { get; set; }
        public Guid OrderId { get; set; }
        public bool OrderIsGiftSelected { get; set; }
        public bool OrderProcessedSelected { get; set; }
        public bool OrderShippedSelected { get; set; }
        public bool OrderTotalSelected { get; set; }
        public string OrderTotalAmount { get; set; }
        public bool ProposedDeliveryScheduleSelected { get; set; }
        public string ProposedDeliveryScheduleDate { get; set; }
        public string ProposedDeliveryScheduleTime { get; set; }
    }
}