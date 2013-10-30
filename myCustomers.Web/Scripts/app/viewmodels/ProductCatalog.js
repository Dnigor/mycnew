var myc = myc || {};

myc.ProductCatalog = function(config) {
  var self = this;

  self.products = ko.observableArray([]);
  self.isEmpty  = ko.computed(function() { return self.products().length === 0 });

  self.enableCDSSamples = ko.observable(false);
  self.enableCDSSamples = ko.observable(false);
  self.queryString      = ko.observable('');
  self.cat              = ko.observable('');

  self.isCDSFree = ko.computed(function() { return self.cat() === 'CDSSamplesFree'; });
  self.isCDSPaid = ko.computed(function() { return self.cat() === 'CDSSamplesPaid'; });

  // paging
  self.pageSize    = ko.observable(config.pageSize);
  self.currentPage = ko.observable(1);
  self.itemCount   = ko.observable(0);
  self.pageCount   = ko.computed(function() { return Math.ceil(self.itemCount() / self.pageSize()); });

  self.searchCommand = ko.asyncCommand({
    execute: function (cb) { _doSearch(1).always(cb); },
    canExecute: function() { return !config.ajaxService.isBusy(); }
  });

  self.prevPageCommand = ko.asyncCommand({
    execute: function (cb) { _doSearch(self.currentPage() - 1).always(cb); },
    canExecute: function() { return !config.ajaxService.isBusy() && self.currentPage() > 1; }
  });

  self.nextPageCommand = ko.asyncCommand({
    execute: function (cb) { _doSearch(self.currentPage() + 1).always(cb); },
    canExecute: function() { return !config.ajaxService.isBusy() && self.currentPage() < self.pageCount(); }
  });

  self.prevPageCommand.isExecuting(false);
  self.nextPageCommand.isExecuting(false);
  self.pageCommandIsExecuting = ko.computed(function () {
    return self.prevPageCommand.isExecuting() || self.nextPageCommand.isExecuting();
  });

  function _getCriteria() {
    var criteria = {
      q:    self.queryString(),
      cat:  (self.isCDSFree() || self.isCDSFree()) ? null : self.cat(),
      cdsf: self.enableCDSSamples() ? self.isCDSFree() : false,
      cdsp: self.enableCDSSamples() ? self.isCDSPaid() : false,
      s:    self.pageSize()
    };

    return criteria;
  }

  function _doSearch(page) {
    var criteria = _getCriteria();
    criteria.i = page;

    return config
      .ajaxService
      .getJSON(config.apiUrl,criteria)
      .done(function(data) {
        self.itemCount(data.totalCount);
        self.currentPage(page);

        if (self.currentPage() > self.pageCount())
          self.currentPage(self.pageCount());

        var pvms = [];
        $.each(data.products, function (i, p) {
          var pvm = new myc.Product(p);
          pvms.push(pvm);
        });

        self.products(pvms);
      });
  }

  // execute search whenever parameters change
  ko.computed(function () {
    self.enableCDSSamples();
    self.queryString();
    self.cat();
    self.pageSize();
    return;
  }).subscribe(function (mode) {
    _doSearch(1);
  });

  self.enableCDSSamples.subscribe(function(val) {
    self.cat(null);
  });

  _doSearch(self.currentPage());
};
