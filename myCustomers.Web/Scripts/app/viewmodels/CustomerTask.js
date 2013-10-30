var myc = myc || {};

myc.CustomerTask = function (data) {
  var self = this;

  self.dueDateUtc = ko.observable(data.dueDateUtc);
  self.title = ko.observable(data.title);
  self.isComplete = ko.observable(data.isComplete);
  self.description = ko.observable(data.description);
  self.customerId = ko.observable(data.customerId);
  self.taskId = ko.observable(data.taskId);
  self.isEditing = ko.observable(false);
  self.isNotEditing = ko.computed(function () { return !self.isEditing(); });
  self.taskType = ko.observable(data.taskType);
  self.editTaskTitleInput = ko.observable();
  self.editTaskDescriptionInput = ko.observable();
  self.editTaskDueDateUtcInput = ko.observable();
  self.editTaskTitleHasContent = ko.computed(function () { return self.editTaskTitleInput() && self.editTaskTitleInput().length > 0; });

  self.enableEditTask = function () {
    self.editTaskTitleInput(self.title());
    self.editTaskDescriptionInput(self.description());
    self.editTaskDueDateUtcInput(self.dueDateUtc());
    self.isEditing(true);
  };

  self.disableEditTask = function () {
    self.isEditing(false);
  };

  self.isCustomerTaskType = ko.computed(function () { return self.taskType() === "Customer"; });
};