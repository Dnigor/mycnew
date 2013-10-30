var app = app || {};

if (!window.console) { window.console = { log: function () { } }; }

app.isNonEmptyString = function (s) {
  return typeof s === 'string' && '' !== s;
};

app.safeTrim = function (str) {
  if (str === undefined || str === null)
    return '';
  return str.replace(/^\s+|\s+$/g, '');
};

app.promptIfChanged = function (dirtyFlag) {
  window.onbeforeunload = function (e) {
    var msg = app.localize('PROMPT_DISCARDCHANGES');
    var isDirty = ko.unwrap(dirtyFlag);
    if (isDirty) {
      e = e || window.event;
      if (e) { e.returnValue = msg; }
      return msg;
    }
  };
};

app.showProgressModal = function (messageResKey) {
  var vm = {
    message: app.localize(messageResKey)
  };

  var placeHolder = $('#progressModal');
  if (placeHolder.length === 1) {
    ko.renderTemplate(
      'progressModalTemplate',
      vm,
      {
        afterRender: function (nodes) {
          placeHolder.modal();
          $("#progressbar").progressbar({ value: false });
        }
      },
      placeHolder.get(0)
    );
  }
};

app.closeProgressModal = function () {
  var placeHolder = $('#progressModal');
  if (placeHolder.length === 1) {
    $("#progressbar").progressbar('destroy');
    placeHolder.modal('hide');
    placeHolder.empty();
  }
};

app.confirm = function (headerResKey, bodyResKey, bodyParams, primaryLabelResKey) {
  var result = $.Deferred();
  var body = app.localize(bodyResKey);

  if (bodyParams instanceof Array)
    for (var i = 0; i < bodyParams.length; i++) {
      var placeholder = '{' + i + '}';
      body = body.replace(placeholder, bodyParams[i]);
    }

  var vm = {
    header: app.localize(headerResKey),
    body: body,
    closeLabel: app.localize('CANCEL'),
    primaryLabel: primaryLabelResKey ? app.localize(primaryLabelResKey) : app.localize('OK'),
    close: function () {
      placeHolder.modal('hide');
      result.reject();
    },
    action: function () {
      placeHolder.modal('hide');
      result.resolve();
    }
  };

  var placeHolder = $('#confirmModal');
  if (placeHolder.length === 1) {
    ko.renderTemplate(
      'confirmModalTemplate',
      vm,
      {
        afterRender: function (nodes) { placeHolder.modal(); }
      },
      placeHolder.get(0)
    );
  }

  return result.promise({});
};

app.session = {
  getItem: function (key) { return window.sessionStorage.getItem(app.subsidiary + ':' + app.consultantId + ':' + key); },
  setItem: function (key, value) { window.sessionStorage.setItem(app.subsidiary + ':' + app.consultantId + ':' + key, value); }
};

app.formatCurrency = function (value) {
  if (typeof value === 'string')
    value = parseFloat(value);

  if (isNaN(value))
    value = 0.0;

  var format = app.settings.currencyFormat;
  return format.replace(/\{(\d+)\:([^\}]+)\}/g, function (m) { return Globalize.format(value, arguments[2]); });
};

app.toISO8601 = function (date) {
  function pad(n) { return n < 10 ? '0' + n : n; }

  if (!date)
    return null;

  if (typeof date === 'string')
    if (date.substring(date.length - 1) === 'Z') // ISO date
      date = moment(date).toDate();
    else
      date = Globalize.parseDate(date, Globalize.culture().calendar.patterns.d);

  if (!date)
    return null;

  return date.getUTCFullYear() + '-' + pad(date.getUTCMonth() + 1) + '-' + pad(date.getUTCDate()) + 'T' + pad(date.getUTCHours()) + ':' + pad(date.getUTCMinutes()) + ':' + pad(date.getUTCSeconds()) + 'Z';
};

