var myc = myc || {};

myc.AjaxService = function () {
  var self = this;

  self.isBusy = ko.observable(false);

  $(document).ajaxStart(function() { 
    self.isBusy(true); 
  });

  $(document).ajaxError(function() { 
    self.isBusy(false); 
  });

  $(document).ajaxStop(function() { 
    self.isBusy(false); 
  });

  self.getJSON = function(url, data) {
    // strip empty properties from data object
    if (data !== null && typeof data === 'object')
      for (p in data)
        if (data[p] === null || data[p] === '')
          delete data[p];

    return $.getJSON(url, data);
  };

  self.postJSON = function(url, data) {
    if (data !== null && typeof data === 'object')
      data = ko.mapping.toJSON(data);
    
    return $
      .ajax({
        url: url, 
        type: 'POST',
        dataType: 'json',
        contentType: 'application/json; charset=utf-8',
        data: data
      });
  };

  self.del = function(url, data) {
    if (data !== null && typeof data === 'object')
      data = ko.mapping.toJSON(data);

    return $
      .ajax({ 
        url: url, 
        type: 'POST', 
        headers: { 'X-HTTP-Method-Override': 'DELETE' },
        contentType: 'application/json; charset=utf-8',
        data: data
      });
  };
};
