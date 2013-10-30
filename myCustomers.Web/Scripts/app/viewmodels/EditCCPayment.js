var myc = myc || {};

myc.EditCCPayment = function(config, res, model) {
  var self = this;
  
  self.paymentId = ko.observable(model.paymentId);
  self.expMonth  = ko.observable(model.expMonth);
  self.expYear   = ko.observable(model.expYear);

  self.cardHolderName = ko.observable(model.cardHolderName).extend({ required: true });
  self.billingAddress = ko.observable(model.billingAddress).extend({ required: true });
  self.billingZip     = ko.observable(model.billingZip).extend({ required: true });

  self.amount = ko.observable(model.amount).extend({
    required : true,
    validAmount: true
  });

  self.creditCardNumberHasLostFocus = ko.observable(false);
  self.creditCardNumber = ko.observable(model.creditCardNumber).extend({
    required: true,
    validation: {
      validator: function (value) {
        if (!self.creditCardNumberHasLostFocus()) return true;
        if (!value || typeof value !== 'string') return true;
        if (!app.luhnCheck(value)) return false;
        var type = app.getCCType(config.proPayAccountType, value);
        if (!type) return false;
        switch (type) {
          case 'Visa':
            return value.length === 13 || value.length === 16;
          case 'Master':
          case 'Discover':
            return value.length === 16;
          case 'Amex':
            return config.proPayAccountType === 2 && value.length === 15;
          default:
            return true; 
        }
      },
      message: 'INVALID_CCNUM'
    }
  });

  self.cardType = ko.computed(function() {
    return app.getCCType(config.proPayAccountType, self.creditCardNumber());
  });

  self.showVisa = ko.computed(function() {
    var type = self.cardType();
    return !type || type === 'Visa';
  });

  self.showMaster = ko.computed(function() {
    var type = self.cardType();
    return !type || type === 'Master';
  });

  self.showAmex = ko.computed(function() {
    var type = self.cardType();
    return config.proPayAccountType === 2 && !type || type === 'Amex';
  });

  self.showDiscover = ko.computed(function() {
    var type = self.cardType();
    return !type || type === 'Discover';
  });

  function _getModel() {
    var model = {
      paymentId: self.paymentId(),
      cardHolderName: self.cardHolderName(),
      billingAddress: self.billingAddress(),
      billingZip: self.billingZip(),
      creditCardNumber: self.creditCardNumber(),
      cardType: self.cardType(),
      expMonth: self.expMonth(),
      expYear: self.expYear(),
      amount: self.amount()
    };

    return model;
  };

  self.paymentResult = null;

  self.save = function() {
    var saveUrl = config.saveCCPaymentUrl.replace('_orderId_', config.orderId);
    app.showProgressModal('CCPAYMENT_SAVING');
    $('#editCCPaymentModal').modal('hide');
    config.ajaxService
      .postJSON(saveUrl, _getModel())
      .then(function(r) { 
        res.resolve(r); 
      })
      .fail(function() { $('#editCCPaymentModal').modal('show'); })
      .always(app.closeProgressModal);
  };

  self.enableProcessing = ko.observable(model.enableProcessing);

  self.process = function() {
    if (!self.enableProcessing()) return;
    var saveUrl = config.saveCCPaymentUrl.replace('_orderId_', config.orderId);
    var processUrl = config.processCCPaymentUrl.replace('_orderId_', config.orderId);
    app.showProgressModal('CCPAYMENT_PROCESSING');
    $('#editCCPaymentModal').modal('hide');
    config
      .ajaxService
      .postJSON(saveUrl, _getModel())
      .then(function(r) { 
        return config.ajaxService.postJSON(processUrl, { paymentId: r.paymentId }).then(function(r) { 
          self.paymentResult = r.payment;
          if (r.status === 'Approved') {
            res.resolve(r.payment); 
          } else if (r.status === 'Declined') {
            $('#editCCPaymentModal').modal('show');
            var msg = app.localize('CCPAYMENT_ALERT_DECLINED').replace('{0}', r.payment.transactionStatus);
            app.notifyError(msg, app.localize('DECLINED'));
          } else {
            $('#editCCPaymentModal').modal('show');
            app.notifyError(r.error, app.localize('ERROR'));
          }
        });
      })
      .fail(function() { $('#editCCPaymentModal').modal('show'); })
      .always(app.closeProgressModal);
  };

  self.cancel = function() {
    $('#editCCPaymentModal').modal('hide');
    res.resolve(self.paymentResult);
  };

  self.validationState = ko.validatedObservable(self);
};
