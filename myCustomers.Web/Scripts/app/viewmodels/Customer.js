var myc = myc || {};

myc.Customer = function(model, config) {
  var self = this;

  //#region Data Properties

  // NOTE: below are the proeprties that get mapped form the model
  // place all view specific functions and properties after the mapping section

  self.customerId                  = ko.observable(null);
  self.firstName                   = ko.observable('');
  self.middleName                  = ko.observable('');
  self.lastName                    = ko.observable('');
  self.emailAddress                = ko.observable(null);
  self.phoneNumbers                = ko.observableArray([]);
  self.addresses                   = ko.observableArray([]);
  self.profileDateUtc              = ko.observable(new Date());
  self.preferredLanguage           = ko.observable(config.defaultLanguage);
  self.birthday                    = ko.observable(null);
  self.anniversary                 = ko.observable(null);
  self.importantDates              = ko.observableArray([]);
  self.occupation                  = ko.observable('');
  self.spouse                      = ko.observable(null);
  self.pictureLastUpdatedDateUtc   = ko.observable(null);
  self.note                        = ko.observable('').extend({ maxLength: { max: 1000 } });
  self.socialNetworks              = ko.observableArray([]);
  self.gender                      = ko.observable('');
  self.learningTopicsOfInterest    = ko.observableArray([]);
  self.preferredShoppingMethods    = ko.observableArray([]);
  self.preferredContactDays        = ko.observableArray([]);
  self.preferredContactTimes       = ko.observableArray([]);
  self.preferredContactFrequencies = ko.observableArray([]);
  self.preferredContactMethods     = ko.observableArray([]);
  self.referredBy                  = ko.observable('');
  self.employer                    = ko.observable('');
  self.canSendSMSToCell            = ko.observable(false);
  self.profileQuestionGroups       = ko.observableArray([]);
  self.subscriptions               = ko.observableArray([]);
  self.isRegistered                = ko.observable();

  //#endregion

  //#region Mapping 

  var _mapping = {
    phoneNumbers: {
      create: function(ctx) {
        return new myc.PhoneNumber(ctx.data);
      }
    },
    addresses: {
      create: function(ctx) {
        return new myc.Address(ctx.data);
      }
    },
    importantDates: {
      key: function(item) { return ko.unwrap(item.type); },
      create: function(ctx) {
        return new myc.ImportantDate(ctx.data);
      }
    },
    birthday: {
      create: function(ctx) {
        return new myc.ImportantDate(ctx.data);
      }
    },
    anniversary: {
      create: function(ctx) {
        return new myc.ImportantDate(ctx.data);
      }
    },
    socialNetworks: {
      create: function(ctx) {
        var snc = ko.utils.arrayFirst(config.socialNetworksConfig, function(snc) { return snc.Type === ctx.data.type; });
        return new myc.SocialNetwork(ctx.data, snc);
      }
    },
    spouse: {
      create: function(ctx) {
        return new myc.Spouse(ctx.data);
      }
    },
    profileQuestionGroups: {
      create: function(ctx) {
        return new myc.ProfileQuestionGroup(ctx.data);
      }
    },
    subscriptions:
    {
      create: function(ctx) {
        return new myc.Subscription(ctx.data);
      }
    }
  };

  // Filter social networks that are not defined in config file but exist in database
  model.socialNetworks = ko.utils.arrayFilter(model.socialNetworks, function(sn) {
    return null !== ko.utils.arrayFirst(config.socialNetworksConfig, function(snc) { 
      return snc.Type === sn.type 
    });
  });

  model = $.extend({}, self, model);
  ko.mapping.fromJS(model, _mapping, self);

  //#endregion

  //#region Phone Numbers

  self.getAvailablePhoneNumberTypes = function(phoneNumber) {
    return ko.computed(function() {
      var selectedTypes = Enumerable
        .From(self.phoneNumbers())
        .Where(function(p) {
          return p && p.id !== phoneNumber.id;
        })
        .Select(function(p) {
          return p.phoneNumberType();
        })
      .ToArray();

      var availableTypes = Enumerable
        .From(['Home', 'Mobile', 'Work'])
        .Except(selectedTypes)
        .ToArray();

      return availableTypes;
    });
  };

  self.addPhoneNumberCommand = ko.command({
    execute: function() {
      var phoneNumber = new myc.PhoneNumber({ isPrimary: self.phoneNumbers().length === 0 });
      self.phoneNumbers.push(phoneNumber);
      phoneNumber.hasFocus(true);
    },
    canExecute: function() {
      var notEmpty = Enumerable
        .From(self.phoneNumbers())
        .Aggregate(true, function(res, a) { return res && a.notEmpty(); });

      return notEmpty && self.phoneNumbers().length < 3;
    }
  });

  function _removePhoneNumber(phoneNumber) {
    self.phoneNumbers.remove(phoneNumber);
    if(self.phoneNumbers().length === 1 || (phoneNumber.isPrimary() && self.phoneNumbers().length > 0))
      self.setPrimaryPhoneNumber(self.phoneNumbers()[0]);
  }

  self.removePhoneNumberCommand = ko.command({
    execute: function(phoneNumber) {
      if(phoneNumber.isEmpty())
        _removePhoneNumber(phoneNumber);
      else
        app.confirm('CONFIRM_REMOVEPHONE_TITLE', 'CONFIRM_REMOVEPHONE_BODY').done(function() { _removePhoneNumber(phoneNumber); });
    }
  });

  self.setPrimaryPhoneNumber = function(phoneNumber) {
    ko.utils.arrayForEach(self.phoneNumbers(), function(p) {
      p.isPrimary(p.id === phoneNumber.id);
    });
  };

  //#endregion

  //#region Addresses

  self.addAddressCommand = ko.command({
    execute: function() {
      var address = new myc.Address({ isPrimary: self.addresses().length === 0 });
      self.addresses.push(address);
      address.hasFocus(true);
    },
    canExecute: function() {
      var notEmpty = Enumerable
        .From(self.addresses())
        .Aggregate(true, function(res, a) { return res && a.notEmpty(); });
      return notEmpty && self.addresses().length < 5;
    }
  });

  function _removeAddress(address) {
    self.addresses.remove(address);

    if(self.addresses().length === 0)
    {
      self.addAddressCommand.execute();
    }

    if(self.addresses().length === 1 || (address.isPrimary() && self.addresses().length > 0))
      self.setPrimaryAddress(self.addresses()[0]);
  }

  self.removeAddressCommand = ko.command({
    execute: function(address) {
      if(address.isEmpty())
        _removeAddress(address);
      else
        app.confirm('CONFIRM_REMOVEADDRESS_TITLE', 'CONFIRM_REMOVEADDRESS_BODY').done(function() { _removeAddress(address); });
    }
  });

  self.setPrimaryAddress = function(address) {
    ko.utils.arrayForEach(self.addresses(), function(a) {
      a.isPrimary(a.id === address.id);
    });
  };

  //#endregion

  //#region Important Dates

  self.getImportantDate = function(type) {
    var date = ko.utils.arrayFirst(self.importantDates(), function(d) { return d.type() === type; });
    return date;
  };

  //#endregion

  //#region Social Networks

  $.each(config.socialNetworksConfig, function(i, sn) {
    sn.Tooltip = app.localize(sn.Tooltip);
  });

  self.socialNetworksConfig = {};
  $.each(config.socialNetworksConfig, function(i, sn) { self.socialNetworksConfig[sn.Type] = sn; });

  self.availableSocialNetworkTypes = ko.computed(function() {
    var types = Enumerable
      .From(self.socialNetworks())
      .Select(function(sn) { return sn.type(); })
      .ToArray();

    var available = Enumerable
      .From(config.socialNetworksConfig)
      .Select(function(sn) { return sn.Type; })
      .Except(types)
      .Select(function(t) { return self.socialNetworksConfig[t]; })
      .ToArray();

    return available;
  });

  self.addSocialNetwork = function($data) {
    var sn = new myc.SocialNetwork({
      type: $data.Type,
      url: $data.Url
    }, $data);
    sn.hasFocus = true;
    self.socialNetworks.push(sn);
  };

  self.removeSocialNetwork = function($data) {
    self.socialNetworks.remove(function(sn) { return sn.type() === $data.type(); });
  };

  //#endregion


  //#region Subscriptions

  if(model.subscriptions.length === 0)
  {
    $.each(config.subscriptionsConfig, function(i, sb) {
      self.subscriptions.push(new myc.Subscription({ subscriptionType: sb.SubscriptionType }));
    });
  }

  self.updateSubscription = function(data) {
    return ko.computed({
      read: function() {
        if(data.subscriptionStatus() === 'OptedIn')
          return true;
        else return false;
      },
      write: function(checked) {
        if(checked)
          data.subscriptionStatus('OptedIn');
        else
          data.subscriptionStatus('OptedOutByConsultant');
      }
    });
  };

  self.canModifySubscription = function(data) {
    return data.subscriptionStatus() != 'OptedOutByCustomer';
  };

  //#endregion 

  //#region Validation

  self.emailHasBeenFocused = ko.observable(false);
  self.enableEmailValidation = ko.computed(function() {
    return self.emailHasBeenFocused() || app.showValidationErrors();
  });

  self.validationState = ko.validation.applyTo(self, {
    global: {
      firstName: {
        required: true,
        pattern: { message: 'INVALID_NAME', params: /^[^<>0123456789!@#\$\[\]\+\(\)\{\}\*,\.;:&\^%_=\\\|\/\?]+$/gi }
      },
      lastName: {
        required: true,
        pattern: { message: 'INVALID_NAME', params: /^[^<>0123456789!@#\$\[\]\+\(\)\{\}\*,\.;:&\^%_=\\\|\/\?]+$/gi }
      },
      middleName: { pattern: { message: 'INVALID_NAME', params: /^[^<>0123456789!@#\$\[\]\+\(\)\{\}\*,\.;:&\^%_=\\\|\/\?]+$/gi } },
      emailAddress: {
        required: {
          onlyIf: self.isRegistered,
          message: 'EMAIL_REQUIRED',
        },
        pattern: {
          onlyIf: self.enableEmailValidation,
          message: 'INVALID_EMAIL',
          params: /^[a-z0-9!#$%&'*+\/=?\^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+\/=?\^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?$/gi
        },
        uniqueEmail: {
          onlyIf: self.enableEmailValidation,
          message: 'INVALID_EMAIL',
          params: {
            url: config.validationUrl.replace('_action_', 'email'),
            customerId: self.customerId()
          }
        }
      },
      profileDateUtc: { required: true, validDate: true },
      phoneNumbers: { validArray: true },
      addresses: { validArray: true },
      birthday: { validObject: true },
      anniversary: { validObject: true },
      importantDates: { validArray: true },
      spouse: { validObject: true },
      socialNetworks: { validArray: true }
    }
  });

  //#endregion

  // Initialize defaults
  if(self.phoneNumbers().length === 0)
    self.phoneNumbers.push(new myc.PhoneNumber({ isPrimary: true }));

  if(self.addresses().length === 0)
    self.addresses.push(new myc.Address({ isPrimary: true }, { showAddressee: !config.isNew }));

  if(!self.birthday())
    self.birthday(new myc.ImportantDate({ type: 'Birthday' }));

  if(!self.anniversary())
    self.anniversary(new myc.ImportantDate({ type: 'Anniversary' }));

  //#region Dirty Flag

  self.isDirty = new ko.dirtyFlag([
    self.firstName,
    self.middleName,
    self.lastName,
    self.emailAddress,
    self.phoneNumbers,
    self.addresses,
    self.profileDateUtc,
    self.preferredLanguage,
    self.birthday,
    self.anniversary,
    self.importantDates,
    self.occupation,
    self.spouse,
    self.pictureLastUpdatedDateUtc,
    self.note,
    self.socialNetworks,
    self.gender,
    self.learningTopicsOfInterest,
    self.preferredShoppingMethods,
    self.preferredContactDays,
    self.preferredContactTimes,
    self.preferredContactFrequencies,
    self.preferredContactMethods,
    self.referredBy,
    self.employer,
    self.canSendSMSToCell,
    self.subscriptions
    //self.profileQuestionGroups // Slows IE8 down too much
  ]);

  //#endregion
};
