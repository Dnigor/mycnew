ko.bindingHandlers.busy = {
  update: function(element, valueAccessor, allBindingsAccessor, viewModel) {
    var allBindings = allBindingsAccessor();
    var value = ko.unwrap(valueAccessor());
    if(value)
      $(element).show();
    else
      $(element).hide();
  }
};

ko.bindingHandlers.updateOnEnter = {
  init: function(element, valueAccessor, allBindingsAccessor, viewModel) {
    var allBindings = allBindingsAccessor();
    $(element).keypress(function(event) {
      var keyCode = (event.which ? event.which : event.keyCode);
      if(keyCode === 13)
      {
        // HACK: trigger a blur so that the value binding will update the view model
        $(element).trigger('blur');
        $(element).trigger('focus');
        return false;
      }
      return true;
    });
  }
};

ko.bindingHandlers.localize = {
  update: function(element, valueAccessor, allBindingsAccessor) {
    var bindings = allBindingsAccessor();
    var resourceKey = bindings.resourceKey;
    var value = ko.unwrap(valueAccessor());

    value = app.localize(resourceKey, value);

    $(element).text(value);
  }
};

ko.bindingHandlers.currency = {
  update: function(element, valueAccessor, allBindingsAccessor) {
    var value = valueAccessor();
    value = ko.unwrap(value);
    $(element).text(app.formatCurrency(value));
  }
};

ko.bindingHandlers.phone = {
  update: function(element, valueAccessor, allBindingsAccessor) {
    var value = valueAccessor();
    value = ko.unwrap(value);
    $(element).text(app.formatPhone(value));
  }
};

ko.bindingHandlers.shortDate = {
  update: function(element, valueAccessor, allBindingsAccessor) {
    var value = valueAccessor();
    value = ko.unwrap(value);
    $(element).text(app.formatShortDate(value));
  }
};

ko.bindingHandlers.fullDayMonthDate = {
  update: function(element, valueAccessor, allBindingsAccessor) {
    var value = valueAccessor();
    value = ko.unwrap(value);
    $(element).text(app.formatFullDayMonthDate(value));
  }
};

ko.bindingHandlers.shortMonthDayDate = {
  update: function(element, valueAccessor, allBindingsAccessor) {
    var value = valueAccessor();
    value = ko.unwrap(value);
    $(element).text(app.formatShortMonthDayDate(value));
  }
};

ko.bindingHandlers.shortMonthYearDate = {
  update: function(element, valueAccessor, allBindingsAccessor) {
    var value = valueAccessor();
    value = ko.unwrap(value);
    $(element).text(app.formatShortMonthYearDate(value));
  }
};

ko.bindingHandlers.hasLostFocus = {
  init: function (element, valueAccessor, allBindingsAccessor) {
    var value = valueAccessor();

    if (!ko.isObservable(value))
      return;

    var $element = $(element);
    $element.blur(function (e) { value(true); });
  }
};

ko.bindingHandlers.hasBeenFocused = {
  init: function (element, valueAccessor, allBindingsAccessor) {
    var value = valueAccessor();
    if (!ko.isObservable(value))
      return;

    var $element = $(element);
    var $descendants = $element.find('*');

    var hasBeenFocused = false;
    $element.focusin(function (e) {
      hasBeenFocused = true;
      e.stopPropagation();
    });

    $('body').focusin(function (e) {
      if (hasBeenFocused && !$(e.target).is($element) && !$(e.target).is($descendants))
        value(true);
    });
  }
};

ko.bindingHandlers.showValidationFor = {
  init: function(element, valueAccessor, allBindingsAccessor) {
    var $element  = $(element);
    var bindings  = allBindingsAccessor();
    var value     = valueAccessor();
    var placement = bindings.validationPlacement;

    if (!placement) placement = 'right';

    if (!ko.isObservable(value) || !ko.isObservable(value.error))
      return;

    var dataContent = $element.attr('data-content');
    var dataTrigger = $element.attr('data-trigger');

    if (dataTrigger) $element.removeAttr('data-trigger');
    if (dataContent) {
      $element.removeAttr('data-content');
      $element.popover('destroy');
    }

    var errorModel = ko.computed(function() { 
      // hit the observables so that they all can trigger re-evaluation of the computed
      var isModified           = value.isModified();
      var isValid              = value.isValid();
      var showValidationErrors = app.showValidationErrors();
      var error                = value.error();

      return { 
        show: (isModified && !isValid) || (showValidationErrors && error), 
        error: error
      };
    });

    var hasFocus = $element.is(':focus');
    $element.focusin(function(e) {
      if ($element.is($(e.currentTarget))) {
        hasFocus = true;
        errorPresenter(errorModel());
      }
    });

    $element.focusout(function(e) {
      if ($element.is($(e.currentTarget))) {
        $element.popover('hide');
        hasFocus = false;
      }
    });

    var errorPresenter = function(e) {
      if (e.show) {
        $element
          .addClass('error')
          .popover('destroy')
          .popover({ placement: placement, animation: false, trigger: 'manual', content: app.localize(e.error) });

        if (hasFocus)
          $element.popover('show');
      }
      else {
        $element
          .removeClass('error')
          .popover('destroy');
        
        if (dataContent) {
          $element.popover({ placement: placement, html: true, content: dataContent, animation: false, trigger: dataTrigger });
          if (hasFocus)
            $element.popover('show');
        }
      }
    };

    errorModel.subscribe(errorPresenter);
  }
};

