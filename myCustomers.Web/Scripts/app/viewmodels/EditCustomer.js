var myc = myc || {};

myc.EditCustomer = function(config) {
  var self = this;

  self.isNew                = ko.observable(config.isNew);
  self.isLoaded             = ko.observable(false);
  self.customer             = ko.observable();
  self.showAdditionalFields = ko.observable(false);

  self.socialMediaLinksConfig = config.socialMediaLinksConfig;

  //#region Import

  self.importForm = new myc.UploadCustomers(config.csvFileUploadModel);
  self.showImportForm = ko.observable(false);

  //#endregion

  //#region Photo

  self.canChangePhoto    = ko.observable(FileAPI.support.cors || FileAPI.support.flash);
  self.photoRemoved      = ko.observable(false);
  self.photoChanged      = ko.observable(false);
  self.showBlankPhoto    = ko.computed(function() { return self.photoRemoved() || !(self.photoChanged() || config.hasPhoto); });
  self.showOriginalPhoto = ko.computed(function() { return !self.photoChanged() && !self.photoRemoved() && config.hasPhoto; });

  var _photoFile;
  self.changePhoto = function(data, evt) {
    // https://github.com/mailru/FileAPI#html-default
    var files = FileAPI.getFiles(evt.target);
    var imageFiles = FileAPI.filter(files, function(file) { return /image/.test(file.type); });
    if (imageFiles.length > 0) {
      _photoFile = imageFiles[0];
      FileAPI.Image(_photoFile).resize(160,200,'max').get(function(err, image) {
        if (!err) {
          self.photoRemoved(false);
          self.photoChanged(true);
          image.id = 'newPhoto';
          $('#newPhoto').remove();
          $('#photoPreview').get(0).appendChild(image);
        }
      });
    }
  };

  self.removePhotoCommand = ko.command({
    execute: function() {
      $('#newPhoto').remove();
      self.photoRemoved(true);
      self.photoChanged(false);
    },
    canExecute: function() { return config.hasPhoto && !self.photoRemoved(); }
  });

  self.undoChangePhotoCommand = ko.command({
    execute: function() {
      $('#newPhoto').remove();
      self.photoRemoved(false);
      self.photoChanged(false);
    },
    canExecute: function() { return self.photoRemoved() || self.photoChanged(); }
  });

  // NOTE: photos are handled outside of the Customer model
  // because the image cannot be uploaded with the JSON payload
  function _savePhoto(resp) {
    if (self.photoChanged()) {
      var upload = $.Deferred();
      FileAPI.upload({
        url: resp.links.photo.href,
        files: { images: [ _photoFile ] },
        imageOriginal: false,
        imageTransform: {
          max: { maxWidth: 480, maxHeight: 640 }
        },
        complete: function(err, xhr) {
          if (!err)
            upload.resolve(resp);
          else {
            app.notifyError(app.localize('ERROR_PHOTOUPLOAD'));
            upload.reject(xhr);
          }
        }
      });
      return upload;
    }
    else if (self.photoRemoved()) {
      // remove existing photo
      return config.ajaxService
        .del(resp.links.photo.href)
        .then(function() { return resp; }); 
    }
    else
      return resp;
  }

  //#endregion

  //#region Persistence
  
  self.onload = function (model) {
    // HACK: add empty important dates so they show up in the UI
    if (!model.importantDates) model.importantDates = [];
    $.each(['Graduation', 'NewBaby', 'NewJob', 'Vacation', 'Wedding', 'Other'], function(i,t) {
      var date = ko.utils.arrayFirst(model.importantDates, function(d) { return d.type === t; });
      if (!date) {
        date = { key: null, type: t, day: null, month: null, year: null, description: null };
        model.importantDates.push(date);
      }
    });

    var customer = new myc.Customer(model, config);

    self.customer(customer);
    self.isLoaded(true);

    if(!config.isNew)
      _displayAdditionalFields(model);

    $('#customerEmail').popover({html: true});
    // Sometimes isDirty flag is reset before customer has been initialized.
    // Set timeout value to 500 ms to avoid such situation.
    setTimeout(function () {
      customer.isDirty.reset();
      app.promptIfChanged(customer.isDirty);
    }, 500);
  };

  self.onsaved = function (resp) {
    self.customer().isDirty.reset();
    app.redirectSuccess('CUSTOMER_SAVESUCCESS_MSG', 'CUSTOMER_SAVESUCCESS_TITLE', resp.links.detail.href);
  };

  self.saveCommand = ko.asyncCommand({
    execute: function(complete) {
      app.showValidationErrors(true);
      if (self.validationState.isValid())
      {
        app.showProgressModal('SAVING_CUSTOMER');
        _saveCustomer()
          .then(_savePhoto)
          .done(self.onsaved)
          .fail(app.closeProgressModal) // only close the progress modal on fail since on success the page is redirecting
          .always(complete);
      } 
      else {
        app.clearNotifications();
        app.notifyError(app.localize('EDITCUSTOMER_VALIDATION_MESSAGE'), app.localize('EDITCUSTOMER_VALIDATION_MESSAGE_TITLE'));
        complete();
      }
    },
    canExecute: function() { 
      return (!config.ajaxService.isBusy());
    }
  });

  function _loadCustomer() {
    app.showProgressModal('CUSTOMER_LOADING');
    if (config.isNew) {
      // only need to load questionnair data when adding a new customer
      config.ajaxService
        .getJSON(config.questionnaireApiUrl, config.customerId)
        .done(function (data) { self.onload({ profileQuestionGroups: data }); })
        .always(app.closeProgressModal);
    } else {
      config.ajaxService
        .getJSON(config.customerApiUrl)
        .done(self.onload)
        .always(app.closeProgressModal);
    }
  }

  function _saveCustomer() {
    var model = ko.mapping.toJS(self.customer());

    // HACK: clean off ui only data from profile questions
    ko.utils.arrayForEach(model.profileQuestionGroups || [], function(g) {
      delete g.text;
      ko.utils.arrayForEach(g.questions, function(q) {
        delete q.text;
        ko.utils.arrayForEach(q.options, function(o) {
          delete o.text;
          delete o.imageUrl;
        });
      });
    });

    // HACK: remove empty important dates
    model.importantDates = Enumerable.From(model.importantDates).Where(function(d) { return !(!d.day && !d.month && !d.year); }).ToArray();

    return config.ajaxService.postJSON(config.customerApiUrl, ko.toJSON(model));
  }
  
  function _displayAdditionalFields(model) {
    if (model.socialNetworks.length > 0
      || model.gender
      || model.learningTopicsOfInterest.length > 0
      || model.preferredContactMethods.length > 0
      || model.preferredContactDays.length > 0
      || model.preferredContactTimes.length > 0
      || model.preferredContactFrequencies.length > 0
      || model.spouse
      || model.referredBy
      || model.employer
      || _activeSubscriptions(model.subscriptions))
      self.showAdditionalFields(true);
  };


  function _activeSubscriptions (subscriptions) {
    var match = ko.utils.arrayFirst(subscriptions, function (subscription) {
      return subscription.emailAddress != null && subscription.subscriptionStatus == 'OptedIn';
    });
    if (match)
      return true;
    return false;
  };

  //#endregion

  //#region Validation

  self.validationState = ko.validation.applyTo(self, {
    global: {
      customer: { validObject: true },
    }
  });

  //#endregion

  self.init = function() {
    _loadCustomer();
  };
};
