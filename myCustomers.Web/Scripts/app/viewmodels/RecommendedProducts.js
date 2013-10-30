var myc = myc || {};

myc.RecommendedProducts = function (config) {
  var self = this;

  self.cats = ko.observableArray([]);

  function _load(custId) {
    config
      .ajaxService
      .getJSON(config.apiUrl)
      .done(function(data) {
        var vms = [];

        $.each(data, function (i, o) {
          var vm = {
            cat: app.localize('RECOMMENDATION_CATEGORY_' + o.cat.toUpperCase()), // TODO: move localization to the view binding
            isEmpty: true,
            products: []
          };

          $.each(o.products, function (i, p) {
            var pvm = new myc.Product(p);
            vm.products.push(pvm);
            vm.isEmpty = false;
          });

          vms.push(vm);
        });

        self.cats(vms);
      });
  }

  _load();
};
