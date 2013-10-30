var myc = myc || {};

myc.SampleRequestSearch = function (config) {
  var self = this;

  self.queryString = ko.observable(null);
  self.pageSize = ko.observable(5);
  self.currentPage = ko.observable(1);
  self.itemCount = ko.observable(0);
  self.searchResults = ko.observable([]);
  self.pageCount = ko.computed(function () { return Math.ceil(self.itemCount() / self.pageSize()); });
  self.hasResults = ko.computed(function () { return self.searchResults().length > 0 });
  self.noResults = ko.computed(function () { return !self.hasResults() });
  self.isBusy = ko.computed(function () { return config.ajaxService.isBusy(); });

  function _doSearch(page) {
    var criteria = {
      q: self.queryString(),
      i: page,
      s: self.pageSize()
    };

    return config
      .ajaxService
      .getJSON(config.apiUrl, criteria)
      .done(function (data) {
        self.currentPage(page);
        self.itemCount(data.length);
        self.searchResults(data);
      });
  }

  self.searchCommand = ko.asyncCommand({
    execute: function (cb) { _doSearch(1).always(cb); },
    canExecute: function () { return !self.isBusy(); }
  });

  self.prevPageCommand = ko.asyncCommand({
    execute: function (cb) { _doSearch(self.currentPage() - 1).always(cb); },
    canExecute: function () { return !self.isBusy() && self.currentPage() > 1; }
  });

  self.nextPageCommand = ko.asyncCommand({
    execute: function (cb) { _doSearch(self.currentPage() + 1).always(cb); },
    canExecute: function () { return !self.isBusy() && self.currentPage() < self.pageCount(); }
  });

  // fire off a search when pageSize changes
  self.pageSize.subscribe(function () {
    self.searchCommand.execute();
  });

  self.queryString.subscribe(function () {
    self.searchCommand.execute();
  });

  self.getStatus = function (isCompleted) {
    if (isCompleted) return app.localize("SAMPLEREQUEST_COMPLETE");
    else return app.localize("SAMPLEREQUEST_INCOMPLETE");
  }
}