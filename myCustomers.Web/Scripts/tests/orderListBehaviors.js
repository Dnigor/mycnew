describe("Order list", function () {

  var emptyMethod = function () { };

  var emptySearchResult = {
    hits: {
      total: 0,
      hits: []
    }
  };

  // stub out state service
  app.session = {
    setItem: jasmine.createSpy('setItem'),
    getItem: jasmine.createSpy('getItem').andReturn(null)
  };

  it("should use session from app object", function () {
    var config = {
      pageSize: 10,
      ajaxService: {
        isBusy: ko.observable(false),
        get: jasmine.createSpy('get')
      },
      enablePersistence: true
    };

    var model = new myc.OrderList(config);

    expect(app.session.getItem).wasCalled();
  });


  it("should use ajax service from config object", function () {
    var config = {
      pageSize: 10,
      ajaxService: {
        isBusy: ko.observable(false),
        getJSON: jasmine.createSpy('getJSON').andCallFake(function () {
          return $.Deferred(function () {
            return emptySearchResult;
          })})
      }
    };

    var model = new myc.OrderList(config);
    model.searchCommand.execute();

    expect(config.ajaxService.getJSON).wasCalled();
  });

  it("should use orders api url from config object", function () {
    var config = {
      apiUrl: '/api/orders',
      pageSize: 10,
      ajaxService: {
        isBusy: ko.observable(false),
        getJSON: jasmine.createSpy('getJSON').andCallFake(function () {
          return $.Deferred(function () {
            return emptySearchResult;
          })
        })
      }
    };

    var model = new myc.OrderList(config);
    model.searchCommand.execute();

    expect(config.ajaxService.getJSON.mostRecentCall.args[0]).toBe(config.apiUrl);
  });

  it("should disable searchCommand when ajax call is busy", function() {
    // Arrange
    var config = {
      pageSize: 10,
      ajaxService: {
        isBusy: ko.observable(false),
        get: emptyMethod
      }
    };

    var model = new myc.OrderList(config);
    expect(model.searchCommand.canExecute()).toBe(true);

    config.ajaxService.isBusy(true);
    expect(model.searchCommand.canExecute()).toBe(false);
  })

  it("should disable prevPageCommand when currentPage is 1", function () {
    var config = {
      pageSize: 10,
      ajaxService: {
        isBusy: ko.observable(false),
        get: emptyMethod
      }
    };

    var model = new myc.OrderList(config);
    model.currentPage(1);

    expect(model.prevPageCommand.canExecute()).toBe(false);
  })

  it("should disable prevPageCommand when ajax call is busy", function () {
    var config = {
      pageSize: 10,
      ajaxService: {
        isBusy: ko.observable(false),
        get: emptyMethod
      }
    };

    var model = new myc.OrderList(config);

    model.currentPage(2);

    expect(model.prevPageCommand.canExecute()).toBe(true);
    config.ajaxService.isBusy(true);
    expect(model.prevPageCommand.canExecute()).toBe(false);
  })

  it("should disable nextPageCommand when currentPage is equal to pageCount", function () {
    var config = {
      pageSize: 10,
      ajaxService: {
        isBusy: ko.observable(false),
        get: emptyMethod
      }
    };

    var model = new myc.OrderList(config);

    model.itemCount(100);
    model.currentPage(10);

    expect(model.nextPageCommand.canExecute()).toBe(false);
  })

  it("should disable nextPageCommand when ajax call is busy", function () {
    var config = {
      pageSize: 10,
      ajaxService: {
        isBusy: ko.observable(false),
        get: emptyMethod
      }
    };

    var model = new myc.OrderList(config);

    model.itemCount(100);

    expect(model.nextPageCommand.canExecute()).toBe(true);
    config.ajaxService.isBusy(true);
    expect(model.nextPageCommand.canExecute()).toBe(false);
  })

  it("should add order id to orderDetailUrl using format passed in config object for Processed", function () {
    var config = {
      orderDetailUrl: '/orders/_id_',
      pageSize: 10,
      ajaxService: {
        isBusy: ko.observable(false),
        get: emptyMethod
      }
    };

    var model = new myc.OrderList(config);
    model.navigate = jasmine.createSpy('navigate');
    model.viewOrder({ _source: { OrderId: '1', OrderStatus: 'Processed' } });

    expect(model.navigate).toHaveBeenCalledWith('/orders/1');
  });

  it("should add order id to orderDetailUrl using format passed in config object for ShippedDelivered", function () {
    var config = {
      orderDetailUrl: '/orders/_id_',
      pageSize: 10,
      ajaxService: {
        isBusy: ko.observable(false),
        get: emptyMethod
      }
    };

    var model = new myc.OrderList(config);
    model.navigate = jasmine.createSpy('navigate');
    model.viewOrder({ _source: { OrderId: '1', OrderStatus: 'ShippedDelivered' } });

    expect(model.navigate).toHaveBeenCalledWith('/orders/1');
  });

  it("should add order id to editOrderUrl using format passed in config object for Pending", function () {
    var config = {
      editOrderUrl: '/editorders/_id_',
      pageSize: 10,
      ajaxService: {
        isBusy: ko.observable(false),
        get: emptyMethod
      }
    };

    var model = new myc.OrderList(config);
    model.navigate = jasmine.createSpy('navigate');
    model.viewOrder({ _source: { OrderId: '1', OrderStatus: 'Pending' } });

    expect(model.navigate).toHaveBeenCalledWith('/editorders/1');
  });
});