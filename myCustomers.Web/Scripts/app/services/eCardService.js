var myc = myc || {};

myc.eCardService = function(config) {
  var self = this;

  self.submit = function(message) {
    if (message !== null && typeof message === 'object')
      message = ko.mapping.toJSON(message);

    return $
      .ajax({
        url: config.url, 
        type: 'POST',
        dataType: 'json',
        contentType: 'application/json; charset=utf-8',
        data: message
      });
  };
};