ko.bindingHandlers.paymentAmount = {
  init: function(element, valueAccessor, allBindingsAccessor) {
    var observable = valueAccessor();
    var $element = $(element);
    var valueUpdate = allBindingsAccessor().valueUpdate;

    if (!valueUpdate) {
      valueUpdate = "change";
    }

    // set initial value
    var value = ko.unwrap(observable);    
    $element.val(Globalize.format(value, "N", Globalize.culture()));

    //handle the field changing
    ko.utils.registerEventHandler(element, valueUpdate, function () {
      var value = $element.val();
      if (value !== undefined && value !== '')
        value = Globalize.parseFloat(value);     
      observable(value);
      if (!isNaN(value))
        $element.val(Globalize.format(value, "N", Globalize.culture()));
    });
  }
};

ko.bindingHandlers.datepicker = {
  init: function(element, valueAccessor, allBindingsAccessor) {
    var observable = valueAccessor();
    var bindings = allBindingsAccessor();
    var $element = $(element);

    // set initial value
    var value = ko.unwrap(observable);
    if (typeof value === 'string')
      if (value.substring(value.length-1) === 'Z') // ISO date
        value = moment(value).toDate();
      else
        value = Globalize.parseDate(value, Globalize.culture().calendar.patterns.d);

    value = Globalize.format(value, 'd');
    if (!value) 
      value = ko.unwrap(observable);

    $element.val(value);

    // init jQueryUI datepicker
    var options = $.extend({}, app.datePickerGlobalOptions, bindings.datepickerOptions || {});
    
    // prevent press ENTER key to not reset incorrect date to today's date(default jquery datepicker behavior)
    $element.bind("keydown", function (event) {
      if (event.which == $.ui.keyCode.ENTER) {
        event.preventDefault();
        event.stopImmediatePropagation();
        return false;
      }
    });

    $element.datepicker(options);

    // force the datepicker to show when clicking into the edit box (for IE)
    $element.click(function() { $element.datepicker('show'); });

    var considerTime = allBindingsAccessor().considerTime;
    
    //handle the field changing
    ko.utils.registerEventHandler(element, "change", function() {
      var value = $element.val();
      var date = Globalize.parseDate(value, Globalize.culture().calendar.patterns.d);

      if (date && considerTime) {        
        var m = new Date();
        value = moment(date).hour(m.getHours()).minute(m.getMinutes()).second(m.getSeconds()).millisecond(m.getMilliseconds()).toDate();
      }
    
      date = app.toISO8601(value);
      if (date)
          value = date;
      observable(value);
      $element.datepicker('hide');
    });

    //handle disposal (if KO removes by the template binding)
    ko.utils.domNodeDisposal.addDisposeCallback(element, function() {
      $element.datepicker("destroy");
    });
  },
  update: function(element, valueAccessor, allBindingsAccessor) {
    var value = ko.unwrap(valueAccessor());

    if (!value)
      $(element).val(null);

    if (typeof value === 'string')
      if (value.substring(value.length - 1) === 'Z') // ISO date
        value = moment(value).toDate();
      else
        value = Globalize.parseDate(value, Globalize.culture().calendar.patterns.d);

    value = Globalize.format(value, 'd');
    if (value) $(element).val(value);
  }
};

ko.extenders.date = function(target, options) {
  var result = ko.computed({
    read: target,  // always return the original observables value
    write: function(newValue) {
      newValue = moment(newValue);
      if(newValue !== null)
        target(newValue.toDate());
      else
        target.notifySubscribers(target());
    }
  });

  result(target());

  return result;
};

