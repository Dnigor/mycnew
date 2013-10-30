describe('ImportList', function () {
  var config = {
    pendingCustomers: ko.observableArray([]),
    importCustomersUrl: '~/api/import/importcustomers',
    submitSubscriptionsUrl: '~/api/subscriptions/submitsubscriptions',
    ajaxService: {
      isBusy: ko.observable(false),
      postJSON: jasmine.createSpy('postJSON').andCallFake(function () { return $.Deferred(function () { })})
    }
  };

  it('should make post call to import customers', function () {

    var importService = new myc.ImportList(config);
    importService.acknowledge(true);
    importService.importCustomersCommand.execute();
    expect(config.ajaxService.postJSON).wasCalled();
  });

  it('should not import customers when acknowledgement checkbox not checked', function () {

    var importService = new myc.ImportList(config);
    importService.acknowledge(false);
    expect(importService.importCustomersCommand.canExecute()).toBe(false);
  });

  it('should make post call to submit subscriptions', function () {
    var importService = new myc.ImportList(config);
    importService.submitSubscriptionsCommand.execute();
    expect(config.ajaxService.postJSON).wasCalled();
  });

  it('should disable prevPage when current page is 1', function () {

    var importService = new myc.ImportList(config);
    importService.currentPage(1);
    expect(importService.isPrevPageEnabled()).toBe(false);
  });

  it('should disable nextPage when current page is last', function () {

    var importService = new myc.ImportList(config);
    importService.itemCount(5);
    importService.currentPage(1);
    expect(importService.isNextPageEnabled()).toBe(false);
  });
});