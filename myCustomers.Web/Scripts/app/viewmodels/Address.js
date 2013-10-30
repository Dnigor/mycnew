var myc = myc || {};

myc.Address = function (model, config) {
  var self = this;

  config = $.extend({ showAddressee: true }, config);

  self.addressKey  = ko.observable();
  self.addressee   = ko.observable('');
  self.street      = ko.observable('');
  self.unitNumber  = ko.observable('');
  self.city        = ko.observable('');
  self.countryCode = ko.observable('');
  self.regionCode  = ko.observable('');
  self.countryCode = ko.observable('');
  self.postalCode  = ko.observable('');
  self.isPrimary   = ko.observable(false);
  self.telephone   = ko.observable('');

  //#region Mapping

  model = $.extend({}, self, model);
  ko.mapping.fromJS(model, {}, self);

  //#endregion

  // NOTE: this property is only used on the client
  // to cpmpare two instances.
  self.id = app.newId();

  self.canEdit = ko.observable(true);

  self.hasFocus = ko.observable(false);

  self.showAddressee = ko.observable(config.showAddressee);

  //#region Validation

  self.isEmpty = ko.computed(function() {
    var a = ko.mapping.toJS(self);
    
    var isEmpty =
      !a.addressee && 
      !a.street && 
      !a.unitNumber && 
      !a.city && 
      !a.regionCode && 
      !a.countryCode && 
      !a.regionCode && 
      !a.postalCode &&
      !a.telephone;

    return isEmpty;
  });

  self.notEmpty = ko.computed(function() { return !self.isEmpty(); });

  //#endregion
};