app.phoneUtil = i18n.phonenumbers.PhoneNumberUtil.getInstance();
app.formatPhone = function (value) {
  if (typeof value !== 'string') return '';
  var country = app.settings.phoneRegion;
  var phone = cleanPhone(value);
  try {
    var phoneUtil = app.phoneUtil;
    var number = phoneUtil.parseAndKeepRawInput(phone, country);
    if (phoneUtil.isValidNumberForRegion(number, country)) {
      return phoneUtil.format(number, i18n.phonenumbers.PhoneNumberFormat.NATIONAL).toString();
    } else if (phoneUtil.isValidNumber(number)) {
      return phoneUtil.format(number, i18n.phonenumbers.PhoneNumberFormat.INTERNATIONAL).toString();
    } else {
      return value; //phoneUtil.formatInOriginalFormat(number, country).toString();
    }
  } catch (e) { return phone; }
};

app.formatPhoneTypeAbbr = function (value) {
  switch (value) {
    case 'Home':
      return app.localize('ABBR_HOMEPHONE');
    case 'Work':
      return app.localize('ABBR_WORKPHONE');
    case 'Mobile':
      return app.localize('ABBR_MOBILEPHONE');
    default:
      return app.localize('ABBR_OTHERPHONE');
  }
};

app.formatShortDate = function (value) {
  if (value === undefined || value === null)
    return '';

  if (typeof value === 'string')
    if (value.substring(value.length - 1) === 'Z') // ISO date
      value = moment(value).toDate();
    else
      value = Globalize.parseDate(value, Globalize.culture().calendar.patterns.d);

  return Globalize.format(value, 'd');
};

app.formatFullDayMonthDate = function (value) {
  if (value === undefined || value === null)
    return '';

  if (typeof value === 'string')
    if (value.substring(value.length - 1) === 'Z') // ISO date
      value = moment(value).toDate();
    else
      value = Globalize.parseDate(value, Globalize.culture().calendar.patterns.d);
    return Globalize.format(value, app.settings.formatFullDayMonthDate);

};

app.formatShortMonthDayDate = function (value) {
  if (value === undefined || value === null)
    return '';

  if (typeof value === 'string')
    if (value.substring(value.length - 1) === 'Z') // ISO date
      value = moment(value).toDate();
    else
      value = Globalize.parseDate(value, Globalize.culture().calendar.patterns.d);

  return Globalize.format(value, "MMM d");
};

app.formatShortMonthYearDate = function (value) {
  if (value === undefined || value === null)
    return '';

  if (typeof value === 'string')
    if (value.substring(value.length - 1) === 'Z') // ISO date
      value = moment(value).toDate();
    else
      value = Globalize.parseDate(value, Globalize.culture().calendar.patterns.d);

  return Globalize.format(value, "MMM yyyy");
};

app.formatSpecialDate = function (month, day) {
  // Use a leap year so that Feb 29th is an acceptable value
  var date = new Date(2000, month - 1);
  date.setDate(day);
  return Globalize.format(date, 'MMM d');
};

app.localize = function (key, value) {
  if (!key) {
    return undefined;
  }

  key = key.replace('{0}', value).toUpperCase();
  var lvalue = Globalize.localize(key, app.language2);
  return (lvalue === undefined || lvalue === null) ? '[[' + key + ']]' : lvalue;
};

app.localizeFormat = function (key) {
  var args = arguments;
  if (args.length < 2)
    throw new Error('app.localizeFormat requires at least two arguments.');

  var format = Globalize.localize(args[0], app.language2);
  if (!format)
    return '[[' + args[0] + ']]';

  return format.replace(/\{(\d+)\}/g, function (match, number) {
    var val = args[+number + 1];
    return val ? val : match;
  });
};

app.scrollTo = function (selector) {
  setTimeout(function () {
    $('html, body').animate({
      scrollTop: $(selector).offset().top
    }, 500);
  }, 0);
};

app.redirectSuccess = function (msg, title, url) {
  app.session.setItem('myc.successNotification', ko.toJSON({ msg: msg, title: title }));
  location.href = url;
};

app.processNotifications = function () {
  var notification = app.session.getItem('myc.successNotification');
  if (notification) {
    app.session.setItem('myc.successNotification', null);
    notification = ko.utils.parseJson(notification);
    if (notification)
      app.notifySuccess(app.localize(notification.msg), app.localize(notification.title));
  }
};

