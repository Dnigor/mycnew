var myc = myc || {};

myc.CCPayment = function (model) {
  var self = this;

  self.paymentId            = ko.observable();
  self.invoiceId            = ko.observable();
  self.type                 = ko.observable();
  self.status               = ko.observable();
  self.billingAddress       = ko.observable();
  self.billingZip           = ko.observable();
  self.last4Digits          = ko.observable();
  self.expMonth             = ko.observable();
  self.expYear              = ko.observable();
  self.amount               = ko.observable();
  self.token                = ko.observable();
  self.approvalCode         = ko.observable();
  self.avsResponseCode      = ko.observable();
  self.transactionId        = ko.observable();
  self.transactionStatus    = ko.observable();
  self.postAuthAmount       = ko.observable();
  self.postAuthDateUtc      = ko.observable();
  self.preAuthAmount        = ko.observable();
  self.preAuthDateUtc       = ko.observable();
  self.paymentSettleDateUtc = ko.observable();
  self.paymentDateUtc       = ko.observable();
  self.proPayLink           = ko.observable();

  //#region Mapping

  model = $.extend({}, self, model);
  ko.mapping.fromJS(model, {}, self);

  //#endregion

  self.displayTransactionId = ko.computed(function() {
    var tid = parseInt(self.transactionId());
    if (tid) return tid;
  });

  self.displayStatus = ko.computed(function() {
    var status = self.status();
    var tstatus = parseInt(self.transactionStatus());
    if (tstatus) return status + ' (Code: ' + tstatus + ')';
    return status;
  });

  self.expDate = ko.computed(function() {
    var expMonth = self.expMonth();
    var expYear = self.expYear();
    var expDate = new Date(expYear, expMonth - 1, 1);
    return expDate;
  });
};
