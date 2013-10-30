var myc = myc || {};

myc.SocialNetwork = function(model, config) {
  var self = this;

  self.key  = ko.observable(null);
  self.type = ko.observable(null);
  self.url  = ko.observable(null);

  //#region Mapping

  model = $.extend({}, self, model);
  ko.mapping.fromJS(model, {}, self);

  //#endregion

  // NOTE: this property is only used on the client
  // to cpmpare two instances.
  self.id = app.newId();

  self.hasFocus = ko.observable(false);

  //#region Validation

  self.isEmpty = ko.computed(function() {
    var url = self.url();
    return !url;
  });
  
  self.validationState = ko.validation.applyTo(self, {
    global: {
      url: { 
        validation: { // ko validation pattern rule is bugged out
          message: 'VALIDATION_URL',
          validator: function(val) {
            return new RegExp(config.Validation, 'gi').test(val);
          }
        }
      }
    }
  });

  //#endregion
}