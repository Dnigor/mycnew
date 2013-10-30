var myc = myc || {};

myc.SampleRequestDetail = function (config) {
  var self = this,
      _loadSampleRequestDetails,
      ajaxService = new myc.AjaxService(),
      jqXHR;

  self.isVisible = ko.observable(false);

  self.requestDateUtc = ko.observable();
  self.deliveryDateUtc = ko.observable();
  self.isCompleted = ko.observable(false);
  self.productName = ko.observable();
  self.followUpDateUtc = ko.observable();
  self.validationMessage = ko.observable();

  self.isBusy = ko.computed(function () {
    return ajaxService.isBusy();
  });

  self.isFormValid = function () {
    self.validationMessage('');
    
    if (self.isCompleted() === '0') {
      self.validationMessage('Please change status to "Completed" before save');
      return false;
    }
    if (! self.deliveryDateUtc()) {
      self.validationMessage('Please enter "Delivery Date" to save the sample request');
      return false;
    }

    if (! self.followUpDateUtc()) {
      self.validationMessage('Please enter "Follow Up Date" to save the sample request');
      return false;
    }
    
    return true;
  };

  self.save = ko.asyncCommand({
    execute: function (cb) {
      if (self.isFormValid()) {
        var postXHR = ajaxService.postJSON(config.sampleRequestPostUrl, {
          sampleRequestId: config.sampleRequestId,
          deliveryDate: self.deliveryDateUtc(),
          followUpDate: self.followUpDateUtc()
        });

        postXHR.done(function (result) {
          window.location = config.ordersIndexUrl;
        });
      }
      cb();
    },
    canExecute: function (isExecuting) {
      return !isExecuting;
    }
  });

  _loadSampleRequestDetails = function () {
    jqXHR = ajaxService.getJSON(config.getByIdSampleRequestUrl, { id: config.sampleRequestId });

    jqXHR.done(function (result) {
      self.requestDateUtc(app.formatShortDate(result.RequestDateUtc));
      self.deliveryDateUtc(result.DeliveryDateUtc);
      self.isCompleted(result.IsCompleted);
      self.productName(result.ProductName);
      self.followUpDateUtc(result.FollowUpDateUtc);

      console.log(result);
      self.isVisible(true);
    });
  };

  _loadSampleRequestDetails();
};