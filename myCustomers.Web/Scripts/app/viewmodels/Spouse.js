var mycm = myc || {};

myc.Spouse = function(model) {
  var self = this;

  self.spouseName = ko.observable(),
  self.phoneNumber = ko.observable(),
  self.emailAddress = ko.observable();

  model = $.extend({}, self, model);
  ko.mapping.fromJS(model, {}, self);

  //#region Validation
  
  self.validator = ko.validation.applyTo(self, {
    global: {
      spouseName: {
        maxLength: 100,
        pattern: "[\w]"
      },
      emailAddress: {
        maxLength: 255,
        pattern: /^[a-z0-9!#$%&'*+\/=?\^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+\/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?$/gi
      },
      phoneNumber: {
        maxLength: 20,
        pattern: '^[\s\(\)\+\-\d]+$' // Quartet regexp
      }
    }
  });

  //#endregion
};