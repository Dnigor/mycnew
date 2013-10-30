describe("Customer list", function () {
	var config = {
		search: '/search',
		searchNoDetail: '/searchNoDetail',
		baseUrl: '~/',
		showImages: 'true',
    ajaxService: {
      isBusy: ko.observable(false),
      get: jasmine.createSpy('get'),
      post: jasmine.createSpy('post')
    }
	};

	var emptySearchResult = {
		hits: {
			total: 0,
			hits: []
		}
	};

	it("should use api url from config object", function () {
	  var config = {
      apiUrl: '/api/customers',
      pageSize: 10,
      ajaxService: {
        isBusy: ko.observable(false),
        getJSON: jasmine.createSpy('post').andCallFake(function () { return $.Deferred(function () {
          return emptySearchResult;
        })})
      }
    };

    var model = new myc.CustomerList(config);
    model.searchCommand.execute();

    expect(config.ajaxService.getJSON.mostRecentCall.args[0]).toBe(config.apiUrl);
	});

	it("should have selections when selectAll is true and no customers have been unchecked", function () {
    //Arrange
	  var model = new myc.CustomerList(config);
    model.itemCount(10);

    //Act
    model.selectAll(true);

    // Assert
    expect(model.hasSelections()).toBe(true);
	});

	it("should not have selections when selectAll is true and all customers have been manually uchecked", function () {
    //Arrange
	  var model = new myc.CustomerList(config);
    model.itemCount(3);

    //Act
    model.selectAll(true);
    model.customerChecked({ CustomerId: 1 })(false);
    model.customerChecked({ CustomerId: 2 })(false);
    model.customerChecked({ CustomerId: 3 })(false);

    // Assert
    expect(model.hasSelections()).toBe(false);
	});
});
