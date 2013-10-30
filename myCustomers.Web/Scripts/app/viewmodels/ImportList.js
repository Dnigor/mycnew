var myc = myc || {};

myc.ImportList = function (config) {  	  
	var self = this,
  _customers = config.pendingCustomers;

	self.pageSize				   = ko.observable(5);
	self.currentPage		   = ko.observable(1);
	self.itemCount			   = ko.observable(_customers().length);
	self.pageCount			   = ko.computed(function() { return Math.ceil(self.itemCount() / self.pageSize()); });
	self.isPrevPageEnabled = ko.computed(function() { return self.currentPage() > 1; });
	self.isNextPageEnabled = ko.computed(function() { return self.currentPage() < self.pageCount(); });
	self.pagedCustomers		 = ko.computed(function() {
		return Enumerable.From(_customers())
					 .Skip((self.currentPage() - 1) * self.pageSize())
					 .Take(self.pageSize())
					 .ToArray();
	});

	self.prevPage = function () { self.currentPage(self.currentPage() - 1); };		
	self.nextPage = function () { self.currentPage(self.currentPage() + 1); };

  self.pageSize.subscribe(function() {
    self.currentPage(1);
  });

  self.isUploadedCustomersVisible = (_customers().length > 0) ? ko.observable(true) : ko.observable(false);
  self.isImportedCustomersVisible = ko.observable(false);  
  self.acknowledge = ko.observable(false);
  self.added = ko.observable(0);
  self.skipped = ko.observable(0);
  self.merged = ko.observable(0);
  self.notimported = ko.observable(0);
  self.total = ko.computed(function () { return self.added() + self.merged(); });
  

  self.isSubscriptionEnabled = function (data) {
    return data.SubscriptionStatus !== 1;
  };

  self.updateSubscription = function (data) {
    return ko.computed({
      read: function () {
        if (data.SubscriptionStatus === 0)
          return true;
        else return false;
      },
      write: function (checked) {
        if (checked)
          data.SubscriptionStatus = 0;
        else
          data.SubscriptionStatus = 2;
      }
    });
  };

  self.selectAction = function(data){
    return ko.computed({
      read: function(){
        return data.ImportAction;
      },
      write: function(value){
        data.ImportAction = value;
      }
    });
  };

  self.importActionTranslations = {
    "Skip" : app.localize("IMPORTCUSTOMERS_SKIP"),
    "Add"  : app.localize("IMPORTCUSTOMERS_ADD"),
    "Merge": app.localize("IMPORTCUSTOMERS_MERGE")
  };

  var _calculateTotals = function () {
    self.added(Enumerable.From(_customers()).Count("$.ImportAction == 'Add' && $.IsFaulted == 'False'"));
    self.skipped(Enumerable.From(_customers()).Count("$.ImportAction == 'Skip' && $.IsFaulted == 'False'"));
    self.merged(Enumerable.From(_customers()).Count("$.ImportAction == 'Merge' && $.IsFaulted == 'False'"));
    self.notimported(Enumerable.From(_customers()).Count("$.IsFaulted == 'True'"));
  };

  self.importCustomersCommand = ko.asyncCommand({
    execute: function () {
      var modelJson = ko.toJSON(_customers());
      config.ajaxService.postJSON(config.importCustomersUrl, modelJson).done(_importCustomersCommandSuccess);
    },
    canExecute: function () { return self.acknowledge() && !config.ajaxService.isBusy(); }
  });

  function _importCustomersCommandSuccess(data) {    
    _customers(data);    
    self.pageSize(5);
    self.currentPage(1);

    _calculateTotals();
    self.isUploadedCustomersVisible(false);
    self.isImportedCustomersVisible(true);
    if (self.total() > 0) {
      app.notifySuccess(app.localizeFormat('IMPORTCUSTOMERS_NOTIFY_SUCCESS', self.total()));
    }
  }

  self.submitSubscriptionsCommand = ko.asyncCommand({
    execute: function () {
      var modelJson = ko.toJSON(_customers());
      config.ajaxService.postJSON(config.submitSubscriptionsUrl, modelJson).done(function () {
        window.location.href = config.importStartPage;
      });
    },
    canExecute: function () { return !config.ajaxService.isBusy() }
  });


}