ko.extenders.numeric = function(target, options) {
  // create a writeable computed observable to intercept writes to our observable
  var result = ko.computed({
    read: target,  // always return the original observables value
    write: function(newValue) {
      if(typeof (options) !== 'object')
      {
        options = { precision: options };
      }

      if(options.required === undefined || options.required === null)
        options.required = true;

      if(options.precision === undefined || options.precision === null)
        options.precision = 0;

      if(options.required || (newValue !== undefined && newValue !== null && newValue !== ''))
      {
        var current = target();
        var roundingMultiplier = Math.pow(10, options.precision);
        var newValueAsNum = isNaN(newValue) ? current : parseFloat(+newValue);
        var valueToWrite = Math.round(newValueAsNum * roundingMultiplier) / roundingMultiplier;

        if(typeof (options.min) === 'number' && newValue < options.min)
          valueToWrite = current;
        else if(typeof (options.max) === 'number' && newValue > options.max)
          valueToWrite = current;

        if(isNaN(valueToWrite))
          valueToWrite = current;

        // only write if it changed
        if(valueToWrite !== current)
        {
          target(valueToWrite);
        } else
        {
          // if the rounded value is the same, but a different value was written, force a notification for the current field
          if(newValue !== current)
          {
            target.notifySubscribers(valueToWrite);
          }
        }
      }
      else
        target(null);
    }
  });

  //initialize with current value to make sure it is rounded appropriately
  result(target());

  //return the new computed observable
  return result;
};

ko.bindingHandlers.typeAhead = {
  init: function(element, valueAccessor, allBindingsAccessor) {
    var observable = valueAccessor();
    var bindings = allBindingsAccessor();
    var sourceBinding = bindings.typeAheadSource;
    var $element = $(element);

    $element.change(function() {
      // if current value of the text box is empty 
      // the user is trying to remove the text
      if($element.val() === '')
      {
        // clear the data attributes off the element
        $element.attr('data-value', null);
        $element.attr('data-text', null);
      }

      var value = $element.attr('data-value');
      var text = $element.attr('data-text');

      observable(value);
      $element.val(text);
    });

    $element.typeahead({
      minLength: 1,
      autocomplete: 'off',
      source: sourceBinding,
      matcher: function(item) { return true; }, // results are already matched by ajax query so everything in the results should match
      updater: function(val, type) {
        // tuck the selected text value away on a data attribute
        if(type === 'text')
          $element.attr('data-text', val);
        return val;
      }
    });
  }
};

// This shoudl no longer be needed since Knockout 2.3.0 fixed issues with hasFocus binding
//ko.bindingHandlers.setFocus = {
//  init: function (element, valueAccessor) {
//    var value = ko.unwrap(valueAccessor());
//    value ? element.focus() : element.blur();
//    ko.utils.triggerEvent(element, value ? "focusin" : "focusout");  //IE
//  }
//};

ko.extenders.maxLength = function(target, options) {
  var _maxValue = parseInt(options.max, 10);
  var _characterCount = ko.observable(0);

  var result = ko.computed({
    read: target,
    write: function(newValue) {
      // normalize line endings
      if(newValue)
        newValue = newValue.replace(/\r/gm, '').replace(/\n/gm, '\r\n');

      if(newValue)
      {
        if(newValue.length > _maxValue)
        {
          newValue = newValue.substring(0, _maxValue);
        }
        _characterCount(newValue.length);
      }
      else
      {
        _characterCount(0);
      }

      target(newValue);

      if(newValue === target())
        target.notifySubscribers(newValue);
    }
  });

  result.remainingCharacterCount = ko.computed(function() { return _maxValue - _characterCount(); });
  result.maxCharacterCount = _maxValue;
  result.characterCount = _characterCount;

  return result;
};

ko.dirtyFlag = function(root) {
  var _isDirty = ko.observable(false);

  var result = ko.computed(function() {
    if(!_isDirty())
      ko.mapping.toJS(root); //just for subscriptions
    return _isDirty();
  });

  result.reset = function() { _isDirty(false); };

  result.subscribe(function() {
    if(!_isDirty())
      _isDirty(true);
  });

  return result;
};

ko.validation.applyTo = function (target, validators) {
  var p;
  if (validators.hasOwnProperty('global')) {
    for (p in validators.global) {
      if (target.hasOwnProperty(p)) {
        target[p].extend(validators.global[p]);
      }
    }
  }

  var sub = app.subsidiary.toLowerCase();
  if (validators.hasOwnProperty(sub)) {
    for (p in validators[sub]) {
      if (target.hasOwnProperty(p)) {
        target[p].extend(validators[sub][p]);
      }
    }
  }

  return ko.validatedObservable(target);
};

