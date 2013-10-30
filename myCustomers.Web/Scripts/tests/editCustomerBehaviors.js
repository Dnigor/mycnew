describe('EditCustomer', function () {
  var config = {
    validationUrl: '@Url.RouteUrl(new { httproute = "ValidationApi", controller = "validation", action = "_action_" })',
    socialNetworksConfig: [],
    ajaxService: {
      isBusy: ko.observable(false),
      getJSON: jasmine.createSpy('getJSON').andCallFake(function () {
        return $.Deferred(function () { });
      }),
      postJSON: jasmine.createSpy('postJSON').andCallFake(function () {
        return $.Deferred(function () { });
      })
    }
  };

  app.subsidiary = 'xx';
  app.showValidationErrors = ko.observable(false);

  it('should make post call to save customer', function () {

    var model = new myc.EditCustomer(config);
    var customer = new myc.Customer({ profileQuestionGroups: [] }, config);
    model.customer(customer);

    model.saveCommand.execute();
    expect(config.ajaxService.postJSON).wasCalled();
  });

  it('should make get call to load profileQuestionGroups', function () {
    config.isNew = true;
    var model = new myc.EditCustomer(config);
    model.init();
    expect(config.ajaxService.getJSON).wasCalled();
  });

  it('should make get call to load customer data when in edit mode', function () {
    config.isNew = false;
    var model = new myc.EditCustomer(config);
    model.init();
    expect(config.ajaxService.getJSON).wasCalled();
  });

  it('should execute jQuery remove to remove photo from page', function () {
    config.hasPhoto = true;
    var remove_fn = spyOn($.fn, 'remove');
    var model = new myc.EditCustomer(config);
    model.removePhotoCommand.execute();
    expect(remove_fn).toHaveBeenCalled();
  });

  it('should not execute removePhotoCommand unless customer has a photo', function () {
    config.hasPhoto = false;
    var model = new myc.EditCustomer(config);
    expect(model.removePhotoCommand.canExecute()).toBe(false);
  });

  it('should add phone number', function () {

    var model = new myc.Customer({ profileQuestionGroups: [] }, config);
    model.addPhoneNumberCommand.execute();
    expect(model.phoneNumbers().length).toBe(1);
  });

  it('should not add more than 3 phone numbers', function () {

    var model = new myc.Customer({ profileQuestionGroups: [] }, config);
    var number = new myc.PhoneNumber();
    number.number('12345');
    model.phoneNumbers([number, number, number]);
    expect(model.addPhoneNumberCommand.canExecute()).toBe(false);
  });

  it('should remove phone number', function () {

    var model = new myc.Customer({ profileQuestionGroups: [] }, config);
    var number = new myc.PhoneNumber();
    number.number('12345');
    model.phoneNumbers([number]);
    app.confirm = jasmine.createSpy('confirm').andCallFake(function () {
      var result = $.Deferred();
      result.resolve();
      return result.promise({});
    });
    model.removePhoneNumberCommand.execute(number);
    expect(model.phoneNumbers().length).toBe(0);
  });

  it('should add address', function () {

    var model = new myc.Customer({ profileQuestionGroups: [] }, config);
    model.addAddressCommand.execute();
    expect(model.addresses().length).toBe(1);
  });

  it('should not add more than 5 addresses', function () {

    var model = new myc.Customer({ profileQuestionGroups: [] }, config);
    var address = new myc.Address();
    address.street('some street');
    model.addresses([address, address, address, address, address]);
    expect(model.addAddressCommand.canExecute()).toBe(false);
  });

  it('should remove address', function () {

    var model = new myc.Customer({ profileQuestionGroups: [] }, config);
    var address1 = new myc.Address();
    address1.street('some street1');
    var address2 = new myc.Address();
    address2.street('some street2');
    model.addresses([address1, address2]);
    app.confirm = jasmine.createSpy('confirm').andCallFake(function () {
      var result = $.Deferred();
      result.resolve();
      return result.promise({});
    });
    model.removeAddressCommand.execute(address1);
    expect(model.addresses().length).toBe(1);
  });

  it('should set primary address', function () {

    var model = new myc.Customer({ profileQuestionGroups: [] }, config);
    var address1 = new myc.Address();
    address1.street('some street1');
    var address2 = new myc.Address();
    address2.street('some street2');
    model.addresses([address1, address2]);
    model.setPrimaryAddress(address2);
    expect(model.addresses()[1].isPrimary()).toBe(true);
  });

  it('should add social link', function () {

    var model = new myc.Customer({ profileQuestionGroups: [] }, config);
    model.addSocialNetwork(new myc.SocialNetwork(null, { Validation: null }));
    expect(model.socialNetworks().length).toBe(1);
  });

  it('should remove social link', function () {

    var model = new myc.Customer({ profileQuestionGroups: [] }, config);
    var socialLink = new myc.SocialNetwork(null, { Validation: null });    
    model.socialNetworks.push(socialLink);
    model.removeSocialNetwork(socialLink);
    expect(model.socialNetworks().length).toBe(0);
  });

});