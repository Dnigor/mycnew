var myc = myc || {};

myc.OrderList = function (config) {
  var self = this;

  self.isBusy = ko.computed(function() { return config.ajaxService.isBusy(); });

  self.sampleRequestSearch = ko.observable(config.sampleRequestSearch);  
  
  self.deletedOrdersSearch = ko.observable(config.deletedOrdersSearch);
  
  //#region Search 

  self.searchResults = ko.observable([]);
  self.queryString   = ko.observable('');
  self.hasResults    = ko.computed(function() { return self.searchResults().length > 0; });
  self.noResults     = ko.computed(function() { return !self.hasResults() });

  self.searchCommand = ko.asyncCommand({
    execute: function(cb) { _doSearch(1).always(cb); },
    canExecute: function() { return !self.isBusy(); }
  });

  function _doSearch(page) {
    var criteria = _getCriteria();
    criteria.i = page;

    return config.ajaxService.getJSON(config.apiUrl, criteria)
      .done(function (data) {
        self.itemCount(data.hits.total);
        self.currentPage(page);

        if (self.currentPage() > self.pageCount())
          self.currentPage(self.pageCount());

        self.searchResults(data.hits.hits);

        _saveState();
        
        self.deletedOrdersSearch().searchCommand.execute();
      });
  }

  function _getCriteria() {
    var criteria = {
      q: self.queryString(),
      sf: _sortField(),
      sd: self.sortDescending(),
      s: self.pageSize(),
      i: self.currentPage(),
      p: null,
      src: null,
      sts: null,
      ps: null,
      ods: null,
      ode: null,
      d: false,
      a: false
    };

    var filterMode = self.filterMode();

    if (filterMode === 'Product' || filterMode === 'Advanced')
      criteria.p = self.product();

    if (filterMode === 'OrderSource' || filterMode === 'Advanced')
      criteria.src = self.orderSource();

    if (filterMode === 'OrderStatus' || filterMode === 'Advanced')
      criteria.sts = self.orderStatus();

    if (filterMode === 'PaymentStatus' || filterMode === 'Advanced')
      criteria.ps = self.paymentStatus();

    if (filterMode === 'OrderDate' || filterMode === 'Advanced') {
      criteria.ods = self.minOrderDate();
      criteria.ode = self.maxOrderDate();

      if (self.maxOrderDate()) {
        var time = moment(self.maxOrderDate());
        if (time._d.getSeconds() != 59)
          criteria.ode = moment(self.maxOrderDate()).add('days', 1).subtract('seconds', 1)._d;
      }
    }

    if (filterMode === 'Deleted' || filterMode === 'Advanced')
      criteria.d = self.isDeleted();

    if (filterMode === 'Archived' || filterMode === 'Advanced')
      criteria.a = self.isArchived();

    if (criteria.ods !== null && criteria.ods !== '')
      criteria.ods = app.toISO8601(criteria.ods);

    if (criteria.ode !== null && criteria.ode !== '') 
     criteria.ode = app.toISO8601(criteria.ode);
   

    return criteria;
  }

  //#endregion

  //#region Sorting

  self.sortDescending = ko.observable(true);

  self.queryString.subscribe(function (value) {
    if (value) {
      self.sortField(null);
      self.sortDescending(null);
    }
    else {
      self.sortField('OrderDateUtc');
      self.sortDescending(true);
    }
  });

  _sortField = ko.observable('OrderDateUtc');
  self.sortField = ko.computed({
    read: function () {
      var field = _sortField();
      switch (field) {
        case 'OrderDateUtc':
          return app.localize("SORT_ORDERDATE");
        case 'FirstName.Sort':
          return app.localize("SORT_FIRSTNAME");
        case 'LastName.Sort':
          return app.localize("SORT_LASTNAME");
        case 'OrderSourceSort':
          return app.localize("SORT_ORDERTYPE");
        case 'OrderStatusSort':
          return app.localize("SORT_ORDERSTATUS");
        case 'PaymentStatusSort':
          return app.localize("SORT_ORDERPAYMENTSTATUS");
        case 'EstimatedOrderAmount':
          return app.localize("SORT_AMOUNT");
        default:
          return app.localize("SORT_RELEVANCE");
      }
    },
    write: function (value) {
      var currentField = _sortField();
      if (currentField === value) {
        if (value === null) return;
        self.sortDescending(!self.sortDescending());
      }
      else if (value === null) {
        self.sortDescending(null);
        _sortField(null);
      }
      else {
        switch (value) {
          case 'OrderDateUtc':
            self.sortDescending(true);
            break;
          default:
            self.sortDescending(false);
        }
        _sortField(value);
      }
    }
  });

  //#endregion

  //#region Paging

  self.pageSize    = ko.observable('10');
  self.currentPage = ko.observable(1);
  self.itemCount   = ko.observable(0);
  self.pageCount   = ko.computed(function () { return Math.ceil(self.itemCount() / self.pageSize()); });

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

  var _selectedOrders = {};
  var _selectedOrderCount = ko.observable(0)
  var _selectAll = ko.observable(false);

  function _resetSelections() {
    _selectAll(false);
    _selectedOrders = {};
    _selectedOrderCount(0);
  }

  self.hasSelections = ko.computed(function () {
    var all = _selectAll();
    var some = _selectedOrderCount() > 0;
    var none = (!all && _selectedOrderCount() === 0) || (all && _selectedOrderCount() == self.itemCount());
    return !none && (all || some);
  });

  self.selectAll = ko.computed({
    read: function () {
      var all = _selectAll();
      var some = _selectedOrderCount() > 0;
      return all && !some;
    },
    write: function (value) {
      _selectedOrders = {};
      _selectedOrderCount(0);
      _selectAll(value);
    }
  });

  // factory method that creates a computed observable for every check box that binds checked: orderChecked($data)
  self.orderChecked = function ($data) {
    return ko.computed({
      read: function () {
        // Important! We must ensure that we invoke the observables used during this read and not let any short circuiting ifs
        // cause them not to get called. Otherwise KO will not wire up the dependancies.
        var all = _selectAll();
        var count = _selectedOrderCount(); // called so that KO will update checked when the count changes
        var selected = typeof(_selectedOrders[$data.OrderId]) !== 'undefined';
        return (all && !selected) || (!all && selected);
      },
      write: function (value) {
        if (_selectAll()) {
          // when in select all mode we track which items are not selected
          if (value) {
            delete _selectedOrders[$data.OrderId];
            _selectedOrderCount(_selectedOrderCount() - 1);
          }
          else {
            _selectedOrders[$data.OrderId] = $data;
            _selectedOrderCount(_selectedOrderCount() + 1);
          }
        }
        else {
          // when not in select all mode we track wich items are selected
          if (value) {
            _selectedOrders[$data.OrderId] = $data;
            _selectedOrderCount(_selectedOrderCount() + 1);
          }
          else {
            delete _selectedOrders[$data.OrderId];
            _selectedOrderCount(_selectedOrderCount() - 1);
          }
        }
      }
    });
  }

  function _getSelectedOrderIds() {
    var ids = [];
    for (var id in _selectedOrders)
      ids.push(id);

    // if not in select all mode then just return the selected ids
    if (!_selectAll())
      return ids;

    // for select all mode fetch all the customer ids for the current search
    // and exclude any that have be explicitly unchecked
    var criteria = _getCriteria();
    criteria.i = 1;
    criteria.s = 65535;

    // NOTE: pass empty result fields 'rf=' qstring param to cause the search to not return any source data
    // but just return the doc id which is the same guid as the customer id.
    return config
      .ajaxService.getJSON(config.apiUrl + '?rf=', criteria)
      .then(function(data) {
        var res = Enumerable
          .From(data.hits.hits)
          .Select(function(c) { return c._id; })
          .Except(ids)
          .ToArray();
        return res;
      });
  }

  //#endregion

  //#region Filtering

  self.filterMode    = ko.observable(null);
  self.product       = ko.observable(null);
  self.orderSource   = ko.observable(null);
  self.orderStatus   = ko.observable(null);
  self.paymentStatus = ko.observable(null);
  self.minOrderDate  = ko.observable(null);
  self.maxOrderDate  = ko.observable(null);
  self.isDeleted     = ko.observable(false);
  self.isArchived    = ko.observable(false);

  function _resetFilter() {
    self.minOrderDate(null);
    self.maxOrderDate(null);
    self.orderSource('');
    self.orderStatus('');
    self.paymentStatus('');
    self.product('');
    self.isDeleted(false);
    self.isArchived(false);
  };

  //#endregion

  //#region State

  var _stateKey = 'myc.orderList.state.v1';
  function _saveState() {
    if (!config.enablePersistence)
      return;

    var state = {
      criteria: _getCriteria(),
      filterMode: self.filterMode()
    };
    var jsonState = ko.toJSON(state);
    app.session.setItem(_stateKey, jsonState);
  }

  function _loadState() {
    if (!config.enablePersistence)
      return;

    var jsonState = app.session.getItem(_stateKey);
    if (jsonState !== undefined && jsonState !== null) {
      var state = $.parseJSON(jsonState);
      if (state !== undefined && state !== null) {
        if (state.criteria !== undefined && state.criteria !== null) {
          self.queryString(state.criteria.q);
          self.minOrderDate(state.criteria.ods);
          self.maxOrderDate(state.criteria.ode);
          self.orderSource(state.criteria.src);
          self.orderStatus(state.criteria.sts);
          self.paymentStatus(state.criteria.ps);
          self.isDeleted(state.criteria.d);
          self.isArchived(state.criteria.a);
          self.product(state.criteria.p);

          self.filterMode(state.filterMode);

          if (!isNaN(state.s))
            self.pageSize(state.criteria.s);

          if (!isNaN(state.i))
            self.currentPage(state.criteria.i);

          self.sortDescending(state.criteria.sd);
          _sortField(state.criteria.sf);
        }
      }
    }
  }

  //#endregion

  //#region Actions

  self.navigateToSRDetails = function (elem) {
    window.location = config.sampleRequestDetailsUrl + '/' + elem.SampleRequestId;
  };

  self.isUpdatePaymentPanelVisible = ko.observable(false);
  self.paymentStatusAlert = ko.observable(false);

  self.togglePaymentPanel = function(on) {
    if (on) self.isUpdatePaymentPanelVisible(true);
    else self.isUpdatePaymentPanelVisible(false);
    return false;
  };

  self.selectedPaymentStatus = ko.observable('');

  self.updatePaymentStatusCommand = ko.asyncCommand({
    execute: function(cb) {
      var succeeded = false;
      app
        .confirm('CONFIRM_UPDATEPAYMENTSTATUS_TITLE', 'CONFIRM_UPDATEPAYMENTSTATUS_BODY')
        .then(function() {
          app.showProgressModal('PROGRESS_UPDATEPAYMENTSTATUS');
        })
        .then(_getSelectedOrderIds)
        .then(function(ids) {
          var newPaymentStatus = parseInt(self.selectedPaymentStatus(), 10);

          var model = {
            OrderIds: ids,
            PaymentStatus: newPaymentStatus
          };

          return config
            .ajaxService
            .getJSON(config.apiUrl + '/' + ids[ids.length - 1], { fields:  'LastEventSeq' })
            .then(function(res) {
              succeeded = true;
              var lastEventSeq = res.fields.LastEventSeq;
              return config
                .ajaxService
                .postJSON(config.updatePaymentStatusUrl, model)
                .then(function() {
                  succeeded = true;
                  return $.ajaxPoll({
                    url: config.apiUrl + '/' + ids[ids.length - 1], 
                    type: 'GET',
                    data: { fields:  'LastEventSeq' },
                    dataType: 'json',
                    pollingType: 'interval',
                    interval: 1000,
                    maxInterval: 1000,
                    expireAfter: 15,
                    successCondition: function(res) {
                      try { return res.exists && res.fields.LastEventSeq > lastEventSeq }
                      catch(ex) { return false; }
                    },
                    expired: function() {
                      // TODO: show message indicating the search results may not be up to date
                    }
                  });
                });
            });
        })
        .always(function() {
          _resetSelections();
          _doSearch(self.currentPage());
          self.isUpdatePaymentPanelVisible(false);
          app.closeProgressModal();
          if (succeeded)
            app.notifySuccess(app.localize('PAYMENT_STATUS_SUCCESS'));
        })
        .always(cb);
    },
    canExecute: function() { return self.hasSelections() && !self.isBusy(); }
  });

  self.archiveOrdersCommand = ko.asyncCommand({
    execute: function(cb) {
      var succeeded = false;
      app
        .confirm('CONFIRM_ARCHIVEORDERS_TITLE', 'CONFIRM_ARCHIVEORDERS_BODY')
        .then(function() {
          app.showProgressModal('PROGRESS_ARCHIVEORDER');
        })
        .then(_getSelectedOrderIds)
        .then(function(ids) {
          return config
            .ajaxService
            .postJSON(config.archiveApiUrl, { ids: ids })
            .then(function () {
              var lastOrderId = ids[ids.length - 1];
              succeeded = true;
              return $.ajaxPoll({
                url: config.apiUrl, 
                type: 'GET',
                data: {q: lastOrderId, qf: ['OrderId'], rf: ['IsArchived']},
                dataType: 'json',
                pollingType: 'interval',
                interval: 1000,
                maxInterval: 1000,
                expireAfter: 15,
                successCondition: function(res) {
                  var isSuccess;

                  if (res && res.hits && res.hits.hits) {
                    $.each(res.hits.hits, function (index, ival) {

                      if (lastOrderId === ival._id && ival.fields && ival.fields.IsArchived === true) {
                        app.logTrace('Orders archived', ids);
                        isSuccess = true;
                      }
                    });
                  }

                  return isSuccess;
                },
                expired: function() { 
                  // TODO: show message indicating the search results may not be up to date
                }
              });
            });
        })
        .always(function() {
          _resetSelections();
          _doSearch(1);
          app.closeProgressModal();
          if (succeeded)
            app.notifySuccess(app.localize('ARCHIVEORDERS_SUCCESS'));
        })
        .always(cb);
    },
    canExecute: function() { return self.hasSelections() && !self.isBusy(); }
  });

  self.unarchiveOrdersCommand = ko.asyncCommand({
    execute: function (cb) {
      var succeeded = false;
      app
        .confirm('CONFIRM_UNARCHIVEORDERS_TITLE', 'CONFIRM_UNARCHIVEORDERS_BODY')
        .then(function() {
          app.showProgressModal('PROGRESS_UNARCHIVEORDER');
        })
        .then(_getSelectedOrderIds)
        .then(function(ids) {
          return config
            .ajaxService
            .postJSON(config.unarchiveApiUrl, { ids: ids })
            .then(function () {
              var lastOrderId = ids[ids.length - 1];
              succeeded = true;
              return $.ajaxPoll({
                url: config.apiUrl, 
                type: 'GET',
                data: { q: lastOrderId, qf: ['OrderId'], rf: ['IsArchived'] },
                dataType: 'json',
                pollingType: 'interval',
                interval: 1000,
                maxInterval: 1000,
                expireAfter: 15,
                expire: function() { 
                  // TODO: show message indicating the operation will take longer
                },
                successCondition: function(res) {
                  var isSuccess;

                  if (res && res.hits && res.hits.hits) {
                      $.each(res.hits.hits, function (index, ival) {

                      if (lastOrderId === ival._id && ival.fields && ival.fields.IsArchived === false) {
                        app.logTrace('Orders unarchived', ids);
                        isSuccess = true;
                      }
                    });
                  }

                  return isSuccess;
                }
              });
            });
        })
        .always(function() {
          _resetSelections();
          _doSearch(1);
          app.closeProgressModal();
          if (succeeded)
            app.notifySuccess(app.localize('UNARCHIVEORDERS_SUCCESS'));
        })
        .always(cb);
    },
    canExecute: function() { return self.hasSelections() && !self.isBusy(); }
  });
  
  self.deleteOrdersCommand = ko.asyncCommand({
    execute: function (cb) {
      var succeeded = false;
      app
        .confirm('CONFIRM_DELETEORDERS_TITLE', 'CONFIRM_DELETEORDERS_BODY')
        .then(function () {
          app.showProgressModal('PROGRESS_DELETEORDERS');
        })
        .then(_getSelectedOrderIds)
        .then(function (ids) {
          return config
            .ajaxService
            .postJSON(config.deleteOrdersUrl, { ids: ids })
            .then(function () {
              var lastOrderId = ids[ids.length - 1];
              succeeded = true;
              return $.ajaxPoll({
                url: config.apiUrl,
                type: 'GET',
                data: {q: lastOrderId, qf: ['OrderId'], rf: ['IsDeleted']},
                dataType: 'json',
                pollingType: 'interval',
                interval: 500,
                maxInterval: 5000,
                expireAfter: 10,
                expire: function () {
                  // TODO: show message indicating the operation will take longer
                },
                successCondition: function (res) {
                  var isSuccess;

                  if (res && res.hits && res.hits.hits) {
                    $.each(res.hits.hits, function (index, ival) {

                      if (lastOrderId === ival._id && ival.fields && ival.fields.IsDeleted === true) {
                        app.logTrace('Orders deleted', ids);
                        isSuccess = true;
                      }
                    });
                  }

                  return isSuccess;
                }
              });
            });
        })
        .always(function (res) {
          _resetSelections();
          _doSearch(1);
          self.deletedOrdersSearch().searchCommand.execute();
          app.closeProgressModal();
          if (succeeded)
            app.notifySuccess(app.localize('DELETEORDERS_SUCCESS'));
        })
        .always(cb);
    },
    canExecute: function () { return self.hasSelections() && !self.isBusy(); }
  });

  self.viewOrder = function(data) {
    var status = data._source.OrderStatus;
    switch (status) {
      case 'Processed':
      case 'ShippedDelivered':
      case 'UnderReview':
        self.navigate(config.orderDetailUrl.replace('_id_', data._source.OrderId));
        break;
      default:
        self.navigate(config.editOrderUrl.replace('_id_', data._source.OrderId));
        break;
    }
  };

  self.selectedTab = ko.observable('List');

  self.navigate = function (url) {
    location.href = url;
  };
  
  self.showOrders = function () {
    self.selectedTab('List');
    _doSearch(1);
  };
  

  self.showDeletedOrders = function () {
    if (self.deletedOrdersSearch().itemCount() > 0) {
      self.selectedTab('Deleted');
    }
  };

  //#endregion

  //#region Validation

  self.validationState = ko.validation.applyTo(self, {
    global: {
      minOrderDate: { required: false, validDate: true },
      maxOrderDate: { required: false, validDate: true }
    }
  });

  //#endregion

  //#region Initialization

  _loadState();

  self.filterMode.subscribe(function (mode) {
    if (mode === null) {
      _resetFilter();
    } else if (mode === 'Archived') {
      self.isArchived(true);
    }
  });
  
  // do a search when any of the filters change
  ko.computed(function () {
    self.queryString();
    self.pageSize();
    self.minOrderDate();
    self.maxOrderDate();
    self.orderSource();
    self.orderStatus();
    self.paymentStatus();
    self.product();
    self.isDeleted();
    self.isArchived();
    self.filterMode();
    self.sortDescending();
    self.sortField();
    return;
  }).extend({throttle: 1}).subscribe(function (mode) {
    if (self.minOrderDate.isValid() && self.maxOrderDate.isValid()) {
      _doSearch(1);
    }
  });

  //#endregion
};