ko.validation.rules['uniqueEmail'] = {
  async: true,
  validator: function(email, options, callback) {
    if(!email) {
      callback(true);
      return;
    }

    var data = ko.toJSON();
    $.getJSON(options.url, { value: email, customerId: options.customerId }).done(callback);
  }
};

/*
 * This rules checks the given array of objects/observables and returns 
 * true if at least one of the elements validates agains the the default
 * 'required' rules
 * 
 * Example:
 * 
 *
 * self.mobilePhone.extend({ requiresOneOf: [self.homePhone, self.mobilePhone] });
 * self.homePhone.extend({ requiresOneOf: [self.homePhone, self.mobilePhone] }); 
 *
*/
ko.validation.rules['requiresOneOf'] = {
  getValue: function(o) {
    return (typeof o === 'function' ? o() : o);
  },
  validator: function(val, fields) {
    var self = this;

    var anyOne = ko.utils.arrayFirst(fields, function(field) {
      var stringTrimRegEx = /^\s+|\s+$/g,
                testVal;

      var val = self.getValue(field);

      if(val === undefined || val === null)
        return false;

      testVal = val;
      if(typeof (val) === "string")
      {
        testVal = val.replace(stringTrimRegEx, '');
      }

      return ((testVal + '').length > 0);

    });

    return (anyOne !== null);
  },
  message: 'VALIDATION_REQUIRESONEOF'
};

/*
 * Aggregate validation of all the validated properties within an object
 * Parameter: true|false
 * Example:
 *
 * viewModel = {
 *    person: ko.observable({
 *       name: ko.observable().extend({ required: true }),
 *       age: ko.observable().extend({ min: 0, max: 120 })
 *    }.extend({ validObject: true })
 * }   
*/
ko.validation.rules["validObject"] = {
  validator: function(obj, bool) {
    if(obj === null || obj === undefined)
      return true;

    if(typeof obj !== "object")
    {
      throw "[validObject] Parameter must be an object";
    }

    return bool === (ko.validation.group(obj)().length === 0);
  },
  message: "VALIDATION_OBJECT"
};

/*
 * Aggregate validation of all the validated elements within an array
 * Parameter: true|false
 * Example
 *
 * viewModel = {
 *    person: ko.observableArray([{
 *       name: ko.observable().extend({ required: true }),
 *       age: ko.observable().extend({ min: 0, max: 120 })
 *    }, {
 *       name: ko.observable().extend({ required: true }),
 *       age: ko.observable().extend({ min:0, max:120 })
 *    }].extend({ validArray: true })
 * }   
*/
ko.validation.rules["validArray"] = {
  validator: function(arr, bool) {
    if(!arr || typeof arr !== "object" || !(arr instanceof Array))
    {
      throw "[validArray] Parameter must be an array";
    }

    var filtered = ko.utils.arrayFilter(arr, function(element) {
      return ko.validation.group(ko.unwrap(element))().length !== 0;
    });

    return bool === (filtered.length === 0);
  },
  message: "VALIDATION_ARRAY"
};

ko.validation.rules["validPhone"] = {
  validator: function(value, bool) {
    if(!value)
      return true;

    var country = app.settings.phoneRegion;
    try
    {
      var phoneUtil = app.phoneUtil;
      var number = phoneUtil.parseAndKeepRawInput(value, country);

      if(!phoneUtil.isPossibleNumber(number))
        return false;

      if(!phoneUtil.isValidNumberForRegion(number, country) && !phoneUtil.isValidNumber(number))
        return false;

      return true;
    } catch(e) { return false; }
  },
  message: "VALIDATION_PHONE"
};

ko.validation.rules['validDate'] = {
  validator: function(val) {
    if (val === undefined || val === null || val === '')
      return true;

    if (typeof val === 'string')
      if (val.substring(val.length-1) === 'Z') // ISO date
        val = moment(val).toDate();
      else
        val = Globalize.parseDate(val, Globalize.culture().calendar.patterns.d);

    return val !== null;
  },
  message: 'INVALID_DATE'
};

ko.validation.rules['validAmount'] = {
    validator: function (value) {
        if (typeof value === "string")
            value = Globalize.parseFloat(value);
        return !isNaN(value) && value > 0.0;
  },
  message: 'INVALID_AMOUNT'
}