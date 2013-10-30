var myc = myc || {};

myc.CustomerGroup = function (data) {
  var self = this;

  self.name = ko.observable(data.name);
  self.groupId = ko.observable(data.groupId);
  self.hasName = ko.computed(function () { return self.name() !== null || self.name() !== undefined; });
};