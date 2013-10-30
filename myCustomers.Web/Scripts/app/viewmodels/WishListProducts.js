var myc = myc || {};

myc.WishListProducts = function(config) {
  var self = this;

  self.products = ko.observableArray([]);
  self.isEmpty  = ko.computed(function () { return self.products().length === 0 });

  function _load(custId) {
    config
      .ajaxService
      .getJSON(config.apiUrl)
      .done(function (data) {
        var pvms = [];

        $.each(data, function (i, p) {
          var pvm = new myc.Product(p);
          pvms.push(pvm);
        });

        self.products(pvms);
      });
  }

  _load();
};
