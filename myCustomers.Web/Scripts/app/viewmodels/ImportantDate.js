var myc = myc || {};

myc.ImportantDate = function (model) {
  var self = this;

  //#region Data Properties

  self.key         = ko.observable();
  self.type        = ko.observable();
  self.day         = ko.observable(); 
  self.month       = ko.observable();
  self.year        = ko.observable(); 
  self.description = ko.observable().extend({
      validation: {
          validator: function (value) {
              if (self.type() !== 'Other' || (!self.date())) return true;
              if (value && $.trim(value).length > 0) return true;
              return false;
          },
          message: 'VALIDATION_REQUIRED'
      }
  });

  //#endregion

  //#region Mapping

  model = $.extend({}, self, model);
  ko.mapping.fromJS(model, {}, self);

  //#endregion

  self.isEmpty = ko.computed(function() {
    var day   = self.day();
    var month = self.month();
    var year  = self.year();
    return !day && !month && !year;
  });

  var regExp = '^\\d{1,2}\\' + Globalize.culture().calendar['/'] + '\\d{1,2}$';
  var rxNoYear = new RegExp(regExp);
  var sdate = '';
  if (!!self.month() && !!self.day()) {
    sdate = app.formatShortDate(moment([!self.year() ? 1904 : self.year(), self.month() - 1, self.day()]).toDate());
    if (!self.year())
      sdate = sdate.replace(/[^\d]\d{4}$/, '');
  }

  function _updateDateFields(value) {
    var date = value;
    if (typeof date === 'string') {
      if (rxNoYear.test(date)) {
        date += Globalize.culture().calendar['/'] + '1904'; // REVIEW: Hardcoded minYear value '1904'
        self.year(null);
        date = Globalize.parseDate(date, Globalize.culture().calendar.patterns.d);
      }
      else {
        date = app.toISO8601(date);
        if (moment(date) && moment(date).isValid()) {
          date = moment(date).toDate();
        }
        if (date && typeof date === 'object') self.year(date.getFullYear());
      }
    }

    if (date && typeof date === 'object') {
      self.day(date.getDate());
      self.month(date.getMonth() + 1);
    } else {
      self.day(null);
      self.month(null);
    }
  };

  // used for binding textboxes that require a full date to be entered
  self.date = ko.observable(sdate).extend({
    validation: {
      validator: function(value) {
        if (self.type() === 'Birthday' || self.type() === 'Anniversary') return true;
        if (!value) return true;
        var date = app.toISO8601(value);
        return  date !== null;
      },
      message: 'INVALID_DATE'
    }
  });

  self.date.subscribe(_updateDateFields);

  // used to bind to textbox with birthday specific validation
  self.birthday = ko.observable(sdate).extend({ 
    validation: {
      validator: function(value) {
        if (self.type() !== 'Birthday') return true;
        if (!value) return true;
        if (rxNoYear.test(value))
          value += Globalize.culture().calendar['/'] + '1904'; // REVIEW: Hardcoded minYear value '1904'
        var date = app.toISO8601(value);
        return  date !== null && moment().diff(moment(date), 'years') >= 14; // REVIEW: Hardcoded value '14'
      },
      message: 'INVALID_BIRTHDAY'
    }
  });

  self.birthday.subscribe(_updateDateFields);

  // used to bind to textbox with anniversary specific validation
  self.anniversary = ko.observable(sdate).extend({ 
    validation: {
      validator: function(value) {
        if (self.type() !== 'Anniversary') return true;
        if (!value) return true;
        if (rxNoYear.test(value))
          value += Globalize.culture().calendar['/'] + '1904'; // REVIEW: Hardcoded minYear value '1904'
        var date = app.toISO8601(value);
        return date !== null;
      },
      message: 'INVALID_ANNIVERSARY'
    }
  });

  self.anniversary.subscribe(_updateDateFields);
};