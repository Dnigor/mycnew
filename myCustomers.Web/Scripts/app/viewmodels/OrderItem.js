var myc = myc || {};

myc.OrderItem = function(model) {
  var self = this;

  self.id              = ko.observable(model.id);
  self.partId          = ko.observable(model.partId);
  self.dispPartId      = ko.observable(model.dispPartId);
  self.name            = ko.observable(model.name);
  self.desc            = ko.observable(model.desc);
  self.shade           = ko.observable(model.shade);
  self.price           = ko.observable(model.price);
  self.listImage       = ko.observable(model.listImage);
  self.useupRate       = ko.observable(model.useupRate);
  self.cdsFree         = ko.observable(model.cdsFree);
  self.cdsPaid         = ko.observable(model.cdsPaid);
  self.isGWP           = ko.observable(model.isGWP);
  self.availableForCDS = ko.observable(model.availableForCDS);
  self.qty             = ko.observable(model.qty).extend({ numeric: { min: 1 } });
  self.total           = ko.computed(function() { return self.qty() * self.price(); });
  self.isDetailVisible = ko.observable(false);

  self.toggleDetail = function() {
    self.isDetailVisible(!self.isDetailVisible());
  };

  self.incrementQtyCommand = ko.command({
    execute: function () { self.qty(self.qty() + 1); }
  });

  self.decrementQtyCommand = ko.command({
    execute: function () { self.qty(self.qty() - 1); },
    canExecute: function () { return self.qty() > 1; }
  });
};
