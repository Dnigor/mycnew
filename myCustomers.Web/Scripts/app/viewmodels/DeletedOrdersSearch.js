var myc = myc || {};

myc.DeletedOrdersSearch = function (config) {
    var self = this;

    self.searchResults = ko.observable([]);
    self.hasResults = ko.computed(function () { return self.searchResults().length > 0 });
    self.noResults = ko.computed(function () { return !self.hasResults(); });
    self.isBusy = ko.computed(function () { return config.ajaxService.isBusy(); });
    self.itemCount = ko.observable(0);

    function _doSearch(page) {
        var criteria = { s: self.pageSize(), i: page, d: true };
        return config.ajaxService.getJSON(config.apiUrl, criteria)
          .done(function (data) {
              self.currentPage(page);
              self.itemCount(data.hits.total);
              self.searchResults(data.hits.hits);
          });
    };

    //#region Paging

    self.pageSize = ko.observable(10);
    self.currentPage = ko.observable(1);
    self.itemCount = ko.observable(0);
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

    var _selectedDeletedOrders = {};
    var _selectedDeletedOrderCount = ko.observable(0);
    var _selectAll = ko.observable(false);

    function _resetSelections() {
        _selectAll(false);
        _selectedDeletedOrders = {};
        _selectedDeletedOrderCount(0);
    };

    self.hasSelections = ko.computed(function () {
        var all = _selectAll();
        var some = _selectedDeletedOrderCount() > 0;
        var none = (!all && _selectedDeletedOrderCount() === 0) || (all && _selectedDeletedOrderCount() == self.itemCount());
        return !none && (all || some);
    });

    self.selectAll = ko.computed({
        read: function () {
            var all = _selectAll();
            var some = _selectedDeletedOrderCount() > 0;
            return all && !some;
        },
        write: function (value) {
            _selectedDeletedOrders = {};
            _selectedDeletedOrderCount(0);
            _selectAll(value);
        }
    });

    // factory method that creates a computed observable for every check box that binds checked: orderDeletedChecked($data)
    self.orderDeletedChecked = function ($data) {
        return ko.computed({
            read: function () {
                // Important! We must ensure that we invoke the observables used during this read and not let any short circuiting ifs
                // cause them not to get called. Otherwise KO will not wire up the dependancies.
                var all = _selectAll();
                var count = _selectedDeletedOrderCount(); // called so that KO will update checked when the count changes
                var selected = typeof (_selectedDeletedOrders[$data.OrderId]) !== 'undefined';
                return (all && !selected) || (!all && selected);
            },
            write: function (value) {
                if (_selectAll()) {
                    // when in select all mode we track which items are not selected
                    if (value) {
                        delete _selectedDeletedOrders[$data.OrderId];
                        _selectedDeletedOrderCount(_selectedDeletedOrderCount() - 1);
                    } else {
                        _selectedDeletedOrders[$data.OrderId] = $data;
                        _selectedDeletedOrderCount(_selectedDeletedOrderCount() + 1);
                    }
                } else {
                    // when not in select all mode we track wich items are selected
                    if (value) {
                        _selectedDeletedOrders[$data.OrderId] = $data;
                        _selectedDeletedOrderCount(_selectedDeletedOrderCount() + 1);
                    } else {
                        delete _selectedDeletedOrders[$data.OrderId];
                        _selectedDeletedOrderCount(_selectedDeletedOrderCount() - 1);
                    }
                }
            }
        });
    };

    // self.rolodex = ko.observableArray(Enumerable.Range(65, 26).Select('String.fromCharCode($)').ToArray());

    function _getSelectedOrderIds() {
        var ids = [];
        for (var id in _selectedDeletedOrders)
            ids.push(id);

        // if not in select all mode then just return the selected ids
        if (!_selectAll())
            return $.Deferred().resolve(ids);

        // for select all mode fetch all the order ids for the current search
        // and exclude any that have be explicitly unchecked
        var criteria = { s: 65535, i: 1, d: true };


        // NOTE: pass empty result fields 'rf=' qstring param to cause the search to not return any source data
        // but just return the doc id which is the same guid as the order id.
        return config.ajaxService.getJSON(config.apiUrl, criteria)
          .then(function (data) {
              var res = Enumerable
                .From(data.hits.hits)
                .Select(function (c) { return c._id; })
                .Except(ids)
                .ToArray();
              return res;
          });
    }

    //#endregion


    //#region Commands

    self.searchCommand = ko.asyncCommand({
        execute: function (cb) { _resetSelections(); _doSearch(1).always(cb); }
    });


    self.unDeleteOrdersCommand = ko.asyncCommand({
        execute: function (cb) {
            var succeeded = false;
            app
              .confirm('CONFIRM_RESTOREORDERS_TITLE', 'CONFIRM_RESTOREORDERS_BODY')
              .then(function () {
                  app.showProgressModal('PROGRESS_RESTOREORDERS');
              })
              .then(_getSelectedOrderIds)
              .then(function (ids) {
                  return config
                    .ajaxService
                    .postJSON(config.unDeleteOrdersUrl, { ids: ids })
                    .then(function () {
                      var lastOrderId = ids[ids.length - 1];
                        succeeded = true;
                        return $.ajaxPoll({
                            url: config.apiUrl,
                            type: 'GET',
                            data: {q: lastOrderId, qf:['OrderId'], rf:['IsDeleted']},
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

                                  if (lastOrderId === ival._id && ival.fields && ival.fields.IsDeleted === false) {
                                    app.logTrace('Orders undeleted', ids);
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
                  app.closeProgressModal();
                  if (succeeded)
                      app.notifySuccess(app.localize('RESTOREORDERS_SUCCESS'));
              })
              .always(cb);
        },
        canExecute: function () { return self.hasSelections() && !self.isBusy(); }
    });
  
    self.purgeOrdersCommand = ko.asyncCommand({
      execute: function (cb) {
        var succeeded = false;
        app
          .confirm('CONFIRM_PURGEORDERS_TITLE', 'CONFIRM_PURGEORDERS_BODY')
          .then(function () {
            app.showProgressModal('PROGRESS_PURGEORDERS');
          })
          .then(_getSelectedOrderIds)
          .then(function (ids) {
            return config
              .ajaxService
              .postJSON(config.purgeOrdersUrl, { ids: ids })
              .then(function () {
                var lastOrderId = ids[ids.length - 1];
                succeeded = true;
                return $.ajaxPoll({
                  url: config.apiUrl,
                  type: 'GET',
                  data: { q: lastOrderId, qf: ['OrderId'], rf: ['IsDeleted'] },
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
                      if (res.hits.hits.length == 0) {
                        app.logTrace('Orders permanently deleted', ids);
                        isSuccess = true;
                      }
                    }

                    return isSuccess;
                  }
                });
              });
          })
          .always(function (res) {
            _resetSelections();
            _doSearch(1);
            app.closeProgressModal();
            if (succeeded)
              app.notifySuccess(app.localize('PURGEORDERS_SUCCESS'));
          })
          .always(cb);
      },
      canExecute: function () { return self.hasSelections() && !self.isBusy(); }
    });

    //#endregion

    self.pageSize.subscribe(function () {
        self.searchCommand.execute();
    });

    
    //#endregion

}