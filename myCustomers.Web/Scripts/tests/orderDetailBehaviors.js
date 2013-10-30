describe('OrderDetail', function () {
  //it('should send confirmation email if everything is valid', function () {
  //  var model = new myc.OrderDetail(config);
  //  model.orderTotalAmount.hasError(false);
  //  model.proposedDeliveryScheduleTime.hasError(false);
  //  model.sendEmailCommand.execute();
  //  expect(config.ajaxService.post).wasCalled();
  //});

//  it('should not send confirmation email on validation errors', function () {
//    var model = new myc.OrderDetail(config);
//    model.orderTotalAmount("NaN");    
//    model.sendEmailCommand.execute();
//    expect(config.ajaxService.post).wasNotCalled();
//  });

  it('should make Ajax call to delete order', function () {
    var config = {
      ajaxService: {
        isBusy: ko.observable(false),
        postJSON: jasmine.createSpy('postJSON').andCallFake(function () {
          return $.Deferred(function () { });
        })
      }
    };

    //bypass confirm dialog
    var originalConfirm = app.confirm;
    app.confirm = function () {
      return $.Deferred(function () {
        this.resolve();
      })
    };

    var model = new myc.OrderDetail(config);
    model.deleteOrderCommand.execute();
    expect(config.ajaxService.postJSON).wasCalled();

    //restore confirm dialog
    app.confirm = originalConfirm;
  });

  it('should make Ajax call to undelete order', function () {
    var config = {
      ajaxService: {
        isBusy: ko.observable(false),
        postJSON: jasmine.createSpy('postJSON').andCallFake(function () {
          return $.Deferred(function () { });
        })
      }
    };

    //bypass confirm dialog
    var originalConfirm = app.confirm;
    app.confirm = function () {
      return $.Deferred(function () {
        this.resolve();
      })
    };

    var model = new myc.OrderDetail(config);
    model.orderIsDeleted(true);
    model.undeleteOrderCommand.execute();
    expect(config.ajaxService.postJSON).wasCalled();

    //restore confirm dialog
    app.confirm = originalConfirm;
  });
});