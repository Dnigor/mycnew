var myc = myc || {};

myc.Subscription = function (model) {
  
  var self = this;

  self.subscriptionType   = ko.observable(null);
  self.subscriptionStatus = ko.observable('OptedOutByConsultant');
  self.name               = ko.observable(null);
  
 //#region Mapping

  model = $.extend({}, self, model);
  ko.mapping.fromJS(model, {}, self);
 
  //#endregion

}