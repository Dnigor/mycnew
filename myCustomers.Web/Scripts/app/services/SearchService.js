var myc = myc || {};

myc.SearchService = function (config) {

  var _url      = ko.observable(config.url);
  var _criteria = ko.observable({});
  var _isBusy   = ko.observable(false);

  function _search(options) {
    _isBusy(true);

    $.ajax({
      url: _url(),
      type: "POST",
      dataType: "json",
      contentType: "application/json; charset=utf-8",

      data: ko.toJSON(_criteria()),

      complete: function () {
        _isBusy(false);
        if (options.always)
          options.always();
      },

      success: options.success
    });
  }

  return {
    url:      _url,
    criteria: _criteria,
    isBusy:   _isBusy,
    search:   _search
  };
};
