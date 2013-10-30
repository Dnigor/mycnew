var myc = myc || {};

myc.PhoneNumber = function (model) {
  var self = this;

  self.phoneNumberKey  = ko.observable(null);
  self.phoneNumberType = ko.observable(null);
  self.number          = ko.observable(null);
  self.extension       = ko.observable(null);
  self.isPrimary       = ko.observable(false);

  //#region Mapping

  model = $.extend({}, self, model);
  ko.mapping.fromJS(model, {}, self);

  //#endregion

  // NOTE: this property is only used on the client
  // to cpmpare two instances.
  self.id = app.newId();

  self.isMobile    = ko.computed(function() { return self.phoneNumberType() === 'Mobile'; });
  self.isWorkPhone = ko.computed(function() { return self.phoneNumberType() === 'Work'; });
  self.hasFocus    = ko.observable(false);

  //#region Validation

  self.isEmpty = ko.computed(function() {
    var p = ko.mapping.toJS(self);
    return !p.number && !p.extension;
  });

  self.notEmpty = ko.computed(function() { return !self.isEmpty(); });

  self.validationState = ko.validation.applyTo(self, {  
    global: {
      number: { 
        pattern: { message: 'VALIDATION_PHONENUMBER', params: /^[\+\s\(\)\-\d/]{1,20}$/g }
      },
      extension: { pattern: { message: 'VALIDATION_PHONEEXTENSION', params: /^\d{1,5}$/g } }
    }
  });

  //#endregion
};