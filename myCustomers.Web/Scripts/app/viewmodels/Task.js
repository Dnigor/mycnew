var myc = myc || {};

myc.Task = function () {
  var self = this;

  self.Title       = ko.observable(""),
  self.Description = ko.observable(null),
  self.DueDateUtc  = ko.observable(new Date(moment())),
  self.CustomerId  = ko.observable(null),
  self.TaskId      = ko.observable(null),
  self.Name        = ko.observable(""),
  self.IsComplete  = ko.observable(false),
  self.TargetUrl   = ko.observable(null),
  self.ProfileUrl  = ko.observable(null),
  self.TaskType    = ko.observable(null),
  self.UrlText     = ko.observable(null),
  self.PrefixText  = ko.observable(app.localize('REMINDERS_PREFIX')),
  self.EndText     = ko.observable("");
};
