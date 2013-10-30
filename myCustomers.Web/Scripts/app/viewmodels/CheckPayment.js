var myc = myc || {};

myc.CheckPayment = function (model) {
  var self = this;

  self.paymentId      = ko.observable();
  self.amount         = ko.observable();
  self.checkNumber    = ko.observable();
  self.paymentDateUtc = ko.observable();

  //#region Mapping

  model = $.extend({}, self, model);
  ko.mapping.fromJS(model, {}, self);

  //#endregion

  self.validationState = ko.validation.applyTo(self, {
    global: {
      paymentDateUtc: {
        required: true,
        validDate: true
      },
      amount:
      {
        required: true,
        validAmount: true
      }
    }
  });

  //#endregion
};
