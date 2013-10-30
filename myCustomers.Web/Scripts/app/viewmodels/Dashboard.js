var myc = myc || {};

myc.Dashboard = function (config) {

  var self = this;

  // REVIEW: this should be passed in config
  self.ajaxService = new myc.AjaxService();

  self.customersCount = ko.observable(0);
  self.ordersCount    = ko.observable(0);
  self.tasksCount     = ko.observable(0);
  
  function _getCounts(cb) {
   self.ajaxService.getJSON(config.actions.apiUrl)
     .done(function (data) {
        self.customersCount(data.customersCount);
        self.ordersCount(data.ordersCount);
        self.tasksCount(data.tasksCount);
      })
      .always(cb);
  }
  
  self.getCountsCommand = ko.asyncCommand({
    execute: function (cb) { _getCounts(cb); },
    canExecute: function () { return !self.ajaxService.isBusy(); }
  });
};