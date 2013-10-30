var myc = myc || {};

myc.EditOrder = function (config) {
  var self = this;

  self.giftWithPurchase        = config.gwp;
  self.isEligibleForCDS        = config.isEligibleForCDS;
  self.addProductsForm         = config.addProductsForm;
  self.isProductCatalogVisible = ko.observable(false);
  self.showProductCatalog      = function() { self.isProductCatalogVisible(true); };
  self.closeProductCatalog     = function() { self.isProductCatalogVisible(false); };

  self.isLoaded                          = ko.observable(false);
  self.orderId                           = ko.observable();
  self.confirmationNumber                = ko.observable();
  self.orderSource                       = ko.observable();
  self.customerId                        = ko.observable(config.customerId);
  self.orderDate                         = ko.observable(new Date());
  self.orderStatus                       = ko.observable('UnderReview');
  self.paymentStatus                     = ko.observable('NA');
  self.fulfillmentOption                 = ko.observable(config.isEligibleForCDS ? null : 'MyInventory');
  self.deliveryDate                      = ko.observable();
  self.deliveryAddress                   = ko.observable();
  self.addDeliveryAddressToCustomer      = ko.observable(false);
  self.isGift                            = ko.observable(false);
  self.giftMessage                       = ko.observable(null).extend({ maxLength: { max: 250 } });
  self.items                             = ko.observableArray([]);
  self.cashPayments                      = ko.observableArray([]);
  self.checkPayments                     = ko.observableArray([]);
  self.ccPayments                        = ko.observableArray([]);
  self.deletedCCPaymentIds               = ko.observableArray([]);
  self.notes                             = ko.observable(null).extend({ maxLength: { max: 1000 } });
  self.followups                         = ko.observableArray([]);
  self.selectRecommendedAddressViewModel = ko.observable();
  self.editCCPaymentViewModel            = ko.observable();

  // properties provided by the customer on PWS
  self.customerComment                 = ko.observable();
  self.isInterestedInSample            = ko.observable(false);
  self.isInterestedInGift              = ko.observable(false);
  self.deliveryPreference              = ko.observable();
  self.deliveryPreferenceDetailIfOther = ko.observable();

  self.showCustomerDirections           = ko.computed(function() { return self.orderSource() === 'Online' && (self.customerComment() || self.isInterestedInSample() || self.isInterestedInGift() || self.deliveryPreference() || self.isGift()); });
  self.customerAddressListVisible       = ko.observable(false);
  self.customerAddresses                = ko.observableArray([]);
  self.customerReorderReminders         = ko.observableArray([]);
  self.hasItems                         = ko.computed(function() { return self.items().length > 0; });
  self.hasCashPayments                  = ko.computed(function() { return self.cashPayments().length > 0; });
  self.hasCheckPayments                 = ko.computed(function() { return self.checkPayments().length > 0; });
  self.hasCCPayments                    = ko.computed(function() { return self.ccPayments().length > 0; });
  self.total                            = ko.computed(function() { return Enumerable.From(self.items()).Sum(function(i) { return i.total(); }); });
  self.customerHasAddresses             = ko.computed(function() { return self.customerAddresses() !== null && self.customerAddresses().length > 0; });
  self.customerAddressListEnabled       = ko.computed(function() { return self.customerHasAddresses(); });
  self.selectCustomerAddressLinkVisible = ko.computed(function() { return self.customerAddressListEnabled() && !self.customerAddressListVisible(); });
  self.isProcessed                      = ko.computed(function() { return self.orderStatus() === 'Processed' || self.orderStatus() === 'ShippedDelivered'; });
  self.isCDS                            = ko.computed(function() { return self.fulfillmentOption() === 'CDS'; });
  self.enableCCProcessing               = ko.computed(function() { return self.fulfillmentOption() !== 'CDS'; });
  self.canEditOrderDate                 = ko.computed(function() { return !self.isCDS() || !self.isProcessed(); });
  self.canEditFulfillment               = ko.computed(function() { return !self.isCDS() || !self.isProcessed(); });
  self.canEditProducts                  = ko.computed(function() { return !self.isCDS() || !self.isProcessed(); });
  self.canEditDelivery                  = ko.computed(function() { return !self.isCDS() || !self.isProcessed(); });
  self.canEditPayment                   = ko.computed(function() { return !self.isCDS() || !self.isProcessed(); });

  //#region Followups

  self.followupsMode = ko.computed({
    read: function() {
      var followups = Enumerable.From(self.followups());
      
      if (!followups.Any())
        return '';

      if 
      (
        followups.Any(function(f) { return f.periodUnit() === 'Days' && f.value() === 2; }) &&
        followups.Any(function(f) { return f.periodUnit() === 'Weeks' && f.value() === 2; }) &&
        followups.Any(function(f) { return f.periodUnit() === 'Months' && f.value() === 2; })
      )
        return '222';

      return 'Custom';
    },
    write: function(value) {
      var followups = [];

      switch(value) {
        case '222':
          followups.push(ko.mapping.fromJS({ periodUnit: 'Days', value: 2 }));
          followups.push(ko.mapping.fromJS({ periodUnit: 'Weeks', value: 2 }));
          followups.push(ko.mapping.fromJS({ periodUnit: 'Months', value: 2 }));
          break;
        case 'Custom':
          var vm = ko.mapping.fromJS({ periodUnit: 'Days', value: 0 });
          _extendFollowupDate(vm);
          followups.push(vm);
          break;
      }

      self.followups(followups);
    }
  });

  self.addFollowupCommand = ko.command({
    execute: function() {
      var vm = ko.mapping.fromJS({ periodUnit: 'Days', value: 0 });
      _extendFollowupDate(vm);
      self.followups.push(vm);
    },
    canExecute: function() { return self.followups().length < 3; }
  });

  self.removeFollowupCommand = ko.command({
    execute: function (followup) {
      app
     .confirm('CONFIRM_REMOVEFOLLOWUP_TITLE', 'CONFIRM_REMOVEFOLLOWUP_BODY')
     .then(function () { self.followups.remove(followup); });
      
    }
  });

  //#endregion

  function _compareValue(v1,v2) {
    if (v1 === '') v1 = null;
    if (v2 === '') v2 = null;
    return v1 === v2;
  }

  function _compareAddress(a1, a2) {
    var res = 
      _compareValue(a1.addressee(),   a2.addressee())   &&
      _compareValue(a1.street(),      a2.street())      &&
      _compareValue(a1.unitNumber(),  a2.unitNumber())  &&
      _compareValue(a1.city(),        a2.city())        &&
      _compareValue(a1.regionCode(),  a2.regionCode())  &&
      _compareValue(a1.countryCode(), a2.countryCode()) &&
      _compareValue(a1.postalCode(),  a2.postalCode())  &&
      _compareValue(a1.telephone(),   a2.telephone());

    return res;
  }

  function _getModel() {
    var model = {
      orderId:                          self.orderId(),
      orderSource:                      self.orderSource(),
      customerId:                       self.customerId(),
      orderDateUtc:                     self.orderDate(),
      orderStatus:                      self.orderStatus(),
      paymentStatus:                    self.paymentStatus(),
      shipCDS:                          self.isCDS(),
      deliveryDateUtc:                  self.deliveryDate(),
      addDeliveryAddressToCustomer:     self.addDeliveryAddressToCustomer(),
      isGift:                           self.isGift(),
      giftMessage:                      self.giftMessage(),
      notes:                            self.notes(),
      deliveryAddress:                  !self.deliveryAddress().isEmpty() ? ko.mapping.toJS(self.deliveryAddress()) : null,
      followups:                        ko.toJS(self.followups),
      customerComment:                  self.customerComment(),
      isInterestedInSample:             self.isInterestedInSample(),
      isInterestedInGift:               self.isInterestedInGift(),
      deliveryPreference:               self.deliveryPreference(),
      deliveryPreferenceDetailIfOther:  self.deliveryPreferenceDetailIfOther(),
      cashPayments:                     ko.mapping.toJS(self.cashPayments),
      checkPayments:                    ko.mapping.toJS(self.checkPayments),
      deletedCCPaymentIds:              ko.toJS(self.deletedCCPaymentIds),

      items: Enumerable
        .From(self.items())
        .Select(
          function(i) { 
            return { 
              id: i.id(), 
              price: i.price(),
              qty: i.qty(),
              useupRate: i.useupRate()
            }; 
          }
        ).ToArray(),
    };

    model.orderDateUtc = app.toISO8601(model.orderDateUtc);

    if (model.deliveryDateUtc)
      model.deliveryDateUtc = app.toISO8601(model.deliveryDateUtc);

    $.each(model.items, function(i) {
      if (i.remindDateUtc)
        i.remindDateUtc = app.toISO8601(i.remindDateUtc);
    });
    
    // when saving a submitted order from PWS
    // automatically change the order status to 
    // under review
    if (model.orderStatus === 'Submitted')
      model.orderStatus = 'UnderReview';

    // convert payment dates to iso utc
    if (model.cashPayments)
      $.each(model.cashPayments, function(i,p) {
        p.paymentDateUtc = app.toISO8601(p.paymentDateUtc);
      });

    if (model.checkPayments)
      $.each(model.checkPayments, function(i,p) {
        p.paymentDateUtc = app.toISO8601(p.paymentDateUtc);
      });

    return model;
  }

  function _saveOrder() {
    var model = _getModel();
    var url = config.ordersApiUrl;
    app.showProgressModal('SAVING_ORDER');
    return config
      .ajaxService
      .postJSON(url, model);
  }

  function _selectRecommendedAddress(address, recommendedAddress) {
    var res = $.Deferred();
    
    recommendedAddress = new myc.Address($.extend(JSON.parse(ko.mapping.toJSON(address)), recommendedAddress));

    self.selectRecommendedAddressViewModel({
      address: address,
      recommendedAddress: recommendedAddress,
      accept: function() {
        $('#recommendAddressModal').modal('hide');
        self.deliveryAddress(recommendedAddress);
        res.resolve();
      },
      close: function() {
        $('#recommendAddressModal').modal('hide');
        res.resolve();
      }
    });

    $('#recommendAddressModal').modal('show');

    return res;
  }

  self.saveOrderCommand = ko.asyncCommand({
    execute: function(cb) {
      app.showValidationErrors(true);
      if (self.validationState.isValid()) {
        _saveOrder()
          .done(function(res) { 
            self.isDirty.reset();
            app.redirectSuccess('SAVEORDER_SUCCESS', 'EDITORDER_TITLE', res.links.detail.href);
          })
          .fail(app.closeProgressModal) // only close the progress modal on fail since on success the page is redirecting
          .always(cb()); 
      } else {
        app.clearNotifications();
        app.notifyError(app.localize('EDITORDER_VALIDATION_MESSAGE'), app.localize('EDITORDER_VALIDATION_TITLE'));
        cb();
      }
    },
    canExecute: function(isExecuting) { return !config.ajaxService.isBusy(); }
  });

  self.markCompleteCommand = ko.asyncCommand({
    execute: function(cb) {
      app.showValidationErrors(true);
      if (self.validationState.isValid())
        app
          .confirm('CONFIRM_MARKCOMPLETE_TITLE', 'CONFIRM_MARKCOMPLETE_BODY')
          .then(function() { 
            self.orderStatus('PROCESSED'); 
            return _saveOrder(); 
          })
          .done(function(res) { 
            self.isDirty.reset();
            app.redirectSuccess('MARKCOMPLETE_SUCCESS', 'EDITORDER_TITLE', res.links.detail.href);
          })
          .fail(app.closeProgressModal) // only close the progress modal on fail since on success the page is redirecting
          .always(cb);
      else {
        app.clearNotifications();
        app.notifyError(app.localize('EDITORDER_VALIDATION_MESSAGE'), app.localize('EDITORDER_VALIDATION_TITLE'));
        cb();
      }
    },
    canExecute: function() { return !config.ajaxService.isBusy(); }
  });
  
  function _scrubDeliveryAddress() {
    var address = ko.mapping.toJS(self.deliveryAddress());
    if (config.verifyAddress && config.verifyAddressUrl && config.isEligibleForCDS && self.fulfillmentOption() === 'CDS') {
      app.showProgressModal('CHECKING_DELIVERYADDRESS');
      return config
        .ajaxService
        .postJSON(config.verifyAddressUrl, ko.toJSON(address))
        .always(app.closeProgressModal)
        .then(function(res) {
          if (res.verificationState === 'RecommendChange')
            return _selectRecommendedAddress(address, res.recommendedAddress);
        });
    } 
    return $.Deferred().resolve();
  }

  self.submitCDSCommand = ko.asyncCommand({
    execute: function(cb) {
      // submits the order data to CDS by invoking the CDS api controller
      function _beginExpressCheckout(saveOrderResult) {
        self.isDirty.reset();
        self.orderId(saveOrderResult.id);
        app.showProgressModal('SUBMITTING_CDSORDER');
        return config.ajaxService
          .postJSON(config.cdsSubmitUrl, { orderId: saveOrderResult.id });
      }

      // processes the response from CDS and redirects to express checkout if successful
      function _redirectToExpressCheckout(res) {
        if (res.exception) {
          if (res.exception.code)
            app.notifyError(app.localize('SUBMITCDSORDER_ERROR_{0}', res.exception.code), app.localize('SUBMITCDSORDER_ERROR_TITLE'));

          app.logError(ko.toJSON(res));

          return $.Deferred().reject(res.exception);
        }
        else
          window.location.href = res.redirectUrl;
      }

      // enable all validation
      app.showValidationErrors(true);
      var isValidForCDS = 
        self.validationState.isValid()
        self.productsAreValidForCDS() && 
        self.sampleProductsAreValidForCDS() && 
        self.deliveryAddressIsValidForCDS() && 
        self.paymentsAreValidForCDS();

      if (isValidForCDS) {
        app
          .confirm('CONFIRM_SUBMITCDS_TITLE', 'CONFIRM_SUBMITCDS_BODY')
          .then(_scrubDeliveryAddress)
          .then(_saveOrder)
          .then(_beginExpressCheckout)
          .then(_redirectToExpressCheckout)
          .fail(app.closeProgressModal) // only close the progress modal on fail since on success the page is redirecting
          .always(cb);
      } else {
        app.clearNotifications();
        app.notifyError(app.localize('SUBMITCDSORDER_VALIDATION_MESSAGE'), app.localize('SUBMITCDSORDER_VALIDATION_TITLE'));
        cb();
      }
    },
    canExecute: function() {
      return config.isEligibleForCDS;
    }
  });

  self.markShippedCommand = ko.asyncCommand({
    execute: function(cb) {
      app.showValidationErrors(true);
      if (self.validationState.isValid())
        app
          .confirm('CONFIRM_MARKSHIPPED_TITLE', 'CONFIRM_MARKSHIPPED_BODY')
          .then(function() { 
            self.orderStatus('SHIPPEDDELIVERED'); 
            return _saveOrder(); 
          })
          .done(function(res) { 
            self.isDirty.reset();
            app.redirectSuccess('MARKSHIPPED_SUCCESS', 'EDITORDER_TITLE', res.links.detail.href);
          })
          .fail(app.closeProgressModal) // only close the progress modal on fail since on success the page is redirecting
          .always(cb);
      else {
        app.clearNotifications();
        app.notifyError(app.localize('EDITORDER_VALIDATION_MESSAGE'), app.localize('EDITORDER_VALIDATION_TITLE'));
        cb();
      }
    },
    canExecute: function() { return !config.ajaxService.isBusy(); }
  });

  self.addDeliveryAddressToCustomerEnabled = ko.computed(function () {
    if (self.deliveryAddress() === undefined || !self.deliveryAddress().notEmpty())
      return false;

    var da = self.deliveryAddress();
    if (da) {
      var matches = Enumerable
        .From(self.customerAddresses())
        .Aggregate(false, function(res, a) { return res || _compareAddress(da, a); });

      return !matches;
    }

    return false;
  });

  self.addDeliveryAddress = function() {
    var address = null;

    // initialize delivery address to the customer's primary address if they have one
    if(self.customerAddresses().length > 0) {
      var primaryAddress = Enumerable.From(self.customerAddresses()).FirstOrDefault(function(a) { return a.isPrimary(); });
      if (primaryAddress !== null)
        address = new myc.Address(primaryAddress);
    } 

    if (address === null)
      address = new myc.Address();

    self.deliveryAddress(address);
  };

  self.removeDeliveryAddress = function() {
    self.deliveryAddress(new myc.Address());
  };

  self.selectCustomerAddress = function(address) {
    self.customerAddressListVisible(false);
    self.deliveryAddress(new myc.Address(ko.mapping.toJS(address)));
  };

  self.addProduct = function(product) {
    var existing = Enumerable.From(self.items()).FirstOrDefault(null, function(i) { return i.id() === product.id(); });

    if (existing) {
      existing.qty(existing.qty() + 1);
      app.clearNotifications();
      app.notifySuccess(app.localize('NOTIFY_UPDATEPRODUCT_SUCCESS'), app.localize('ORDER_EDIT'));
    }
    else {
      var dto = ko.toJS(product);
      dto.qty = 1;
      var item = new myc.OrderItem(dto);
      self.items.push(item);
      app.clearNotifications();
      app.notifySuccess(app.localize('NOTIFY_ADDPRODUCT_SUCCESS'), app.localize('ORDER_EDIT'));
    }

    return true;
  };

  self.addGWP = function() {
    var gwp = config.gwp;

    var item = new myc.OrderItem({
      isGWP: true,
      id: gwp.productId,
      partId: gwp.partId,
      dispPartId: gwp.displayPartId,
      name: gwp.name,
      desc: gwp.description,
      shade: gwp.shade,
      price: 0.0,
      qty: 1,
      listImage: gwp.imageUrl,
      availableForCDS: gwp.availableForCDS
    });

    self.items.push(item);
    app.clearNotifications();
    app.notifySuccess(app.localize('NOTIFY_ADDPRODUCT_SUCCESS'), app.localize('ORDER_EDIT'));
  };

  self.showGWP = ko.computed(function() {
    if (!config.gwp || (self.isCDS() && !config.gwp.availableForCDS)) return false;
    
    var total = self.total();
    var items = Enumerable.From(self.items());
    var hasAdded = items.Any(function(i) { return i.id() === config.gwp.productId; });

    // HACK: hit the quantity property so that the comupted will get re-evalueated if it changes
    items.ForEach(function(i) { i.qty(); }); 

    return !hasAdded && config.gwp.minOrderAmount <= total;
  });

  self.removeProduct = function(item) { 
    app
      .confirm('CONFIRM_REMOVEORDERITEM_TITLE', 'CONFIRM_REMOVEORDERITEM_BODY')
      .then(function() { self.items.remove(item); });
  };

  self.addCashPayment = function() {
    self.cashPayments.push(new myc.CashPayment({ amount: 0.0, paymentDateUtc: new Date() }));
  };

  self.addCheckPayment = function() {
    self.checkPayments.push(new myc.CheckPayment({ amount: 0.0, checkNumber: '', paymentDateUtc: new Date() }));
  };
  
  self.addCCPaymentCommand = ko.command({
    execute: function() {
      function showCCPaymentModal() {
        var res = $.Deferred();

        function amount(p) { return p.amount(); }

        var total = Enumerable
          .From(self.checkPayments())
          .Select(amount)
          .Concat(Enumerable.From(self.cashPayments()).Select(amount))
          .Concat(Enumerable.From(self.ccPayments()).Where(function (p) { return p.status() === 'Approved'; }).Select(amount))
          .Sum();
    
        var balance = self.total() - total;

        // use primary address as default billing address information
        var address = null;
        if (self.customerAddresses().length > 0) {
          address = Enumerable.From(self.customerAddresses()).FirstOrDefault(function(a) { return a.isPrimary(); });
        } 

        self.editCCPaymentViewModel(new myc.EditCCPayment(config, res, {
          cardHolderName: config.customerName,
          billingAddress: address ? address.street() + ' ' + address.unitNumber() : null,
          billingZip: address ? address.postalCode() : null,
          expMonth: 1,
          expYear: new Date().getFullYear(),
          amount: balance > 0.0 ? balance : null,
          enableProcessing: self.enableCCProcessing()
        }));

        $('#editCCPaymentModal').modal('show');

        res.done(function(p) {
          if (p) self.ccPayments.push(new myc.CCPayment(p));
        });

        return res;
      }

      if (!config.orderId)
        app
          .confirm('CONFIRM_SAVEORDER_TITLE', 'CONFIRM_SAVEORDERBEFOREPAYMENT_BODY')
          .then(function() {
            if (self.validationState.isValid())
              return _scrubDeliveryAddress()
                .then(_saveOrder)
                .always(app.closeProgressModal);
            else {
              app.showValidationErrors(true);
              app.clearNotifications();
              app.notifyError(app.localize('EDITORDER_VALIDATION_MESSAGE'), app.localize('EDITORDER_VALIDATION_TITLE'));
              return $.Deferred().reject();
            }
          })
          .then(function(res) {
            config.orderId = res.id;
            config.ordersApiUrl += '/' + res.id;
          })
          .then(showCCPaymentModal);
      else
        showCCPaymentModal();
    },
    canExecute: function() {
      return self.isCDS() ? self.ccPayments().length < 1 : true;
    }
  });

  self.changeCCPayment = function(payment) {
    var res = $.Deferred();
    
    self.editCCPaymentViewModel(new myc.EditCCPayment(config, res, {
      paymentId: payment.paymentId(),
      cardHolderName: payment.cardHolderName(),
      billingAddress: payment.billingAddress(),
      billingZip: payment.billingZip(),
      expMonth: payment.expMonth(),
      expYear: payment.expYear(),
      amount: payment.amount(),
      enableProcessing: self.enableCCProcessing()
    }));

    res.done(function (p) {
      if (p) 
        self.ccPayments.replace(payment, new myc.CCPayment(p));
    });

    $('#editCCPaymentModal').modal('show');
  };

  self.processCCPayment = function(payment) {
    var processUrl = config.processCCPaymentUrl.replace('_orderId_', config.orderId);
    app.showProgressModal('CCPAYMENT_PROCESSING');
    config.ajaxService.postJSON(processUrl, { paymentId: payment.paymentId }).then(function(r) { 
      self.ccPayments.replace(payment, new myc.CCPayment(r.payment));
      if (r.status === 'Approved') {
        app.notifySuccess(app.localize('APPROVED'));
      } else if (r.status === 'Declined') {
        var msg = app.localize('CCPAYMENT_ALERT_DECLINED').replace('{0}', r.payment.transactionStatus);
        app.notifyError(msg, app.localize('DECLINED'));
      } else {
        app.notifyError(r.error, app.localize('ERROR'));
      }
    }).always(app.closeProgressModal);
  };

  self.removeCashPayment = function(payment) {
    app
      .confirm('CONFIRM_REMOVEPAYMENT_TITLE', 'CONFIRM_REMOVEPAYMENT_BODY')
      .then(function() { self.cashPayments.remove(payment); });
  };

  self.removeCheckPayment = function(payment) {
    app
      .confirm('CONFIRM_REMOVEPAYMENT_TITLE', 'CONFIRM_REMOVEPAYMENT_BODY')
      .then(function() { self.checkPayments.remove(payment); });
  };

  self.removeCCPayment = function(payment) {
    app
      .confirm('CONFIRM_REMOVEPAYMENT_TITLE', 'CONFIRM_REMOVEPAYMENT_BODY')
      .then(function() { 
        self.ccPayments.remove(payment); 
        self.deletedCCPaymentIds.push(payment.paymentId);
      });
  };

  function _mapCustomerAddresses(addresses) {
    var vms = Enumerable
      .From(addresses)
      .Select(function(a) { return ko.mapping.fromJS(a, { ignore: ['hasAddressee','hasTelephone'] }); })
      .ToArray();
        
    self.customerAddresses(vms);
  }

  function _extendFollowupDate(followup) {
    var initialDate = moment(app.toISO8601(self.orderDate())).hour(0).minute(0).second(0).millisecond(0);
    switch (followup.periodUnit())
    {
      case 'Days':
        initialDate = initialDate.add('days', followup.value()).toDate();
        break;
      case 'Weeks':
        initialDate = initialDate.add('weeks', followup.value()).toDate();
        break;
      case 'Months':
        initialDate = initialDate.add('months', followup.value()).toDate();
        break;
    }

    followup.date = ko.observable(initialDate).extend({ required: true, validDate: true });
    followup.date.subscribe(function(fd) {
      if (followup.date.isValid()) {
        fd = moment(fd);
        if (fd) {
          fd = fd.hour(0).minute(0).second(0).millisecond(0);
          var orderDate = moment(app.toISO8601(self.orderDate())).hour(0).minute(0).second(0).millisecond(0);
          followup.value(fd.diff(orderDate, 'days'));
          followup.periodUnit('Days');
        }
      }
    });
  }

  function _mapOrder(order) {
    self.orderId(order.orderId);
    self.confirmationNumber(order.confirmationNumber);
    self.orderSource(order.orderSource);
    self.customerId(order.customerId);
    self.orderDate(order.orderDateUtc);
    self.orderStatus(order.orderStatus === 'Submitted' ? 'UnderReview' : order.orderStatus);
    self.paymentStatus(order.paymentStatus);
    self.fulfillmentOption(order.shipCDS ? 'CDS' : 'MyInventory');
    self.deliveryDate(order.deliveryDateUtc);
    self.isGift(order.isGift);
    self.giftMessage(order.giftMessage);
    self.notes(order.notes);
    self.customerComment(order.customerComment);
    self.isInterestedInSample(order.isInterestedInSample);
    self.isInterestedInGift(order.isInterestedInGift);
    self.deliveryPreference(order.deliveryPreference);
    self.deliveryPreferenceDetailIfOther(order.deliveryPreferenceDetailIfOther);

    if (order.items !== null) {
      var items = [];
      $.each(order.items, function (i, item) {
        item.isGWP = config.gwp && item.id === config.gwp.productId;
        var vm = new myc.OrderItem(item);
        items.push(vm);
      });
      self.items(items);
    }

    if (order.deliveryAddress)
      self.deliveryAddress(new myc.Address(order.deliveryAddress));
    else {
      self.deliveryAddress(new myc.Address());
    }

    if (order.cashPayments !== null) {
      $.each(order.cashPayments,function(i,v) {
        self.cashPayments.push(new myc.CashPayment(v));
      });
    }

    if (order.checkPayments !== null) {
      $.each(order.checkPayments,function(i,v) {
        self.checkPayments.push(new myc.CheckPayment(v));
      });
    }

    if (order.creditCardPayments !== null) {
      $.each(order.creditCardPayments,function(i,v) {
        self.ccPayments.push(new myc.CCPayment(v));
      });
    }

    if(order.followups !== null) {
      var fvms = ko.mapping.fromJS(order.followups)();
      $.each(fvms, function(i,f) { _extendFollowupDate(f); });
      self.followups(fvms);
    }
  }

  self.load = function() {
    function mapData(addresses, order) {
      _mapCustomerAddresses(addresses[0]);
      
      if (order !== self) // in the case of a new order order will === self
        _mapOrder(order[0]);

      if (!self.deliveryAddress() && order === self) {
        var primaryAddress = ko.utils.arrayFirst(self.customerAddresses(), function(a) { return a.isPrimary(); });
        if (primaryAddress)
          self.deliveryAddress(new myc.Address(ko.mapping.toJS(primaryAddress)));
        else
          self.deliveryAddress(new myc.Address());
      }

      if (self.deliveryAddress())
        self.deliveryAddress().canEdit(self.canEditDelivery());

      self.isProductCatalogVisible(false);
      self.isDirty.reset();
      app.promptIfChanged(self.isDirty);

      self.isLoaded(true);
    }

    var addrReq  = config.ajaxService.getJSON(config.customerApiUrl, { part: 'addresses' });
    var orderReq = $.Deferred();
      
    if (config.orderId !== null && config.orderId !== '')
      orderReq = config.ajaxService.getJSON(config.ordersApiUrl);
    else
      orderReq.resolve(self);

    app.showProgressModal('ORDER_LOADING');
    return $.when(addrReq, orderReq)
      .always(app.closeProgressModal)
      .done(mapData)
      .fail(function(e) { app.logError(e); });
  };

  // enable/disable CDS Samples in the product catalog search based on the CDS fulfillment selection
  self.fulfillmentOption.subscribe(function(val) {
    self.addProductsForm.productCatalog.enableCDSSamples(config.isEligibleForCDS && val === 'CDS');
  });

  //#region Dirty Flag

  self.isDirty = new ko.dirtyFlag([
    self.orderDate, 
    self.orderStatus,
    self.paymentStatus,
    self.fulfillmentOption,
    self.deliveryDate,
    self.deliveryAddress,
    self.isGift,
    self.giftMessage,
    self.items,
    self.cashPayments,
    self.checkPayments,
    self.notes,
    self.followups
  ]);

  //#endregion

  //#region Validation

  self.deliveryAddressIsValidForCDS = ko.computed(function() {
    var isProecssed     = self.isProcessed();
    var isCDS           = self.isCDS();
    var deliveryAddress = self.deliveryAddress();

    if (isProecssed) return true;
    if (!isCDS) return true;

    var isValidDeliveryAddress = 
      deliveryAddress && 
      !deliveryAddress.isEmpty() && 
      deliveryAddress.isValid() && 
      /^\s*\w+\s+\w+.*$/gi.test(deliveryAddress.addressee());

    return isValidDeliveryAddress;
  });

  self.sampleProductsAreValidForCDS = ko.computed(function() {
    var isProecssed = self.isProcessed();
    var isCDS       = self.isCDS();
    var items       = Enumerable.From(self.items());
    
    if (isProecssed) return true;
    if (!isCDS) return true;

    // check how many sample products were added to the order
    samplesCount = items.Where(function(i) { return i.cdsFree(); }).Sum(function(i) { return i.qty(); });

    return samplesCount <= 2;
  });

  self.productsAreValidForCDS = ko.computed(function() {
    var isProecssed = self.isProcessed();
    var isCDS       = self.isCDS();
    var items       = Enumerable.From(self.items());
    
    if (isProecssed) return true;
    if (!isCDS) return true;

    // check for expired or out of stock items
    if (items.Count(function(i) { return !i.availableForCDS(); }) > 0)
      return false;

    return true;
  });

  self.paymentsAreValidForCDS = ko.computed(function() {
    var isProecssed = self.isProcessed();
    var isCDS       = self.isCDS();
    var payments    = self.ccPayments();

    if (isProecssed) return true;
    if (!isCDS) return true;
    if (payments.length === 0) return true;
    if (payments.length === 1) {
      var payment = payments[0];
      var status = payment.status();
      var isValid = 
        status !== 'Approved' && 
        status !== 'Declined' && 
        status !== 'Error';

      return isValid;
    }

    return false;
  });

  self.validationState = ko.validation.applyTo(self, {
    global: {
      orderDate: {
        required: true,
        validDate: true
      },
      items: { 
        validation: {
          validator: function(val) { 
            return $.isArray(val) && val.length > 0; 
          },
          message: 'VALIDATION_ORDER_NOPRODUCTS'
        }
      },
      fulfillmentOption: {
        validation: {
          validator: function(val) {
            if (!config.isEligibleForCDS) return true;
            return (typeof val === 'string' && val.length > 0);
          },
          message: 'VALIDATION_ORDER_FULFILLMENTOPTION_REQUIRED'
        }
      },
      cashPayments: { validArray: true },
      checkPayments: { validArray: true },
      followups: { validArray: true },
      deliveryDate: { validDate: true },
      deliveryAddress: { validObject: true }
    }
  });

  //#endregion
};
