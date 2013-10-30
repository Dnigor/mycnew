var myc = myc || {};

myc.GroupsSearch = function (config) {
  var self = this;
  
  self.searchResults = ko.observable([]);
  self.hasResults    = ko.computed(function () { return self.searchResults().length > 0 });
  self.noResults     = ko.computed(function () { return !self.hasResults(); });
  self.isBusy        = ko.computed(function () { return config.ajaxService.isBusy(); });
  self.itemCount     = ko.observable(0);

  function _doSearch() {
    return config
      .ajaxService
      .getJSON(config.apiUrl)
      .done(function (data) {
       self.itemCount(data.length);
       self.searchResults(data);
      });
  }

  //#region Selection

  var _selectedGroups = {};
  var _selectedGroupCount = ko.observable(0);
  var _selectAll = ko.observable(false);

  function _resetSelections() {
    _selectAll(false);
    _selectedGroups = {};
    _selectedGroupCount(0);
  }

  self.hasSelections = ko.computed(function () {
    var all = _selectAll();
    var some = _selectedGroupCount() > 0;
    var none = (!all && _selectedGroupCount() === 0) || (all && _selectedGroupCount() == self.itemCount());
    return !none && (all || some);
  });

  self.selectAll = ko.computed({
    read: function () {
      var all = _selectAll();
      var some = _selectedGroupCount() > 0;
      return all && !some;
    },
    write: function (value) {
      _selectedGroups = {};
      _selectedGroupCount(0);
      _selectAll(value);
    }
  });

  // factory method that creates a computed observable for every check box that binds checked: groupChecked($data)
  self.groupChecked = function($data) {
    return ko.computed({
      read: function() {
        // Important! We must ensure that we invoke the observables used during this read and not let any short circuiting ifs
        // cause them not to get called. Otherwise KO will not wire up the dependancies.
        var all = _selectAll();
        var count = _selectedGroupCount(); // called so that KO will update checked when the count changes
        var selected = typeof(_selectedGroups[$data.GroupId]) !== 'undefined';
        return (all && !selected) || (!all && selected);
      },
      write: function(value) {
        if (_selectAll()) {
          // when in select all mode we track which items are not selected
          if (value) {
            delete _selectedGroups[$data.GroupId];
            _selectedGroupCount(_selectedGroupCount() - 1);
          } else {
            _selectedGroups[$data.GroupId] = $data;
            _selectedGroupCount(_selectedGroupCount() + 1);
          }
        } else {
          // when not in select all mode we track wich items are selected
          if (value) {
            _selectedGroups[$data.GroupId] = $data;
            _selectedGroupCount(_selectedGroupCount() + 1);
          } else {
            delete _selectedGroups[$data.GroupId];
            _selectedGroupCount(_selectedGroupCount() - 1);
          }
        }
      }
    });
  };

  function _getSelectedGroupIds() {
    var ids = [];
    for (var id in _selectedGroups)
      ids.push(id);
    
    if(!_selectAll())
      return $.Deferred().resolve(ids);
   
    return config.ajaxService.getJSON(config.apiUrl)
      .then(function (data) {
        var res = Enumerable
          .From(data)
          .Select(function (g) { return g.GroupId; })
          .Except(ids)
          .ToArray();
        return res;
      });
  }

  //#endregion

  //#region Commands

  self.searchCommand = ko.asyncCommand({
    execute: function (cb) { _doSearch().always(cb); },
    canExecute: function () { return !self.isBusy(); }
  });

  self.deleteGroupsCommand = ko.asyncCommand({
    execute: function (cb) {
      var succeeded = false;
      app
        .confirm('CONFIRM_DELETEGROUPS_TITLE', 'CONFIRM_DELETEGROUPS_BODY')
        .then(function () {
          app.showProgressModal('PROGRESS_DELETEGROUP');
        })
      .then(_getSelectedGroupIds)
      .then(function (ids) {
       return config.ajaxService.postJSON(config.deleteGroupsApiUrl, { ids: ids })
        .then(function () {
          // TODO: This is 5 sec delay and 10 expensive AJAX call to server. ...
          // TODO: ... Need to fix this when Quartet will provide API to search groups by IsDeleted criteria
          succeeded = true;
          return $.ajaxPoll({
            url: config.apiUrl + '/' + ids[ids.length - 1], 
            type: 'GET',
            dataType: 'json',
            pollingType: 'interval',
            interval: 500,
            maxInterval: 5000,
            expireAfter: 10,
            expire: function() { 
              // TODO: show message indicating the operation will take longer
            }
          });
        });
        })
        .always(function (res) {
          _resetSelections();
          _doSearch();
          app.closeProgressModal();
          if (succeeded)
            app.notifySuccess(app.localize('DELETEGROUPS_SUCCESS'));
        })
        .always(cb);
    },
    canExecute: function () { return self.hasSelections() && !self.isBusy(); }
  });

  self.searchCommand.execute();
  
  //#endregion
 
}