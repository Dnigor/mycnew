var myc = myc || {};

myc.AddProductsForm = function (config) {
  var self = this;

  self.productCatalog      = config.productCatalog;
  self.previouslyOrdered   = config.previouslyOrdered;
  self.recommendedProducts = config.recommendedProducts;
  self.wishListProducts    = config.wishListProducts;
}
