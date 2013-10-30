var myc = myc || {};

myc.CustomerPrrModel = function(dto) {
  var self = this;

  self.productId       = ko.observable(ko.unwrap(dto.productId));
  self.catalogName     = ko.observable(ko.unwrap(dto.catalogName));
  self.customerId      = ko.observable(ko.unwrap(dto.customerId));
  self.displayPartId   = ko.observable(ko.unwrap(dto.displayPartId));
  self.name            = ko.observable(ko.unwrap(dto.name));
  self.reminderDateUtc = ko.observable(ko.unwrap(dto.reminderDateUtc));
  self.newReminderDate = ko.observable();

  self.canEdit = ko.computed(function() {
    var reminderDateUtc = self.reminderDateUtc();
    reminderDateUtc = moment(app.toISO8601(reminderDateUtc)).hour(0).minute(0).second(0).millisecond(0);

    var now = moment().hour(0).minute(0).second(0).millisecond(0);
    if (!reminderDateUtc || now.toDate() >= reminderDateUtc.toDate())
      return false;

    return true;
  });

  self.validationState = ko.validation.applyTo(self, {
    global: {
      newReminderDate: {
        validation: {
          validator: function(val) {
            if (!val) return false;

            val = app.toISO8601(val);
            if (!val) return false;

            val = moment(val).hour(0).minute(0).second(0).millisecond(0);
            if (!val) return false;

            var tomorrow = moment().hour(0).minute(0).second(0).millisecond(0).add('days', 1);
            if (tomorrow.toDate() > val.toDate())
              return false;

            return true;
          },
          message: 'INVALID_DATE'
        }
      }
    }
  });
};

myc.CustomerPrrView = function(options) {
  var self = this;

  var DISPLAY_MODE_TEMPLATE_ID = {
    READONLY: 'prrDisplaModeReadonly',
    EDITABLE: 'prrDisplaModeEditable'
  };

  self.prrCollection = ko.observableArray([]);
  self.selectedCustomerPrr = ko.observable();

  self.isEmpty = ko.computed(function() {
    return self.prrCollection().length === 0;
  });

  self.show = function() {
    _fetchPrrCollection();
  }

  self.prrTemplateToUse = function(customerPrr) {
    return self.selectedCustomerPrr() === customerPrr ? DISPLAY_MODE_TEMPLATE_ID.EDITABLE : DISPLAY_MODE_TEMPLATE_ID.READONLY;
  };

  self.editPrr = function (customerPrr) {
    customerPrr.newReminderDate(ko.unwrap(customerPrr.reminderDateUtc));
    self.selectedCustomerPrr(customerPrr);
  };

  self.deletePrr = function(customerPrr) {
    app
      .confirm('CONFIRM_REMOVEPRR_TITLE', 'CONFIRM_REMOVEPRR_BODY')
      .then(function() {
        app.showProgressModal('REMOVEPRR_PROGRESS');
        options
          .ajaxService
          .del(options.url, {
            productId: customerPrr.productId(),
            catalogName: customerPrr.catalogName(),
            customerId: customerPrr.customerId()
          })
          .done(function() {
            self.prrCollection.remove(customerPrr);
            app.notifySuccess(app.localize('REMOVEPRR_SUCCESS_MESSAGE'));
          })
          .always(app.closeProgressModal);
        });
  };

  self.savePrrUpdate = function(customerPrr) {
    if (customerPrr.validationState.isValid()) {
      var reminderDateUtc = app.toISO8601(customerPrr.newReminderDate()); 
      app.showProgressModal('UPDATEPRR_PROGRESS');
      options
        .ajaxService
        .postJSON(options.url, {
          productId: customerPrr.productId(),
          catalogName: customerPrr.catalogName(),
          customerId: customerPrr.customerId(),
          reminderDateUtc: reminderDateUtc
        })
        .done(function() {
          customerPrr.reminderDateUtc(moment(reminderDateUtc).toDate());
          self.selectedCustomerPrr(undefined);
          app.notifySuccess(app.localize('UPDATEPRR_SUCCESS_MESSAGE'));
        })
        .always(app.closeProgressModal);
    }
  };

  self.cancelPrrUpdate = function(customerPrr) {
    customerPrr.newReminderDate(undefined);
    self.selectedCustomerPrr(undefined);
  };

  function _fetchPrrCollection() {
    options
      .ajaxService
      .getJSON(options.url)
      .done(function(data) {
        var items = [];
        $.each(data.items, function(index, item) {
          var currentPrr = new myc.CustomerPrrModel(item);
          items.push(currentPrr);
        });
        self.prrCollection(items);
      });
  };
};