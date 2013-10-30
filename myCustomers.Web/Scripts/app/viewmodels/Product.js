var myc = myc || {};

myc.Product             = function(dto) {
  var self              = this;
  self.id               = ko.observable(dto.id);
  self.partId           = ko.observable(dto.partId);
  self.dispPartId       = ko.observable(dto.dispPartId);
  self.name             = ko.observable(dto.name);
  self.desc             = ko.observable(dto.desc);
  self.shade            = ko.observable(dto.shade);
  self.price            = ko.observable(dto.price);
  self.cdsFree          = ko.observable(dto.cdsFree);
  self.cdsPaid          = ko.observable(dto.cdsPaid);
  self.isGift           = ko.observable(dto.isGift);
  self.isSample         = ko.observable(dto.isSample);
  self.isLimited        = ko.observable(dto.isLimited);
  self.useupRate        = ko.observable(dto.useupRate);
  self.isWishList       = ko.observable(dto.isWishList);
  self.listImage        = ko.observable(dto.listImage);
  self.useupRate        = ko.observable(dto.useupRate);
  self.availableForCDS  = ko.observable(dto.availableForCDS);
  self.lastOrderDateUtc = ko.observable(dto.lastOrderDateUtc);
  self.qty              = ko.observable(0);
  self.total            = ko.computed(function () { return self.qty() * self.price() });
  self.isDetailVisible  = ko.observable(false);

  self.incrementQtyCommand = ko.command({
    execute: function () { self.qty(self.qty() + 1); }
  });

  self.decrementQtyCommand = ko.command({
    execute: function () { self.qty(self.qty() - 1) },
    canExectute: function() { return self.qty() > 1 }
  });

  self.toggleDetail = function() {
    self.isDetailVisible(!self.isDetailVisible());
  }
}
