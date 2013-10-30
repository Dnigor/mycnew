var myc = myc || {};

myc.CustomerNote = function (data) {
  var self = this;

  self.isEditing = ko.observable(false);
  self.isNotEditing = ko.computed(function () { return !self.isEditing(); });
  self.dateCreatedUtc = ko.observable(data.dateCreatedUtc);
  self.content = ko.observable(data.content);
  self.customerNoteKey = ko.observable(data.customerNoteKey);
  self.editModeContent = ko.observable();
  self.hasEditModeContent = ko.computed(function () { return self.editModeContent() && self.editModeContent().length > 0; });

  self.enableEditNote = function () {
    self.editModeContent(self.content());
    self.isEditing(true);
  };

  self.disableEditNote = function () {
    self.isEditing(false);
  };
};