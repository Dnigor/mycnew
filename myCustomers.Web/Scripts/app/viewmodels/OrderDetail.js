var myc = myc || {};

myc.OrderDetail = function (config) {

  var self = this;

  self.orderTotalAmount                 = ko.observable();
  self.orderTotalSelected               = ko.observable(false);
  self.orderProcessedSelected           = ko.observable(false);
  self.orderShippedSelected             = ko.observable(false);
  self.orderIsGiftSelected              = ko.observable(false);
  self.contactMeAboutPaymentSelected    = ko.observable(false);
  self.contactMeAboutProcessingSelected = ko.observable(false);
  self.contactMeAsDesiredSelected       = ko.observable(false);
  self.contactMeAboutDeliverySelected   = ko.observable(false);
  self.proposedDeliveryScheduleSelected = ko.observable(false);
  self.proposedDeliveryScheduleDate     = ko.observable();
  self.proposedDeliveryScheduleTime     = ko.observable();
  self.orderIsDeleted                   = ko.observable(config.orderIsDeleted === 'True');
  self.orderIsArchived                  = ko.observable(config.orderIsArchived === 'True');
  self.isProcessed                      = config.orderStatus == 'Processed' ? ko.observable(true) : ko.observable(false);
  self.isShipped                        = (config.orderStatus == 'UnderReview' || config.orderStatus == 'ShippedDelivered') ? ko.observable(true) : ko.observable(false);

  self.validationState = ko.validation.applyTo(self, {
    global: {         
      proposedDeliveryScheduleDate: { 
        validDate: true,
        required: { params: true, onlyIf: self.proposedDeliveryScheduleSelected }
      },
      orderTotalAmount: { 
        required: { params: true, onlyIf: self.orderTotalSelected },
        validAmount: { params: true, onlyIf: self.orderTotalSelected }
      }
    }
  });

  self.orderTotalAmount.subscribe(function(value) {
    (value !== undefined && value !== '') ? self.orderTotalSelected(true) : self.orderTotalSelected(false);
  });

  self.proposedDeliveryScheduleDate.subscribe(function() {
    (self.proposedDeliveryScheduleDate.isValid()) ? self.proposedDeliveryScheduleSelected(true) : self.proposedDeliveryScheduleSelected(false);
  });

  self.deleteUndeleteText = ko.computed(function () {
    if (self.orderIsDeleted()) return app.localize('ORDERDETAIL_UNDELETE');
    else return app.localize('ORDERDETAIL_DELETE');
  });

  self.archiveUnarchiveText = ko.computed(function () {
    if (self.orderIsArchived()) return app.localize('ORDERDETAIL_UNARCHIVE');
    else return app.localize('ORDERDETAIL_ARCHIVE');
  });
    
  var _getConfirmationEmailModel = function() {
    return {
      OrderId: config.orderId,
      OrderTotalSelected: self.orderTotalSelected(),
      OrderTotalAmount: self.orderTotalAmount(),
      OrderProcessedSelected: self.orderProcessedSelected(),
      OrderShippedSelected: self.orderShippedSelected(),
      OrderIsGiftSelected: self.orderIsGiftSelected(),
      ContactMeAboutPaymentSelected: self.contactMeAboutPaymentSelected(),
      ContactMeAboutProcessingSelected: self.contactMeAboutProcessingSelected(),
      ContactMeAsDesiredSelected: self.contactMeAsDesiredSelected(),
      ContactMeAboutDeliverySelected: self.contactMeAboutDeliverySelected(),
      ProposedDeliveryScheduleSelected: self.proposedDeliveryScheduleSelected(),
      ProposedDeliveryScheduleDate: app.formatShortDate(self.proposedDeliveryScheduleDate()),
      ProposedDeliveryScheduleTime: self.proposedDeliveryScheduleTime()
    };
  };
  
 self.sendEmailCommand = ko.asyncCommand({
    execute: function(cb) {
        app.clearNotifications();
        if (self.validationState.isValid()) {
            var model = _getConfirmationEmailModel();
            config
                .ajaxService
                .postJSON(config.sendORDERConfirmationEmailUrl, model)
                .done(function() {
                    app.notifySuccess(app.localize('SEND_CONFIRMATION_EMAIL_SUCCESS'));
                })
                .always(cb);
            
            $('#sendConfirmationEmail').hide();
        } else {
            app.showValidationErrors(true);
            app.notifyError(app.localize('SEND_CONFIRMATION_EMAIL_ERROR'));
            cb();
        }
    },
    canExecute: function() { return !config.ajaxService.isBusy() }
  });

  //#region Create Invoice

  self.showCreateInvoice           = ko.observable(false);

  self.invoiceFormat               = ko.observable('pdf');
  self.isInvoiceMessageVisible     = ko.observable(false);
  self.invoiceMessage              = ko.observable().extend({ maxLength: { max: 250 } });
  self.addInvoiceMessage           = function() { self.isInvoiceMessageVisible(true); }
  self.removeInvoiceMessage        = function() { self.isInvoiceMessageVisible(false); }

  self.isInvoiceGiftMessageVisible = ko.observable(false);
  self.invoiceGiftMessage          = ko.observable(config.giftMessage).extend({ maxLength: { max: 250 } });
  self.removeInvoiceGiftMessage    = function() { self.isInvoiceGiftMessageVisible(false); }
  self.addInvoiceGiftMessage       = function() { self.isInvoiceGiftMessageVisible(true); }

  self.createInvoiceCommand = ko.asyncCommand({
    execute: function(complete) {
      $.fileDownload(config.createInvoiceUrl, {
        httpMethod: 'POST',
        data: { 
          OrderId:        config.orderId,  
          SaveFormat:     self.invoiceFormat(),
          InvoiceMessage: self.isInvoiceMessageVisible() ? self.invoiceMessage() : null,
          GiftMessage:    self.isInvoiceGiftMessageVisible() ? self.invoiceGiftMessage() : null
        },
        successCallback: function() {
          $('#printInvoiceForm').hide();
          complete(); 
        },
        failCallback: function() { 
          app.notifyError(app.localize('INVOICE_CREATE_ERROR')); 
          complete(); 
        },
      });
    }
  });

  //#endregion

  self.deleteOrderCommand = ko.asyncCommand({
    execute: function(cb) {
      app
        .confirm('CONFIRM_DELETEORDER_TITLE', 'CONFIRM_DELETEORDER_BODY')
        .then(function() { return config.ajaxService.postJSON(config.deleteOrderUrl, { id: config.orderId }); })
        .done(function() { self.orderIsDeleted(true); })
        .always(cb);
    },
    canExecute: function() { return !self.orderIsDeleted() && !config.ajaxService.isBusy() }
  });

  self.undeleteOrderCommand = ko.asyncCommand({
    execute: function (cb) {
      app
        .confirm('CONFIRM_UNDELETEORDER_TITLE', 'CONFIRM_UNDELETEORDER_BODY')
        .then(function() { return config.ajaxService.postJSON(config.undeleteOrderUrl, { id: config.orderId }); })
        .done(function() { self.orderIsDeleted(false); })
        .always(cb);
    },
    canExecute: function() { return self.orderIsDeleted() && !config.ajaxService.isBusy() }
  });

  self.archiveOrderCommand = ko.asyncCommand({
    execute: function (cb) {
      app
        .confirm('CONFIRM_ARCHIVEORDER_TITLE', 'CONFIRM_ARCHIVEORDER_BODY')
        .then(function () { return config.ajaxService.postJSON(config.archiveOrderUrl, { id: config.orderId }); })
        .done(function () { self.orderIsArchived(true); })
        .always(cb);
    },
    canExecute: function () { return !self.orderIsArchived() && !config.ajaxService.isBusy() }
  });

  self.updateOrderStatus = ko.asyncCommand({
    execute: function (cb) {
      _updateOrderStatus()
      .done(function () { app.redirectSuccess('ORDER_UPDATESTATUS_SUCCESS', 'ORDER_UPDATESTATUS_TITLE', location.href); })
      .always(cb);
    },
    canExecute: function () { return !config.ajaxService.isBusy(); }
  });

  self.unarchiveOrderCommand = ko.asyncCommand({
    execute: function (cb) {
      app
        .confirm('CONFIRM_UNARCHIVEORDER_TITLE', 'CONFIRM_UNARCHIVEORDER_BODY')
        .then(function () { return config.ajaxService.postJSON(config.unarchiveOrderUrl, { id: config.orderId }); })
        .done(function () { self.orderIsArchived(false); })
        .always(cb);
    },
    canExecute: function () { return self.orderIsArchived() && !config.ajaxService.isBusy() }
  });

  function _updateOrderStatus() {
    var orderStatus = self.isProcessed() ? 'ShippedDelivered' : 'Processed';
    return config.ajaxService.postJSON(config.updateOrderStatusUrl, { id: config.orderId, orderStatus: orderStatus });
  };
};
