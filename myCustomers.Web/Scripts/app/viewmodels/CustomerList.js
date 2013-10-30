var myc = myc || {};

myc.CustomerList = function (config) {
  var self = this;

  self.isBusy = ko.computed(function() { return config.ajaxService.isBusy(); });

  //#region Search

  self.queryString   = ko.observable('');
  self.searchResults = ko.observable([]);
  self.hasResults    = ko.computed(function () { return self.searchResults().length > 0 });
  self.noResults     = ko.computed(function () { return !self.hasResults(); });

  function _doSearch(page) {
    if (self.selectedTab() !== 'List')
      return;

    var criteria = _getCriteria();
    criteria.i = page;

    return config.ajaxService.getJSON(config.apiUrl, criteria)
      .done(function (data) {
        self.currentPage(page);
        self.itemCount(data.hits.total);
        self.searchResults(data.hits.hits);
        _saveState();
      });
  }

  function _getCriteria() {
    var criteria = {
      q:    self.queryString(),
      ln:   self.lastNamePrefix(),
      sf:   _sortField(),
      sd:   self.sortDescending(),
      s:    self.pageSize(),
      i:    self.currentPage(),
      bdms: null,
      bdme: null,
      ams:  null,
      ame:  null,
      das:  null,
      dae: null,
      pds: null,
      pde: null,
      lods: null,
      lode: null,
      drs:  null,
      dre:  null,
      d:    false,
      a:    false
    };

    var filterMode = self.filterMode();

    if (filterMode === 'Birthday' || filterMode === 'Advanced') {
      criteria.bdms = self.selectedBirthMonth();
      criteria.bdme = self.selectedBirthMonth();
    }

    if (filterMode === 'Anniversary' || filterMode === 'Advanced') {
      criteria.ams = self.selectedAnniversaryMonth();
      criteria.ame = self.selectedAnniversaryMonth();
    }

    if (filterMode === 'ProfileDate' || filterMode === 'Advanced') {
      criteria.pds = self.minProfileDate();
      criteria.pde = self.maxProfileDate();

      if (self.maxProfileDate()) {       
        var time = moment(self.maxProfileDate());
        if (time._d.getSeconds() != 59) {
          criteria.pde = time.add('days', 1).subtract('seconds', 1)._d;
        }
      }
    }

    if (filterMode === 'OrderDate' || filterMode === 'Advanced') {
      criteria.lods = self.minLastOrderDate();
      criteria.lode = self.maxLastOrderDate();

      if (self.maxLastOrderDate()) {
        var time = moment(self.maxLastOrderDate());
        if (time._d.getSeconds() != 59) {
          criteria.lode = time.add('days', 1).subtract('seconds', 1)._d;
        }
      }
    }

    if (filterMode === 'Deleted' || filterMode === 'Advanced') {
      criteria.d = self.isDeleted();
    }

    if (filterMode === 'Archived' || filterMode === 'Advanced') {
      criteria.a = self.isArchived();
    }

    if (criteria.das !== null && criteria.das !== '')
      criteria.das = app.toISO8601(criteria.das);

    if (criteria.dae !== null && criteria.dae !== '')
      criteria.dae = app.toISO8601(criteria.dae);

    if (criteria.lods !== null && criteria.lods !== '')
      criteria.lods = app.toISO8601(criteria.lods);

    if (criteria.lode !== null && criteria.lode !== '')
      criteria.lode = app.toISO8601(criteria.lode);

    if (criteria.drs !== null && criteria.drs !== '')
      criteria.drs = app.toISO8601(criteria.drs);

    if (criteria.dre !== null && criteria.dre !== '')
      criteria.dre = app.toISO8601(criteria.dre);

    if (criteria.pds !== null && criteria.pds !== '')
      criteria.pds = app.toISO8601(criteria.pds);

    if (criteria.pde !== null && criteria.pde !== '')
      criteria.pde = app.toISO8601(criteria.pde);

    return criteria;
  }

  self.searchCommand = ko.asyncCommand({
    execute: function (cb) { _resetSelections(); _doSearch(1).always(cb); },
    canExecute: function () { return !self.isBusy(); }
  });

  //#endregion

  //#region Filters

  self.filterMode               = ko.observable(null);
  self.lastNamePrefix           = ko.observable(null);
  self.selectedAnniversaryMonth = ko.observable(null);
  self.selectedBirthMonth       = ko.observable(null);
  self.minDateAdded             = ko.observable(null);
  self.maxDateAdded             = ko.observable(null);
  self.minProfileDate           = ko.observable(null);
  self.maxProfileDate           = ko.observable(null);
  self.minLastOrderDate         = ko.observable(null);
  self.maxLastOrderDate         = ko.observable(null);
  self.isDeleted                = ko.observable(false);
  self.isArchived               = ko.observable(false);
  self.sortDescending           = ko.observable(false);

  function _resetFilter() {
    self.selectedAnniversaryMonth('');
    self.selectedBirthMonth('');
    self.minDateAdded(null);
    self.maxDateAdded(null);
    self.minProfileDate(null);
    self.maxProfileDate(null);
    self.minLastOrderDate(null);
    self.maxLastOrderDate(null);   
    self.isDeleted(false);
    self.isArchived(false);
  }

  //#endregion

  //#region Sorting

  _sortField = ko.observable('LastName.Sort');
  self.sortField = ko.computed({
    read: function() {
      var field = _sortField();
      switch(field) {
        case 'FirstName.Sort':
          return app.localize("SORT_FIRSTNAME");
        case 'LastName.Sort':
          return app.localize("SORT_LASTNAME");
        case 'ProfileDateUtc':
          return app.localize("SORT_PROFILEDATE");
        case 'DateAddedUtc':
          return app.localize("SORT_DATEADDED");
        case 'LastOrderDateUtc':
          return app.localize("SORT_LASTORDERDATE");
        default:
          return app.localize("SORT_RELEVANCE");
      }
    },
    write: function(value) {
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
        switch(value)
        {
          case 'DateAddedUtc':          
          case 'LastOrderDateUtc':
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

  self.pageSize    = ko.observable(10);
  self.currentPage = ko.observable(1);
  self.itemCount   = ko.observable(0);
  self.pageCount   = ko.computed(function() { return Math.ceil(self.itemCount() / self.pageSize()); });

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

  var _selectedCustomers     = {};
  var _selectedCustomerCount = ko.observable(0)
  var _selectAll             = ko.observable(false);

  function _resetSelections() {
    _selectAll(false);
    _selectedCustomers = {};
    _selectedCustomerCount(0);
  }

  self.hasSelections = ko.computed(function() { 
    var all = _selectAll();
    var some = _selectedCustomerCount() > 0;
    var none = (!all && _selectedCustomerCount() === 0) || (all && _selectedCustomerCount() == self.itemCount());
    return !none && (all || some);
  });

  self.selectAll = ko.computed({
    read: function() { 
      var all = _selectAll();
      var some = _selectedCustomerCount() > 0;
      return all && !some; 
    },
    write: function(value) {
      _selectedCustomers = {};
      _selectedCustomerCount(0);
      _selectAll(value);
    }
  });

  // factory method that creates a computed observable for every check box that binds checked: customerChecked($data)
  self.customerChecked = function($data) {
    return ko.computed({
      read: function() {
        // Important! We must ensure that we invoke the observables used during this read and not let any short circuiting ifs
        // cause them not to get called. Otherwise KO will not wire up the dependancies.
        var all = _selectAll();
        var count = _selectedCustomerCount(); // called so that KO will update checked when the count changes
        var selected = typeof(_selectedCustomers[$data.CustomerId]) !== 'undefined'
        return (all && !selected) || (!all && selected);
      },
      write: function(value) {
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

  self.rolodex = ko.observableArray(Enumerable.Range(65, 26).Select('String.fromCharCode($)').ToArray());

  function _getSelectedCustomerEcardDetails() {
    var idLookup = {};
    var ids = [];
    for (var id in _selectedCustomers) {
      ids.push(id);
      idLookup[id] = true;
    }

    var criteria = _getCriteria();
    criteria.i = 1;
    criteria.s = 65535;
    criteria.rf = ['Email','FirstName','LastName', 'MkeCardsSubscriptionStatus'];

    return config.ajaxService.getJSON(config.apiUrl, criteria)
      .then(function(data) {
        return Enumerable
          .From(data.hits.hits)
          .Where(function(c) {
            var hasEmail  = app.isNonEmptyString(c.fields.Email);
            var isOptedIn = c.fields.MkeCardsSubscriptionStatus === 'OptedIn';
            var included  = (_selectAll() && !idLookup.hasOwnProperty(c._id)) || (!_selectAll() && idLookup.hasOwnProperty(c._id));
            return hasEmail && isOptedIn && included
          })
          .Select(function(c) { 
            return {
              CustomerId: c._id,
              Email:      c.fields.Email,
              FirstName:  c.fields.FirstName,
              LastName:   c.fields.LastName
            }; 
          })
          .ToArray();
      });
  }

  function _getSelectedCustomerIds() {
    var ids = [];
    for (var id in _selectedCustomers)
      ids.push(id);

    // if not in select all mode then just return the selected ids
    if (!_selectAll())
      return $.Deferred().resolve(ids);

    // for select all mode fetch all the customer ids for the current search
    // and exclude any that have be explicitly unchecked
    var criteria = _getCriteria();
    criteria.i = 1;
    criteria.s = 65535;

    // NOTE: pass empty result fields 'rf=' qstring param to cause the search to not return any source data
    // but just return the doc id which is the same guid as the customer id.
    return config.ajaxService.getJSON(config.apiUrl + '?rf=', criteria)
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

  //#region Actions

  function _export(url, ids) {
    var res = $.Deferred();

    $.fileDownload(url, {
      httpMethod: 'POST',
      data: { id: ids },
      successCallback: function() {  res.resolve(); },
      failCallback: function(err) { 
        app.notifyError(app.localize('CUSTOMERLIST_EXPORT_ERROR')); 
        res.reject(err);
      },
    });

    return res;
  }

  self.printCommand = ko.asyncCommand({
    execute: function(cb) {
      _getSelectedCustomerIds()
        .then(function(ids) {
          return _export(config.printUrl, ids);
        })
        .always(cb);
    },
    canExecute: function() { return self.hasSelections() && !self.isBusy(); }
  });

  self.labelsCommand = ko.asyncCommand({
    execute: function(cb) {
      _getSelectedCustomerIds()
        .then(function(ids) {
          return _export(config.labelsUrl, ids);
        })
      .always(cb);
    },
    canExecute: function() { return self.hasSelections() && !self.isBusy(); }
  });

  self.exportCommand = ko.asyncCommand({
    execute: function(cb) {
      _getSelectedCustomerIds()
        .then(function(ids) {
          return _export(config.exportUrl, ids);
        })
      .always(cb);
    },
    canExecute: function() { return self.hasSelections() && !self.isBusy(); }
  });

  self.archiveCustomersCommand = ko.asyncCommand({
    execute: function(cb) {      
      var succeeded = false;
      app
        .confirm('CONFIRM_ARCHIVECUSTOMERS_TITLE', 'CONFIRM_ARCHIVECUSTOMERS_BODY')
        .then(function() {
          app.showProgressModal('PROGRESS_ARCHIVECUSTOMER');
        })
        .then(_getSelectedCustomerIds)
        .then(function (ids) {
          var lastCustomerId = ids[ids.length - 1];
          return config
            .ajaxService
            .postJSON(config.archiveApiUrl, { ids: ids })
            .then(function() {
              succeeded = true;
              return $.ajaxPoll({
                url: config.apiUrl, 
                type: 'GET',
                data: { q: lastCustomerId, qf: ['CustomerId'], rf: ['IsArchived'] },
                dataType: 'json',
                pollingType: 'interval',
                interval: 500,
                maxInterval: 5000,
                expireAfter: 10,
                expire: function() { 
                  // TODO: show message indicating the operation will take longer
                },
                successCondition: function(res) {
                  var isSuccess;

                  if (res && res.hits && res.hits.hits) {
                    $.each(res.hits.hits, function (index, ival) {

                      if (lastCustomerId === ival._id && ival.fields && ival.fields.IsArchived === true) {
                        app.logTrace('Customers archived', ids);
                        isSuccess = true;
                      }
                    });
                  }

                  return isSuccess;
                }
              });
            });
        })
        .always(function(res) {
          _resetSelections();
          _doSearch(1);
          app.closeProgressModal();
          if (succeeded)
            app.notifySuccess(app.localize('ARCHIVECUSTOMERS_SUCCESS'));
        })
        .always(cb);
    },
    canExecute: function() { return self.hasSelections() && !self.isBusy(); }
  });

  self.unarchiveCustomersCommand = ko.asyncCommand({
    execute: function (cb) {
      var succeeded = false;
      app
        .confirm('CONFIRM_UNARCHIVECUSTOMERS_TITLE', 'CONFIRM_UNARCHIVECUSTOMERS_BODY')
        .then(function() {
          app.showProgressModal('PROGRESS_UNARCHIVECUSTOMER');
        })
        .then(_getSelectedCustomerIds)
        .then(function (ids) {
          var lastCustomerId = ids[ids.length - 1];
          return config
            .ajaxService
            .postJSON(config.unarchiveApiUrl, { ids: ids })
            .then(function() {
              succeeded = true;
              return $.ajaxPoll({
                url: config.apiUrl, 
                type: 'GET',
                data: { q: lastCustomerId, qf: ['CustomerId'], rf: ['IsArchived'] },
                dataType: 'json',
                pollingType: 'interval',
                interval: 500,
                maxInterval: 5000,
                expireAfter: 10,
                expire: function() { 
                  // TODO: show message indicating the operation will take longer
                },
                successCondition: function(res) {
                  var isSuccess;

                  if (res && res.hits && res.hits.hits) {
                    $.each(res.hits.hits, function (index, ival) {

                      if (lastCustomerId === ival._id && ival.fields && ival.fields.IsArchived === false) {
                        app.logTrace('Customers unarchived', ids);
                        isSuccess = true;
                      }
                    });
                  }

                  return isSuccess;
                }
              });
            });
        })
        .always(function(res) {
          _resetSelections();
          _doSearch(1);
          app.closeProgressModal();
          if (succeeded)
            app.notifySuccess(app.localize('UNARCHIVECUSTOMERS_SUCCESS'));
        })
        .always(cb);
    },
    canExecute: function () { return self.hasSelections() && !self.isBusy(); }
  });
    
  self.addToGroupCommand = ko.asyncCommand({
    execute: function(cb) {
      var succeeded = false;
      app.confirm('ADDTOGROUP_MODAL_TITLE', 'ADDTOGROUP_MODAL_DROPDOWNLABEL', [$('#addToGroupModal').html()], 'ADDTOGROUP_MODAL_BUTTONADD')
      .then(function() {
        app.showProgressModal('PROGRESS_ADDTOGROUP');
      })
      .then(_getSelectedCustomerIds)
      .then(function(ids) {
        var groupId = $('select[name="addToGroupSelect"]:not(#addToGroupModal select[name="addToGroupSelect"])').val();
        return config
          .ajaxService
          .postJSON(config.addToGroupApiUrl, { GroupId: groupId, Ids: ids })
          .then(function() {
            succeeded = true;
          });
      })
        .always(function (res) {
          _resetSelections();
          app.closeProgressModal();
          if (succeeded)
            app.notifySuccess(app.localize('ADDTOGROUP_SUCCESS'));
        })
        .always(cb);
    },
    canExecute: function() { return self.hasSelections() && !self.isBusy(); }
  });
  

  self.deleteCustomersCommand = ko.asyncCommand({
    execute: function (cb) {
      var succeeded = false;
      app
        .confirm('CONFIRM_DELETECUSTOMERS_TITLE', 'CONFIRM_DELETECUSTOMERS_BODY')
        .then(function () {
          app.showProgressModal('PROGRESS_DELETECUSTOMERS');
        })
        .then(_getSelectedCustomerIds)
        .then(function (ids) {
          var lastCustomerId = ids[ids.length - 1];
          return config
            .ajaxService
            .postJSON(config.deleteCustomersUrl, { ids: ids })
            .then(function () {
              succeeded = true;
              return $.ajaxPoll({
                url: config.apiUrl,
                type: 'GET',
                data: { q: lastCustomerId, qf: ['CustomerId'], rf: ['IsDeleted'] },
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

                      if (lastCustomerId === ival._id && ival.fields && ival.fields.IsDeleted === true) {
                        app.logTrace('Customers deleted', ids);
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
          self.deletedCustomersSearch().searchCommand.execute();
          app.closeProgressModal();
          if (succeeded)
            app.notifySuccess(app.localize('DELETECUSTOMERS_SUCCESS'));
        })
        .always(cb);
    },
    canExecute: function () { return self.hasSelections() && !self.isBusy(); }
  });

  self.sendeCardCommand = ko.asyncCommand({
    execute: function(cb) {
      app
        .confirm('CONFIRM_SUBMITECARD_TITLE', 'CONFIRM_SUBMITECARD_BODY')
        .then(function() {
          app.showProgressModal('SUBMITTING_ECARD');
          return _getSelectedCustomerEcardDetails();
        })
        .then(function(customers) {
          // _getSelectedCustomerEcardDetails will not return customer's
          // that have no email address or have opted out. Verify that
          // after the selection list has been filtered that there are
          // still customers to send an ecard to
          if (customers.length === 0) {
            app.notifyError(app.localize('ECARDS_VALIDATION_MESSAGE'), app.localize('ECARDS_VALIDATION_TITLE'));
            return $.Deferred().reject();
          }

          var message = {
            ContentId:      config.eCards.contentId,
            SubsidiaryCode: config.eCards.subsidiaryCode,
            ReturnUrl:      config.eCards.returnUrl,
            ContentMode:    config.eCards.contentMode,
            Recipients:     []
          };

          // Add customers to the message
          $.each(customers, function(i, c) {
            message.Recipients.push({
              RecipientKey: c.CustomerId,
              EmailAddress: c.Email,
              Attributes: {
                RecipientKey:   c.CustomerId,
                CustomerId:     c.CustomerId,
                SubsidaryCode:  config.eCards.subsidiaryCode,
                FirstName:      c.FirstName,
                LastName:       c.LastName
              }
            });
          });

          return config.eCards.ecardService.submit(message, config.eCards.consultantId);
        })
        .then(function(res) {
          // redirect to ecards application
          window.location.href = res.continueUrl;
        })
        .fail(app.closeProgressModal) // only close the modal on fail because the success case will redirect to a new page
        .always(cb);
    },
    canExecute: function() { return self.hasSelections() && config.isEcardsEnabled; }
  });

  //#endregion

  //#region Navigation

  self.selectedTab = ko.observable('List');

  self.onCustomerRowClick = function() {
    var url = config.targetUrlFormat.replace('_id_', this._id);
    self.navigate(url);
  }

  self.onNoteRowClick = function() {
    var url = config.targetUrlFormat.replace('_id_', this._source.CustomerId);
    self.navigate(url);
  }
  
  self.onGroupRowClick = function (group) {
    var url = config.groupUrl.replace('_id_', group.GroupId);    
    url += '&groupName=' + group.Name;
    self.navigate(url);
  }

  self.onNewOrderClick = function() {
    var url = config.newOrderUrl.replace('_id_', this.CustomerId);
    self.navigate(url);
  }

  self.navigate = function (url) {
    location.href = url;
  };

  self.showCustomers = function() {
    self.selectedTab('List');
    _doSearch(1);
  };

  //#endregion

  //#region Notes

  self.notesSearch = ko.observable(config.notesSearch);
  
  self.showNotes = function() {
    self.selectedTab('Notes');
  };
  
  //#endregion
  
  //#region Groups
  
  self.groupsSearch = ko.observable(config.groupsSearch);

  self.showGroups = function () {
    self.selectedTab('Groups');
    self.groupsSearch().searchCommand.execute();
  };
  
  //#endregion

  
  //#region Deleted Customers
 
  self.deletedCustomersSearch = ko.observable(config.deletedCustomersSearch);
 
  self.showDeletedCustomers = function () {
    if (self.deletedCustomersSearch().itemCount() > 0) {
      self.selectedTab('Deleted');
    }
  };

  //#endregion

  //#region Profile Images

  self.showImages = ko.observable(false);
  self.toggleImages = function() { self.showImages(!self.showImages()); }

  self.profileImageUrl = function($data) {
    if ($data.ImageLastUpdatedDateUtc !== null) {
      var ticks = moment($data.ImageLastUpdatedDateUtc);
      var url = config.profileImageUrl.replace('_id_', $data.CustomerId);
      return url + '?h=74&ts=' + encodeURIComponent(app.toISO8601(new Date(ticks)));
    }

    return config.defaultProfileImageUrl;
  };

  //#endregion

  //#region State

  var _stateKey = 'myc.customerList.state.v1';
  function _saveState() {
    if (!config.enablePersistence)
      return;

    var state = {
      filterMode: self.filterMode(),
      criteria: _getCriteria(),
      showImages: self.showImages()
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
      self.queryString(state.criteria.q);

      self.lastNamePrefix(state.criteria.ln);
      self.selectedAnniversaryMonth(state.criteria.ams);
      self.selectedBirthMonth(state.criteria.bdms);
      self.minDateAdded(state.criteria.das);
      self.maxDateAdded(state.criteria.dae);
      self.minProfileDate(state.criteria.pds);
      self.maxProfileDate(state.criteria.pde);
      self.minLastOrderDate(state.criteria.lods);
      self.maxLastOrderDate(state.criteria.lode);     
      self.isDeleted(state.criteria.d);
      self.isArchived(state.criteria.a);

      if (!isNaN(state.s))
        self.pageSize(state.criteria.s);

      if (!isNaN(state.criteria.i))
        self.currentPage(state.criteria.i);

      self.filterMode(state.filterMode);

      var showImages = state.showImages;
      if (showImages === undefined || showImages === null)
        showImages = true;
      self.showImages(showImages);

      self.sortDescending(state.criteria.sd);
      _sortField(state.criteria.sf);
    }
  }

  //#endregion

  //#region Initialization

  _loadState();

  // reset the sort when user changes the search query
  // null sort field defaults to 'relevance'
  self.queryString.subscribe(function(value) {
    if (value) {
      self.sortDescending(null);
      self.sortField(null);
    }
    else {
      self.sortDescending(false);
      self.sortField('LastName.Sort');
    }
  });

  // clear filter data when user removes the filter
  self.filterMode.subscribe(function (mode) {
    if (mode === null) {
      _resetFilter();
    } else if (mode === 'Archived') {
      self.isArchived(true);
    }
  });

  self.validationState = ko.validation.applyTo(self, {
    global: {
      minProfileDate: {
        required: false,
        validDate: true
      },
      maxProfileDate: {
        required: false,
        validDate: true
      },
      minLastOrderDate: {
        required: false,
        validDate: true
      },
      maxLastOrderDate: {
        required: false,
        validDate: true
      },
      
    }
  });

  // trigger a search when any of these properties change
  ko.computed(function() {
    self.queryString();
    self.pageSize();
    self.lastNamePrefix();
    self.selectedAnniversaryMonth();
    self.selectedBirthMonth();
    self.minDateAdded();
    self.maxDateAdded();
    self.minLastOrderDate();
    self.maxLastOrderDate();    
    self.isDeleted();
    self.isArchived();
    self.maxProfileDate();
    self.minProfileDate();
    self.filterMode();
    self.sortDescending();
    self.sortField();
    return;
  }).extend({ throttle: 1 }).subscribe(function (mode) {
    if (self.validationState.isValid()) {
      _doSearch(1);
    }
  });
 
  // save state when showImages changes
  self.showImages.subscribe(function () {
    _saveState();
  });

  //#endregion
};
