var myc = myc || {};

myc.NewCCPayment = function (model) {
  var self = this;

  self.amount         = ko.observable();
  self.ccName         = ko.observable();
  self.ccType         = ko.observable();
  self.ccNumber       = ko.observable();
  self.ccExpMonth     = ko.observable();
  self.ccExpYear      = ko.observable();
  self.billingAddress = ko.observable();
  self.billingZip     = ko.observable();

  //#region Mapping

  model = $.extend({}, self, model);
  ko.mapping.fromJS(model, {}, self);

  //#endregion

  self.validationState = ko.validation.applyTo(self, {
    global: {
    }
  });

  //#endregion
};