app.notifySuccess = function (msg, title) {
  toastr.success(msg, title, { timeOut: 5000 });
};

app.notifyError = function (msg, title) {
  toastr.error(msg, title);
};

app.clearNotifications = function () {
  toastr.clear();
};

app.LOGLEVEL_TRACE = 1;
app.LOGLEVEL_INFO = 2;
app.LOGLEVEL_WARN = 3;
app.LOGLEVEL_ERROR = 4;
app.LOGLEVELS = {
  1: 'Trace',
  2: 'Info',
  3: 'Warn',
  4: 'Error'
};

app.minLogLevel = 4;
app.minConsoleLogLevel = 4;

app.log = function (lvl, msg) {
  if (lvl >= app.minConsoleLogLevel)
    console.log(app.LOGLEVELS[lvl] + ': ' + msg);

  if (lvl >= app.minLogLevel)
    $.ajax({
      url: app.logUrl,
      type: 'POST',
      dataType: 'json',
      contentType: 'application/json; charset=utf-8',
      data: ko.toJSON({ level: app.LOGLEVELS[lvl], message: msg })
    });
};

app.logTrace = function (msg) { app.log(app.LOGLEVEL_TRACE, msg); };
app.logInfo = function (msg) { app.log(app.LOGLEVEL_INFO, msg); };
app.logWarn = function (msg) { app.log(app.LOGLEVEL_WARN, msg); };
app.logError = function (msg) { app.log(app.LOGLEVEL_ERROR, msg); };

// global ajax error handler
$(document).ajaxError(function (event, jqxhr, settings, error) {
  // ignore errors due to user navigating or reloading the page.
  // see http://ilikestuffblog.com/2009/11/30/how-to-distinguish-a-user-aborted-ajax-call-from-an-error/
  function userAborted() { return !jqxhr.getAllResponseHeaders(); }
  if (!userAborted() && settings.url !== app.logUrl) {
    app.notifyError(app.localize('ERROR_NOTIFY_MESSAGE'), error);
  }
});

// global javascript error handler
window.onerror = function (msg, url, line) {
  // format the message
  msg = msg + '\r\nurl:' + url + '\r\nline:' + line;

  console.error(msg);

  // show error notification
  var errCode = (Math.floor((1 + Math.random()) * 0x10000).toString(16).substring(1)).toUpperCase();
  app.notifyError(app.localize('ERROR_NOTIFY_MESSAGE'), app.localize('ERROR_NOTIFY_TITLE').replace('{code}', errCode));

  app.logError(errCode + ': ' + msg);

  // If you return true, then error alerts (like in older versions of 
  // Internet Explorer) will be suppressed.
  return true;
};

// How to create a GUID / UUID in Javascript? 
// http://stackoverflow.com/a/105074
app.newId = function () {
  function s4() { return Math.floor((1 + Math.random()) * 0x10000).toString(16).substring(1); }
  return (s4() + s4() + "-" + s4() + "-" + s4() + "-" + s4() + "-" + s4() + s4() + s4());
};

app.luhnArr = [0, 2, 4, 6, 8, 1, 3, 5, 7, 9];
app.luhnCheck = function (ccnum) {
  var counter = 0;
  var incNum;
  var odd = false;
  var temp = String(ccnum).replace(/[^\d]/g, "");
  if (temp.length === 0) return false;
  for (var i = temp.length - 1; i >= 0; --i) {
    incNum = parseInt(temp.charAt(i), 10);
    counter += (odd = !odd) ? incNum : app.luhnArr[incNum];
  }
  return (counter % 10 === 0);
};

app.getCCType = function (accountType, ccnum) {
  if (/^4/gi.test(ccnum))
    return 'Visa';
  else if (/^34|37/gi.test(ccnum) && accountType === 2)
    return 'Amex';
  else if (/^51|52|53|54|55/gi.test(ccnum))
    return 'Master';
  else if (/^6011/gi.test(ccnum))
    return 'Discover';
  return null;
};

app.datePickerGlobalOptions = {
  minDate: moment().year(moment().year() - 100).toDate() // minDate = -100 years from current date
};