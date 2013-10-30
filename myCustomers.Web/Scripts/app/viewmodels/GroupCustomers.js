var myc = myc || {};

myc.GroupCustomers = function (config) {
  var self = this;

  self.isBusy = ko.computed(function () { return config.ajaxService.isBusy(); });

  //#region Search

  self.queryString = ko.observable(null);
  self.searchResults = ko.observable([]);
  self.hasResults = ko.computed(function () { return self.searchResults().length > 0; });
  self.noResults = ko.computed(function () { return !self.hasResults(); });

 
  

  function _doSearch(page) {
    var criteria = _getCriteria();
    criteria.i = page;
    return config.ajaxService.getJSON(config.apiUrl, criteria)
      .done(function (data) {
        self.currentPage(page);
        self.itemCount(data.length);
        self.searchResults(data);    
      });
  }

  self.searchCommand = ko.asyncCommand({
    execute: function (cb) { _resetSelections(); _doSearch(1).always(cb); },
    canExecute: function () { return !self.isBusy(); }
  });

  function _getCriteria() {
    var criteria = {
      g: config.groupId,
      s: self.pageSize(),
      i: self.currentPage()
    };

    return criteria;
  }

  //#endregion

  //#region Paging

  self.pageSize = ko.observable(10);
  self.currentPage = ko.observable(1);
  self.itemCount = ko.observable(config.customerCount);
  self.pageCount = ko.computed(function () { return Math.ceil(self.itemCount() / self.pageSize()); });

  self.prevPageCommand = ko.asyncCommand({
    execute: function (cb) { _doSearch(self.currentPage() - 1).always(cb); },
    canExecute: function () { return !self.isBusy() && self.currentPage() > 1; }
  });

  self.nextPageCommand = ko.asyncCommand({
    execute: function (cb) { _doSearch(self.currentPage() + 1).always(cb); },
    canExecute: function () { return !self.isBusy() && self.currentPage() < self.pageCount(); }
  });

  //#endregion

  //#region Selection

  var _selectedCustomers = {};
  var _selectedCustomerCount = ko.observable(0);
  var _selectAll = ko.observable(false);

  function _resetSelections() {
    _selectAll(false);
    _selectedCustomers = {};
    _selectedCustomerCount(0);
  }

  self.hasSelections = ko.computed(function () {
    var all = _selectAll();
    var some = _selectedCustomerCount() > 0;
    var none = (!all && _selectedCustomerCount() === 0) || (all && _selectedCustomerCount() == self.itemCount());
    return !none && (all || some);
  });

  self.selectAll = ko.computed({
    read: function () {
      var all = _selectAll();
      var some = _selectedCustomerCount() > 0;
      return all && !some;
    },
    write: function (value) {
      _selectedCustomers = {};
      _selectedCustomerCount(0);
      _selectAll(value);
    }
  });

  // factory method that creates a computed observable for every check box that binds checked: customerChecked($data)
  self.customerChecked = function ($data) {
    return ko.computed({
      read: function () {
        // Important! We must ensure that we invoke the observables used during this read and not let any short circuiting ifs
        // cause them not to get called. Otherwise KO will not wire up the dependancies.
        var all = _selectAll();
        var count = _selectedCustomerCount(); // called so that KO will update checked when the count changes
        var selected = typeof (_selectedCustomers[$data.CustomerId]) !== 'undefined'
        return (all && !selected) || (!all && selected);
      },
      write: function (value) {
        if (_selectAll()) {
          // when in select all mode we track which items are not selected
          if (value) {
            delete _selectedCustomers[$data.CustomerId];
            _selectedCustomerCount(_selectedCustomerCount() - 1);
          }
          else {
            _selectedCustomers[$data.CustomerId] = $data;
            _selectedCustomerCount(_selectedCustomerCount() + 1);
          }
        }
        else {
          // when not in select all mode we track wich items are selected
          if (value) {
            _selectedCustomers[$data.CustomerId] = $data;
            _selectedCustomerCount(_selectedCustomerCount() + 1);
          }
          else {
            delete _selectedCustomers[$data.CustomerId];
            _selectedCustomerCount(_selectedCustomerCount() - 1);
          }
        }
      }
    });
  }

  function _getSelectedCustomerIds() {
    var ids = [];
    for (var id in _selectedCustomers)
      ids.push(id);

      return $.Deferred().resolve(ids);

  }

  //#endregion

  //#region Actions

  self.deleteFromGroup = ko.asyncCommand({
    execute: function (cb) {
      var succeeded = false;
      app
        .confirm('CONFIRM_DELETEFROMGROUP_TITLE', 'CONFIRM_DELETEFROMGROUP_BODY')
        .then(function () {
          app.showProgressModal('PROGRESS_DELETEFROMGROUP');
        })
        .then(_getSelectedCustomerIds)
        .then(function (ids) {
          return config
            .ajaxService
            .postJSON(config.deleteFromGroupUrl, { ids: ids, groupId: config.groupId, allSelected: _selectAll() })
            .then(function () {
              succeeded = true;
              return succeeded;
            });
        })
        .always(function (res) {
          _resetSelections();
          _doSearch(1);
          app.closeProgressModal();
          if (succeeded)
            app.notifySuccess(app.localize('DELETEFROMGROUP_SUCCESS'));
        })
        .always(cb);
    },
    canExecute: function () { return self.hasSelections() && !self.isBusy(); }
  });


  //#endregion

  //#region Navigation


//  self.onCustomerRowClick = function () {
//    var url = config.targetUrlFormat.replace('_id_', this._id);
//    self.navigate(url);
//  }

//  self.navigate = function (url) {
//    location.href = url;
//  };

  //#endregion

  // trigger a search when any of these properties change
  ko.computed(function () {
    self.pageSize();
    return;
  }).subscribe(function (mode) {
    _doSearch(1);
  });


};
